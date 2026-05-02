public interface IShootState
{
    void EnterState(EnemyManager enemyManager);
    void ExecuteState(EnemyManager enemyManager);
    void ExitState(EnemyManager enemyManager);
}
