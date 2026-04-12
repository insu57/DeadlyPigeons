using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Burning : IWeaponEffect
{
    //히트박스(투사체)에만 적용
    private int _duration;
    private int _damage;
    private float _tick;
    
    public IWeaponEffect Clone()
    {
        return new Burning();
    }

    public void Init(PlayerWeapon playerWeapon, List<float> value) { }
    public void SetExecuteData(PlayerWeapon playerWeapon,List<float> values)
    {
        _duration = (int)values[0];
        _damage = (int)values[1];
        _tick = playerWeapon.BurningTick;
    }

    public void Execute(IDamageable target)
    {
       //values = { 지속시간, 데미지, 데미지 틱(기본 1초) }
       //타겟에 화상효과 적용.
        Debug.Log($"Burning! damage:{_damage},  tick:{_tick}, duration:{_duration}");
       target.DotDamage(_duration, _damage, _tick); //도트 데미지(화상), 화상을 제외한 도트 데미지가 생긴다면 enum으로 구분
    }
}
