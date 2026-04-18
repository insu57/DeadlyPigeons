using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataSync : MonoBehaviour
{
    [SerializeField] private TextAsset characterCSV;
    [SerializeField] private TextAsset charPassiveCSV;
    [SerializeField] private TextAsset itemCSV;
    [SerializeField] private TextAsset weaponCSV;
    [SerializeField] private TextAsset weaponClassCSV;
    [SerializeField] private TextAsset weaponEffectCSV;
    
    [SerializeField] private string charPath = "Assets/Sprites/Characters/";
    [SerializeField] private string itemPath = "Assets/Sprites/Items/";
    [SerializeField] private string weaponPath =  "Assets/Sprites/Weapons/";
    [SerializeField] private WeaponClassBonusData classBonusData;
    
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
        Dictionary<int, string[]> charDict = new();
        
        for (int i = 2; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            int id = int.Parse(rowData[0]);
            
            charDict[id] = rowData;
        }
        
        
        int charUpdateCount = 0;
        foreach (var so in characters)
        {
            if (charDict.TryGetValue(so.ID, out var rowData))
            {
                var characterName = rowData[1];
                string initWeapons = rowData[2];
                string[] weaponStrArray =  initWeapons.Split('|');
                List<int> parsedWeaponID = new();

                foreach (var str in weaponStrArray)
                {
                    if (int.TryParse(str, out int id))
                    {
                        parsedWeaponID.Add(id);
                    }
                }
                
                var description = rowData[3];
                
                var statTotal = rowData[4];
                List<InitStats> initStatsList = new();
                string[] statStrArray = statTotal.Split('|');
                
                foreach (var str in statStrArray)
                {
                    
                    string[] stat = str.Split(':');
                    
                    string statStr = stat[0];
                    int statValue = int.Parse(stat[1]);
                    
                    var mainStat = statStr.StringToMainStats();
                    var subStat = statStr.StringToSubStats();
                    if (mainStat != MainStats.None)
                    {
                        var initStat = new InitStats
                        {
                            mainStats = mainStat,
                            subStats = SubStats.None,
                            amount = statValue,
                        };
                        initStatsList.Add(initStat);
                    }
                    else if (subStat != SubStats.None)
                    {
                        var initStat = new InitStats
                        {
                            mainStats = MainStats.None,
                            subStats = subStat,
                            amount = statValue,
                        };
                        initStatsList.Add(initStat);
                    }
                    else
                    {
                        Debug.LogError("Character InitStat Not Found : " + so.ID);
                    }
                }
                
                
#if  UNITY_EDITOR
                string charSpritePath = $"{charPath}{so.ID}.png";
                Sprite charSprite = AssetDatabase.LoadAssetAtPath<Sprite>(charSpritePath);
                if (!charSprite)
                {
                    Debug.LogError("Character Sprite Not Found : " + so.ID);
                }

                so.SyncDataCSV(characterName, description, parsedWeaponID, initStatsList, charSprite);
                
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

    [ContextMenu("Sync Item Data")]
    public void SyncItemDataFromCSV()
    {
        if (!charPassiveCSV || !itemCSV)
        {
            Debug.LogError("Item CSV Error : null");
            return;
        }

        ItemData[] items = Resources.LoadAll<ItemData>("Data/Items");
        
        // 캐릭터 패시브 CSV 파싱 준비
        string[] passiveLines = charPassiveCSV.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> passiveDict = new();
        for (int i = 2; i < passiveLines.Length; i++)
        {
            string[] rowData = passiveLines[i].Split(',');
            if (int.TryParse(rowData[0], out int id))
            {
                passiveDict[id] = rowData;
            }
        }

        // 일반 아이템 CSV 파싱 준비
        string[] itemLines = itemCSV.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> itemDict = new();
        for (int i = 2; i < itemLines.Length; i++)
        {
            string[] rowData = itemLines[i].Split(',');
            if (int.TryParse(rowData[0], out int id))
            {
                itemDict[id] = rowData;
            }
        }

        int itemUpdateCount = 0;

        foreach (var so in items)
        {
            string[] rowData = null;
            bool isFound = false;

            ItemStat itemStat = new();
            string itemSpritePath = null;
            if (passiveDict.TryGetValue(so.ID, out rowData))
            {
                isFound = true;
                itemSpritePath = $"{charPath}{so.ID}.png";
                // TODO: 캐릭터 패시브 데이터 파싱 로직
                itemStat.itemName =  rowData[1];
                itemStat.tier = 1; //패시브는 티어 1로
                int idx = 3;
                List<StatAmount> values = new();
                List<StatAmount> multipliers = new();
                for (int i = 0; i < 5; i++)
                {
                    int curIdx = idx + i * 3;
                    if(curIdx >= rowData.Length || string.IsNullOrEmpty(rowData[curIdx]))
                        break;
                    
                    var statStr = rowData[curIdx];
                    var mainStat = statStr.StringToMainStats();
                    var subStat = statStr.StringToSubStats();
                    int amount = int.Parse(rowData[curIdx+1]);
                    var statAmount = new StatAmount
                    {
                        mainStat = mainStat,
                        subStat = subStat,
                        amount = amount,
                    };
                    bool isMultiplier = bool.Parse(rowData[curIdx+2]);
                    if (isMultiplier)
                    {
                        multipliers.Add(statAmount);
                    }
                    else
                    {
                        values.Add(statAmount);
                    }
                }
                
                itemStat.statMultipliers = multipliers;
                itemStat.statValues = values;
                itemStat.description = rowData[17];
            }
            else if (itemDict.TryGetValue(so.ID, out rowData))
            {
                isFound = true;
                itemSpritePath = $"{itemPath}{so.ID}.png";
                // TODO: 일반 아이템 데이터 파싱 로직
                itemStat.itemName = rowData[1];
                itemStat.tier = int.Parse(rowData[2]);
                int idx = 2;
                List<StatAmount> values = new();
                for (int i = 0; i < 5; i++)
                {
                    int curIdx = idx + i * 2;
                    if(curIdx >= rowData.Length || string.IsNullOrEmpty(rowData[curIdx]))
                        break;
                    
                    var statStr = rowData[curIdx];
                    var mainStat = statStr.StringToMainStats();
                    var subStat = statStr.StringToSubStats();
                    int amount = int.Parse(rowData[curIdx+1]);
                    var statAmount = new StatAmount
                    {
                        mainStat = mainStat,
                        subStat = subStat,
                        amount = amount,
                    };
                    values.Add(statAmount);
                }
                itemStat.statValues = values;
                itemStat.description = rowData[13];
            }

            if (isFound)
            {
                
#if UNITY_EDITOR
                // TODO: 파싱된 데이터를 기반으로 so.SyncItemData 호출
                Sprite itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(itemSpritePath);
                if (!itemSprite)
                {
                    Debug.LogError("Item Sprite Not Found : " + so.ID);
                }
                
                so.SyncItemData(itemStat);
                
                itemUpdateCount++;
                EditorUtility.SetDirty(so);
#endif
            }
            else
            {
                Debug.LogError("Item Data Not Found : " + so.ID);
            }
        }

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Item Data Updated: " + itemUpdateCount + "/" + items.Length);
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
                
                var attackType = AttackType.None;
                if (rowData[5] == nameof(AttackType.Sweep)) attackType = AttackType.Sweep;
                else if (rowData[5] == nameof(AttackType.Thrust)) attackType = AttackType.Thrust;
                
                var strArr = rowData[6].Split('|');//무기 클래스
                List<WeaponClasses> weaponTypes = new();
                foreach (var str in strArr)
                {
                    var newType = WeaponData.ToWeaponClass(str);
                    if (newType != WeaponClasses.None)
                    {
                        weaponTypes.Add(newType);
                    }
                }
                
                strArr = rowData[7].Split('|'); //기본 데미지(티어마다)
                List<int> baseDamage = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out int damage))
                    {
                        baseDamage.Add(damage);
                    }
                }
                
                strArr = rowData[8].Split('/'); //데미지 스탯 배수
                List<StatMultiplier> damageTypeMultipliers = new();
                foreach (var str in strArr)
                {
                    var tmp = str.Split(':');
                    var statStr = tmp[0];
                    var stat = statStr.StringToMainStats();
                    if(stat == MainStats.None) continue;
                    var multiplierStr = tmp[1].Split('|');
                    List<int> multipliers = new(); //타입 별 데미지 배수
                    foreach (var mp in multiplierStr) 
                    {
                        if (int.TryParse(mp, out int multiplier)) //티어 별
                        {
                            multipliers.Add(multiplier);
                        }
                    }
                    
                    var newType = new StatMultiplier
                    {
                        stat = stat,
                        value = multipliers
                    };
                    damageTypeMultipliers.Add(newType);
                }

                strArr = rowData[9].Split('|'); //공격속도
                List<float> attackSpeed = new();
                foreach (var str in strArr)
                {
                    if (float.TryParse(str, out var speed))
                    {
                        attackSpeed.Add(speed);
                    }
                }
                
                strArr = rowData[10].Split('|'); //치명확률
                List<int> critChance = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var crit))
                    {
                        critChance.Add(crit);
                    }
                }
                
                strArr = rowData[11].Split('|'); //치명데미지
                List<float> critDamage = new();
                foreach (var str in strArr)
                {
                    if (float.TryParse(str, out var crit))
                    {
                        critDamage.Add(crit);
                    }
                }
                
                strArr = rowData[12].Split('|'); //범위
                List<int> range = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var r))
                    {
                        range.Add(r);
                    }
                }
                
                strArr = rowData[13].Split('|'); //넉백
                List<int> knockback = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var knock))
                    {
                        knockback.Add(knock);
                    }
                }
                
                strArr = rowData[14].Split('|'); //체력흡수
                List<int> healthAbsorb = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var health))
                    {
                        healthAbsorb.Add(health);
                    }
                }
                
                strArr = rowData[15].Split('|'); //판매가격
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
                    attackType = attackType,
                    classes = weaponTypes,
                    baseDamage = baseDamage,
                    damageMultipliers = damageTypeMultipliers,
                    attackSpeed = attackSpeed,
                    critChance = critChance,
                    critDamage = critDamage,
                    range = range,
                    knockBack = knockback,
                    healthAbsorb = healthAbsorb,
                    prices = prices,
                    description = rowData[16]
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

    [ContextMenu("Sync Weapon Effect Data")]
    public void SyncWeaponEffectFromCSV()
    {
        WeaponData[] weapons = Resources.LoadAll<WeaponData>("Data/Weapons");
        string[] lines = weaponEffectCSV.text.Split(new []{ '\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> csvDict = new();
        List<string[]> csvList = new();
        Dictionary<int, List<WeaponEffectData>> effectDataDict = new(); //id : effectData list
        for (int i = 1; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            int id = int.Parse(rowData[0]);

            csvDict[id] = rowData;
            csvList.Add(rowData);
        }
        
        const int maxParameters = 3;

        foreach (var rowData in csvList)
        {
            int id = int.Parse(rowData[0]);
            
            var effectTypeStr = rowData[3];
            var effectType = WeaponData.StringToEffectType(effectTypeStr);
            if(effectType == WeaponEffectType.None) continue;
            
            var weaponEffectData = new WeaponEffectData();
            List<WeaponEffectValues> effectValues = new();

            weaponEffectData.effectType = effectType;
            weaponEffectData.valuesList = effectValues;

            int currentColIdx = 5;//해당 WeaponEffectData 시트 참조. 파라미터 시작 열
            
            for (int i = 0; i < maxParameters; i++)
            {
                int valueIdx = currentColIdx + i * 3;
                if (valueIdx >= rowData.Length || string.IsNullOrEmpty(rowData[valueIdx]))
                    break;

                List<float> values = new();
                var strArr = rowData[valueIdx].Split('|');
                foreach (var str in strArr)
                {
                    if (float.TryParse(str, out var value))
                    {
                        values.Add(value);
                    }
                }
                var effectValue = new WeaponEffectValues();
                effectValue.values = values;

                var statStr = rowData[valueIdx + 1];
                var mainStat = statStr.StringToMainStats();
                var subStat = statStr.StringToSubStats();
                effectValue.mainStat = mainStat;
                effectValue.subStat = subStat;

                strArr = rowData[valueIdx + 2].Split('|');
                List<int> multipliers = new();
                foreach (var str in strArr)
                {
                    if (int.TryParse(str, out var multiplier))
                    {
                        multipliers.Add(multiplier);
                    }
                }
                effectValue.multipliers = multipliers;
                effectValues.Add(effectValue);
            }
            
           
            Debug.Log($"ID:{id} , effectValues count:{effectValues.Count}");
            
            if (effectDataDict.ContainsKey(id))
            {
                effectDataDict[id].Add(weaponEffectData);
            }
            else
            {
                List<WeaponEffectData> effectDataList = new() { weaponEffectData };
                effectDataDict[id] = effectDataList;
                
            }
        }
        int weaponEffectUpdateCount = 0;
        foreach (var weaponData in weapons)
        {
            if (effectDataDict.TryGetValue(weaponData.ID, out var effectDataList))
            {
#if UNITY_EDITOR
                Debug.Log(weaponData.Name + ' ' + effectDataList.Count);
                
                weaponData.SetWeaponEffectData(effectDataList);
                weaponEffectUpdateCount++;
                
                EditorUtility.SetDirty(weaponData);
#endif
            }
        }
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Weapon Effect Data Updated " + weaponEffectUpdateCount);
    }

    [ContextMenu("Sync WeaponClasses Bonus Data")]
    public void SyncWeaponClassBonusFromCSV()
    {
        string[] lines = weaponClassCSV.text.Split(new []{ '\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, string[]> csvDict = new();
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            string className = rowData[0];
            
            csvDict[className] = rowData;
        }

        List<WeaponClassBonusValue> effects = new();
        
        foreach (var (className, rowData) in csvDict)
        {
            //var mainStat = className.String
            var weaponClass = WeaponData.ToWeaponClass(className);
            if(weaponClass == WeaponClasses.None) continue;
            int startIdx = 1;
            int colsPerEffect = 6;
            int maxEffects = 3;
            
            List<WeaponClassBonus> effectStats = new();
            for (int i = 0; i < maxEffects; i++)
            {
                int statIdx = startIdx + i * colsPerEffect;
                if (statIdx >= rowData.Length || string.IsNullOrWhiteSpace(rowData[statIdx]))
                    break;

                string statString = rowData[statIdx].Trim();
                var effectStatValue = new WeaponClassBonus();
                
                if (statString.StringToMainStats() != MainStats.None)
                {
                    effectStatValue.mainStat =  statString.StringToMainStats();
                    effectStatValue.subStat = SubStats.None;
                }
                else
                {
                    if (statString.StringToSubStats() == SubStats.None) break;
                    effectStatValue.mainStat = MainStats.None;
                    effectStatValue.subStat = statString.StringToSubStats();
                }
                
                List<int> values = new();
                for (int v = 0; v < 5; v ++)//2단계~6단계
                {
                    int valueIdx = statIdx + v + 1;
                    if (valueIdx < rowData.Length && int.TryParse(rowData[valueIdx], out int value))
                    {
                        values.Add(value);
                    }
                    else
                    {
                        values.Add(0);
                    }
                }
                effectStatValue.values = values;
                
                effectStats.Add(effectStatValue);
            }

            var weaponClassValue = new WeaponClassBonusValue
            {
                weaponClass = weaponClass,
                statsValues = effectStats
            };
            
            effects.Add(weaponClassValue);
        }
        
#if UNITY_EDITOR
        classBonusData.SyncCSVData(effects);
        EditorUtility.SetDirty(classBonusData);
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Sync WeaponClass Data");
    }
}
