using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Bounces : IWeaponEffect
{
    private int _bounces;
    public bool IsExecuteType => false;
    
    public void Init(PlayerWeapon playerWeapon, List<float> value)
    {
        if(value.Count < 1) return;
        _bounces = (int)value[0];
        playerWeapon.SetBounces(_bounces);
    }

    public void SetExecuteData(PlayerWeapon playerWeapon, List<float> values) { }

    public void Execute(IDamageable target) { }
    public void AttackEnd()
    {
        throw new NotImplementedException();
    }

    public void Remove(PlayerWeapon playerWeapon)
    {
        playerWeapon.SetBounces(-_bounces);
    }
}
