using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    [SerializeField] private TextAsset itemEffectCSV;
    [SerializeField] private TextAsset weaponCSV;
    [SerializeField] private TextAsset weaponClassCSV;
    [SerializeField] private TextAsset weaponEffectCSV;
    [SerializeField] private TextAsset levelUpUpgradeCSV;
    [SerializeField] private TextAsset enemyStatCSV;
    [SerializeField] private TextAsset enemyStateCSV;
    [SerializeField] private TextAsset enemyTransitionCSV;
    [SerializeField] private TextAsset waveCSV;
    [SerializeField] private TextAsset waveEnemyCSV;
    
    [SerializeField] private Sprite placeHolder;
    private const string CharSpritePath = "Sprites/Characters/";
    private const string ItemSpritePath = "Sprites/Items/";
    private const string WeaponSpritePath = "Sprites/Weapons/";
    private const string EnemySpritePath = "Sprites/Enemies/";
    private const string ProjectileSpritePath = "Sprites/Projectiles/";
    
    [SerializeField] private WeaponClassBonusData classBonusData;
    [SerializeField] private LevelUpStat levelUpStat;
    
    [ContextMenu("Sync Character Data")]
    public void SyncCharDataFromCSV()
    {
        if (!characterCSV)
        {
            Debug.LogError("Character CSV Error : null");
            return;
        }
        
        CharacterData[] characters = Resources.LoadAll<CharacterData>("Data/Characters");
        Sprite[] sprites = Resources.LoadAll<Sprite>(CharSpritePath);
        Dictionary<int, Sprite> spriteDict = new();
        foreach (var sprite in sprites)
        {
            if (int.TryParse(sprite.name, out int id))
            {
                spriteDict[id] = sprite;
            }
            else
            {
                Debug.LogWarning("Character Sprite is Not INT type. : " + sprite.name);
            }
        }
        
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

                var charSprite = spriteDict.GetValueOrDefault(so.ID, placeHolder);

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
        ItemData[] passives = Resources.LoadAll<ItemData>("Data/Characters/Passive");
        Sprite[] charSprites = Resources.LoadAll<Sprite>(CharSpritePath);
        Sprite[] itemSprites = Resources.LoadAll<Sprite>(ItemSpritePath);
        Dictionary<int, Sprite> spriteDict = new();
        
        foreach (var sprite in charSprites)
        {
            if (int.TryParse(sprite.name, out int id))
            {
                spriteDict[id] = sprite;
            }
            else
            {
                Debug.LogWarning("Character Sprite is Not INT type. : " + sprite.name);
            }
        }

        foreach (var sprite in itemSprites)
        {
            if (int.TryParse(sprite.name, out int id))
            {
                spriteDict[id] = sprite;
            }
            else
            {
                Debug.LogWarning("Item Sprite is Not INT type. : " + sprite.name);
            }
        }
        
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


        foreach (var so in passives)
        {
            bool isFound = false;

            ItemStat itemStat = new();
            if (passiveDict.TryGetValue(so.ID, out var rowData))
            {
                isFound = true;
                
                itemStat.icon = spriteDict.GetValueOrDefault(so.ID, placeHolder);
                
                itemStat.itemName =  rowData[1];
                itemStat.tier = 1; //패시브는 티어 1로
                itemStat.itemClass = ItemClass.Character;
                int idx = 2;
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
            
            if (isFound)
            {
                
#if UNITY_EDITOR
                // TODO: 파싱된 데이터를 기반으로 so.SyncItemData 호출
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
        
        
        
        foreach (var so in items)
        {
            bool isFound = false;

            ItemStat itemStat = new();
            
            if (itemDict.TryGetValue(so.ID, out var rowData))
            {
                isFound = true;
               
                itemStat.icon = spriteDict.GetValueOrDefault(so.ID, placeHolder);
                
                itemStat.itemName = rowData[1];
                itemStat.tier = int.Parse(rowData[2]);
                string itemClassString = rowData[3];
                itemStat.itemClass = ItemData.ToItemClass(itemClassString);
                
                int idx = 4;
                List<StatAmount> values = new();
                for (int i = 0; i < 5; i++)
                {
                    int curIdx = idx + i * 2;
                    if(curIdx >= rowData.Length || string.IsNullOrEmpty(rowData[curIdx]))
                        break;
                    
                    var statStr = rowData[curIdx];
                    var mainStat = statStr.StringToMainStats();
                    var subStat = statStr.StringToSubStats();
                    Debug.Log(rowData[curIdx+1]);
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
                itemStat.description = rowData[14];
                itemStat.max = int.Parse(rowData[15]);
                itemStat.price = int.Parse(rowData[16]);
            }

            if (isFound)
            {
                
#if UNITY_EDITOR
                // TODO: 파싱된 데이터를 기반으로 so.SyncItemData 호출
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
        Debug.Log("Item Data Updated: " + itemUpdateCount + "/" + (items.Length + passives.Length));
    }

    [ContextMenu("Sync Item Effect Data")]
    public void SyncItemEffectFromCSV()
    {
        if (!itemEffectCSV)
        {
            Debug.LogError("ItemEffect CSV Error : null");
            return;
        }

        ItemData[] items = Resources.LoadAll<ItemData>("Data/Items");
        ItemData[] passives = Resources.LoadAll<ItemData>("Data/Characters/Passive");
        Dictionary<int, ItemData> itemDataDict = new();
        
        foreach (var item in items)
        {
            itemDataDict[item.ID] = item;
        }
        
        foreach (var passive in passives)
        {
            
            itemDataDict[passive.ID] = passive;
        }

        string[] lines = itemEffectCSV.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, List<ItemEffect>> effectDataDict = new(); //id : effect list

        const int effectTypeIdx = 2; //효과 타입 열
        const int descriptionIdx = 4;//설명 포맷 열 (Effect)
        const int valuesIdx = 5;     //values 열 ('|'로 구분)
        const int statStartIdx = 6;  //스탯 시작 열(순서대로 나열, 6~9)
        const int maxStats = 4;      //효과당 최대 참조 스탯 수

        //0행: 헤더, 1행: 열 인덱스 → 실제 데이터는 2행부터
        for (int i = 2; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');

            if (!int.TryParse(rowData[0], out int id)) //빈 줄/잘못된 줄 스킵
                continue;

            var effectType = ItemData.StringToEffectType(rowData[effectTypeIdx]);
            if (effectType == ItemEffectType.None) continue;

            //values('|'로 구분, float 리스트)
            List<float> values = new();
            var valueStrArr = rowData[valuesIdx].Split('|');
            foreach (var str in valueStrArr)
            {
                if (float.TryParse(str, out var value))
                {
                    values.Add(value);
                }
            }

            //설명 포맷: [n](value) / {n}(stat) 자리표시자로 분할(브래킷 보존)
            var descriptionFormat = rowData[descriptionIdx];
            var description = Regex.Split(descriptionFormat, @"(\[\d+\]|\{\d+\})");

            //스탯 파싱(순서 보존). 메인/서브 중 매칭되는 쪽만 유효, 나머지는 None
            List<StatRef> stats = new();
            for (int s = 0; s < maxStats; s++)
            {
                int statIdx = statStartIdx + s;
                if (statIdx >= rowData.Length || string.IsNullOrEmpty(rowData[statIdx]))
                    break;

                var statStr = rowData[statIdx];
                var statRef = new StatRef
                {
                    mainStat = statStr.StringToMainStats(),
                    subStat = statStr.StringToSubStats(),
                };
                stats.Add(statRef);
            }

            var itemEffect = new ItemEffect
            {
                effectType = effectType,
                values = values,
                stats = stats,
                description = description,
            };

            if (effectDataDict.ContainsKey(id))
            {
                effectDataDict[id].Add(itemEffect);
            }
            else
            {
                effectDataDict[id] = new List<ItemEffect> { itemEffect };
            }
        }

        int itemEffectUpdateCount = 0;
        foreach (var (id, effectList) in effectDataDict)
        {
            if (itemDataDict.TryGetValue(id, out var itemData))
            {
#if UNITY_EDITOR
                itemData.SetItemEffectData(effectList);
                itemEffectUpdateCount++;
                EditorUtility.SetDirty(itemData);
#endif
            }
            else
            {
                Debug.LogError("ItemEffect : ItemData Not Found : " + id);
            }
        }

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Item Effect Data Updated " + itemEffectUpdateCount);
    }

    [ContextMenu("Sync Weapon Data")]
    public void SyncWeaponDataFromCSV()
    {
        WeaponData[] weapons = Resources.LoadAll<WeaponData>("Data/Weapons");
        Sprite[] weaponSprites = Resources.LoadAll<Sprite>(WeaponSpritePath);
        Dictionary<int, Sprite>  weaponSpriteDict = new();
        Sprite[] projectileSprites = Resources.LoadAll<Sprite>(ProjectileSpritePath);
        Dictionary<int, Sprite> projectileSpriteDict = new();
        
        foreach (var sprite in weaponSprites)
        {
            if (int.TryParse(sprite.name, out int id))
            {
                weaponSpriteDict[id] = sprite;
            }
            else
            {
                Debug.LogWarning("Weapon Sprite is Not INT type. : " + sprite.name);
            }
        }

        foreach (var sprite in projectileSprites)
        {
            if (int.TryParse(sprite.name, out int id))
            {
                projectileSpriteDict[id] = sprite;
            }
        }
        
        string[] lines = weaponCSV.text.Split(new []{ '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
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
                var weaponSprite = weaponSpriteDict.GetValueOrDefault(so.ID, placeHolder);
                
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


                int.TryParse(rowData[18], out var projectileID);
                projectileSpriteDict.TryGetValue(projectileID, out var projectileSprite);
                var projectileSpriteScale = new Vector2();
                var projectileColliderSize = new Vector2();
                strArr = rowData[19].Split('|');
                if (strArr.Length > 1)
                {
                    projectileSpriteScale.x = float.Parse(strArr[0]);
                    projectileSpriteScale.y = float.Parse(strArr[1]);
                }
                strArr = rowData[20].Split('|');
                if (strArr.Length > 1)
                {
                    projectileColliderSize.x = float.Parse(strArr[0]);
                    projectileColliderSize.y = float.Parse(strArr[1]);
                }

                var sfxStr = rowData[21];
                var sfxType = AudioManager.StringToSfxType(sfxStr);
                
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
                    projectileSprite = projectileSprite,
                    projectileSpriteScale = projectileSpriteScale,
                    projectileColliderSize = projectileColliderSize,
                };
                
#if UNITY_EDITOR
                
                so.SyncDataCSV(weaponName, parsed, weaponSprite, sfxType);

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
        //Dictionary<int, string[]> csvDict = new();
        List<string[]> csvList = new();
        Dictionary<int, List<WeaponEffectData>> effectDataDict = new(); //id : effectData list
        for (int i = 1; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            int id = int.Parse(rowData[0]);

            //csvDict[id] = rowData;
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

            var descriptionFormat = rowData[5]; //효과 설명
            var parsedChunk = Regex.Split(descriptionFormat, @"\{(\d+)\}"); //{인덱스}로 분할
            
            
            weaponEffectData.effectDescription = parsedChunk;
            
            int currentColIdx = 6;//해당 WeaponEffectData 시트 참조. 파라미터 시작 열
            
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

    [ContextMenu("Sync LevelUp Upgrades")]
    public void SyncLevelUpUpgrades()
    {
        string[] lines = levelUpUpgradeCSV.text.Split(new []{ '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, string[]> csvDict = new();
        Dictionary<MainStats, Sprite> mainStatIcon = new();
        var icons = Resources.LoadAll<Sprite>("Sprites/MainStat");
        foreach (var icon in icons)
        {
            var iconName = icon.name;
            var mainStat = iconName.StringToMainStats();
            if (mainStat == MainStats.None) continue;
            mainStatIcon[mainStat] = icon;
        }
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            string stat = rowData[0];
            csvDict[stat] = rowData;
        }
        
        List<MainStatUpgradeValue> upgrades = new();

        foreach (var (stat, rowData) in csvDict)
        {
            var mainStat = stat.StringToMainStats();
            if(mainStat == MainStats.None) continue;
            var values = new int[4];
            int startIdx = 1;
            int maxTier = 4;
            for (int i = 0; i < maxTier; i++)
            {
                values[i] = int.Parse(rowData[startIdx + i].Trim());
            }

            var mainStatValue = new MainStatUpgradeValue
            {
                icon =  mainStatIcon[mainStat],
                mainStat = mainStat,
                values = values
            };
            upgrades.Add(mainStatValue);
        }
#if UNITY_EDITOR
        levelUpStat.SetUpgrades(upgrades);
        EditorUtility.SetDirty(levelUpStat);
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Sync LevelUp Upgrades");
    }

    [ContextMenu("Sync Enemy Stat Data")]
    public void SyncEnemyStatDataFromCSV()
    {
        var enemies = Resources.LoadAll<EnemyData>("Data/Enemies");
        string[] lines = enemyStatCSV.text.Split(new []{ '\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> csvDict = new();
        Sprite[] sprites = Resources.LoadAll<Sprite>(EnemySpritePath);
        Dictionary<int, Sprite> spriteDict = new();
        Sprite[] projectileSprites = Resources.LoadAll<Sprite>(ProjectileSpritePath);
        Dictionary<int, Sprite> projectileSpriteDict = new();

        foreach (var sprite in sprites)
        {
            if (int.TryParse(sprite.name, out int id))
            {
                spriteDict[id] = sprite;
            }
        }

        foreach (var sprite in projectileSprites)
        {
            if (int.TryParse(sprite.name, out int id))
            {
                projectileSpriteDict[id] = sprite;
            }
        }
        
        for (int i = 2; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            if (int.TryParse(rowData[0], out int id))
            {
                csvDict[id] = rowData;
            }
        }

        int enemyUpdateCount = 0;

        foreach (var enemyData in enemies)
        {
            if (csvDict.TryGetValue(enemyData.ID, out var rowData))
            {
                var enemyStat = new EnemyStat
                {
                    enemyName = rowData[1],
                    baseHealth = int.Parse(rowData[3]),
                    healthPerWave =  float.Parse(rowData[4]),
                    baseDamage = int.Parse(rowData[5]),
                    damagePerWave =  float.Parse(rowData[6]),
                    baseSpeed = float.Parse(rowData[7]),
                    knockbackResistance = float.Parse(rowData[8]),
                    materialsDrop = int.Parse(rowData[9]),
                    meatDropChance = int.Parse(rowData[10]),
                    lootCrateDropChance =  int.Parse(rowData[11]),
                    initWave = int.Parse(rowData[12]),
                };

                if (int.TryParse(rowData[14], out var projectileID))
                {
                    if (projectileSpriteDict.TryGetValue(projectileID, out var projectileSprite))
                    {
                        enemyStat.projectileSprite = projectileSprite;
                        
                        var spriteScaleRaw = rowData[15].Split('|');
                        if (spriteScaleRaw.Length >= 2)
                        {
                            var spriteScale = new Vector2(float.Parse(spriteScaleRaw[0]), float.Parse(spriteScaleRaw[1]));
                            enemyStat.projectileSpriteScale = spriteScale;
                        }
                        var colliderSizeRaw =  rowData[16].Split('|');
                        if (colliderSizeRaw.Length >= 2)
                        {
                            var colliderSize = new Vector2(float.Parse(colliderSizeRaw[0]), float.Parse(colliderSizeRaw[1]));
                            enemyStat.projectileColliderSize = colliderSize;
                        }
                    }
                }
                
                var isBoss = bool.Parse(rowData[17]);
                enemyStat.isBoss = isBoss;
                
                var colliderOffsetRaw =  rowData[18].Split('|');
                if (colliderOffsetRaw.Length >= 2)
                {
                    var colliderOffset =  new Vector2(float.Parse(colliderOffsetRaw[0]), float.Parse(colliderOffsetRaw[1]));
                    enemyStat.colliderOffset = colliderOffset;
                }
                var enemyColliderSizeRaw =  rowData[19].Split('|');
                if (enemyColliderSizeRaw.Length >= 2)
                {
                    var enemyColliderSize = new Vector2(float.Parse(enemyColliderSizeRaw[0]),
                        float.Parse(enemyColliderSizeRaw[1]));
                    enemyStat.colliderSize = enemyColliderSize;
                }

                var sprite = spriteDict.GetValueOrDefault(enemyData.ID, placeHolder);
                
#if UNITY_EDITOR
                enemyData.SyncEnemyStat(sprite, enemyStat);

                enemyUpdateCount++;

                EditorUtility.SetDirty(enemyData);
#endif
            }
            else
            {
                Debug.LogError("Enemy Data Not Found");
            }
        }
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Enemy Data Updated " + enemyUpdateCount);
        
    }

    [ContextMenu("Sync Enemy State Data")]
    public void SyncEnemyStateDataFromCSV()
    {
        EnemyData[] enemies = Resources.LoadAll<EnemyData>("Data/Enemies");
        string[] lines = enemyStateCSV.text.Split(new []{ '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> csvList = new();
        Dictionary<int, List<EnemyStateParameter>> parameterDict = new();
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            csvList.Add(rowData);
        }

        foreach (var rowData in csvList)
        {
            int id =  int.Parse(rowData[0]);

            var stateParameter = new EnemyStateParameter
            {
                moveState = EnemyData.ParseEnemyStateType(rowData[2]),
                shootState = EnemyData.ParseShootStateType(rowData[5])
            };
            
            List<float> moveStateParameter = new();
            var strArr = rowData[4].Split('|');
            foreach (var str in strArr)
            {
                if (float.TryParse(str, out var value))
                {
                    moveStateParameter.Add(value);
                }
            }
            stateParameter.moveParameters = moveStateParameter;

            List<float> shootStateParameter = new();
            strArr = rowData[7].Split('|');
            foreach (var str in strArr)
            {
                if (float.TryParse(str, out var value))
                {
                    shootStateParameter.Add(value);
                }
            }
            stateParameter.shootParameters = shootStateParameter;

            if (parameterDict.ContainsKey(id))
            {
                parameterDict[id].Add(stateParameter);
            }
            else
            {
                List<EnemyStateParameter> parameter = new() { stateParameter };
                parameterDict[id] = parameter;
            }

            int enemyStateParameterUpdateCount = 0;
            foreach (var enemyData in enemies)
            {
                if (parameterDict.TryGetValue(enemyData.ID, out var parameterList))
                {
#if UNITY_EDITOR
                    enemyData.SyncEnemyStateParameter(parameterList);
                    enemyStateParameterUpdateCount++;
                    
                    EditorUtility.SetDirty(enemyData);
#endif
                }
            }
#if UNITY_EDITOR
            AssetDatabase.SaveAssets(); 
#endif
            Debug.Log("Enemy State Parameters Updated " + enemyStateParameterUpdateCount);
        }
    }

    [ContextMenu("Sync Enemy Transition Condition")]
    public void SyncEnemyTransitionFromCSV()
    {
        EnemyData[] enemies = Resources.LoadAll<EnemyData>("Data/Enemies");
        string[] lines = enemyTransitionCSV.text.Split(new []{ '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
        List<string[]> csvList = new();
        Dictionary<int, List<StateTransition>> transitionDict = new();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            
            csvList.Add(rowData);
        }

        foreach (var rowData in csvList)
        {
            int id = int.Parse(rowData[0]);

            var transition = new StateTransition
            {
                condition = EnemyData.ParseTransitionCondition(rowData[2]),
                threshold = float.Parse(rowData[4]),
                targetState = EnemyData.ParseEnemyStateType(rowData[5]),
            };

            if (transitionDict.ContainsKey(id))
            {
                transitionDict[id].Add(transition);
            }
            else
            {
                List<StateTransition> condition = new() { transition };
                transitionDict[id] = condition;
            }

            int enemyTransitionUpdateCount = 0;
            foreach (var enemyData in enemies)
            {
                if (transitionDict.TryGetValue(enemyData.ID, out var conditionList))
                {
#if UNITY_EDITOR
                    enemyData.SyncEnemyStateTransition(conditionList);
                    enemyTransitionUpdateCount++;
                    EditorUtility.SetDirty(enemyData);
#endif
                }
            }
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
            Debug.Log("Enemy State Transition Updated: " + enemyTransitionUpdateCount);
        }
    }

    [ContextMenu("Sync Wave Data")]
    public void SyncWaveDataFromCSV()
    {
        if (!waveCSV)
        {
            Debug.LogError("Wave CSV Error : null");
            return;
        }

        WaveData[] waves = Resources.LoadAll<WaveData>("Data/Waves");
        string[] lines = waveCSV.text.Split(new []{ '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, string[]> csvDict = new();

        // 0번 줄: 헤더, 1번 줄: 열 인덱스 → 실제 데이터는 2번부터
        for (int i = 2; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');
            if (int.TryParse(rowData[0], out int waveNumber))
            {
                csvDict[waveNumber] = rowData;
            }
        }

        int waveUpdateCount = 0;
        foreach (var so in waves)
        {
            if (csvDict.TryGetValue(so.WaveNumber, out var rowData))
            {
                int waveLength = int.Parse(rowData[1]);
                float spawnTick = float.Parse(rowData[2]);
                int spawnPerTick = int.Parse(rowData[3]);

                // 적 리스트는 WaveEnemy CSV(SyncWaveEnemyFromCSV)에서 채우므로 기존 값 유지
                var enemies = so.Enemies ?? new List<EnemySpawnInfo>();
                var bossEnemies = so.BossEnemies ?? new List<EnemyData>();

#if UNITY_EDITOR
                so.SyncWaveData(waveLength, enemies, bossEnemies, spawnTick, spawnPerTick);

                waveUpdateCount++;

                EditorUtility.SetDirty(so);
#endif
            }
            else
            {
                Debug.LogError("Wave Data Not Found : " + so.WaveNumber);
            }
        }

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Wave Data Updated " + waveUpdateCount + "/" + waves.Length);
    }

    [ContextMenu("Sync Wave Enemy From CSV")]
    public void SyncWaveEnemyFromCSV()
    {
        if (!waveEnemyCSV)
        {
            Debug.LogError("WaveEnemy CSV Error : null");
            return;
        }

        WaveData[] waves = Resources.LoadAll<WaveData>("Data/Waves");
        EnemyData[] enemies = Resources.LoadAll<EnemyData>("Data/Enemies");
        Dictionary<int, EnemyData> enemyDict = new();
        foreach (var enemy in enemies)
        {
            enemyDict[enemy.ID] = enemy;
        }

        string[] lines = waveEnemyCSV.text.Split(new []{ '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

        // 웨이브 번호별 스폰 적/보스 리스트 구성
        Dictionary<int, List<EnemySpawnInfo>> spawnInfoDict = new();
        Dictionary<int, List<EnemyData>> bossEnemyDict = new();

        // 0번 줄: 헤더, 1번 줄: 열 인덱스 → 실제 데이터는 2번부터
        for (int i = 2; i < lines.Length; i++)
        {
            string[] rowData = lines[i].Split(',');

            if (!int.TryParse(rowData[0], out int waveNumber)) //빈 줄/잘못된 줄 스킵
                continue;
            if (!int.TryParse(rowData[2], out int enemyID))
                continue;

            if (!enemyDict.TryGetValue(enemyID, out var enemyData))
            {
                Debug.LogError("WaveEnemy : EnemyData Not Found : " + enemyID);
                continue;
            }

            bool isBoss = bool.Parse(rowData[3]);
            if (isBoss)
            {
                if (bossEnemyDict.ContainsKey(waveNumber))
                {
                    bossEnemyDict[waveNumber].Add(enemyData);
                }
                else
                {
                    bossEnemyDict[waveNumber] = new List<EnemyData> { enemyData };
                }
            }
            else
            {
                float weight = float.Parse(rowData[4]);
                bool isNearSpawn = bool.Parse(rowData[5]);
                var spawnInfo = new EnemySpawnInfo
                {
                    enemyData = enemyData,
                    weight = weight,
                    spawnLocation = isNearSpawn ? SpawnLocationType.Near : SpawnLocationType.Far,
                };

                if (spawnInfoDict.ContainsKey(waveNumber))
                {
                    spawnInfoDict[waveNumber].Add(spawnInfo);
                }
                else
                {
                    spawnInfoDict[waveNumber] = new List<EnemySpawnInfo> { spawnInfo };
                }
            }
        }

        int waveEnemyUpdateCount = 0;
        foreach (var so in waves)
        {
            var spawnInfos = spawnInfoDict.GetValueOrDefault(so.WaveNumber, new List<EnemySpawnInfo>());
            var bossEnemies = bossEnemyDict.GetValueOrDefault(so.WaveNumber, new List<EnemyData>());

            // 웨이브 단위 값(WaveLength/SpawnTick/SpawnPerTick)은 Wave CSV에서 채우므로 기존 값 유지
#if UNITY_EDITOR
            so.SyncWaveData((int)so.WaveLength, spawnInfos, bossEnemies, so.SpawnTick, so.SpawnPerTick);

            waveEnemyUpdateCount++;

            EditorUtility.SetDirty(so);
#endif
        }

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
        Debug.Log("Wave Enemy Data Updated " + waveEnemyUpdateCount + "/" + waves.Length);
    }
    
}
