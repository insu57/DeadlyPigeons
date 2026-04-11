using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Burning : IWeaponEffect
{
    //히트박스(투사체)에만 적용
    private float _duration;
    private float _damage;
    private float _tick;
    
    public IWeaponEffect Clone()
    {
        return new Burning();
    }

    public void Init(PlayerWeapon playerWeapon, List<float> value) { }
    public void SetExecuteData(PlayerWeapon playerWeapon,List<float> values)
    {
        _duration = values[0];
        _damage = values[1];
        _tick = playerWeapon.BurningTick;
        Debug.Log("Burning:"+_damage);
        Debug.Log("Burning:"+_duration);
        Debug.Log("Burning:"+_tick);
    }

    public void Execute(IDamageable target)
    {
       //values = { 지속시간, 데미지, 데미지 틱(기본 1초) }
       //타겟에 화상효과 적용.
       Debug.Log("Burning:" );
       
    }
}
