using UnityEngine;

public interface IEnemyState
{
    public void EnterState(EnemyManager enemyManager);
    public void ExecuteState(EnemyManager enemyManager);
    public void FixedExecute(EnemyManager enemyManager);
    public void ExitState(EnemyManager enemyManager);
}
