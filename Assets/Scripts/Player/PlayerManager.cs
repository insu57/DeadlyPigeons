using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //Items
    [field: SerializeField] private List<ItemData> itemDataList;

    //Weapons
    [field: SerializeField] private List<GameObject> weaponParents;
    [field: SerializeField] private List<PlayerWeapon> playerWeapons;
    [SerializeField] private PlayerWeapon playerWeaponPrefab;
    public Dictionary<WeaponClasses, int> WeaponClassDict { get; } = new(); //클래스 수치 Dict
    private int _weaponSlotCount = 6; //추후 패시브 등으로 바뀔 수 있음.
    private int _currentWeaponCount = 0;
    public bool WeaponIsFull => _currentWeaponCount == _weaponSlotCount;

    private PlayerControl _playerControl;
    private PlayerStat _playerStat;
    public int CurrentLevel => _playerStat?.CurrentLevel ?? 0;
    private PlayerHurtbox _playerHurtbox;
    private PickupRange _pickupRange;

    private PlayerInfoUI _playerInfoUI;

    public event Action<MainStats, int> OnUpdateMainStat;
    public event Action<SubStats, int> OnUpdateSubStat;
    public event Action<int> OnUpdateLevel; 
    
    public event Action OnCratePickup;
    public event Action<int> OnUpdateMoney;

    public event Action<Sprite, int, int> OnAddWeapon;
    public event Action<ItemData, int> OnAddItem;
    public event Action<int> OnRemoveWeapon;

    public event Action OnPlayerDeath;
    
    private readonly Collider2D[] _hitBuffer = new Collider2D[100];

    private void Awake()
    {
        TryGetComponent(out _playerControl);
        TryGetComponent(out _playerStat);
        _playerHurtbox = GetComponentInChildren<PlayerHurtbox>();
        _pickupRange = GetComponentInChildren<PickupRange>();

        _playerInfoUI = FindFirstObjectByType<PlayerInfoUI>();

        for (int i = 0; i < (int)WeaponClasses.None; i++)
        {
            var weaponClass = (WeaponClasses)i;
            WeaponClassDict[weaponClass] = 0;
        }

        _playerStat.OnChangeMainStats += UpdateStat;
        _playerStat.OnChangeSubStats += UpdateStat;
        _playerStat.OnChangeHealth += UpdateHealth;
        _playerStat.OnChangeExp += UpdateExp;
        _playerStat.OnChangeMoney += UpdateMoney;
        _playerStat.OnPlayerDeath += () => OnPlayerDeath?.Invoke();
        
        _playerHurtbox.OnDamage += HandleOnDamage;
        _playerHurtbox.OnHeal += HandleOnHeal;
        _playerHurtbox.OnGetCollectable += HandleOnGetCollectable;

        _pickupRange.OnGetMaterial += HandleOnGetMaterial;

        _playerInfoUI.Init(this);
    }

    public void InitCharacter(CharacterData charData, List<ItemData> items, List<WeaponData> weapons)
    {
        Debug.Log("InitStat");
        //CharData.. 추후 플레이어 스트라이트 및 애니메이션 관련 초기화

        _playerStat.InitStat();
        foreach (var itemData in items) //초기 아이템 장착
        {
            AddItem(itemData);
        }

        InitWeapons(weapons); //초기 무기 장착
    }

    public void WavePlayerInit()
    {
        _playerStat.WaveStatInit();
    }

    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        _playerInfoUI.UpdateHealthBar(currentHealth, maxHealth);
    }
    
    private void UpdateExp(PlayerLevelInfo playerLevelInfo)
    {
        var lv = playerLevelInfo.currentLevel;
        var currentExp = playerLevelInfo.currentExp;
        var targetExp = playerLevelInfo.targetExp;

        _playerInfoUI.UpdateExpBar(lv, currentExp, targetExp);

        if (playerLevelInfo.hasLevelUp)
        {
            //레벨업...
            OnUpdateLevel?.Invoke(lv);
        }
    }

    private void UpdateMoney(int money)
    {
        _playerInfoUI.UpdateMoney(money);
        OnUpdateMoney?.Invoke(money);
    }

    public int GetMoney => _playerStat.Money;

    public void ChangeMoney(int money)
    {
        _playerStat.ChangeMoney(money);
    }

    public void AddItem(ItemData itemData)
    {
        itemDataList.Add(itemData);
        _playerStat.AddItem(itemData);
        int idx = itemDataList.Count - 1;
        OnAddItem?.Invoke(itemData, idx);
    }

    public int GetItemPriceStat => _playerStat.ItemPrice;

    public CurrentWeaponStat GetWeaponStat(WeaponData weaponData, int tier) =>
        _playerStat.GetWeaponStat(weaponData, tier);

    private void InitWeapons(List<WeaponData> initWeaponList) //무기 초기화
    {
        for (int i = 0; i < _weaponSlotCount; i++) //무기 슬롯 초기화.
        {
            var playerWeapon = Instantiate(playerWeaponPrefab, weaponParents[i].transform);
            playerWeapons.Add(playerWeapon);
            playerWeapon.SetCenter(transform);
            playerWeapon.gameObject.SetActive(false);
            playerWeapon.InitPlayerWeapon(_playerStat.FinalMainStat, _playerStat.FinalSubStat);
            playerWeapon.OnHealthAbsorb += HandleOnHeal; //흡혈 성공 시 회복
            //PlayerWeapon 초기화
        }

        for (int i = 0; i < initWeaponList.Count; i++)
        {
            if (i >= _weaponSlotCount) break;

            playerWeapons[i].SetWeaponData(initWeaponList[i], initWeaponList[i].WeaponStat.initTier); // 초기 무기 장착
            playerWeapons[i].gameObject.SetActive(true);

            _currentWeaponCount++; //현재 무기 수

            var classes = initWeaponList[i].WeaponStat.classes;
            foreach (var weaponClass in classes)
            {
                WeaponClassDict[weaponClass]++; //무기 클래스 보너스 추가
            }
        }

        SetWeaponClassBonus(); //무기 보너스 설정.

        for (int i = 0; i < initWeaponList.Count; i++)
        {
            var sprite = initWeaponList[i].Sprite;
            OnAddWeapon?.Invoke(sprite, initWeaponList[i].WeaponStat.initTier ,i);
        }

        //sync?
        _playerStat.SyncStat();
    }

    public void AddWeapon(WeaponData weaponData, int tier)
    {
        if (WeaponIsFull) return; //무기 최대.

        _currentWeaponCount++; //현재 무기 수 추가

        int idx = _currentWeaponCount - 1; //PlayerWeaponIdx

        playerWeapons[idx].SetWeaponData(weaponData, tier);
        playerWeapons[idx].gameObject.SetActive(true);

        OnAddWeapon?.Invoke(weaponData.Sprite, tier, idx);

        var classes = weaponData.WeaponStat.classes;
        foreach (var weaponClass in classes)
        {
            WeaponClassDict[weaponClass]++; //무기 클래스 보너스 추가
        }

        SetWeaponClassBonus(); //무기 보너스 설정.
    }

    private void RemoveWeapon(int targetIdx)
    {
        //재정렬... UI추가 필요.
        OnRemoveWeapon?.Invoke(targetIdx);

        var classes = playerWeapons[targetIdx].WeaponData.WeaponStat.classes;
        foreach (var weaponClass in classes) //보너스 제거.
        {
            WeaponClassDict[weaponClass]--;
        }

        SetWeaponClassBonus(); //클래스 보너스 업데이트

        for (int i = targetIdx; i < _currentWeaponCount - 1; i++) //무기 재정렬.(빈 부분 당기기)
        {
            var weaponData = playerWeapons[i + 1].WeaponData;
            var tier = playerWeapons[i + 1].Tier;
            playerWeapons[i].SetWeaponData(weaponData, tier);
        }

        playerWeapons[_currentWeaponCount - 1].SetWeaponData(null, 0); //마지막 무기를 비우고 비활성.
        playerWeapons[_currentWeaponCount - 1].gameObject.SetActive(false);

        _currentWeaponCount--;
    }

    public void RecycleWeapon(int idx)
    {
        var targetWeapon = playerWeapons[idx].WeaponData;
        var recyclePrice = WeaponStatCalculator.GetRecyclePrice(targetWeapon, playerWeapons[idx].Tier);
        ChangeMoney(recyclePrice);
        RemoveWeapon(idx);
    }

    public void CombineWeapon(int idx)
    {
        var targetWeapon = playerWeapons[idx];
        var targetWeaponData = targetWeapon.WeaponData;
        var tier = targetWeapon.Tier;

        bool hasDuplicateWeapon = CheckWeaponCanCombine(idx);
        
        if(!hasDuplicateWeapon) return;
        
        var nextTier = targetWeapon.Tier + 1;
        
        RemoveWeapon(idx);
        
        //무기를 제거하면 인덱스가 변하기 때문에 다시 찾기. 개선 방안?
        for (int i = 0; i < playerWeapons.Count; i++) 
        {
            if(!playerWeapons[i].WeaponData) continue;   
            if(playerWeapons[i].WeaponData.ID != targetWeaponData.ID) continue;
            if(playerWeapons[i].Tier != tier) continue;
            
            RemoveWeapon(i);
            break;
        }
        
        AddWeapon(targetWeaponData, nextTier);
    }

    //Remove Item 구현?

    public void GetStatUpgrade(MainStats stat, int tier)
    {
        var amount = DataManager.Instance.LvUpStatUpgradeDict[stat][tier - 1];//idx 0~3
        _playerStat.ChangeStat(stat, amount);
    }
    
    private void UpdateStat(MainStats stat, int value)
    {
        //_playerInfoUI.UpdateMainStat(stat, value);
        OnUpdateMainStat?.Invoke(stat, value);
     
        foreach (var playerWeapon in playerWeapons)
        {
            if (playerWeapon.WeaponData)
            {
                playerWeapon.UpdateStat(stat);
            }
        }
    }

    private void UpdateStat(SubStats stat, int value)
    {
        //_playerInfoUI.UpdateSubStat(stat, value);
        OnUpdateSubStat?.Invoke(stat, value);
        
        foreach (var playerWeapon in playerWeapons)
        {
            if (playerWeapon.WeaponData)
            {
                playerWeapon.UpdateStat(stat);
            }
        }
    }

    private void HandleOnDamage(int damage)
    {
        _playerStat.Damage(damage);
    }

    private void HandleOnHeal(int healAmount)
    {
        _playerStat.Heal(healAmount);
    }

    private void HandleOnGetCollectable(CollectableType collectableType)
    {
        _playerStat.GetMeat(); //공통적으로 회복 아이템 처리
        if (collectableType == CollectableType.Crate)
        {
            //Crate...
            OnCratePickup?.Invoke();
        }
    }

    private void HandleOnGetMaterial(int amount)
    {
        _playerStat.GetMaterial(amount);
    }

    public void GetStock()
    {
        _playerStat.GetStock();
    }
    
    public void GetClosestEnemy(TargetInfo enemy) //가장 가까운 적 => PlayerWeapon에 주입
    {
        foreach (var playerWeapon in playerWeapons)
        {
            playerWeapon.SetTarget(enemy);
        }
    }

    public void HandleOnShowWeaponInfo(int index, SelectButton selectBtn, ItemGridUI itemGridUI) //무기 정보 표시
    {
        var playerWeapon =  playerWeapons[index];
        var currentWeaponStat = GetWeaponStat(playerWeapon.WeaponData, playerWeapon.Tier);

        bool canCombine = CheckWeaponCanCombine(index);
        
        itemGridUI.ShowWeaponInfo(currentWeaponStat, selectBtn, index, canCombine);
    }

    private bool CheckWeaponCanCombine(int idx)
    {
        var playerWeapon =  playerWeapons[idx];
        
        if(playerWeapon.Tier == DataManager.GetMaxTier) return false;
        
        for (int i = 0; i < playerWeapons.Count; i++)
        {
            if(i == idx) continue;
            if(!playerWeapons[i].WeaponData) continue;
            if(playerWeapon.WeaponData.ID != playerWeapons[i].WeaponData.ID) continue;
            if(playerWeapon.Tier != playerWeapons[i].Tier) continue;
                
            return true;
        } 
        
        return false;
    }
    
    private void SetWeaponClassBonus() //클래스 보너스 업데이트
    { 
        foreach (var (weaponClass,bonus) in WeaponClassDict)
        {
            if(bonus <= 1) continue; //1이하는 보너스 x
            var effectList = DataManager.Instance.WeaponClassBonusDict[weaponClass];
            
            _playerStat.ResetStatClassBonus();//보너스 업데이트 전 리셋.
            
            foreach (var effect in effectList)
            {
                if(effect.IsUnavailable) continue;
                
                var amount = effect.values[bonus - 2]; 
                //보너스에 따른 스탯 수치 가져오기.(인덱스 처리 -2, 보너스 2~6)
                
                if (effect.IsMain)
                {
                    _playerStat.UpdateStatClassBonus(effect.mainStat, amount); 
                }
                else
                {
                    _playerStat.UpdateStatClassBonus(effect.subStat, amount);
                }
            }
        }
    }
    
}
