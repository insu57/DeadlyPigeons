using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    //tmp
    [SerializeField] private int health;
    [SerializeField] private int maxHealth;
    
    public Transform Target {get; private set; }
    private Coroutine _activeDotCoroutine;
    
    private IEnemyState _currentState;
    private Dictionary<Type, IEnemyState> _enemyStates = new();
    
    public Rigidbody2D Rigidbody2D{get; private set;}
    
    public void SetTarget(Transform target)
    {
        Debug.Log(name + ' ' + target.name);
        Target = target;
    }

    private void Awake()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        _enemyStates[typeof(ChaseState)] = new ChaseState();
        
        ChangeState(typeof(ChaseState));
    }

    private void Update()
    {
        _currentState?.ExecuteState(this);
    }

    private void FixedUpdate()
    {
        _currentState?.FixedExecute(this);
    }
    
    public void Damage(int damage, bool isCrit)
    {
        health -= damage;

        var dmgTxt = ObjectPoolingManager.Instance.GetDamageTxt();
        dmgTxt.transform.position = transform.position;
        dmgTxt.SetText(damage, isCrit);
        
        
        if (health <= 0)
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
        //지속시간, 데미지는 큰 값으로 갱신 틱은 가장 짧은 것으로)
        _activeDotCoroutine = StartCoroutine(DotDamageCoroutine(duration, damage, tick));
    }

    private IEnumerator DotDamageCoroutine(float duration, int damage, float tick)
    {
        float elapsedTime = 0f;
        
        WaitForSeconds waitTick = new WaitForSeconds(tick);

        while (elapsedTime < duration) //지속 시간 동안
        {
            if (!this)
            {
                yield break;
            }
            yield return waitTick;
            //일정 틱 이후 도트 데미지
            Damage(damage, false);
            elapsedTime += tick;//틱 만큼 추가
        }
        
        _activeDotCoroutine = null;
    }
    
    public Transform GetTransform() => transform;

    private void ChangeState(Type newState)
    {
        _currentState?.ExitState(this);
        
        _currentState = _enemyStates[newState];
        
        _currentState.EnterState(this);
    }
}
