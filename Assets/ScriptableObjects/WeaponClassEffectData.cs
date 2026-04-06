using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct WeaponClassKeyValue 
{
    public WeaponClasses weaponClass; //키 - 무기 클래스
    public List<WeaponClassEffect> statsValues; //스탯(메인, 서브) - 수치 리스트(2~6 보너스)
}

[Serializable]
public struct WeaponClassEffect
{
    public MainStats mainStat;
    public SubStats subStat; //스탯
    public List<int> values; //수치 2~6 보너스
    
    public bool IsMain => mainStat != MainStats.None;
    public bool IsSub => subStat != SubStats.None;
    public bool IsUnavailable => !IsMain && !IsSub;
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
