using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Piercing : IWeaponEffect
{
    private int _piercing;
    private int _piercingDmg;
    
    public IWeaponEffect Clone()
    {
        return new Piercing();
    }

    public void Init(PlayerWeapon playerWeapon, List<float> value)
    {
        _piercing = (int)value[0];
        _piercingDmg = (int)value[1];
        playerWeapon.SetPiercing(_piercing, _piercingDmg);
        Debug.Log($"관통: {_piercing}, 관통 데미지: {_piercingDmg}");
    }

    public void SetExecuteData(PlayerWeapon playerWeapon, List<float> values) { }

    public void Execute(IDamageable target) { }
    public void Remove(PlayerWeapon playerWeapon)
    {
        playerWeapon.SetPiercing(-_piercing, -_piercingDmg);
    }
}
