using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataSync : MonoBehaviour
{
    [SerializeField] private TextAsset characterCSV;
    [SerializeField] private TextAsset charSubStatsCSV;
    [SerializeField] private TextAsset weaponCSV;
    
    [SerializeField] private string charPath = "Assets/Sprites/Characters/";
    [SerializeField] private string weaponPath =  "Assets/Sprites/Weapons/";

    
    [ContextMenu("Sync Character Data")]
    public void SyncCharDataFromCSV()
    {
        if (!characterCSV)
        {
            Debug.LogError("Character CSV Error : null");
            return;
        }
        
        CharacterData[] characters = Resources.LoadAll<CharacterData>("Data/Characters");
        
        string[] lines = characterCSV.text.Split(new []{ '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> mainStatsDict = new();
        
        for (int i = 2; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            int id = int.Parse(rowData[0]);

            mainStatsDict[id] = rowData;
        }
        
        lines = charSubStatsCSV.text.Split(new []{ '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> subStatsDict = new();

        for (int i = 2; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');

            int id = int.Parse(rowData[0]);

            subStatsDict[id] = rowData;
        }
        
        int charUpdateCount = 0;
        foreach (var so in characters)
        {
            if (mainStatsDict.TryGetValue(so.ID, out string[] rowData) &&
                subStatsDict.TryGetValue(so.ID, out var subRowData))
            {
                var characterName = rowData[1];
                
                var mainStatsParsed = new CharMainStats
                {
                    // 0: ID, 1: Name
                    maxHealth = int.Parse(rowData[2]),
                    healthRegen = int.Parse(rowData[3]),
                    healthAbsorb = int.Parse(rowData[4]),
                    armor = int.Parse(rowData[5]),
                    dodgeChance = int.Parse(rowData[6]),
                    speed = int.Parse(rowData[7]),
                    damageMultiplier = int.Parse(rowData[8]),
                    meleeDamage = int.Parse(rowData[9]),
                    rangedDamage = int.Parse(rowData[10]),
                    criticalChance = int.Parse(rowData[11]),
                    luck = int.Parse(rowData[12]),
                    harvest = int.Parse(rowData[13]),
                };
                
                string initWeapons = rowData[14];
                string[] weaponStrArray =  initWeapons.Split('|');
                List<int> parsedWeaponID = new();

                foreach (var str in weaponStrArray)
                {
                    if (int.TryParse(str, out int id))
                    {
                        parsedWeaponID.Add(id);
                    }
                }

                var subStatsParsed = new CharSubStats
                {
                    consumableHeal = int.Parse(subRowData[2]),
                    xpGain = int.Parse(subRowData[3]),
                    itemPrice = int.Parse(subRowData[4]),
                    pickUpRange = int.Parse(subRowData[5]),
                    explosiveDamage = int.Parse(subRowData[6]),
                    explosiveSize = int.Parse(subRowData[7]),
                    bounces = int.Parse(subRowData[8]),
                    piercing = int.Parse(subRowData[9]),
                    freeRerolls = int.Parse(subRowData[10]),
                    enemies = int.Parse(subRowData[11]),
                    enemiesSpeed = int.Parse(subRowData[12]),
                    rerollPrice = int.Parse(subRowData[13])
                };
                
                
#if  UNITY_EDITOR
                string charSpritePath = $"{charPath}{so.ID}.png";
                Sprite charSprite = AssetDatabase.LoadAssetAtPath<Sprite>(charSpritePath);
                if (!charSprite)
                {
                    Debug.LogError("Character Sprite Not Found : " + so.ID);
                }
                
                so.SyncDataCSV(characterName, mainStatsParsed, subStatsParsed ,charSprite, parsedWeaponID);
                
                charUpdateCount++;
                
                EditorUtility.SetDirty(so);
#endif
            }
            else
            {
                Debug.LogError("Character Data Not Found : " + so.ID);
            }
        }
        
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Character Data Updated: " + charUpdateCount + "/" + characters.Length);
    }

    [ContextMenu("Sync Weapon Data")]
    public void SyncWeaponDataFromCSV()
    {
        WeaponData[] weapons = Resources.LoadAll<WeaponData>("Data/Weapons");
        string[] lines = weaponCSV.text.Split(new []{ '\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> csvDict = new();
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            int id = int.Parse(rowData[0]);

            csvDict[id] = rowData;
        }
        
        int weaponUpdateCount = 0;
        
        foreach (var so in weapons)
        {
            if (csvDict.TryGetValue(so.ID, out string[] rowData))
            {
                var weaponName =  rowData[1];
                //0: ID, 1:Name, 3: id-name
                
                var strArr = rowData[5].Split('|');//무기 타입
                List<WeaponTypes> weaponTypes = new();
                foreach (var str in strArr)
                {
                    var newType = WeaponData.ToWeaponTypes(str);
                    if (newType != WeaponTypes.None)
                    {
                        weaponTypes.Add(newType);
                    }
                }
                
                strArr = rowData[6].Split('|'); //기본 데미지(티어마다)
                List<int> baseDamage = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out int damage))
                    {
                        baseDamage.Add(damage);
                    }
                }
                
                strArr = rowData[7].Split('/'); //데미지 스탯 배수
                List<DamageTypeMultiplier> damageTypeMultipliers = new();
                foreach (var str in strArr)
                {
                    var tmp = str.Split(':');
                    var type = WeaponData.ToDamageTypes(tmp[0]);
                    if(type == DamageTypes.None) continue;
                    var multiplierStr = tmp[1].Split('|');
                    List<int> multipliers = new(); //타입 별 데미지 배수
                    foreach (var mp in multiplierStr) 
                    {
                        if (int.TryParse(mp, out int multiplier)) //티어 별
                        {
                            multipliers.Add(multiplier);
                        }
                    }
                    
                    var newType = new DamageTypeMultiplier
                    {
                        type = type,
                        value = multipliers
                    };
                    damageTypeMultipliers.Add(newType);
                }

                strArr = rowData[8].Split('|'); //공격속도
                List<float> attackSpeed = new();
                foreach (var str in strArr)
                {
                    if (float.TryParse(str, out var speed))
                    {
                        attackSpeed.Add(speed);
                    }
                }
                
                strArr = rowData[9].Split('|'); //치명확률
                List<int> critChance = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var crit))
                    {
                        critChance.Add(crit);
                    }
                }
                
                strArr = rowData[10].Split('|'); //치명데미지
                List<float> critDamage = new();
                foreach (var str in strArr)
                {
                    if (float.TryParse(str, out var crit))
                    {
                        critDamage.Add(crit);
                    }
                }
                
                strArr = rowData[11].Split('|'); //범위
                List<int> range = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var r))
                    {
                        range.Add(r);
                    }
                }
                
                strArr = rowData[12].Split('|'); //넉백
                List<int> knockback = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var knock))
                    {
                        knockback.Add(knock);
                    }
                }
                
                strArr = rowData[13].Split('|'); //체력흡수
                List<int> healthAbsorb = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var health))
                    {
                        healthAbsorb.Add(health);
                    }
                }
                
                strArr = rowData[14].Split('|'); //판매가격
                List<int> prices = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var price))
                    {
                        prices.Add(price);
                    }
                }
                
                WeaponStat parsed = new WeaponStat
                {
                    initTier = int.Parse(rowData[2]),
                    isMelee = rowData[4] == "Melee",
                    types = weaponTypes,
                    baseDamage = baseDamage,
                    damageMultipliers = damageTypeMultipliers,
                    attackSpeed = attackSpeed,
                    critChance = critChance,
                    critDamage = critDamage,
                    range = range,
                    knockBack = knockback,
                    healthAbsorb = healthAbsorb,
                    prices = prices,
                    description = rowData[15]
                };
                
#if UNITY_EDITOR
                string weaponSpritePath = $"{weaponPath}{so.ID}.png";
                Sprite weaponSprite = AssetDatabase.LoadAssetAtPath<Sprite>(weaponSpritePath);
                if (!weaponSprite)
                {
                    Debug.LogError("Weapon Sprite Not Found : " + so.ID);
                }
                
                so.SyncDataCSV(weaponName, parsed, weaponSprite);

                weaponUpdateCount++;
                
                EditorUtility.SetDirty(so);
#endif
            }
            else
            {
                Debug.LogError("Weapon Data Not Found : " + so.ID);
            }
            
        }
        
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Weapon Data Updated " + weaponUpdateCount + "/" + weapons.Length);
    }
    
}
