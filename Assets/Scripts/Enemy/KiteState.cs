using UnityEngine;

public class KiteState : IEnemyState
{
    private EnemyStateParameter _stateParameter;
    
    private  float _fleeDistance = 4f; //이것보다 가까우면 도망
    private  float _approachDistance = 6f; //이것보다 멀면 추격

    private bool _isFleeing;

    public void Init(EnemyStateParameter stateParameter)
    {
        _stateParameter = stateParameter;
        if (_stateParameter.moveParameters.Count >= 2) //파라미터 설정.
        {
            _fleeDistance = stateParameter.moveParameters[0];
            _approachDistance = stateParameter.moveParameters[1];
        }
    }

    public void EnterState(EnemyManager enemyManager)
    {
        if (!enemyManager.Target) return;
        //진입 시 거리 처리
        float sqrDist = (enemyManager.Target.position - enemyManager.transform.position).sqrMagnitude;
        _isFleeing = sqrDist < _fleeDistance * _fleeDistance;
    }

    public void ExecuteState(EnemyManager enemyManager) { }

    public void FixedExecute(EnemyManager enemyManager)
    {
        if (!enemyManager.Target) return;

        Vector3 toPlayer = enemyManager.Target.position - enemyManager.transform.position; //방향벡터
        float sqrDist = toPlayer.sqrMagnitude;

        if (_isFleeing && sqrDist > _approachDistance * _approachDistance)
            _isFleeing = false;
        else if (!_isFleeing && sqrDist < _fleeDistance * _fleeDistance)
            _isFleeing = true;

        Vector2 chaseDir = _isFleeing ? -(Vector2)toPlayer.normalized : (Vector2)toPlayer.normalized; // 도망/추격 방향
        Vector2 finalDir = (chaseDir + enemyManager.GetSeparation()).normalized; // 플레이어 반발 합산

        enemyManager.Rigidbody2D.MovePosition((Vector2)enemyManager.transform.position +
                                              finalDir * (enemyManager.Speed * Time.fixedDeltaTime));
        enemyManager.UpdateFacing(enemyManager.Target.position.x - enemyManager.transform.position.x); 
        // 이동 방향이 아닌 플레이어 방향 기준으로 flip
    }

    public void ExitState(EnemyManager enemyManager) { }
}
