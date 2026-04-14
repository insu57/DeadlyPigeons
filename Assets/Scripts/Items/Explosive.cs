using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Explosive : IWeaponEffect
{
    private float _explosionSize;
    private int _damage;
    
    public IWeaponEffect Clone()
    {
        return new Explosive();
    }

    public void Init(PlayerWeapon playerWeapon, List<float> value) { }
    
    //ex 폭발 (폭발 크기, 폭발 데미지)
    // 폭발크기 : 무기 폭발 크기 * (1 + 서브스탯 폭발 크기 / 100),
    // 폭발 데미지: 무기 데미지(데미지 스탯 배율까지) * (1 + 폭발데미지 스탯 / 100)
    
    public void SetExecuteData(PlayerWeapon playerWeapon, List<float> values)
    {
        //폭발 크기 배율
        _explosionSize = values[0];
        _explosionSize *= 1f + playerWeapon.GetStat(SubStats.ExplosiveSize) / 100f;
        _damage = Mathf.FloorToInt(playerWeapon.FinalDamage
                                   * (1f + playerWeapon.GetStat(SubStats.ExplosiveDamage) / 100f));
    }

    public void Execute(IDamageable damageable)
    {
        //착탄 시 폭발 오브젝트 활성.
        //Debug.Log("Explosive");
        var explosion = ObjectPoolingManager.Instance.GetExplosion(_damage, _explosionSize / 100f);
        explosion.transform.position = damageable.GetTransform().position;
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
