using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct WeaponClassKeyValue
{
    public WeaponClasses weaponClass;
    public List<WeaponClassEffect> statsValues;
}

[Serializable]
public struct WeaponClassEffect
{
    public MainStats mainStat;
    public SubStats subStat;
    public List<int> values;
}

[CreateAssetMenu(fileName = "WeaponClassEffect", menuName = "Scriptable Objects/WeaponClassEffect")]
public class WeaponClassEffectData : ScriptableObject
{
    [field: SerializeField] public List<WeaponClassKeyValue> WeaponClassEffects { get; private set; }

#if UNITY_EDITOR
    public void SyncCSVData(List<WeaponClassKeyValue> effectValues )
    {
        WeaponClassEffects = effectValues;
    }
#endif
}
