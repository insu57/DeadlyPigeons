using System.Collections.Generic;
using UnityEngine;

public class ProjectileShootState : IShootState
{
    private const float FireRate = 1.5f;
    private const float ProjectileRange = 15f;

    private float _fireCooldown;

    public void EnterState(EnemyManager enemyManager)
    {
        _fireCooldown = FireRate;
    }

    public void ExecuteState(EnemyManager enemyManager)
    {
        if (!enemyManager.Target) return;

        _fireCooldown -= Time.deltaTime;
        
        if (_fireCooldown <= 0f)
        {
            Shoot(enemyManager);
            _fireCooldown = FireRate;
        }
    }

    public void ExitState(EnemyManager enemyManager) { }

    private void Shoot(EnemyManager enemyManager)
    {
        Projectile projectile = ObjectPoolingManager.Instance.GetProjectile();
        if (!projectile) return;

        projectile.transform.position = enemyManager.transform.position;

        var data = new ProjectileInitData
        {
            Damage = enemyManager.AttackDamage,
            Piercing = 0,
            PiercingDmgPer = 0,
            Bounces = 0,
            HitLayer = DataManager.Instance.EnemyHitboxLayer,
            IsCrit = false,
            WeaponEffects = new List<IWeaponEffect>()
        };

        projectile.Initialize(data);

        Vector3 dir = (enemyManager.Target.position - enemyManager.transform.position).normalized;
        projectile.Fire(dir, ProjectileRange);
    }
}
