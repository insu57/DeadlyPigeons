using System.Text;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    //WIP
    [SerializeField] private TMP_Text stageText;
    private PlayerSelected _playerSelected;
    
    //test
    [SerializeField] private CharacterData testChar;
    [SerializeField] private WeaponData testWeapon;
    [SerializeField] private int testStage;

    private void Start()
    {
        InitStage();

        CharacterData charData;
        WeaponData weaponData;
        int stage;
        
        //test
        var sb = new StringBuilder();
        
        if (_playerSelected == null)
        {
            charData = testChar;
            weaponData = testWeapon;
            stage = testStage;
        }
        else
        {
            charData = DataManager.Instance.CharDict[_playerSelected.CharID];
            weaponData = DataManager.Instance.WeaponDict[_playerSelected.WeaponIDList[0]];
            stage = _playerSelected.StageID;
        }
        
        sb.AppendLine($"Stage: {stage}");
        sb.AppendLine($"Character: {charData.CharacterName}");
        sb.AppendLine($"Weapon: {weaponData.Name}");
        
        stageText.SetText(sb);
    }


    private void InitStage()
    {
        _playerSelected = SceneChanger.Instance.PlayerSelected;
        InputManager.Instance.Input.Player.Enable();
    }
    
}
