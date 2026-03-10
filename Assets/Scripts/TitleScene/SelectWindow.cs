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
    private readonly Dictionary<SelectWindowState, GameObject> _selectWindowDict = new();
    private readonly Dictionary<SelectWindowState, GameObject> _selectPanelDict = new();
    public Selected PlayerSelected { get; private set; }

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

    [Header("Char Description")] [SerializeField]
    private Image charImage;

    [SerializeField] private TMP_Text charName;
    [SerializeField] private TMP_Text charPassive;

    [Header("Weapon Description")] 
    [SerializeField] private GameObject weaponPanel;
    [SerializeField] private Image weaponPanelBorder;
    [SerializeField] private Image weaponImg;
    [SerializeField] private TMP_Text weaponName;
    [SerializeField] private TMP_Text weaponClasses;
    [SerializeField] private TMP_Text weaponDescription;

    [Header("Prefabs")] [SerializeField] private Sprite randomSprite;
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

        InitCharSelectBtn(); //버튼 초기화.
        InitWeaponRandom();

        PlayerSelected = new Selected();



    }

    public void OpenSelectWindow()
    {
        Window.SetActive(true);
        _currentState = SelectWindowState.CharSelect;

        charSelect.SetActive(true); //개선방안?
        weaponSelect.SetActive(false);
        weaponPanel.SetActive(false);
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
                weaponPanel.SetActive(false);
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
        var randomBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        randomBtn.transform.SetParent(charViewportContent);
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

    }

    private void ShowCharDescription(int charID)
    {
        var charData = DataManager.Instance.CharDict[charID];
        charImage.sprite = charData.CharacterSprite;
        charName.text = charData.CharacterName;

        sb.Clear();
        var passive = charData.InitStatsList; //초기 스탯(패시브) -> 패시브 아이템으로 변경?
        foreach (var init in passive)
        {
            if (init.mainStats != MainStats.None)
            {
                sb.Append(init.mainStats.GetIcons()); //스탯 아이콘
                sb.AppendColorString(init.amount); //증감량
                sb.AppendLine(init.mainStats.MainStatsToString()); //해당 스탯명
            }
            else if (init.subStats != SubStats.None)
            {
                sb.AppendColorString(init.amount);
                sb.AppendLine(init.subStats.SubStatsToString());
            }
        }

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
        weaponPanel.SetActive(true);

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

            selectBtn.SetButtonImg(DataManager.Instance.WeaponDict[weaponID].Sprite);

            _weaponSelectList.Add(selectBtn);

            selectBtn.OnBtnPointerEnter += () => ShowWeaponDescription(weaponID); //포인터 이벤트(설명 표시)
        }
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
        PlayerSelected.CharID = charID; //캐릭터 선택 완료.

        SwitchWindow(SelectWindowState.WeaponSelect); //창 변경

        //WEAPON...!
        ShowWeaponButtons(charID); //버튼 설정
    }

    private void ShowWeaponDescription(int weaponID)
    {
        var weaponData = DataManager.Instance.WeaponDict[weaponID];

        sb.Clear();

        weaponName.text = weaponData.Name;
        weaponImg.sprite = weaponData.Sprite;
        
        //무기의 스탯은 초기 티어기준으로.
        var tier = weaponData.WeaponStat.initTier;
        var colorHexStr = DataManager.Instance.TierColorDict[tier]; //티어 컬러 가져오기
        var color = DataManager.Instance.GetHexToColor(colorHexStr);
        weaponName.color = color;
        weaponPanelBorder.color = color;
        
        var weaponClass = weaponData.WeaponStat.classes;
        sb.Append(WeaponData.WeaponClassToString(weaponClass[0])); //첫 클래스
        for (int i = 1; i < weaponClass.Count; i++) //하나 이상의 클래스를 가진 무기라면
        {
            sb.Append(", ").Append(WeaponData.WeaponClassToString(weaponClass[i]));
        } 
        weaponClasses.SetText(sb);
        sb.Clear();
        
        sb.AppendHeadString("데미지:");
        sb.Append(weaponData.WeaponStat.baseDamage[0]).Append(" ("); //기본 데미지
        foreach (var statMultiplier in weaponData.WeaponStat.damageMultipliers) //스탯 별 데미지 계수
        {
            var stat = statMultiplier.stat;
            var value = statMultiplier.value[0];
            sb.Append("+").Append(value).Append("%").Append(stat.GetIcons());
        }
        sb.AppendLine(")");
        
        weaponDescription.SetText(sb);
        //티어 표시?
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
        
    }

private void EnterStageSelect(int charID, int weaponID)
    {
        
    }
}
