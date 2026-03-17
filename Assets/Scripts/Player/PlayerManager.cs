using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PlayerControl _playerControl;
    private PlayerStat _playerStat;
    private PlayerInfoUI _playerInfoUI;

    private void Awake()
    {
        TryGetComponent(out _playerControl);
        TryGetComponent(out _playerStat);
        _playerInfoUI = FindFirstObjectByType<PlayerInfoUI>();
        
        _playerStat.OnChangeMainStats += UpdateMainStat;
        _playerStat.OnChangeSubStats += UpdateSubStat;
    }

    private void Start()
    {
        
    }

    public void InitStat(CharacterData charData)
    {
        Debug.Log("InitStat");
        _playerStat.InitStat(charData);
    }

    private void UpdateMainStat(MainStats stat, int value)
    {
        _playerInfoUI.UpdateMainStat(stat, value);
    }

    private void UpdateSubStat(SubStats stat, int value)
    {
        _playerInfoUI.UpdateSubStat(stat, value);
    }
}
