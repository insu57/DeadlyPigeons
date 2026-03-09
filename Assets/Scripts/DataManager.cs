using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public Dictionary<int, CharacterData> CharDict { get; } = new();
    public List<CharacterData> CharList { get; } = new();
    public Dictionary<int, WeaponData> WeaponDict { get; } = new();
    public List<WeaponData> WeaponList { get; } = new();
    public Dictionary<MainStats, string> StatIcon { get; private set; } = new();
    
    private Dictionary<int, string> TierColorDict { get; } = new();
    private Dictionary<string, Color> HexToColor { get; } = new();
    
    protected override void Awake()
    {
        base.Awake();
        
        InitData();
        InitColor();
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
        
    }

    private void SetColorDict(string colorString)
    {
        if (ColorUtility.TryParseHtmlString(colorString, out var color))
        {
            HexToColor[colorString] = color;
        }
        else
        {
            HexToColor[colorString] = Color.white;
        }
    }

    public Color GetColor(string colorString)
    {
        return HexToColor.TryGetValue(colorString, out var color) ? color : Color.white;
    }
    
}

