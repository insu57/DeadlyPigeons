using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [field: SerializeField] private List<GameObject> weaponParents;
    [field: SerializeField] private List<PlayerWeapon> weapons;
    private int _weaponSlot = 6;

    private void Start()
    {
        
    }

    private void InitWeaponSlot()
    {
        
    }
}
