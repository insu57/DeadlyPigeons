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
    private Dictionary<WeaponClasses, int> WeaponClassDict { get; } = new();//클래스 수치 Dict
    private int _weaponSlotCount = 6;
    
    private PlayerControl _playerControl;
    private PlayerStat _playerStat;
    private PlayerInfoUI _playerInfoUI;
    //private WeaponManager _weaponManager;
    
    //ranged -> playerStat으로?
    private int _globalPiercing = 0;
    private int _globalPiercingDmgPer = 0;

    private void Awake()
    {
        TryGetComponent(out _playerControl);
        TryGetComponent(out _playerStat);
        _playerInfoUI = FindFirstObjectByType<PlayerInfoUI>();
        
        for (int i = 0; i < (int)WeaponClasses.None; i++)
        {
            var weaponClass = (WeaponClasses)i;
            WeaponClassDict[weaponClass] = 0;
        }
        
        _playerStat.OnChangeMainStats += UpdateStat;
        _playerStat.OnChangeSubStats += UpdateStat;

        _playerInfoUI.OnShowWeaponInfo += HandleOnShowWeaponInfo;
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

    private void AddItem(ItemData itemData)
    {
        itemDataList.Add(itemData);
        _playerStat.AddItem(itemData);
        int idx = itemDataList.Count - 1;
        _playerInfoUI.AddItem(itemData, idx);
    }
    
    private void InitWeapons(List<WeaponData> initWeaponList) //무기 초기화
    {
        for (int i = 0; i < _weaponSlotCount; i++) //무기 슬롯 초기화.
        {
            var playerWeapon = Instantiate(playerWeaponPrefab, weaponParents[i].transform); 
            playerWeapons.Add(playerWeapon);
            playerWeapon.SetCenter(transform);
            playerWeapon.gameObject.SetActive(false);
            //PlayerWeapon 초기화
        }
        
        for (int i = 0; i < initWeaponList.Count; i++)
        {
            if(i >= _weaponSlotCount) break;
            
            playerWeapons[i].SetWeaponData(initWeaponList[i], initWeaponList[i].WeaponStat.initTier); // 초기 무기 장착
            playerWeapons[i].gameObject.SetActive(true);

            var classes = initWeaponList[i].WeaponStat.classes;
            foreach (var weaponClass in classes)
            {
                WeaponClassDict[weaponClass]++; //무기 클래스 보너스 추가
            }
        }
        
        SetWeaponClassBonus(); //무기 보너스 설정.

        _playerInfoUI.InitWeaponSlots(_weaponSlotCount); //무기 슬롯 UI 초기화

        for (int i = 0; i < initWeaponList.Count; i++)
        {
            var sprite = initWeaponList[i].Sprite;
            _playerInfoUI.AddWeapon(sprite, i);
        }
    }

    private void UpdateStat(MainStats stat, int value)
    {
        _playerInfoUI.UpdateMainStat(stat, value);
      
        foreach (var playerWeapon in playerWeapons)
        {
            if (playerWeapon.WeaponData)
            {
                playerWeapon.UpdateMainStats(stat, value);
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
                playerWeapon.UpdateSubStats(stat, value);
            }
        }
    }

    public void GetClosestEnemy(TargetInfo enemy)
    {
        foreach (var playerWeapon in playerWeapons)
        {
            playerWeapon.SetTarget(enemy);
        }
    }

    private void HandleOnShowWeaponInfo(int index, SelectButton selectBtn)
    {
        var weaponData = playerWeapons[index].WeaponData;
        _playerInfoUI.ShowWeaponInfo(weaponData, selectBtn, index);
    }

    private void SetWeaponClassBonus() //클래스 보너스 초기화
    {
        _playerInfoUI.SetWeaponClassBonus(WeaponClassDict);
        
        foreach (var (weaponClass,bonus) in WeaponClassDict)
        {
            if(bonus <= 1) continue; //1이하는 보너스 x
            var effectList = DataManager.Instance.WeaponClassBonusDict[weaponClass];
            
            foreach (var effect in effectList)
            {
                if(effect.IsUnavailable) continue;
                
                var amount = effect.values[bonus - 2]; 
                //보너스에 따른 스탯 수치 가져오기.(인덱스 처리 -2, 보너스 2~6)
                
                if (effect.IsMain)
                {
                    _playerStat.UpdateStat(effect.mainStat, amount); 
                    //단순히 더하는 방식 말고??? 기존 스탯(패시브, 아이템, 레벨업 보너스)(변동x) + 무기 클래스 보너스(변동o)
                }
                else
                {
                    _playerStat.UpdateStat(effect.subStat, amount);
                }
            }
        }
    }
}
