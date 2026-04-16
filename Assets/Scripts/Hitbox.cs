using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    //근접, 폭발 등 히트박스.(기본적으로 한번 공격 시 같은 적을 중복하여 피해를 주지않음)
    public CapsuleCollider2D Collider {get; private set;}
    private int _damage;
    private bool _isCrit;
    private List<IWeaponEffect> _weaponEffects;
    private HashSet<IDamageable> _hitTargets = new(); //공격했던 타겟 해시셋(중복 타격 방지)
    
    private void Awake()
    {
        Collider = GetComponent<CapsuleCollider2D>();
    }
    
    public void AttackInit(int damage, bool isCrit,List<IWeaponEffect> weaponEffects)
    {
        _damage = damage;
        _isCrit = isCrit;
        _weaponEffects = weaponEffects;
        _hitTargets.Clear();//공격했던 타겟 해시셋 비우기
    }

    public void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }
    //hitTarget 추가 메서드 -> 폭발무기 첫 적중 타겟은 폭발 피해에서 제외?or 지금 처럼 두번 데미지

    public void AddHitTarget(IDamageable target) //폭발 등 이중으로 데미지가 들어가지 않도록 타겟 헤시셋에 추가
    {
        _hitTargets.Add(target);
    }

    public void HitboxStartRoutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
    
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable target))
        {
            if(!_hitTargets.Add(target)) return; //타겟이 이미 있다면 스킵 아니라면 해시셋에 추가

            target.Damage(_damage, _isCrit);
            if(_weaponEffects == null) return;
            foreach (var weaponEffect in _weaponEffects)
            {
                weaponEffect.Execute(target);
            }
        }
    }
}
