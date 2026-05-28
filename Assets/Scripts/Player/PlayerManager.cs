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
    public Dictionary<WeaponClasses, int> WeaponClassDict { get; } = new();//클래스 수치 Dict
    private int _weaponSlotCount = 6; //추후 패시브 등으로 바뀔 수 있음.
    private int _currentWeaponCount = 0;
    public bool WeaponIsFull => _currentWeaponCount == _weaponSlotCount;
    
    private PlayerControl _playerControl;
    private PlayerStat _playerStat;
    public int CurrentLevel => _playerStat?.CurrentLevel ?? 0;
    private PlayerHurtbox  _playerHurtbox;
    private PickupRange _pickupRange;
    
    private PlayerInfoUI _playerInfoUI;
    
    public event Action OnPlayerLevelUp;
    public event Action OnCratePickup;
    public event Action<int> OnUpdateMoney; 

    public event Action<Sprite, int> OnAddWeapon;
    public event Action<ItemData, int> OnAddItem;
    //public event Action<Dictionary<WeaponClasses, int>> OnSetWeaponClassBonus; 
    
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
            OnPlayerLevelUp?.Invoke();
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
            //PlayerWeapon 초기화
        }
        
        for (int i = 0; i < initWeaponList.Count; i++)
        {
            if(i >= _weaponSlotCount) break;
            
            playerWeapons[i].SetWeaponData(initWeaponList[i], initWeaponList[i].WeaponStat.initTier); // 초기 무기 장착
            playerWeapons[i].gameObject.SetActive(true);

            _currentWeaponCount++;//현재 무기 수
            
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
            //_playerInfoUI.AddWeapon(sprite, i);
            OnAddWeapon?.Invoke(sprite, i);
        }
        
        //sync?
        _playerStat.SyncStat();
    }

    public void AddWeapon(WeaponData weaponData, int tier) //에러...
    {
        if(WeaponIsFull) return; //무기 최대.
        
        _currentWeaponCount++;//현재 무기 수 추가
        
        int idx = _currentWeaponCount - 1; //PlayerWeaponIdx
        
        playerWeapons[idx].SetWeaponData(weaponData, tier);
        playerWeapons[idx].gameObject.SetActive(true);

        OnAddWeapon?.Invoke(weaponData.Sprite, idx);
            
        var classes = weaponData.WeaponStat.classes;
        foreach (var weaponClass in classes)
        {
            WeaponClassDict[weaponClass]++; //무기 클래스 보너스 추가
        }
        SetWeaponClassBonus(); //무기 보너스 설정.
    }

    public void RemoveWeapon(int targetIdx)
    {
        //재정렬... UI추가 필요.
        var classes = playerWeapons[targetIdx].WeaponData.WeaponStat.classes;
        foreach (var weaponClass in classes) //보너스 제거.
        {
            WeaponClassDict[weaponClass]--;
        }
        SetWeaponClassBonus(); //클래스 보너스 업데이트
        
        for (int i = targetIdx; i < _currentWeaponCount - 1; i++) //무기 재정렬.(빈 부분 당기기)
        {
            var weaponData = playerWeapons[i].WeaponData;
            var tier = playerWeapons[i].Tier;
            playerWeapons[i + 1].SetWeaponData(weaponData, tier);
        }
        
        playerWeapons[_currentWeaponCount - 1].SetWeaponData(null, 0); //마지막 무기를 비우고 비활성.
        playerWeapons[_currentWeaponCount - 1].gameObject.SetActive(false);
        
        _currentWeaponCount--;
    }

    //Remove Item 구현?

    public void GetStatUpgrade(MainStats stat, int tier)
    {
        var amount = DataManager.Instance.LvUpStatUpgradeDict[stat][tier - 1];//idx 0~3
        _playerStat.UpdateStat(stat, amount);
    }
    
    private void UpdateStat(MainStats stat, int value)
    {
        _playerInfoUI.UpdateMainStat(stat, value);
     
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
        _playerInfoUI.UpdateSubStat(stat, value);
        
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
        //var currentWeaponStat = playerWeapon.GetCurrentWeaponStat();
        var finalMain = _playerStat.FinalMainStat;
        var finalSub = _playerStat.FinalSubStat;
        var currentWeaponStat = WeaponStatCalculator.GetCurrentWeaponStat
            (playerWeapon.WeaponData, playerWeapon.Tier, finalMain, finalSub);
        itemGridUI.ShowWeaponInfo(currentWeaponStat, selectBtn, index);
    }
    
    private void SetWeaponClassBonus() //클래스 보너스 업데이트
    { 
        //OnSetWeaponClassBonus?.Invoke(WeaponClassDict); //개선?
        
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
