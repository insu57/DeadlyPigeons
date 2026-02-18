using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    //캐릭터 ID 필요.
    [SerializeField] private  string characterName;
    public string CharacterName => characterName;
    [SerializeField] private Sprite characterSprite;
    public Sprite CharacterSprite => characterSprite;
    
    
    //0미만 효과 설정 필요
    [Header("Initial Main Stats")]
    [SerializeField] private float maxHealth = 10;
    public float MaxHealth => maxHealth;
    [SerializeField] private float healthRegen = 0;
    public float HealthRegen => healthRegen;
    [SerializeField] private float healthAbsorb = 0;
    public float HealthAbsorb => healthAbsorb;
    [SerializeField] private float armor = 0;
    public float Armor => armor;
    [SerializeField] private float dodgeChance = 0;
    public float DodgeChance => dodgeChance;
    [SerializeField] private float speed = 0;
    public float Speed => speed;
    
    [SerializeField] private float damageMultiplier = 0;
    public float DamageMultiplier => damageMultiplier;
    [SerializeField] private float meleeDamage;
    public float MeleeDamage => meleeDamage;
    [SerializeField] private float rangedDamage;
    public float RangedDamager => rangedDamage;
    [SerializeField] private float criticalChance;
    public float CriticalChance => criticalChance;
    [SerializeField] private float attackSpeed;
    public float AttackSpeed => attackSpeed;

    [SerializeField] private float luck;
    public float Luck => luck;
    [SerializeField] private float harvest;
    public float Harvest => harvest;

    //Need Weapon List
}
