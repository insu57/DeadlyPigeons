using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Selected
{
    public int CharID;
    public List<int> WeaponIDList;
    public int StageID;//?
}

public class SelectWindow : MonoBehaviour
{
    private enum SelectWindowState
    {
        CharSelect = 0,
        WeaponSelect = 1,
        StageSelect = 2
    }
    
    [field: SerializeField] public GameObject Window { get; private set; }
    [SerializeField] private Button backButton;
    private Dictionary<SelectWindowState, GameObject> _selectWindowDict = new();
    private Dictionary<SelectWindowState, GameObject> _selectPanelDict = new();
    public Selected PlayerSelected { get; private set; }
    
    private SelectWindowState _currentState;
    StringBuilder sb = new();
    
    [Header("Char Select")]
    [SerializeField] private GameObject charSelect;
    [SerializeField] private Transform charViewportContent;

    [Header("Weapon Select")] 
    [SerializeField] private GameObject weaponSelect;
    [SerializeField] private Transform weaponViewportContent;
    private SelectButton _randomWeaponBtn;
    private List<SelectButton> _weaponSelectList = new();
    [SerializeField] private GameObject weaponPanel;
    
    [Header("Stage Select")]
    [SerializeField] private GameObject stageSelect;
    [SerializeField] private Transform stageViewportContent;
    [SerializeField] private GameObject stagePanel;
    
    [Header("Char Description")] 
    [SerializeField] private Image charImage;
    [SerializeField] private TMP_Text charName;
    [SerializeField] private TMP_Text charHealth;

    [Header("Weapon Description")]
    [SerializeField] private Image weaponImg;
    [SerializeField] private TMP_Text weaponName;
    [SerializeField] private TMP_Text weaponDescription;
    
    [Header("Prefabs")]
    [SerializeField] private Sprite randomSprite;
    [SerializeField] private SelectButton selectButton; //Prefab

    
    
    private void Awake()
    {
        _selectWindowDict.Add(SelectWindowState.CharSelect, charSelect);
        _selectWindowDict.Add(SelectWindowState.WeaponSelect, weaponSelect);
        _selectWindowDict.Add(SelectWindowState.StageSelect, stageSelect);
        
        _selectPanelDict.Add(SelectWindowState.CharSelect, null);
        _selectPanelDict.Add(SelectWindowState.WeaponSelect, weaponPanel);
        _selectPanelDict.Add(SelectWindowState.StageSelect, stagePanel);
    }

    private void Start()
    {
        backButton.onClick.AddListener(OnBackBtnClick);
        
        InitCharSelectBtn();
        
        _randomWeaponBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        _randomWeaponBtn.transform.SetParent(weaponViewportContent);
        
        PlayerSelected = new Selected();

        weaponDescription.text = $"<sprite name=\"elemental_icon\">";

    }

    public void OpenSelectWindow()
    {
        Window.SetActive(true);
        _currentState = SelectWindowState.CharSelect;
        
        charSelect.SetActive(true);
        weaponSelect.SetActive(false);
        stageSelect.SetActive(false);
    }

    private void SwitchWindow(SelectWindowState nextState)
    {
        _selectWindowDict[_currentState].SetActive(false);
        _selectWindowDict[nextState].SetActive(true);
        
        var panel = _selectPanelDict[_currentState];
        panel?.gameObject.SetActive(false);
        panel = _selectPanelDict[nextState];
        panel?.gameObject.SetActive(true);
        
        _currentState = nextState;
    }
    
    public void OnBackBtnClick()//개선 방안 필요.
    {
        switch (_currentState)
        {
            case SelectWindowState.CharSelect:
            {
                Window.SetActive(false);
                break;
            }
            case SelectWindowState.WeaponSelect:
            {
                SwitchWindow(SelectWindowState.CharSelect);
                break;
            }
            case SelectWindowState.StageSelect:
            {
                SwitchWindow(SelectWindowState.WeaponSelect);
                break;
            }
            default: return;
        }
    }
    
    private void InitCharSelectBtn()
    {
        //랜덤 캐릭터 버튼 추가 필요
        var randomBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        randomBtn.transform.SetParent(charViewportContent);
        randomBtn.OnBtnPointerEnter += ShowRandCharDescription;
        randomBtn.SelectBtn.onClick.AddListener(SelectRandomCharacter);
        
        foreach (var (id, charData) in DataManager.Instance.CharDict)
        {
            var newBtn = ObjectPoolingManager.Instance.GetSelectBtn();
            newBtn.transform.SetParent(charViewportContent);
            
            newBtn.SetButtonImg(charData.CharacterSprite);
           
            newBtn.OnBtnPointerEnter += () => ShowCharDescription(id);

            newBtn.SelectBtn.onClick.AddListener(() => EnterWeaponSelect(id));
        }
        
    }

    private void ShowCharDescription(int charID)
    {
        var charData = DataManager.Instance.CharDict[charID];
        charImage.sprite = charData.CharacterSprite;
        charName.text = charData.CharacterName;
        //charHealth.text = charData.CharMainStats.maxHealth.ToString(CultureInfo.CurrentCulture);
    }

    private void ShowWeaponList(int charID) //개선필요!
    {
        foreach (var selectBtn in _weaponSelectList)
        {
            ObjectPoolingManager.Instance.ReleaseSelectBtn(selectBtn);
            //초과하는 버튼은 이벤트 구독 해제...
        }
        
        _weaponSelectList.Clear();

        foreach (var weaponID in DataManager.Instance.CharDict[charID].InitWeaponIDList)
        {
            var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
            
            selectBtn.transform.SetParent(weaponViewportContent);
            
            selectBtn.SetButtonImg(DataManager.Instance.WeaponDict[weaponID].Sprite);
            
            _weaponSelectList.Add(selectBtn);
        }
    }

    private void ShowRandCharDescription()
    {
        //Localization 주의.
        charImage.sprite = randomSprite;
        charName.text = "랜덤";
        charHealth.text = "?";
    }

    private void SelectRandomCharacter()
    {
        var randIdx = Random.Range(0, DataManager.Instance.CharList.Count);
        var charData = DataManager.Instance.CharList[randIdx];
        charImage.sprite = charData.CharacterSprite;
        charName.text = charData.CharacterName;
        //charHealth.text = charData.CharMainStats.maxHealth.ToString(CultureInfo.CurrentCulture);
        
        EnterWeaponSelect(charData.ID);
    }

    private void EnterWeaponSelect(int charID)
    {
        ShowCharDescription(charID);
        PlayerSelected.CharID = charID; //캐릭터 선택 완료.
        
        SwitchWindow(SelectWindowState.WeaponSelect);
        
        //WEAPON...!
        ShowWeaponList(charID);
    }

    private void ShowWeaponDescription(int weaponID)
    {
        var weaponData = DataManager.Instance.WeaponDict[weaponID];

        sb.Clear();

        weaponName.text = weaponData.Name;
        
    }
    
    
    private void SelectRandomWeapon()
    {
        
    }

    private void EnterStageSelect(int charID, int weaponID)
    {
        
    }
}
