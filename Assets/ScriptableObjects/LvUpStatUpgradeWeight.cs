using System;
using UnityEngine;

[Serializable]
public struct TierLevelWeightConfig
{
    public float baseWeight;
    public int minLevel;
    public float perLevel;
    public float maxChance;
}

[CreateAssetMenu(fileName = "LvUpStatUpgradeWeight", menuName = "Scriptable Objects/LvUpStatUpgradeWeight")]
public class LvUpStatUpgradeWeight : ScriptableObject
{
    [field: SerializeField]
    public TierLevelWeightConfig[] TierLevelWeightConfigs { get; private set; } =
    {
            new() { baseWeight = 100, minLevel = 0, perLevel = 0, maxChance = 100 }, //tier = 1
            new() { baseWeight = 0, minLevel = 2, perLevel = 6, maxChance = 60 },
            new() { baseWeight = 0, minLevel = 4, perLevel = 2, maxChance = 25 },
            new() { baseWeight = 0, minLevel = 8, perLevel = 0.23f, maxChance = 8 }, // tier = 4
    };
}
