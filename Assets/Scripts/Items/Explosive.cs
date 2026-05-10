using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Explosive : IWeaponEffect
{
    private float _explosionSize;
    private int _damage;

    public bool IsExecuteType => true;

    public void Init(PlayerWeapon playerWeapon, List<float> value) { }
    
    //ex 폭발 (폭발 크기, 폭발 데미지)
    // 폭발크기 : 무기 폭발 크기 * (1 + 서브스탯 폭발 크기 / 100),
    // 폭발 데미지: 무기 데미지(데미지 스탯 배율까지) * (1 + 폭발데미지 스탯 / 100)
    
    public void SetExecuteData(PlayerWeapon playerWeapon, List<float> values)
    {
        //폭발 크기 배율
        if(values.Count < 1) return;
        _explosionSize = values[0];
        _explosionSize *= 1f + playerWeapon.GetStat(SubStats.ExplosiveSize) / 100f;
        _damage = Mathf.FloorToInt(playerWeapon.FinalDamage
                                   * (1f + playerWeapon.GetStat(SubStats.ExplosiveDamage) / 100f));
    }

    public void Execute(IDamageable damageable)
    {
        //착탄 시 폭발 오브젝트 활성.
        var explosion = ObjectPoolingManager.Instance.GetExplosion();
        explosion.SetScale(_explosionSize / 100f);
        explosion.AttackInit(_damage, false, null);
        explosion.AddHitTarget(damageable); //예외 타겟(무기가 적중한 타겟은 제외)
        explosion.Collider.enabled = true; //그리고 Collider 활성화
        explosion.transform.position = damageable.GetTransform().position; //폭발 지점
        explosion.HitboxStartRoutine(ExplosionCoroutine(explosion)); //0.5초 후 사라짐
        //점에서 퍼지는 효과 추가?
    }

    private IEnumerator ExplosionCoroutine(Hitbox explosion)
    {
        yield return new WaitForSeconds(0.5f);
        
        ObjectPoolingManager.Instance.ReleaseExplosion(explosion);
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
