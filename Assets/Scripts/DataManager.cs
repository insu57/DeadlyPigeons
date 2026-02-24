using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public Dictionary<int, CharacterData> CharDict { get; } = new();
    public List<CharacterData> CharList { get; } = new();
    public Dictionary<int, WeaponData> WeaponDict { get; } = new();
    public List<WeaponData> WeaponList { get; } = new();
    

    protected override void Awake()
    {
        base.Awake();
        
        InitData();
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
}
