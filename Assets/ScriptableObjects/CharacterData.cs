using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Serialization;

public enum MainStats
{
    MaxHP, HealthRegen, HealthAbsorb, Armor, DodgeChance, Speed, 
    Damage, Melee, Ranged, Elemental ,Engineering, Tactical, AttackSpeed,
    CritChance, Range ,Luck, Harvest,
    None
}

public enum SubStats
{
    ConsumableHeal, XPGain, ItemPrice, PickUpRange, ExplosiveDamage, ExplosiveSize, 
    Bounces,  Piercing, PiercingDamage ,FreeRerolls, Enemies, EnemiesSpeed, RerollPrice,
    Knockback,
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

    
    
    
#if UNITY_EDITOR
    public void SyncDataCSV(string charName,string description, List<int> weaponIDList,List<InitStats> initStatsList,
        Sprite sprite ) //csv -> so 저장
    {
        CharacterName = charName;
        Description = description;
        InitWeaponIDList = weaponIDList;
        InitStatsList = initStatsList;
        CharacterSprite = sprite;
    }
#endif
}
