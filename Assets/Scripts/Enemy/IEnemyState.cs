using UnityEngine;

public interface IEnemyState
{
    public void Init(EnemyStateParameter stateParameter);
    public void EnterState(EnemyManager enemyManager);
    public void ExecuteState(EnemyManager enemyManager);
    public void FixedExecute(EnemyManager enemyManager);
    public void ExitState(EnemyManager enemyManager);
}
