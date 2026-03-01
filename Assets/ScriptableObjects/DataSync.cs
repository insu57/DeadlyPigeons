using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataSync : MonoBehaviour
{
    [SerializeField] private TextAsset characterCSV;
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
        
        string[] lines = characterCSV.text.Split(new []{ '\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> csvDict = new();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            int id = int.Parse(rowData[0]);

            csvDict[id] = rowData;
        }
        
        int charUpdateCount = 0;
        foreach (var so in characters)
        {
            if (csvDict.TryGetValue(so.ID, out string[] rowData))
            {
                var characterName = rowData[1];
                
                CharMainStats parsed = new CharMainStats
                {
                    // 0: ID, 1: Name
                    maxHealth = float.Parse(rowData[2]),
                    healthRegen = float.Parse(rowData[3]),
                    healthAbsorb = float.Parse(rowData[4]),
                    armor = float.Parse(rowData[5]),
                    dodgeChance = float.Parse(rowData[6]),
                    speed = float.Parse(rowData[7]),
                    damageMultiplier = float.Parse(rowData[8]),
                    meleeDamage = float.Parse(rowData[9]),
                    rangedDamage = float.Parse(rowData[10]),
                    criticalChance = float.Parse(rowData[11]),
                    luck = float.Parse(rowData[12]),
                    harvest = float.Parse(rowData[13]),
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
                
                
                
#if  UNITY_EDITOR
                string charSpritePath = $"{charPath}{so.ID}.png";
                Sprite charSprite = AssetDatabase.LoadAssetAtPath<Sprite>(charSpritePath);
                if (!charSprite)
                {
                    Debug.LogError("Character Sprite Not Found : " + so.ID);
                }
                
                so.SyncDataCSV(characterName, parsed, charSprite, parsedWeaponID);
                
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
                WeaponStat parsed = new WeaponStat
                {
                    initTier = int.Parse(rowData[3]),
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
        
    }
}
