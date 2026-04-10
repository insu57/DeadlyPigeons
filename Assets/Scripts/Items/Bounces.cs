using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Bounces : IWeaponEffect
{
    public IWeaponEffect Clone()
    {
        return new Bounces();
    }

    public void Init(PlayerWeapon playerWeapon, List<float> value)
    {
        playerWeapon.SetBounces((int)value[0]);
    }

    public void Execute(IDamageable target, List<float> values) { }
}
