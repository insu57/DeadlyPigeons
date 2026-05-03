using UnityEngine;

public class DashState : IEnemyState
{
    private enum Phase {Charge, Dashing }
    
    private  float _chargeTime = 0.8f;
    private  float _dashSpeed = 10f;
    private  float _dashDistance = 8f;

    private Phase _phase;
    private float _timer;
    private Vector2 _dashDir; //돌진 방향
    private float _distanceTraveled; //이동 거리

    public void Init(EnemyStateParameter stateParameter)
    {
        if (stateParameter.moveParameters.Count >= 3)
        {
            _chargeTime = stateParameter.moveParameters[0];
            _dashSpeed = stateParameter.moveParameters[1];
            _dashDistance = stateParameter.moveParameters[2];
        }
    }

    public void EnterState(EnemyManager enemyManager)
    {
        _timer = _chargeTime;
        _phase = Phase.Charge;
    }

    public void ExecuteState(EnemyManager enemyManager)
    {
        if (_phase != Phase.Charge) return; //Charge일 때만
        
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
            EnterDash(enemyManager);
    }

    public void FixedExecute(EnemyManager enemyManager)
    {
        if (_phase != Phase.Dashing) return;

        float step = _dashSpeed * Time.fixedDeltaTime;
        _distanceTraveled += step; //이동 거리(돌진)

        enemyManager.Rigidbody2D.MovePosition(enemyManager.Rigidbody2D.position + _dashDir * step);
        //돌진 이동

        if (_distanceTraveled >= _dashDistance) //돌진 거리를 넘으면 돌진 종료.
        {
            enemyManager.ChangeState(EnemyStateType.Chase); //돌진이 끝나면 ChaseState로
        }
    }

    public void ExitState(EnemyManager enemyManager) { }

    private void EnterDash(EnemyManager enemyManager)
    {
        if (!enemyManager.Target)
        {
            _timer = _chargeTime;
            _phase = Phase.Charge;
            return;
        }

        _dashDir = (enemyManager.Target.position - enemyManager.transform.position).normalized; //돌진 방향 벡터
        _distanceTraveled = 0f;
        _phase = Phase.Dashing;
    }
}
