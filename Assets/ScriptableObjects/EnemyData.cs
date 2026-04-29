using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum EnemyStateType { Chase, Kite }
public enum TransitionCondition { HealthBelow, PlayerNear, PlayerFar }

[Serializable]
public struct StateTransition //상태 변경
{
    public TransitionCondition condition; //변경 조건
    public float threshold; //임계값
    public EnemyStateType targetState; //변경할 상태
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string EnemyName { get; private set; }
    [field: SerializeField] public int BaseHealth { get; private set; }
    [field: SerializeField] public int HealthPerWave { get; private set; }
    [field: SerializeField] public int BaseDamage { get; private set; }
    [field: SerializeField] public float DamagePerWave { get; private set; }
    [field: SerializeField] public float BaseSpeed { get; private set; }
    
    [field: SerializeField] public float KnockbackResistance { get; private set; }
    
    [field: SerializeField] public float MaterialsDrop{ get; private set; }
    [field: SerializeField] public float ConsumableDropChance { get; private set; }
    [field: SerializeField] public float LootCrateDropChance { get; private set; }

    [field: SerializeField] public int InitWave { get; private set; }
    
    [field: SerializeField] public EnemyStateType InitialState { get; private set; } //초기 상태
    [field: SerializeField] public StateTransition[] Transitions { get; private set; }//상태 변경
}
