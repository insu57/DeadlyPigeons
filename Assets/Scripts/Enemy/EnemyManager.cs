using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData enemyData;
    private int _currentWave = 1;
    private int _health;

    public int AttackDamage { get; private set; }
    public float Speed { get; private set; }

    public Transform Target { get; private set; }
    private Coroutine _activeDotCoroutine;

    private IEnemyState _currentState;
    private EnemyStateType _currentStateType;
    private Dictionary<EnemyStateType, IEnemyState> _enemyStates = new();
    private Dictionary<EnemyStateType, EnemyStateParameter> _stateParameterDict = new();

    private IShootState _currentShootState;
    private Dictionary<ShootStateType, IShootState> _shootStates = new();

    public Rigidbody2D Rigidbody2D { get; private set; }

    private float _stateTimer;

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
        _health = Mathf.FloorToInt(enemyData.EnemyStat.baseHealth 
                                   + enemyData.EnemyStat.healthPerWave * (_currentWave - 1));
        AttackDamage = Mathf.FloorToInt(enemyData.EnemyStat.baseDamage 
                                        + enemyData.EnemyStat.damagePerWave * (_currentWave - 1));
        Speed = enemyData.EnemyStat.baseSpeed;

        _enemyStates[EnemyStateType.Chase] = new ChaseState();
        _enemyStates[EnemyStateType.Kite] = new KiteState();
        _enemyStates[EnemyStateType.Dash] = new DashState();

        _shootStates[ShootStateType.None] = new NoneShootState();
        _shootStates[ShootStateType.Single] = new ProjectileShootState();

        foreach (var enemyStateParameter in enemyData.States)
        {
            var enemyState = enemyStateParameter.moveState;
            _enemyStates[enemyState].Init(enemyStateParameter);
            var shootState = enemyStateParameter.shootState;
            _shootStates[shootState].Init(enemyStateParameter);
            
            _stateParameterDict[enemyState] = enemyStateParameter;
        }

        if (enemyData.States.Count == 0)
        {
            Debug.LogError("Enemy States None!!: " + enemyData.EnemyStat.enemyName);
            return;
        }
        
        var initStateParameter = enemyData.States[0]; //초기 상태 파라미터
        var initState = initStateParameter.moveState;
        var initShootState = initStateParameter.shootState;
        
        ChangeState(initState);
        ChangeShootState(initShootState, initStateParameter);
    }

    private void Update()
    {
        _stateTimer += Time.deltaTime;
        _currentState?.ExecuteState(this);
        _currentShootState?.ExecuteState(this);
        
        CheckTransitions();//변경 조건 체크
    }

    private void FixedUpdate()
    {
        _currentState?.FixedExecute(this);
    }

    private void CheckTransitions()
    {
        if (enemyData.Transitions == null || !Target) return;

        foreach (var transition in enemyData.Transitions)
        {
            if (transition.targetState == _currentStateType) continue;

            if (EvaluateCondition(transition)) //상태 변경 조건 체크
            {
                ChangeState(transition.targetState);
                return;
            }
        }
    }

    private bool EvaluateCondition(StateTransition transition)
    {
        switch (transition.condition)
        {
            case TransitionCondition.HealthBelow: //체력 임계점
                float healthPct = (float)_health / enemyData.EnemyStat.baseHealth * 100f;
                return healthPct < transition.threshold;

            case TransitionCondition.TimerElapsed: //시간
                return _stateTimer >= transition.threshold;

            default:
                return false;
        }
    }

    public void ChangeState(EnemyStateType stateType)
    {
        _currentState?.ExitState(this);
        _currentStateType = stateType;
        _currentState = _enemyStates[stateType];
        _stateTimer = 0f;
       
        if (_stateParameterDict.TryGetValue(stateType, out var stateParameter))
        {
            _currentState.EnterState(this);
            var shootType = stateParameter.shootState;
            ChangeShootState(shootType, stateParameter);
        }
    }

    private void ChangeShootState(ShootStateType shootStateType, EnemyStateParameter stateParameter)
    {
        _currentShootState?.ExitState(this);
        _currentShootState = _shootStates[shootStateType];
        _currentShootState.EnterState(this);
    }

    public void Damage(int damage, bool isCrit)
    {
        _health -= damage;

        var dmgTxt = ObjectPoolingManager.Instance.GetDamageTxt();
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
        //지속시간, 데미지는 큰 값으로, 틱 시간은 작은(짧은) 값으로???
    }

    private IEnumerator DotDamageCoroutine(float duration, int damage, float tick)
    {
        float elapsedTime = 0f;
        WaitForSeconds waitTick = new WaitForSeconds(tick);

        while (elapsedTime < duration)
        {
            if (!this) yield break;

            yield return waitTick; //틱 시간

            Damage(damage, false);

            elapsedTime += tick;
        }

        _activeDotCoroutine = null;
    }

    public Transform GetTransform() => transform;
}
