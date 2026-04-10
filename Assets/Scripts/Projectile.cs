using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

public class Projectile : MonoBehaviour
{
    private int _damage;
    private int _piercing;
    private int _piercingDmgPer;
    //public void SetDamage(int damage) => _damage = damage; //수정필
    private float _lifeTimer;
    private float _speed = 10f; //temp
    private Rigidbody2D _rigidbody2D;
    private List<IWeaponEffect> _weaponEffects;
    
    private void Awake()
    {
        TryGetComponent(out _rigidbody2D);
    }

    private void Update()
    {
        ProjectileRange();
    }

    public void Initialize(int damage, int piercing, int piercingDmgPer, int layer)
    {
        _damage = damage;
        _piercing = piercing;
        _piercingDmgPer = piercingDmgPer;
        gameObject.layer = layer;
        //_weaponEffects = effects;
    }
    
    public void Fire(Vector3 direction, float range)//발사
    {
        _rigidbody2D.linearVelocity = direction.normalized * _speed;
        _lifeTimer = range / _speed; //거리/속도 = 시간
    }

    private void ProjectileRange()
    {
        if (_lifeTimer > 0f)
        {
            _lifeTimer -= Time.deltaTime;

            if (_lifeTimer <= 0f)
            {
                
                ObjectPoolingManager.Instance.ReleaseProjectile(this);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable target))
        {
            target.Damage(_damage);
            if (_weaponEffects.Count > 0)
            {
               
            }
            _piercing--;
            if (_piercing < 0)
            {
                ObjectPoolingManager.Instance.ReleaseProjectile(this);
            }
        }
    }
    
    public void OnDisable()
    {
        if(!_rigidbody2D) return;
        _rigidbody2D.linearVelocity = Vector2.zero;
    }
}
