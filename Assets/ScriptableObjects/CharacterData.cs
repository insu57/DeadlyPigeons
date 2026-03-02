using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct CharMainStats
{
    public int maxHealth;
    public int healthRegen;
    public int healthAbsorb;
    public int armor;
    public int dodgeChance;
    public int speed;
    public int damageMultiplier;
    public int meleeDamage;
    public int rangedDamage;
    public int criticalChance;
    public int attackSpeed;
    public int luck;
    public int harvest;
}

[Serializable]
public struct CharSubStats
{
    public int consumableHeal;
    public int xpGain;
    public int itemPrice;
    public int pickUpRange;
    public int explosiveDamage;
    public int explosiveSize;
    public int bounces;
    public int piercing;
    public int freeRerolls;
    public int enemies;
    public int enemiesSpeed;
    public int rerollPrice;
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public Sprite CharacterSprite { get; private set; }
    [field: SerializeField] public CharMainStats CharMainStats { get; private set; }
    
    [field: SerializeField] public List<int> InitWeaponIDList {get; private set;}
    [field: SerializeField] public CharSubStats CharSubStats { get; private set; }

#if UNITY_EDITOR
    public void SyncDataCSV(string charName, CharMainStats charMainStats, CharSubStats charSubStats,
        Sprite sprite, List<int> weaponIDList) //임시
    {
        CharacterName = charName;
        CharMainStats = charMainStats;
        CharSubStats = charSubStats;
        CharacterSprite = sprite;
        InitWeaponIDList = weaponIDList;
    }
#endif
}
