using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    [Header("Main")] 
    [SerializeField] private Button startBtn;
    [SerializeField] private SceneName sceneNameToChange;
    [SerializeField] private Button optionBtn;
    [SerializeField] private Button quitBtn;
    [Header("Select")] 
    [SerializeField] private SelectWindow selectWindow;
    [Header("Option")]
    [SerializeField] private OptionWindow optionWindow;


    private void Start()
    {   
        //startBtn.onClick.AddListener(LoadScene);
        startBtn.onClick.AddListener(OpenSelectWindow);
        optionBtn.onClick.AddListener(OpenOptionWindow);
        quitBtn.onClick.AddListener(QuitGame);
        
        selectWindow.Window.SetActive(false);
        optionWindow.Window.SetActive(false);
    }

    private void OpenSelectWindow()
    {
        selectWindow.OpenSelectWindow();
        optionWindow.Window.SetActive(false);
    }

    private void OpenOptionWindow()
    {
        optionWindow.Window.SetActive(true);
        selectWindow.Window.SetActive(false);
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
