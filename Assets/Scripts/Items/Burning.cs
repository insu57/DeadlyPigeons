using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Burning : IWeaponEffect
{
    //히트박스(투사체)에만 적용
    public IWeaponEffect Clone()
    {
        return new Burning();
    }

    public void Init(PlayerWeapon playerWeapon, List<float> value) { }

    public void Execute(IDamageable target, List<float> values)
    {
       //values = { 지속시간, 데미지, 데미지 틱(기본 1초) }
       //타겟에 화상효과 적용.
    }
}
