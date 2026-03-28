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
    Elemental,
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
    public AttackType attackType;
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
    public List<int> piercing;
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
    [SerializeReference] private List<IWeaponEffect> effects;
    [SerializeReference, SubclassSelector] private IWeaponEffect effect;
   
    [field: SerializeField] public Vector3 SpriteScale { get; private set; }
    [field: SerializeField] public Vector3 SpriteOffset { get; private set; }
    [field: SerializeField] public Vector3 SpriteAngle { get; private set; }
    [field: SerializeField] public Vector3 MuzzleOffset { get; private set; }
    [field: SerializeField] public Vector2 ColliderOffset { get; private set; }
    [field: SerializeField] public Vector2 ColliderSize { get; private set; }

    public static WeaponClasses ToWeaponClass(string type)
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
            case "Elemental": return WeaponClasses.Elemental;
            default:
            {
                Debug.LogWarning("Weapon Class Can't Find: "+ type);
                return WeaponClasses.None;
            }
        }
    }
    public static string WeaponClassToString(WeaponClasses weaponClass)
    {
        return weaponClass switch
        {
            WeaponClasses.Blunt => nameof(WeaponClasses.Blunt),
            WeaponClasses.Precise => nameof(WeaponClasses.Precise),
            WeaponClasses.Primitive => nameof(WeaponClasses.Primitive),
            WeaponClasses.Gun => nameof(WeaponClasses.Gun),
            WeaponClasses.Medieval => nameof(WeaponClasses.Medieval),
            WeaponClasses.Blade => nameof(WeaponClasses.Blade),
            WeaponClasses.Heavy => nameof(WeaponClasses.Heavy),
            WeaponClasses.Elemental => nameof(WeaponClasses.Elemental),
            // 예외 처리
            _ => nameof(WeaponClasses.None)
        };
    }
    
#if UNITY_EDITOR
    public void SyncDataCSV(string weaponName, WeaponStat weaponStat, Sprite sprite) //임시
    {
        Name = weaponName;
        WeaponStat = weaponStat;
        Sprite = sprite;
    }

    public void SetWeaponTransform(WeaponTransform weaponTransform)
    {
        SpriteScale = weaponTransform.SpriteScale;
        SpriteOffset = weaponTransform.SpriteOffset;
        SpriteAngle =  weaponTransform.SpriteAngle;
        MuzzleOffset = weaponTransform.MuzzleOffest;
        ColliderSize = weaponTransform.ColliderSize;
        ColliderOffset = weaponTransform.ColliderOffset;
    }
#endif
}
