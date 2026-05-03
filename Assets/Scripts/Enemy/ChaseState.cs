using UnityEngine;

public class ChaseState : IEnemyState
{
    public void Init(EnemyStateParameter stateParameter) { }

    public void EnterState(EnemyManager enemyManager) { }

    public void ExecuteState(EnemyManager enemyManager) { }

    public void FixedExecute(EnemyManager enemyManager)
    {
        if(!enemyManager.Target) return;
        
        Vector3 dir = (enemyManager.Target.position - enemyManager.transform.position).normalized;
        float moveSpeed = enemyManager.Speed;
        Vector2 targetPos = enemyManager.transform.position + dir * (moveSpeed * Time.fixedDeltaTime);
        
        enemyManager.Rigidbody2D.MovePosition(targetPos);
    }

    public void ExitState(EnemyManager enemyManager) { }
}
