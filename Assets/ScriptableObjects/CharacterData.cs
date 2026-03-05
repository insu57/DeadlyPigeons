using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Serialization;

public enum MainStats
{
    MaxHP, HealthRegen, HealthAbsorb, Armor, DodgeChance, Speed, 
    Damage, Melee, Ranged, Elemental ,Engineering, Tactical, AttackSpeed,
    CritChance, Luck, Harvest,
    None
}

public enum SubStats
{
    ConsumableHeal, XPGain, ItemPrice, PickUpRange, ExplosiveDamage, ExplosiveSize, 
    Bounces,  Piercing, FreeRerolls, Enemies, EnemiesSpeed, RerollPrice,
    None
}

[Serializable]
public struct InitStats
{
    public MainStats mainStats;
    public SubStats subStats;
    public int amount;
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public List<int> InitWeaponIDList {get; private set;}
    [field: SerializeField] public string Description { get; private set; }
    //패시브 아이템 형식으로 교체될 수 있음.
    [field:SerializeField] public List<InitStats> InitStatsList { get; private set; }
    [field: SerializeField] public Sprite CharacterSprite { get; private set; }

    public static MainStats StringToMainStats(string str)
    {
        return str switch
        {
            "MaxHP" => MainStats.MaxHP,
            "HealthRegen" => MainStats.HealthRegen,
            "HealthAbsorb" => MainStats.HealthAbsorb,
            "Armor" => MainStats.Armor,
            "DodgeChance" => MainStats.DodgeChance,
            "Speed" => MainStats.Speed,
            "Damage" => MainStats.Damage,
            "Melee" => MainStats.Melee,
            "Ranged" => MainStats.Ranged,
            "Elemental" => MainStats.Elemental,
            "Engineering" => MainStats.Engineering,
            "Tactical" => MainStats.Tactical,
            "AttackSpeed" => MainStats.AttackSpeed,
            "CritChance" => MainStats.CritChance,
            "Luck" => MainStats.Luck,
            "Harvest" => MainStats.Harvest,
            _ => MainStats.None
        };
    }

    public static SubStats StringToSubStats(string str)
    {
        return str switch
        {
            "ConsumableHeal" => SubStats.ConsumableHeal,
            "XPGain"         => SubStats.XPGain,
            "ItemPrice"      => SubStats.ItemPrice,
            "PickUpRange"    => SubStats.PickUpRange,
            "ExplosiveDamage"=> SubStats.ExplosiveDamage,
            "ExplosiveSize"  => SubStats.ExplosiveSize,
            "Bounces"        => SubStats.Bounces,
            "Piercing"       => SubStats.Piercing,
            "FreeRerolls"    => SubStats.FreeRerolls,
            "Enemies"        => SubStats.Enemies,
            "EnemiesSpeed"   => SubStats.EnemiesSpeed,
            "RerollPrice"    => SubStats.RerollPrice, 
            _ => SubStats.None,
        };
    }

#if UNITY_EDITOR
    public void SyncDataCSV(string charName,string description, List<int> weaponIDList,List<InitStats> initStatsList,
        Sprite sprite )
    {
        CharacterName = charName;
        Description = description;
        InitWeaponIDList = weaponIDList;
        InitStatsList = initStatsList;
        CharacterSprite = sprite;
    }
#endif
}
