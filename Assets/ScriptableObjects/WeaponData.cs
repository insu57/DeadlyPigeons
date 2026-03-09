using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum WeaponClasses
{
    Precise,
    Blunt,
    Primitive,
    Gun,
    Medieval,
    Blade,
    Heavy,
    None
}

[Serializable]
public struct StatMultiplier
{
    public MainStats stat;
    public List<int> value;
}

[Serializable]
public struct WeaponStat
{
    public int initTier;
    public bool isMelee;
    public List<WeaponClasses> classes;
    public List<int> baseDamage;
    public List<StatMultiplier> damageMultipliers;
    public List<float> attackSpeed;
    public List<int> critChance;
    public List<float> critDamage;
    public List<int> range;
    public List<int>  knockBack;
    public List<int>  healthAbsorb;
    public List<int> prices;
    public string description;
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    //need weapon id
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public WeaponStat WeaponStat { get; private set; }

    public static WeaponClasses ToWeaponTypes(string type)
    {
        switch (type)
        {
            case "Blunt": return WeaponClasses.Blunt;
            case "Precise": return WeaponClasses.Precise;
            case "Primitive": return WeaponClasses.Primitive;
            case "Gun": return WeaponClasses.Gun;
            case "Medieval": return WeaponClasses.Medieval;
            case "Blade": return WeaponClasses.Blade;
            case "Heavy": return WeaponClasses.Heavy;
            default:
            {
                Debug.LogWarning("Weapon Type Can't Find: "+ type);
                return WeaponClasses.None;
            }
        }
    }
    
    
#if UNITY_EDITOR
    public void SyncDataCSV(string weaponName, WeaponStat weaponStat, Sprite sprite) //임시
    {
        Name = weaponName;
        WeaponStat = weaponStat;
        Sprite = sprite;
    }
#endif
}
