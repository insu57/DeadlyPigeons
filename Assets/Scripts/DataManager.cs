using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public Dictionary<int, CharacterData> CharDict { get; } = new();
    public List<CharacterData> CharList { get; } = new();
    public Dictionary<int, ItemData> ItemDict { get; } = new();
    public List<ItemData> ItemList { get; } = new();
    public Dictionary<int, WeaponData> WeaponDict { get; } = new();
    public List<WeaponData> WeaponList { get; } = new();
    public Dictionary<WeaponClasses, List<WeaponClassBonus>> WeaponClassBonusDict { get; } = new();
    //무기 클래스 - 클래스 별 보너스(각 스탯 리스트(스탯 - 보너스 수치))
    public Dictionary<MainStats, int[]> LevelUpStatsDict { get; } = new();
    public Dictionary<MainStats, Sprite> MainStatSpriteDict { get; } = new();
    
    public List<WaveData> WaveDataList { get; private set; }
    public Dictionary<CollectableType, Sprite> CollectableSpriteDict { get; } = new();
    [field:SerializeField] public Sprite PlaceHolderSprite { get; private set; }
    
    public Dictionary<int, string> TierColorDict { get; } = new();
    private Dictionary<string, Color> HexToColor { get; } = new();

    public int PlayerHitboxLayer {get; private set;}
    public int EnemyHitboxLayer  {get; private set;}
    
    
    protected override void Awake()
    {
        base.Awake();
        
        InitData();
        InitColor();
        
        PlayerHitboxLayer = LayerMask.NameToLayer("PlayerHitbox");
        EnemyHitboxLayer = LayerMask.NameToLayer("EnemyHitbox");
    }

    private void Start()
    {
        StatUtil.Initialize();
    }

    private void InitData()
    {
        //SO 데이터 불러오기.
        CharacterData[] characters = Resources.LoadAll<CharacterData>("Data/Characters");
        foreach (var charData in characters)
        {
            CharDict.Add(charData.ID, charData);
            CharList.Add(charData);
        }
        
        ItemData[] items = Resources.LoadAll<ItemData>("Data/Items");
        foreach (var itemData in items)
        {
            ItemDict.Add(itemData.ID, itemData);
            ItemList.Add(itemData);
        }
        
        WeaponData[] weapons = Resources.LoadAll<WeaponData>("Data/Weapons");
        foreach (var wData in weapons)
        {
            WeaponDict.Add(wData.ID, wData);
            WeaponList.Add(wData);
        }
        
        var weaponClassData = Resources.Load<WeaponClassBonusData>("Data/WeaponClassBonus");
        foreach (var weaponClassValue in weaponClassData.WeaponClassBonusValues)
        {
            var weaponClass = weaponClassValue.weaponClass;
            WeaponClassBonusDict[weaponClass] = weaponClassValue.statsValues;
        }
        
        var levelUpData = Resources.Load<LevelUpStat>("Data/LevelUpStat");
        foreach (var upgradeValue in levelUpData.MainStatUpgrades)
        {
            var mainStat = upgradeValue.mainStat;
            LevelUpStatsDict[mainStat] = upgradeValue.values;
            MainStatSpriteDict[mainStat] = upgradeValue.icon;
        }
        
        
        //WaveNumber 정렬 리스트
        WaveDataList = Resources.LoadAll<WaveData>("Data/Waves")
            .OrderBy(w => w.WaveNumber).ToList();

        var collectableSpriteData = Resources.Load<CollectableSpriteData>("Data/CollectableSpriteData");
        foreach (var collectableSprite in collectableSpriteData.CollectableSprites)
        {
            CollectableSpriteDict[collectableSprite.collectableType] = collectableSprite.sprite;
        }

        PlaceHolderSprite = Resources.Load<Sprite>("Sprites/PlaceHolder");
    }

    private void InitColor()
    {
        TierColorDict[1] = StatUtil.Tier1Color;
        TierColorDict[2] = StatUtil.Tier2Color;
        TierColorDict[3] = StatUtil.Tier3Color;
        TierColorDict[4] = StatUtil.Tier4Color;

       SetColorDict(StatUtil.Tier1Color);
       SetColorDict(StatUtil.Tier2Color);
       SetColorDict(StatUtil.Tier3Color);
       SetColorDict(StatUtil.Tier4Color);
       SetColorDict(StatUtil.DefaultWhite);
       SetColorDict(StatUtil.YellowColor);
       SetColorDict(StatUtil.RedColor);
       SetColorDict(StatUtil.GreenColor);
       SetColorDict(StatUtil.GrayColor);
    }

    private void SetColorDict(string colorString)
    {
        if (ColorUtility.TryParseHtmlString(colorString, out var color))
        {
            HexToColor[colorString] = color;
        }
        else
        {
            Debug.Log(colorString);
            HexToColor[colorString] = Color.white;
        }
    }

    public Color GetHexToColor(string colorString)
    {
        return HexToColor.TryGetValue(colorString, out var color) ? color : Color.white;
    }
    
}

