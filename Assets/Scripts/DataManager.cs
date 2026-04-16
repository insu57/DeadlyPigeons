using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public Dictionary<int, CharacterData> CharDict { get; } = new();
    public List<CharacterData> CharList { get; } = new();
    public Dictionary<int, WeaponData> WeaponDict { get; } = new();
    public List<WeaponData> WeaponList { get; } = new();
    public Dictionary<WeaponClasses, List<WeaponClassBonus>> WeaponClassBonusDict { get; } = new();
    //무기 클래스 - 클래스 별 보너스(각 스탯 리스트(스탯 - 보너스 수치))
    
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
        CharacterData[] characters = Resources.LoadAll<CharacterData>("Data/Characters");
        foreach (var charData in characters)
        {
            CharDict.Add(charData.ID, charData);
            CharList.Add(charData);
        }
        
        WeaponData[] weapons = Resources.LoadAll<WeaponData>("Data/Weapons");
        foreach (var wData in weapons)
        {
            WeaponDict.Add(wData.ID, wData);
            WeaponList.Add(wData);
        }

        var weaponClassData = Resources.LoadAll<WeaponClassBonusData>("")[0];
        foreach (var weaponClassValue in weaponClassData.WeaponClassBonusValues)
        {
            var weaponClass = weaponClassValue.weaponClass;
            WeaponClassBonusDict[weaponClass] = weaponClassValue.statsValues;
        }
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

