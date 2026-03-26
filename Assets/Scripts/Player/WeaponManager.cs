using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [field: SerializeField] private List<GameObject> weaponParents;
    [field: SerializeField] private List<PlayerWeapon> weapons;
    [SerializeField] private PlayerWeapon playerWeaponPrefab;
    private int _weaponSlot = 6;

    private void Awake()
    {
        InitWeaponSlot();
    }
    
    private void Start()
    {
        
    }

    private void InitWeaponSlot()
    {
        for (int i = 0; i < _weaponSlot; i++) //무기 슬롯 초기화.
        {
            var playerWeapon = Instantiate(playerWeaponPrefab, weaponParents[i].transform);
            weapons.Add(playerWeapon);
            playerWeapon.gameObject.SetActive(false);
        }
    }

    public void SetInitWeapons(List<WeaponData> weaponList)
    {
        for (int i = 0; i < weaponList.Count; i++)
        {
            if(i >= _weaponSlot) break;
            
            weapons[i].SetWeaponData(weaponList[i], weaponList[i].WeaponStat.initTier); // 초기 무기 장착
            weapons[i].gameObject.SetActive(true);
        }
    }

    public void SetTarget(Transform target)
    {
        foreach (var playerWeapon in weapons)
        {
            playerWeapon.SetTarget(target);
        }
    }
}
