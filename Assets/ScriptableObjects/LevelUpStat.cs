using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MainStatUpgradeValue
{
    public MainStats mainStat;
    public int[] values;
}

[CreateAssetMenu(fileName = "LevelUpStat", menuName = "Scriptable Objects/LevelUpStat")]
public class LevelUpStat : ScriptableObject
{
    [field: SerializeField] public List<MainStatUpgradeValue> MainStatUpgrades { get; private set; }

#if UNITY_EDITOR
    public void SetUpgrades(List<MainStatUpgradeValue> upgrades)
    {
        MainStatUpgrades = upgrades;
    }
#endif
}
