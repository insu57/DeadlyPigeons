using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataSync : MonoBehaviour
{
    [SerializeField] private TextAsset characterCSV;
    [SerializeField] private TextAsset weaponCSV;
    
    [ContextMenu("Sync Character Data")]
    public void SyncDataFromCSV()
    {
        if (!characterCSV)
        {
            Debug.LogError("Character CSV Error : null");
            return;
        }
        
        CharacterData[] characters = Resources.LoadAll<CharacterData>("Data/Characters");
        WeaponData[] weapons = Resources.LoadAll<WeaponData>("Data/Weapons");
        
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
                
                
                so.SyncDataCSV(characterName, parsed, parsedWeaponID);
                
                charUpdateCount++;
#if  UNITY_EDITOR
                EditorUtility.SetDirty(so);
#endif
            }
            else
            {
                Debug.LogError("Character Data Not Found : " + so.ID);
            }
        }
        
        lines = weaponCSV.text.Split(new []{ '\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);
        
        csvDict.Clear();
        
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
                    tier = int.Parse(rowData[3]),
                };
                
                so.SyncDataCSV(weaponName, parsed);

                weaponUpdateCount++;
                
                
#if UNITY_EDITOR
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
        Debug.Log("Character Data Updated: " + charUpdateCount + "/" + characters.Length);
        Debug.Log("Weapon Data Updated " + weaponUpdateCount + "/" + weapons.Length);
    }
}
