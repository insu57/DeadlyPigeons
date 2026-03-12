using System.Text;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    //WIP
    [SerializeField] private TMP_Text stageText;
    private PlayerSelected _playerSelected;
    
    private void Start()
    {
        _playerSelected = SceneChanger.Instance.PlayerSelected;
        
        //test
        var sb = new StringBuilder();
        
        var charData = DataManager.Instance.CharDict[_playerSelected.CharID];
        var weaponData = DataManager.Instance.WeaponDict[_playerSelected.WeaponIDList[0]];
        var stage = _playerSelected.StageID;
        
        sb.AppendLine($"Stage: {stage}");
        sb.AppendLine($"Character: {charData.CharacterName}");
        sb.AppendLine($"Weapon: {weaponData.Name}");
        
        stageText.SetText(sb);
    }
    
}
