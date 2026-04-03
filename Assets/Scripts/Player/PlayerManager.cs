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
        
        _playerStat.OnChangeMainStats += UpdateMainStat;
        _playerStat.OnChangeSubStats += UpdateSubStat;

        _playerInfoUI.OnShowWeaponInfo += HandleOnShowWeaponInfo;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void InitStat(CharacterData charData)
    {
        Debug.Log("InitStat");
        _playerStat.InitStat(charData);
    }

    public void InitWeapons(List<WeaponData> weapons)
    {
        _weaponManager.InitWeaponSlot(_weaponSlotCount);
        _weaponManager.SetInitWeapons(weapons);

        _playerInfoUI.SetWeaponSlots(_weaponSlotCount);

        for (int i = 0; i < weapons.Count; i++)
        {
            var sprite = weapons[i].Sprite;
            _playerInfoUI.AddWeapon(sprite, i);
        }
    }

    private void UpdateMainStat(MainStats stat, int value)
    {
        _playerInfoUI.UpdateMainStat(stat, value);
        _weaponManager.UpdateMainStats(stat, value);        
    }

    private void UpdateSubStat(SubStats stat, int value)
    {
        _playerInfoUI.UpdateSubStat(stat, value);
        _weaponManager.UpdateSubStats(stat, value);
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
}
