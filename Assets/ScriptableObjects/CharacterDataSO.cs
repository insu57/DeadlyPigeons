using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct CharacterStats
{
    public string characterName;
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

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public Sprite CharacterSprite { get; private set; }
    [field: SerializeField] public CharacterStats CharacterStats { get; private set; }
    
    
    /*[field: SerializeField] public string CharacterName { get; private set; }
    //0미만 효과 설정 필요
    [Header("Initial Main Stats")]
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public float HealthRegen { get; private set; }
    [field: SerializeField] public float HealthAbsorb { get; private set; }
    [field: SerializeField] public float Armor { get; private set; }
    [field: SerializeField] public float DodgeChance { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
   
    [field: SerializeField] public float DamageMultiplier { get; private set; }
    [field: SerializeField] public float MeleeDamage{get; private set;}
    [field: SerializeField] public float RangedDamage{get; private set;}
    [field: SerializeField] public float CriticalChance{get; private set;}
    [field: SerializeField] public float AttackSpeed{get; private set;}
   
    [field: SerializeField] public float Luck{get; private set;}
    [field: SerializeField] public float Harvest{get; private set;}
    */
    //Need Weapon List serializeField에서 csv 등에서 파싱으로 수정
    [SerializeField] private List<WeaponData> weapons;
    

#if UNITY_EDITOR
    public void SyncDataCSV(CharacterStats characterStats) //임시
    {
       CharacterStats = characterStats;
    }
#endif
}
