using System;
using UnityEngine;

[Serializable]
public struct TierWeightConfig
{
    public float baseWeight;
    public int minProgression;
    public float perProgression;
    public float maxChance;
}

[CreateAssetMenu(fileName = "ProgressionTierWeightData", menuName = "Scriptable Objects/ProgressionTierWeightData")]
public class ProgressionTierWeightData : ScriptableObject
{
    [field: SerializeField]
    public TierWeightConfig[] TierWeightConfigs { get; private set; } =
    {
            new() { baseWeight = 100, minProgression = 0, perProgression = 0, maxChance = 100 }, //tier = 1
            new() { baseWeight = 0, minProgression = 2, perProgression = 6, maxChance = 60 },
            new() { baseWeight = 0, minProgression = 4, perProgression = 2, maxChance = 25 },
            new() { baseWeight = 0, minProgression = 8, perProgression = 0.23f, maxChance = 8 }, // tier = 4
    };

    [field: SerializeField] public float RerollIncreasePerProgression { get; private set; } = 0.4f;
    [field: SerializeField] public float FirstRerollPricePerProgression { get; private set; } = 0.75f;
}
