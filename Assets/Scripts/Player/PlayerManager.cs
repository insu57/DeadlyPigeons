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
        Debug.Log(value);
        _playerInfoUI.UpdateMainStat(stat, value);
    }
}
