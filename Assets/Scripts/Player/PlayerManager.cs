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
    }

    private void Start()
    {
        _playerControl.OnShowInfoUI += ShowInfoUI;
    }

    public void InitStat(CharacterData charData)
    {
        _playerStat.InitStat(charData);
    }

    public void ShowInfoUI()
    {
        _playerInfoUI.ShowInfoUI();
    }
}
