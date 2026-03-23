using UnityEngine;

[CreateAssetMenu(fileName = "PoolingSetting", menuName = "Scriptable Objects/PoolingSetting")]
public class PoolingSetting : ScriptableObject
{
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public int InitSize { get; private set; } = 30;
    [field: SerializeField] public int MaxSize { get; private set; } = 100;
}
