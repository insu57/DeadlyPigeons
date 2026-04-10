using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Piercing : IWeaponEffect
{
    public IWeaponEffect Clone()
    {
        return new Piercing();
    }

    public void Init(PlayerWeapon playerWeapon, List<float> value)
    {
        int piercing = (int)value[0];
        int piercingDmg = (int)value[1];
        playerWeapon.SetPiercing(piercing, piercingDmg);
        Debug.Log($"관통: {piercing}, 관통 데미지: {piercingDmg}");
    }

    public void Execute(IDamageable target, List<float> values) { }
}
