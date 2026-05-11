using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private int _currentWave = 1;
    private int _health;
    private bool _initialized = false;

    public event Action<EnemyManager> OnDeath;

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

    // 동적 스폰 시 사용. Instantiate 직후, Start() 전에 호출.
    public void Init(EnemyData data, int wave, Transform target)
    {
        enemyData = data;
        _currentWave = wave;
        Target = target;
        _initialized = true;
        InitEnemy();
    }

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (!_initialized) InitEnemy(); //씬에 배치된 적 폴백
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

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void InitEnemy()
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
        if (_health <= 0) return;

        _health -= damage;

        var dmgTxt = ObjectPoolingManager.Instance.GetDamageTxt();
        dmgTxt.transform.position = transform.position;
        dmgTxt.Init(ObjectPoolingManager.Instance.Sb);
        dmgTxt.SetText(damage, isCrit);

        if (_health <= 0)
        {
            OnDeath?.Invoke(this);
            //Destroy(gameObject);
            
            ObjectPoolingManager.Instance.ReleaseEnemy(this);

            var material = ObjectPoolingManager.Instance.GetCollectable();
            material.SetType(CollectableType.Material, enemyData.EnemyStat.materialsDrop);
            material.transform.position = transform.position;
            
            var crateChance = Random.Range(0f, 1f);
            if (crateChance < enemyData.EnemyStat.lootCrateDropChance / 100f)
            {
                var crate = ObjectPoolingManager.Instance.GetCollectable();
                crate.SetType(CollectableType.Crate, 1);
                crate.transform.position = transform.position;
            }
            
            var meatChance = Random.Range(0f, 1f);
            if (meatChance < enemyData.EnemyStat.meatDropChance / 100f)
            {
                var meat  =  ObjectPoolingManager.Instance.GetCollectable();
                meat.SetType(CollectableType.Meat, 1);
                meat.transform.position = transform.position;
            }
        }
    }

    public void Heal(int healAmount)
    {
    }

    public void DotDamage(int duration, int damage, float tick)
    {
        if (!gameObject.activeInHierarchy) return;//코루틴은 이것을 기준으로 체크함.

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
