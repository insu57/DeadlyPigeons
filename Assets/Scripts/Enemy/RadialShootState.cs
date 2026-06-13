using System.Collections.Generic;
using UnityEngine;

public class RadialShootState : IShootState
{
    private const int MinProjectileCount = 4; // 최소 발사 개수(상하좌우)

    private float _fireRate = 3f;
    private float _projectileRange = 15f;
    private int _projectileCount = MinProjectileCount;

    private float _fireCooldown;

    public void Init(EnemyStateParameter stateParameter)
    {
        if (stateParameter.shootParameters.Count >= 2)
        {
            _fireRate = stateParameter.shootParameters[0];
            _projectileCount = Mathf.Max(MinProjectileCount, Mathf.FloorToInt(stateParameter.shootParameters[1]));
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
        float angleStep = 360f / _projectileCount; // 360°를 균등 분할

        for (int i = 0; i < _projectileCount; i++)
        {
            Projectile projectile = ObjectPoolingManager.Instance.GetProjectile();
            if (!projectile) continue;

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

            float angle = angleStep * i * Mathf.Deg2Rad; // 0°(우)부터 균등 간격 → 4개면 우/상/좌/하
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            projectile.Fire(dir, _projectileRange);
        }
    }
}
