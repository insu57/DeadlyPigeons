using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyStateType { Chase, Kite, Dash }
public enum TransitionCondition { HealthBelow, TimerElapsed }
public enum ShootStateType { None, Projectile }

[Serializable]
public struct StateTransition
{
    public TransitionCondition condition;
    public float threshold;
    public EnemyStateType targetState;
}

[Serializable]
public struct EnemyStateParameter
{
    public EnemyStateType state;
    public List<float> parameters;
}

[Serializable]
public struct StateShootBinding
{
    public EnemyStateType moveState;
    public ShootStateType shootState;
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string EnemyName { get; private set; }
    [field: SerializeField] public int BaseHealth { get; private set; }
    [field: SerializeField] public float HealthPerWave { get; private set; }
    [field: SerializeField] public int BaseDamage { get; private set; }
    [field: SerializeField] public float DamagePerWave { get; private set; }
    [field: SerializeField] public float BaseSpeed { get; private set; }

    [field: SerializeField] public float KnockbackResistance { get; private set; }

    [field: SerializeField] public float MaterialsDrop { get; private set; }
    [field: SerializeField] public float ConsumableDropChance { get; private set; }
    [field: SerializeField] public float LootCrateDropChance { get; private set; }

    [field: SerializeField] public int InitWave { get; private set; }

    [field: SerializeField] public EnemyStateType InitialState { get; private set; }
    [field: SerializeField] public ShootStateType InitialShootState { get; private set; }
    [field: SerializeField] public List<EnemyStateParameter> States { get; private set; }
    [field: SerializeField] public List<StateShootBinding> ShootBindings { get; private set; }
    [field: SerializeField] public StateTransition[] Transitions { get; private set; }
}
