using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct WeaponClassBonusValue 
{
    public WeaponClasses weaponClass; //키 - 무기 클래스
    public List<WeaponClassBonus> statsValues; //스탯(메인, 서브) - 수치 리스트(2~6 보너스)
}

[Serializable]
public struct WeaponClassBonus
{
    public MainStats mainStat;
    public SubStats subStat; //스탯
    public List<int> values; //수치 2~6 보너스
    
    public bool IsMain => mainStat != MainStats.None;
    public bool IsSub => subStat != SubStats.None;
    public bool IsUnavailable => !IsMain && !IsSub;
}

[CreateAssetMenu(fileName = "WeaponClassBonus", menuName = "Scriptable Objects/WeaponClassBonus")]
public class WeaponClassBonusData : ScriptableObject
{
    [field: SerializeField] public List<WeaponClassBonusValue> WeaponClassBonusValues { get; private set; }

#if UNITY_EDITOR
    public void SyncCSVData(List<WeaponClassBonusValue> bonusValues )
    {
        WeaponClassBonusValues = bonusValues;
    }
#endif
}
