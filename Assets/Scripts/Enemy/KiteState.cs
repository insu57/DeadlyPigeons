using UnityEngine;

public class KiteState : IEnemyState
{
    private const float FleeDistance = 5f;
    //private const float MoveSpeed = 3f;

    public void EnterState(EnemyManager enemyManager) { }

    public void ExecuteState(EnemyManager enemyManager) { }

    public void FixedExecute(EnemyManager enemyManager)
    {
        if (!enemyManager.Target) return;

        Vector3 toPlayer = enemyManager.Target.position - enemyManager.transform.position;
        float sqrDist = toPlayer.sqrMagnitude;

        Vector3 dir = sqrDist < (FleeDistance * FleeDistance) //거리 비교. 개선점?
            ? -toPlayer.normalized
            : toPlayer.normalized;

        Vector2 targetPos = enemyManager.transform.position + dir * (enemyManager.Speed * Time.fixedDeltaTime);
        enemyManager.Rigidbody2D.MovePosition(targetPos);
    }

    public void ExitState(EnemyManager enemyManager) { }
}
