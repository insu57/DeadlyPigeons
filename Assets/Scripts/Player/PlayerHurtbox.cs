using System;
using UnityEngine;

public class PlayerHurtbox : MonoBehaviour, IDamageable, IPickup
{
    public event Action<int> OnDamage;
    public event Action<int> OnHeal;
    public event Action<CollectableType> OnGetCollectable;
    private float _invincibleTimer;
    private const float InvincibleDuration = 0.5f;
    private bool IsInvincible => _invincibleTimer > 0f;

    public void Update()
    {
        if(IsInvincible) 
        {
            _invincibleTimer -= Time.deltaTime;
        }
    }
    
    public void Damage(int damage, bool isCrit)
    {
        if(IsInvincible) return;
        
        OnDamage?.Invoke(damage);
        
        _invincibleTimer = InvincibleDuration;
    }

    public void Heal(int healAmount)
    {
        OnHeal?.Invoke(healAmount);
    }

    public void DotDamage(int duration, int damage, float tick) { }

    public Transform GetTransform() => transform;
    
    public void Pickup(Collectable collectable)
    {
        if (collectable.CollectableType != CollectableType.Material) //재료 획득(Exp, Money)
        {
            OnGetCollectable?.Invoke(collectable.CollectableType);
            ObjectPoolingManager.Instance.ReleaseCollectable(collectable);
        }
    }
}
