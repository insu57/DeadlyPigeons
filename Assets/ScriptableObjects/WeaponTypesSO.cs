using System;
using System.Collections.Generic;
using UnityEngine;




[Serializable]
public struct WeaponTypeMapping
{
    public int id;
    public WeaponTypes type;
}

[CreateAssetMenu(fileName = "WeaponTypes", menuName = "Scriptable Objects/WeaponTypes")]
public class WeaponTypesSO : ScriptableObject
{
    [SerializeField] private List<WeaponTypeMapping> typeList = new();

    public Dictionary<int, WeaponTypes> WeaponTypesMap { get; private set; } = new();

    public WeaponTypes GetWeaponTypes(int id)
    {
        foreach (var mapping in typeList)
        {
            if (id == mapping.id)
            {
                return mapping.type;
            }
        }
        Debug.LogWarning("무기 타입이 없음: " + id);
        return WeaponTypes.None;
    }
    
#if UNITY_EDITOR
    public void SyncTypes(List<WeaponTypeMapping> newList)
    {
        typeList = newList;
    }
#endif
}
