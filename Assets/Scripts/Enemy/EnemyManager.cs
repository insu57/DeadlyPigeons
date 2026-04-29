using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData enemyData; //stage에서.
    //Spawn시 현재 wave 정보 주입.
    private int _currentWave = 1;
    private int _health;

    public int AttackDamage { get; private set; }

    public Transform Target { get; private set; }
    private Coroutine _activeDotCoroutine;
    private IEnemyState _currentState;
    private EnemyStateType _currentStateType;
    private Dictionary<EnemyStateType, IEnemyState> _enemyStates = new();

    public Rigidbody2D Rigidbody2D { get; private set; }

    public void SetTarget(Transform target)
    {
        Target = target;
    }

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _health = enemyData.BaseHealth + enemyData.HealthPerWave * (_currentWave - 1);
        AttackDamage = Mathf.FloorToInt(enemyData.BaseDamage + enemyData.DamagePerWave * (_currentWave - 1));

        _enemyStates[EnemyStateType.Chase] = new ChaseState();
        _enemyStates[EnemyStateType.Kite] = new KiteState();

        ChangeState(enemyData.InitialState);
    }

    private void Update()
    {
        _currentState?.ExecuteState(this);
        CheckTransitions();
    }

    private void FixedUpdate()
    {
        _currentState?.FixedExecute(this);
    }

    private void CheckTransitions()//상태 변경 체크
    {
        if (enemyData.Transitions == null || !Target) return;

        foreach (var transition in enemyData.Transitions)
        {
            if (transition.targetState == _currentStateType) continue; //동일하면 x

            if (EvaluateCondition(transition)) //조건 체크
            {
                ChangeState(transition.targetState);
                return;
            }
        }
    }

    private bool EvaluateCondition(StateTransition transition) //상태변경 조건 검사
    {
        switch (transition.condition)
        {
            case TransitionCondition.HealthBelow:
                float healthPct = (float)_health / enemyData.BaseHealth * 100f;
                return healthPct < transition.threshold;

            case TransitionCondition.PlayerNear:
                return Vector2.Distance(transform.position, Target.position) < transition.threshold;

            case TransitionCondition.PlayerFar:
                return Vector2.Distance(transform.position, Target.position) > transition.threshold;

            default:
                return false;
        }
    }

    private void ChangeState(EnemyStateType stateType)
    {
        _currentState?.ExitState(this);
        _currentStateType = stateType;
        _currentState = _enemyStates[stateType];
        _currentState.EnterState(this);
    }

    public void Damage(int damage, bool isCrit)
    {
        _health -= damage;

        var dmgTxt = ObjectPoolingManager.Instance.GetDamageTxt(); //데미지 플로팅 텍스트
        dmgTxt.transform.position = transform.position;
        dmgTxt.SetText(damage, isCrit);

        if (_health <= 0)
        {
            Debug.Log("DEAD");
        }
    }

    public void Heal(int healAmount)
    {
    }

    public void DotDamage(int duration, int damage, float tick)
    {
        if (_activeDotCoroutine != null)
        {
            StopCoroutine(_activeDotCoroutine);
        }
        _activeDotCoroutine = StartCoroutine(DotDamageCoroutine(duration, damage, tick));
    }

    private IEnumerator DotDamageCoroutine(float duration, int damage, float tick)
    {
        float elapsedTime = 0f;
        WaitForSeconds waitTick = new WaitForSeconds(tick);

        while (elapsedTime < duration)
        {
            if (!this) yield break; //Dead 시 오류 방지
            
            yield return waitTick; //tick만큼 대기
            
            Damage(damage, false); //도트 데미지 만큼 피해
            
            elapsedTime += tick;
        }

        _activeDotCoroutine = null; //지속 시간 지나면 null
    }

    public Transform GetTransform() => transform;
}
