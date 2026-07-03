using System.Collections.Generic;
using UnityEngine;

public class SingleShootState : IShootState
{
    private float _fireRate = 3f;
    private float _speed = 5;
    private const float ProjectileRange = 15f;

    private float _fireCooldown;

    public void Init(EnemyStateParameter stateParameter)
    {
        if (stateParameter.shootParameters.Count >= 2)
        {
            _fireRate = stateParameter.shootParameters[0];
            _speed = stateParameter.shootParameters[1];
        }
    }

    public void EnterState(EnemyManager enemyManager)
    {
        _fireCooldown = _fireRate;
    }

    public void ExecuteState(EnemyManager enemyManager)
    {
        if (!enemyManager.Target) return;

        _fireCooldown -= Time.deltaTime;
        
        if (_fireCooldown <= 0f)
        {
            Shoot(enemyManager);
            _fireCooldown = _fireRate;
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
            WeaponEffects = new List<IWeaponEffect>(),
        };
        
        if (enemyManager.EnemyData.EnemyStat.projectileSprite)
        {
            data.ProjectileSprite = enemyManager.EnemyData.EnemyStat.projectileSprite;
            data.SpriteScale = enemyManager.EnemyData.EnemyStat.projectileSpriteScale;
            data.ColliderSize = enemyManager.EnemyData.EnemyStat.projectileColliderSize;
        }

        projectile.Initialize(data);

        Vector3 dir = (enemyManager.Target.position - enemyManager.transform.position).normalized;
        projectile.Fire(dir, ProjectileRange, _speed);
    }
}
