using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    //WIP
    private PlayerManager _playerManager;
    [SerializeField] private TMP_Text stageText;
    private PlayerSelected _playerSelected;
    
    //test
    [SerializeField] private CharacterData testChar;
    [SerializeField] private WeaponData testWeapon;
    [SerializeField] private int testStage;

    private void Awake()
    {
        _playerManager = FindFirstObjectByType<PlayerManager>();
    }
    
    private void Start()
    {
        InitStage();

        //test
        var sb = new StringBuilder();
        
        if (_playerSelected == null)
        {
            _playerSelected = new PlayerSelected
            {
                CharID = testChar.ID,
                WeaponIDList = new List<int>(),
                StageID = testStage
            };
            _playerSelected.WeaponIDList.Add(testWeapon.ID);
            
            
        }
        
        var charData = DataManager.Instance.CharDict[_playerSelected.CharID]; 
        var weaponData = DataManager.Instance.WeaponDict[_playerSelected.WeaponIDList[0]];
        var stage = _playerSelected.StageID;
        
        _playerManager.InitStat(charData);
        
        
        sb.AppendLine($"Stage: {stage}");
        sb.AppendLine($"Character: {charData.CharacterName}");
        sb.AppendLine($"Weapon: {weaponData.Name}");
        
        stageText.SetText(sb);
    }


    private void InitStage()
    {
        _playerSelected = SceneChanger.Instance.PlayerSelected;
        InputManager.Instance.Input.Player.Enable();
        InputManager.Instance.Input.UI.Disable();
        InputManager.Instance.Input.Global.Enable();
        Debug.Log("init stage");
    }
    
}
