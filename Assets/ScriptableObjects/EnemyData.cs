using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public string EnemyName {get; private set;}
    [field: SerializeField] public int BaseHealth {get; private set;}
    [field: SerializeField] public int BaseDamage {get; private set;}
    [field: SerializeField] public float BaseSpeed {get; private set;}
    //패턴 관련 추가 필요.
}
