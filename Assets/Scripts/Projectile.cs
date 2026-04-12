using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

public struct ProjectileInitData
{
    public int Damage;
    public int Piercing;
    public int PiercingDmgPer;
    public int Bounces;
    public int HitLayer;
    public bool IsCrit;
    public List<IWeaponEffect> WeaponEffects;
}

public class Projectile : MonoBehaviour
{
    private int _damage;
    private int _piercing;
    private int _piercingDmgPer;
    private int _bounces;
    private bool _isCrit;
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

    public void Initialize(ProjectileInitData data)
    {
        _damage = data.Damage;
        _piercing = data.Piercing;
        _piercingDmgPer = data.PiercingDmgPer;
        _bounces = data.Bounces;
        gameObject.layer = data.HitLayer;
        _isCrit = data.IsCrit;
        _weaponEffects = data.WeaponEffects;
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
            target.Damage(_damage, _isCrit);
            foreach (var weaponEffect in _weaponEffects)
            {
                Debug.Log("EXECUTE");
                weaponEffect.Execute(target);
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
