using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    [Header("Main")] 
    [SerializeField] private Button startBtn;
    [SerializeField] private SceneName sceneNameToChange;
    [SerializeField] private Button quitBtn;
    [Header("Select")] 
    [SerializeField] private SelectWindow selectWindow;
    [Header("Option")]
    [SerializeField] private OptionWindow optionWindow;


    private void Start()
    {   
        startBtn.onClick.AddListener(LoadScene);
        quitBtn.onClick.AddListener(QuitGame);
        
        selectWindow.Window.SetActive(false);
        optionWindow.Window.SetActive(false);
    }

    private void LoadScene()
    {
        SceneChanger.Instance.LoadScene(sceneNameToChange);
    }

    private void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
