using System;
using UnityEngine;

[Serializable]
public struct GuaranteedLevelUpTierConfig
{
    public int level;
    public int tier;
}

[CreateAssetMenu(fileName = "GuaranteedLvUpStatTier", menuName = "Scriptable Objects/GuaranteedLvUpStatTier")]
public class GuaranteedLvUpStatTier : ScriptableObject
{
      
    [field: SerializeField] public GuaranteedLevelUpTierConfig[] GuaranteedTierConfigs {get; private set;} =
    {
        new() { level = 1, tier = 1 },
        new() { level = 5, tier = 2 },
        new() { level = 10, tier = 3 },
        new() { level = 15, tier = 3 },
        new() { level = 20, tier = 3 },
        new() { level = 25, tier = 4 },
    };
}
