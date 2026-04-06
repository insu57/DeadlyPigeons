using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerControl _playerControl;
    private PlayerStat _playerStat;
    private PlayerInfoUI _playerInfoUI;
    private WeaponManager _weaponManager;
    private TargetInfo _closestEnemy;
    private int _weaponSlotCount = 6;

    private void Awake()
    {
        TryGetComponent(out _playerControl);
        TryGetComponent(out _playerStat);
        _playerInfoUI = FindFirstObjectByType<PlayerInfoUI>();
        _weaponManager = GetComponentInChildren<WeaponManager>();
        
        _playerStat.OnChangeMainStats += UpdateStat;
        _playerStat.OnChangeSubStats += UpdateStat;

        _playerInfoUI.OnShowWeaponInfo += HandleOnShowWeaponInfo;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void InitStatWeapons(CharacterData charData, List<WeaponData> weapons)
    {
        Debug.Log("InitStat");
        _playerStat.InitStat(charData);
        InitWeapons(weapons);
    }

    private void InitWeapons(List<WeaponData> weapons)
    {
        _weaponManager.InitWeaponSlot(_weaponSlotCount);
        _weaponManager.SetInitWeapons(weapons);
        SetWeaponClassBonus();

        _playerInfoUI.SetWeaponSlots(_weaponSlotCount);

        for (int i = 0; i < weapons.Count; i++)
        {
            var sprite = weapons[i].Sprite;
            _playerInfoUI.AddWeapon(sprite, i);
        }
    }

    private void UpdateStat(MainStats stat, int value)
    {
        _playerInfoUI.UpdateMainStat(stat, value);
        _weaponManager.UpdateStat(stat, value);        
    }

    private void UpdateStat(SubStats stat, int value)
    {
        _playerInfoUI.UpdateSubStat(stat, value);
        _weaponManager.UpdateStat(stat, value);
    }

    public void GetClosestEnemy(TargetInfo enemy)
    {
        _closestEnemy = enemy;
        _weaponManager.SetTarget(enemy);
    }

    private void HandleOnShowWeaponInfo(int index, SelectButton selectBtn)
    {
        var weaponData = _weaponManager.GetWeaponInfo(index);
        _playerInfoUI.ShowWeaponInfo(weaponData, selectBtn, index);
    }

    private void SetWeaponClassBonus()
    {
        _playerInfoUI.SetWeaponClassBonus(_weaponManager.WeaponClassDict);
        
        foreach (var (weaponClass,bonus) in _weaponManager.WeaponClassDict)
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
