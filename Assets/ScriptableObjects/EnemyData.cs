using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyStateType { Chase, Kite, Dash }
public enum ShootStateType { None, Single }
public enum TransitionCondition { HealthBelow, TimeElapsed }

[Serializable]
public struct StateTransition
{
    public TransitionCondition condition;
    public float threshold;
    public EnemyStateType targetState;
}

[Serializable]
public struct EnemyStateParameter //상태 파라미터
{
    public EnemyStateType moveState;
    public ShootStateType shootState;
    public List<float> moveParameters;
    public List<float> shootParameters;
}

[Serializable]
public struct EnemyStat
{
    public string enemyName;
    public int baseHealth;
    public float healthPerWave;
    public int baseDamage;
    public float damagePerWave;
    public float baseSpeed;
    public float knockbackResistance;
    public int materialsDrop;
    public float meatDropChance;
    public float lootCrateDropChance;
    public int initWave;
    
    public Sprite projectileSprite;
    public Vector2 projectileSpriteScale;
    public Vector2 projectileColliderSize;
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public EnemyStat EnemyStat { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public List<EnemyStateParameter> States { get; private set; }

    [field: SerializeField] public List<StateTransition> Transitions { get; private set; }//조건
    
    public static EnemyStateType ParseEnemyStateType(string value)
    {
        if (Enum.TryParse(value, true, out EnemyStateType result))
            return result;
        Debug.LogWarning($"EnemyStateType 변환 실패: '{value}', 기본값 반환");
        return default;
    }

    public static ShootStateType ParseShootStateType(string value)
    {
        if (Enum.TryParse(value, true, out ShootStateType result))
            return result;
        Debug.LogWarning($"ShootStateType 변환 실패: '{value}', 기본값 반환");
        return default;
    }

    public static TransitionCondition ParseTransitionCondition(string value)
    {
        if (Enum.TryParse(value, true, out TransitionCondition result))
            return result;
        Debug.LogWarning($"TransitionCondition 변환 실패: '{value}', 기본값 반환");
        return default;
    }

#if UNITY_EDITOR

    public void SyncEnemyStat(Sprite sprite, EnemyStat enemyStat)
    {
        Sprite = sprite;
        EnemyStat = enemyStat;
    }

    public void SyncEnemyStateParameter(List<EnemyStateParameter> parameterList)
    {
        States =  parameterList;
    }

    public void SyncEnemyStateTransition(List<StateTransition> transitionList)
    {
        Transitions = transitionList;
    }

#endif
    
}
