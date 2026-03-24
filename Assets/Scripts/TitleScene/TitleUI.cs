
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    private enum TitleState
    {
        Title = 0,
        Select = 1,
        Option = 2,
    }
    
    [Header("Main")] 
    private  TitleState _currentState;
    [SerializeField] private Button startBtn;
    [SerializeField] private SceneName sceneNameToChange;
    [SerializeField] private Button optionBtn;
    [SerializeField] private Button quitBtn;
    [Header("Select")] 
    [SerializeField] private SelectWindow selectWindow;
    [Header("Option")]
    [SerializeField] private OptionWindow optionWindow;

    private void Awake()
    {
        ObjectPoolingManager.Instance.InitSelectBtnPool();
    }

    private void Start()
    {   
        InputManager.Instance.Input.Global.Enable();
        InputManager.Instance.Input.UI.Enable();//패드 조작 추가?
        InputManager.Instance.Input.UI.Cancel.performed += OnCancelAction;
        
        _currentState = TitleState.Title;
        
        startBtn.onClick.AddListener(OpenSelectWindow);
        optionBtn.onClick.AddListener(OpenOptionWindow);
        quitBtn.onClick.AddListener(QuitGame);
        
        selectWindow.Window.SetActive(false);
        optionWindow.Window.SetActive(false);
    }

    private void OpenSelectWindow()
    {
        _currentState = TitleState.Select;
        selectWindow.OpenSelectWindow();
        optionWindow.Window.SetActive(false);
    }

    private void OpenOptionWindow()
    {
        _currentState = TitleState.Option;
        optionWindow.Window.SetActive(true);
        selectWindow.Window.SetActive(false);
    }

    private void OnCancelAction(InputAction.CallbackContext ctx)
    {
        Debug.Log("OnCancelAction");
        switch (_currentState)
        {
            case TitleState.Title:
                QuitGame();
                break;
            case TitleState.Select:
                selectWindow.OnBackBtnClick();
                break;
            case TitleState.Option:
                break;
        }
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
