using UnityEngine;

public class DashState : IEnemyState
{
    private enum Phase {Charge, Dashing }
    
    private const float ChargeTime = 0.8f;
    private const float DashSpeed = 10f;
    private const float DashDistance = 8f;

    private Phase _phase;
    private float _timer;
    private Vector2 _dashDir; //돌진 방향
    private float _distanceTraveled; //이동 거리

    public void EnterState(EnemyManager enemyManager)
    {
        _timer = ChargeTime;
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

        float step = DashSpeed * Time.fixedDeltaTime;
        _distanceTraveled += step; //이동 거리(돌진)

        enemyManager.Rigidbody2D.MovePosition(enemyManager.Rigidbody2D.position + _dashDir * step);
        //돌진 이동

        if (_distanceTraveled >= DashDistance) //돌진 거리를 넘으면 돌진 종료.
        {
            enemyManager.ChangeState(EnemyStateType.Chase); //돌진이 끝나면 ChaseState로
        }
    }

    public void ExitState(EnemyManager enemyManager) { }

    private void EnterDash(EnemyManager enemyManager)
    {
        if (!enemyManager.Target)
        {
            _timer = ChargeTime;
            _phase = Phase.Charge;
            return;
        }

        _dashDir = (enemyManager.Target.position - enemyManager.transform.position).normalized; //돌진 방향 벡터
        _distanceTraveled = 0f;
        _phase = Phase.Dashing;
    }
}
