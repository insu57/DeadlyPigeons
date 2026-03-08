using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public struct WeaponTypeMapping
{
    public int id;
    [FormerlySerializedAs("type")] public WeaponClasses @class;
}

[CreateAssetMenu(fileName = "WeaponTypes", menuName = "Scriptable Objects/WeaponTypes")]
public class WeaponTypesSO : ScriptableObject
{
    [SerializeField] private List<WeaponTypeMapping> typeList = new();

    public Dictionary<int, WeaponClasses> WeaponTypesMap { get; private set; } = new();

    public WeaponClasses GetWeaponTypes(int id)
    {
        foreach (var mapping in typeList)
        {
            if (id == mapping.id)
            {
                return mapping.@class;
            }
        }
        Debug.LogWarning("무기 타입이 없음: " + id);
        return WeaponClasses.None;
    }
    
#if UNITY_EDITOR
    public void SyncTypes(List<WeaponTypeMapping> newList)
    {
        typeList = newList;
    }
#endif
}
