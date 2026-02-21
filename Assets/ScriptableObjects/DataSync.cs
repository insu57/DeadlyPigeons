using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataSync : MonoBehaviour
{
    [SerializeField] private TextAsset characterCSV;
    [SerializeField] private List<CharacterDataSO> characterDataList; //개선?

    [ContextMenu("Sync Character Data")]
    public void SyncDataFromCSV()
    {
        if (!characterCSV)
        {
            Debug.LogError("Character CSV Error : null");
            return;
        }
        
        string[] lines = characterCSV.text.Split(new []{ '\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> csvDict = new();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            int id = int.Parse(rowData[0]);

            csvDict[id] = rowData;
        }
        
        int updateCount = 0;
        foreach (var so in characterDataList)
        {
            if (csvDict.TryGetValue(so.ID, out string[] rowData))
            {
                CharacterStats parsed = new CharacterStats
                {
                    characterName = rowData[1],
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
                
                
                so.SyncDataCSV(parsed);
                
                updateCount++;
#if  UNITY_EDITOR
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
        Debug.Log("Character Data Updated: " + updateCount + "/" + characterDataList.Count);
    }
}
