using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerControl _playerControl;
    private PlayerStat _playerStat;
    private PlayerInfoUI _playerInfoUI;
    private WeaponManager _weaponManager;
    private Transform _closestEnemy;

    private void Awake()
    {
        TryGetComponent(out _playerControl);
        TryGetComponent(out _playerStat);
        _playerInfoUI = FindFirstObjectByType<PlayerInfoUI>();
        _weaponManager = GetComponentInChildren<WeaponManager>();
        
        _playerStat.OnChangeMainStats += UpdateMainStat;
        _playerStat.OnChangeSubStats += UpdateSubStat;
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
        _weaponManager.SetInitWeapons(weapons);
    }

    private void UpdateMainStat(MainStats stat, int value)
    {
        _playerInfoUI.UpdateMainStat(stat, value);
    }

    private void UpdateSubStat(SubStats stat, int value)
    {
        _playerInfoUI.UpdateSubStat(stat, value);
    }

    public void GetClosestEnemy(Transform enemy)
    {
        _closestEnemy = enemy;
        _weaponManager.SetTarget(enemy);
    }
}
