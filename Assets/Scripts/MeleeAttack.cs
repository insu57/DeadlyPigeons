using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    private int _damage;
    private bool _isCrit;
    private List<IWeaponEffect> _weaponEffects;
    
    public void SetDamage(int damage, bool isCrit,List<IWeaponEffect> weaponEffects)
    {
        _damage = damage;
        _isCrit = isCrit;
        _weaponEffects = weaponEffects;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable target))
        {
            target.Damage(_damage, _isCrit);
        }
    }
}
