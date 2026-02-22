using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private Dictionary<int, CharacterData> _charDict = new();
    private Dictionary<int, WeaponData> _weaponDict = new();

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
            _charDict.Add(charData.ID, charData);
        }
        
        WeaponData[] weapons = Resources.LoadAll<WeaponData>("Data/Weapons");
        foreach (var wData in weapons)
        {
            _weaponDict.Add(wData.ID, wData);
        }
    }
}
