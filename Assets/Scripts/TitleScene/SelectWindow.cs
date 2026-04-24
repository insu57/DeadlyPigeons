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

public class PlayerSelected
{
    public int CharID;
    public List<int> ItemIDList = new();
    public List<int> WeaponIDList = new();
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
    private readonly Dictionary<SelectWindowState, GameObject> _selectWindowDict = new();
    private readonly Dictionary<SelectWindowState, GameObject> _selectPanelDict = new();
    public PlayerSelected PlayerSelected { get; private set; }

    private SelectWindowState _currentState;
    StringBuilder sb = new();

    [Header("Char Select")] [SerializeField]
    private GameObject charSelect;

    [SerializeField] private Transform charViewportContent;

    [Header("Weapon Select")] [SerializeField]
    private GameObject weaponSelect;
    [SerializeField] private Transform weaponViewportContent;
    private SelectButton _randomWeaponBtn;
    private readonly List<SelectButton> _weaponSelectList = new();

    [Header("Stage Select")] [SerializeField]
    private GameObject stageSelect;
    [SerializeField] private Transform stageViewportContent;
    [SerializeField] private GameObject stagePanel;
    private List<SelectButton> _stageSelectList = new();

    [Header("Char Description")] [SerializeField]
    private Image charImage;

    [SerializeField] private TMP_Text charName;
    [SerializeField] private TMP_Text charPassive;

    [Header("Weapon Description")] 
    [SerializeField] private InfoPanel weaponPanel;
    [SerializeField] private Image weaponPanelBorder;
    [SerializeField] private Image weaponImg;
    [SerializeField] private TMP_Text weaponName;
    [SerializeField] private TMP_Text weaponClasses;
    [SerializeField] private TMP_Text weaponDescription;

    [Header("Stage Description")] 
    [SerializeField] private TMP_Text stageTxt;
    [SerializeField] private TMP_Text stageDescription;
    
    [Header("Prefabs")] [SerializeField] private Sprite randomSprite;
    [SerializeField] private SelectButton selectButton; //Prefab



    private void Awake()
    {
        _selectWindowDict.Add(SelectWindowState.CharSelect, charSelect);
        _selectWindowDict.Add(SelectWindowState.WeaponSelect, weaponSelect);
        _selectWindowDict.Add(SelectWindowState.StageSelect, stageSelect);

        _selectPanelDict.Add(SelectWindowState.CharSelect, null);
        _selectPanelDict.Add(SelectWindowState.WeaponSelect, weaponPanel.gameObject);
        _selectPanelDict.Add(SelectWindowState.StageSelect, stagePanel);
    }

    private void Start()
    {
        backButton.onClick.AddListener(OnBackBtnClick);

        InitCharSelectBtn(); //버튼 초기화.
        InitWeaponRandom();
        InitStageSelect();
        
        PlayerSelected = new PlayerSelected();
    }

    public void OpenSelectWindow()
    {
        Window.SetActive(true);
        _currentState = SelectWindowState.CharSelect;

        charSelect.SetActive(true); //개선방안?
        weaponSelect.SetActive(false);
        weaponPanel.gameObject.SetActive(false);
        stageSelect.SetActive(false);
        stagePanel.SetActive(false);
    }

    private void SwitchWindow(SelectWindowState nextState)
    {
        _selectWindowDict[_currentState].SetActive(false);
        _selectWindowDict[nextState].SetActive(true);

        var selectPanel = _selectPanelDict[_currentState];
        selectPanel?.gameObject.SetActive(false);
        selectPanel = _selectPanelDict[nextState];
        selectPanel?.gameObject.SetActive(true);

        _currentState = nextState;
    }

    public void OnBackBtnClick() //개선 방안 필요.
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
                weaponPanel.gameObject.SetActive(false);
                break;
            }
            case SelectWindowState.StageSelect:
            {
                SwitchWindow(SelectWindowState.WeaponSelect);
                stagePanel.SetActive(false);
                break;
            }
            default: return;
        }
    }

    private void InitCharSelectBtn()
    {
        var randomBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        randomBtn.transform.SetParent(charViewportContent);
        randomBtn.SetButtonImg(randomSprite);
        randomBtn.OnBtnPointerEnter += ShowRandCharDescription; //랜덤 버튼 
        randomBtn.SelectBtn.onClick.AddListener(SelectRandomCharacter);

        foreach (var (id, charData) in DataManager.Instance.CharDict)
        {
            var newBtn = ObjectPoolingManager.Instance.GetSelectBtn();
            newBtn.transform.SetParent(charViewportContent);

            newBtn.SetButtonImg(charData.CharacterSprite);

            newBtn.OnBtnPointerEnter += () => ShowCharDescription(id); //포인터 진입 시 캐릭터 설명

            newBtn.SelectBtn.onClick.AddListener(() => EnterWeaponSelect(id)); //클릭 시 무기 선택으로
        }

        ShowRandCharDescription();//초기에는 랜덤으로
    }

    private void ShowCharDescription(int charID)
    {
        var charData = DataManager.Instance.CharDict[charID];
        charImage.sprite = charData.CharacterSprite;
        charName.text = charData.CharacterName;
        
        sb.Clear();

        InfoPanel.GetItemStatTxt(sb, charID);

        charPassive.SetText(sb);
    }

    
    
    private void InitWeaponRandom()
    {
        _randomWeaponBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        _randomWeaponBtn.transform.SetParent(weaponViewportContent);
        _randomWeaponBtn.OnBtnPointerEnter += ShowRandomWeapon;
    }

    private void ShowWeaponButtons(int charID) //개선필요!
    {
        weaponPanel.gameObject.SetActive(true);

        _randomWeaponBtn.SelectBtn.onClick.RemoveAllListeners();
        _randomWeaponBtn.SelectBtn.onClick.AddListener(() => SelectRandomWeapon(charID)); 
        //랜덤 무기 선택 이벤트 등록

        foreach (var selectBtn in _weaponSelectList)
        {
            selectBtn.ClearEvent(); //이벤트 구독 해제

            ObjectPoolingManager.Instance.ReleaseSelectBtn(selectBtn); //Pool Release
            //초과하는 버튼은 이벤트 구독 해제...
        }

        _weaponSelectList.Clear();

        foreach (var weaponID in DataManager.Instance.CharDict[charID].InitWeaponIDList)
        {
            var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();

            selectBtn.transform.SetParent(weaponViewportContent); //부모 설정.

            var weaponData = DataManager.Instance.WeaponDict[weaponID];
            const int initTierIdx = 0;
            
            selectBtn.SetButtonImg(weaponData.Sprite);

            _weaponSelectList.Add(selectBtn);

            selectBtn.OnBtnPointerEnter += () => ShowWeaponDescription(weaponID, initTierIdx); 
            //포인터 이벤트(설명 표시)
            selectBtn.SelectBtn.onClick.AddListener( () => ShowStageSelect(charID, weaponID, initTierIdx));
        }
        
        ShowRandomWeapon();
    }

    private void ShowRandCharDescription()
    {
        //Localization 주의.
        charImage.sprite = randomSprite;
        charName.text = "랜덤";
        charPassive.text = "?";
    }

    private void SelectRandomCharacter() //랜덤 캐릭터 선택
    {
        var randIdx = Random.Range(0, DataManager.Instance.CharList.Count); //리스트에서 랜덤 인덱스
        var charData = DataManager.Instance.CharList[randIdx]; //해당 캐릭터 데이터
        charImage.sprite = charData.CharacterSprite;
        charName.text = charData.CharacterName;

        EnterWeaponSelect(charData.ID); //무기 선택창으로
    }

    private void EnterWeaponSelect(int charID) //무기 선택창 진입
    {
        ShowCharDescription(charID); //해당 캐릭터의 설명

        SwitchWindow(SelectWindowState.WeaponSelect); //창 변경

        //WEAPON...!
        ShowWeaponButtons(charID); //버튼 설정
    }

    private void ShowWeaponDescription(int weaponID, int tier) //재활용??
    {
        var weaponData = DataManager.Instance.WeaponDict[weaponID];

        weaponPanel.ShowWeaponInfo(weaponData,tier, sb);
    }


    private void ShowRandomWeapon()
    {
        weaponName.text = "랜덤";
        weaponClasses.text = "???";
        weaponImg.sprite = randomSprite;
        weaponDescription.text = "?";
        weaponPanelBorder.color = DataManager.Instance.GetHexToColor(StatUtil.DefaultWhite);
    }

    private void SelectRandomWeapon(int charID)
    {
        var weaponList = DataManager.Instance.CharDict[charID].InitWeaponIDList;
        var randIdx = Random.Range(0, weaponList.Count);
        var weaponID = weaponList[randIdx];
        const int initTierIdx = 0;
        
        ShowStageSelect(charID, weaponID, initTierIdx);
    }

    private void InitStageSelect()
    {
        for (int i = 0; i < 5; i++)
        {
            var btn = ObjectPoolingManager.Instance.GetSelectBtn();
            btn.transform.SetParent(stageViewportContent);
            
            sb.Clear();
            sb.Append(i);
            btn.SetBtnText(sb);

            int idx = i;
            btn.OnBtnPointerEnter += () => ShowStageDescription(idx);
            
            _stageSelectList.Add(btn);
        }
    }

    private void ShowStageDescription(int level)
    {
        sb.Clear();
        //
        sb.Append(level);
        stageTxt.SetText(sb);
        stageDescription.SetText(sb);
    }
    
    private void ShowStageSelect(int charID, int weaponID, int initTierIdx)
    {
        ShowWeaponDescription(weaponID, initTierIdx);
        SwitchWindow(SelectWindowState.StageSelect);
        stagePanel.SetActive(true);

        for (int i = 0; i < 5; i++)
        {
            var idx = i;
            var btn = _stageSelectList[i];
           btn.SelectBtn.onClick.RemoveAllListeners();
           btn.SelectBtn.onClick.AddListener( () => LoadMain(charID, weaponID, idx));
        }
        
        //PlayerSelected.CharID = charID; //캐릭터 선택 완료.
    }

    private void LoadMain(int charID, int weaponID, int stage)
    {
        PlayerSelected.CharID = charID;
        //개선필요??
        PlayerSelected.ItemIDList.Add(charID);
        PlayerSelected.WeaponIDList.Add(weaponID);
        PlayerSelected.StageID = stage;
        
        SceneChanger.Instance.LoadScene(SceneName.MainScene);
        SceneChanger.Instance.SetTitleSelected(PlayerSelected);
        
    }
}
