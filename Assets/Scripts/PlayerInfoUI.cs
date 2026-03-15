using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;

    public void ShowInfoUI()
    {
        infoPanel.SetActive(true);
    }
}
