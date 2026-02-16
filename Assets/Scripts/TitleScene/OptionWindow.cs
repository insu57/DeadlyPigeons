using UnityEngine;
using UnityEngine.UI;

public class OptionWindow : MonoBehaviour
{
    [SerializeField] private GameObject window;
    [SerializeField] private Button closeBtn;
    
    public GameObject Window => window;
    
    private void Start()
    {
        closeBtn.onClick.AddListener(CloseWindow);
    }

    private void CloseWindow()
    {
        window.SetActive(false);
    }
}
