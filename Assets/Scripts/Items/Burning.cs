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
    //ex 화상 (1 지속시간 3데미지(+100% 원소 데미지)
    //->( 3 + 8(원소 데미지 스탯) * 배율(100%) / 100) * ( 1 + 데미지 스탯 배율 / 100);
    public void SetExecuteData(PlayerWeapon playerWeapon,List<float> values)
    {
        //values = { 지속시간, 데미지, 데미지 틱(기본 1초) }
        _duration = (int)values[0];
        _damage = (int)values[1];
        _tick = playerWeapon.BurningTick;
    }

    public void Execute(IDamageable target)
    {
        //타겟에 화상효과 적용.
        //Debug.Log($"Burning! damage:{_damage},  tick:{_tick}, duration:{_duration}");
        target.DotDamage(_duration, _damage, _tick); //도트 데미지(화상), 화상을 제외한 도트 데미지가 생긴다면 enum으로 구분
    }

    public void AttackEnd()
    {
        throw new NotImplementedException();
    }

    public void Remove(PlayerWeapon playerWeapon)
    {
        throw new NotImplementedException();
    }
}
