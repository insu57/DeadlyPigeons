using System;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private CapsuleCollider2D _collider;
    private int _damage;
    private bool _isCrit;
    private List<IWeaponEffect> _weaponEffects;

    private void Awake()
    {
        _collider = GetComponent<CapsuleCollider2D>();
    }
    
    public void SetDamage(int damage, bool isCrit,List<IWeaponEffect> weaponEffects)
    {
        _damage = damage;
        _isCrit = isCrit;
        _weaponEffects = weaponEffects;
        foreach (var weaponEffect in _weaponEffects)
        {
            //weaponEffect.Se
        }
    }

    public void SetRadius(float radius)
    {
        _collider.size = new Vector2(radius, radius);
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IDamageable target))
        {
            target.Damage(_damage, _isCrit);
            foreach (var weaponEffect in _weaponEffects)
            {
                weaponEffect.Execute(target);
            }
        }
    }
}
