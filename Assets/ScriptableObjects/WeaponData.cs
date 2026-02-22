using System;
using UnityEngine;

[Serializable]
public struct WeaponStat
{
    public int tier;
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    //need weapon id
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public WeaponStat WeaponStat { get; private set; }
    
    
#if UNITY_EDITOR
    public void SyncDataCSV(string name, WeaponStat weaponStat) //임시
    {
        Name = name;
        WeaponStat = weaponStat;
    }
#endif
}
