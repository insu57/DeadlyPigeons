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
    [SerializeField] private TextAsset weaponTypesCSV;
    [SerializeField] private string charPath = "Assets/Sprites/Characters/";
    [SerializeField] private string weaponPath =  "Assets/Sprites/Weapons/";
    [SerializeField] private WeaponTypesSO weaponTypesSO;
    
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
                //0: ID, 1:Name, 2: id-name
                
                var strArr = rowData[5].Split('|');
                List<WeaponTypes> weaponTypes = new();
                foreach (var str in strArr)
                {
                    var newType = WeaponData.ToWeaponTypes(str);
                    if (newType != WeaponTypes.None)
                    {
                        weaponTypes.Add(newType);
                    }
                }
                
                strArr = rowData[6].Split('|');
                List<int> baseDamage = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out int damage))
                    {
                        baseDamage.Add(damage);
                    }
                }
                
                strArr = rowData[7].Split('|');
                List<DamageTypeMultiplier> damageTypeMultipliers = new();
                foreach (var str in strArr)
                {
                    var tmp = str.Split(':');
                    var type = WeaponData.ToDamageTypes(tmp[0]);
                    if(type == DamageTypes.None) continue;
                    var multiplier = int.Parse(tmp[1]);
                    var newType = new DamageTypeMultiplier
                    {
                        type = type,
                        value = multiplier
                    };
                    damageTypeMultipliers.Add(newType);
                }
                
                ////
                
                WeaponStat parsed = new WeaponStat
                {
                    initTier = int.Parse(rowData[3]),
                    isMelee = rowData[4] == "Melee",
                    types = weaponTypes,
                    baseDamage = baseDamage,
                    damageMultipliers = damageTypeMultipliers,
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

    /*
    [ContextMenu("Sync Weapon Types")]
    public void SyncWeaponTypesFromCSV()
    {
        string[] lines = weaponTypesCSV.text.Split(new []{ '\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);
        
        List<WeaponTypeMapping> parsedTypes = new();

        for (int i = 2; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            int id = int.Parse(rowData[0]);
            string typeName = rowData[1];

            if (Enum.TryParse<WeaponTypes>(typeName, out var parsedType))
            {
                parsedTypes.Add(new WeaponTypeMapping
                {
                    id = id,
                    type =  parsedType
                });
            }
            else
            {
                Debug.LogError("Type Name Not Found : " + typeName);
            }
        }
        
#if UNITY_EDITOR
       weaponTypesSO.SyncTypes(parsedTypes);
        
        EditorUtility.SetDirty(weaponTypesSO);
        AssetDatabase.SaveAssets();
#endif
        
    }*/
}
