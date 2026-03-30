using System;
using UnityEngine;
using Object = System.Object;

public class Projectile : MonoBehaviour, IDealer
{
    private int _damage;
    public void SetDamage(int damage) => _damage = damage;
    private float _lifeTimer;
    private float _speed = 10f; //temp
    private Rigidbody2D _rigidbody2D;
    
    private void Awake()
    {
        TryGetComponent(out _rigidbody2D);
    }

    private void Update()
    {
        ProjectileRange();
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
    
    public void OnDisable()
    {
        if(!_rigidbody2D) return;
        _rigidbody2D.linearVelocity = Vector2.zero;
    }
}
