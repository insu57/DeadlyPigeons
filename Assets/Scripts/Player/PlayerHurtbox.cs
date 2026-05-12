using System;
using UnityEngine;

public class PlayerHurtbox : MonoBehaviour, IDamageable, IPickup
{
    public event Action<int> OnDamage;
    public event Action<int> OnHeal;
    
    public void Damage(int damage, bool isCrit)
    {
        OnDamage?.Invoke(damage);
    }

    public void Heal(int healAmount)
    {
        OnHeal?.Invoke(healAmount);
    }

    public void DotDamage(int duration, int damage, float tick) { }

    public Transform GetTransform() => transform;
    
    public void Pickup(Collectable collectable)
    {
        if (collectable.CollectableType != CollectableType.Material)
        {
            ObjectPoolingManager.Instance.ReleaseCollectable(collectable);
        }
    }
}
