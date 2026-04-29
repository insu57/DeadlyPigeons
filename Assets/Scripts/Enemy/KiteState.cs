using System.Collections.Generic;
using UnityEngine;

public class KiteState : IEnemyState
{
    private const float FleeDistance = 5f;
    private const float MoveSpeed = 3f;
    private const float FireRate = 3f;
    private const float ProjectileRange = 15f;

    private float _fireCooldown;

    public void EnterState(EnemyManager enemyManager)
    {
        _fireCooldown = 0f;
    }

    public void ExecuteState(EnemyManager enemyManager)
    {
        if (!enemyManager.Target) return;

        _fireCooldown -= Time.deltaTime;

        float dist = Vector2.Distance(enemyManager.transform.position, enemyManager.Target.position);

        if (dist >= FleeDistance && _fireCooldown <= 0f)
        {
            Shoot(enemyManager);
            _fireCooldown = FireRate;
        }
    }

    public void FixedExecute(EnemyManager enemyManager)
    {
        if (!enemyManager.Target) return;

        Vector3 toPlayer = enemyManager.Target.position - enemyManager.transform.position;
        float dist = toPlayer.magnitude;

        Vector3 dir = dist < FleeDistance
            ? -toPlayer.normalized  // 가까우면 반대 방향으로 도망
            : toPlayer.normalized;  // 멀면 플레이어 방향으로 접근

        Vector2 targetPos = enemyManager.transform.position + dir * (MoveSpeed * Time.fixedDeltaTime);
        enemyManager.Rigidbody2D.MovePosition(targetPos);
    }

    public void ExitState(EnemyManager enemyManager)
    {
    }

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
