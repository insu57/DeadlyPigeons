using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Explosion : IWeaponEffect
{
    public IWeaponEffect Clone()
    {
        return new Explosion();
    }

    public void Init(PlayerWeapon playerWeapon, List<float> value) { }
    public void SetExecuteData(PlayerWeapon playerWeapon, List<float> values)
    {
        
    }

    public void Execute(IDamageable damageable)
    {
        //values = { 폭발 범위, 폭발 데미지(최종) }
        //착탄 시 폭발 오브젝트 활성.
    }
}
