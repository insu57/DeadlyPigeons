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

        Vector3 dir = _isFleeing ? -toPlayer.normalized : toPlayer.normalized; //도망 중에 따라 방향벡터 전환

        Vector2 targetPos = enemyManager.transform.position + dir * (enemyManager.Speed * Time.fixedDeltaTime);
        enemyManager.Rigidbody2D.MovePosition(targetPos);
    }

    public void ExitState(EnemyManager enemyManager) { }
}
