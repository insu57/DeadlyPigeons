using UnityEngine;

public class ChaseState : IEnemyState
{
    public void Init(EnemyStateParameter stateParameter) { }

    public void EnterState(EnemyManager enemyManager) { }

    public void ExecuteState(EnemyManager enemyManager) { }

    public void FixedExecute(EnemyManager enemyManager)
    {
        if (!enemyManager.Target) return;

        Vector2 pos = enemyManager.transform.position;
        Vector2 chase = ((Vector2)enemyManager.Target.position - pos).normalized;
        Vector2 finalDir = (chase + enemyManager.GetSeparation()).normalized; // 추격 + 플레이어 반발 합산

        enemyManager.Rigidbody2D.MovePosition(pos + finalDir * (enemyManager.Speed * Time.fixedDeltaTime));
        enemyManager.UpdateFacing(enemyManager.Target.position.x - pos.x); 
        // 이동 방향이 아닌 플레이어 방향 기준으로 flip (분리 벡터에 의한 떨림 방지)
    }

    public void ExitState(EnemyManager enemyManager) { }
}
