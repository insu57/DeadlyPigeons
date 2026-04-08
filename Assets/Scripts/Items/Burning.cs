using System;
using UnityEngine;

[Serializable]
public class Burning : IWeaponEffect
{
    //히트박스(투사체)에만 적용
    public void Execute(IDamageable target, int[] values)
    {
       //values = { 지속시간, 데미지, 데미지 틱(기본 1초) }
    }
}
