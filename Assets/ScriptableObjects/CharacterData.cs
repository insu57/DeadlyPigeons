using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct CharMainStats
{
    public float maxHealth;
    public float healthRegen;
    public float healthAbsorb;
    public float armor;
    public float dodgeChance;
    public float speed;
    public float damageMultiplier;
    public float meleeDamage;
    public float rangedDamage;
    public float criticalChance;
    public float attackSpeed;
    public float luck;
    public float harvest;
}

[Serializable]
public struct CharSubStats
{
    
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public Sprite CharacterSprite { get; private set; }
    [field: SerializeField] public CharMainStats CharMainStats { get; private set; }
    
    [field: SerializeField] public List<int> InitWeaponIDList {get; private set;}
    

#if UNITY_EDITOR
    public void SyncDataCSV(string charName, CharMainStats charMainStats, Sprite sprite, List<int> weaponIDList) //임시
    {
        CharacterName = charName;
        CharMainStats = charMainStats;
        CharacterSprite = sprite;
        InitWeaponIDList = weaponIDList;
    }
#endif
}
