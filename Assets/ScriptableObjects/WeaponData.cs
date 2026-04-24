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

public enum WeaponEffectType
{
    Burning,
    Explosive,
    Piercing,
    Bounces,
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
}

[Serializable]
public struct WeaponEffectData
{
    public WeaponEffectType effectType;
    public List<WeaponEffectValues> valuesList; //각 항목 리스트 -> 티어 수치 리스트
    public string[] effectDescription;
}

[Serializable]
public struct WeaponEffectValues //티어 수치 리스트 : 초기 티어 ~ 4 티어
{
    public List<float> values;
    public MainStats mainStat;
    public SubStats subStat;
    public List<int> multipliers; //해당 스탯 값 만큼 추가(티어별 수치에 맞게)
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    //need weapon id
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public WeaponStat WeaponStat { get; private set; }
    [field: SerializeField] public List<WeaponEffectData> WeaponEffectDataList {get; private set;}
    
    [field: SerializeField] public Vector3 SpriteScale { get; private set; }
    [field: SerializeField] public Vector3 SpriteOffset { get; private set; }
    [field: SerializeField] public Vector3 SpriteAngle { get; private set; }
    [field: SerializeField] public Vector3 MuzzleOffset { get; private set; }
    [field: SerializeField] public Vector2 ColliderOffset { get; private set; }
    [field: SerializeField] public Vector2 ColliderSize { get; private set; }

    public static WeaponClasses ToWeaponClass(string type)
    {
        // 입력된 type 문자열과 일치하는 Enum이 있으면 반환
        if (Enum.TryParse(type, out WeaponClasses result))
        {
            return result;
        }
        // 일치하는 값이 없을 경우 예외 처리
        Debug.LogWarning($"Weapon Class Can't Find: {type}");
        return WeaponClasses.None;
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

    public static WeaponEffectType StringToEffectType(string effectType)
    {
        return effectType switch
        {
            nameof(WeaponEffectType.Burning) => WeaponEffectType.Burning,
            nameof(WeaponEffectType.Explosive) => WeaponEffectType.Explosive,
            nameof(WeaponEffectType.Piercing) => WeaponEffectType.Piercing,
            nameof(WeaponEffectType.Bounces) =>  WeaponEffectType.Bounces,
            //None
            _ => WeaponEffectType.None
        };
    }

    public static IWeaponEffect GetWeaponEffect(WeaponEffectType effectType)
    {
        return effectType switch
        {
            WeaponEffectType.Burning => new Burning(),
            WeaponEffectType.Explosive => new Explosive(),
            WeaponEffectType.Bounces => new Bounces(),
            WeaponEffectType.Piercing => new Piercing(),
            _ => null
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


    public void SetWeaponEffectData(List<WeaponEffectData> effectDataList)
    {
        WeaponEffectDataList = effectDataList;
    }
#endif
}
