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
    
    public Sprite ProjectileSprite;
    public Vector2 SpriteScale;
    public Vector2 ColliderSize;
}

public class Projectile : MonoBehaviour
{
    private int _damage;
    private int _piercing;
    private int _piercingDmgPer;
    private int _bounces;
    private bool _isCrit;
    
    private float _lifeTimer;
    private float _speed = 10f; //temp?
    private CapsuleCollider2D _collider2D;
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _renderer;
    private List<IWeaponEffect> _weaponEffects;
    
    private void Awake()
    {
        TryGetComponent(out _rigidbody2D);
        TryGetComponent(out _collider2D);
        _renderer = GetComponentInChildren<SpriteRenderer>();
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
        _renderer.sprite = data.ProjectileSprite;
        _renderer.gameObject.transform.localScale = data.SpriteScale;
        _collider2D.size = data.ColliderSize;
    }
    
    public void Fire(Vector3 direction, float range)//발사
    {
        _rigidbody2D.linearVelocity = direction.normalized * _speed;
        _lifeTimer = range / _speed; //거리/속도 = 시간
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
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
                weaponEffect.Execute(target);
            }
            
            if (_piercing < 0)
            {
                ObjectPoolingManager.Instance.ReleaseProjectile(this);
            }
            _piercing--;
            _damage = Mathf.FloorToInt(_damage * (100 +  _piercingDmgPer) / 100f);
            _damage = Mathf.Max(1, _damage);
        }
    }
    
    public void OnDisable()
    {
        if(!_rigidbody2D) return;
        _rigidbody2D.linearVelocity = Vector2.zero;
    }
}
