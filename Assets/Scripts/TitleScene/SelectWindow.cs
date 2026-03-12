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
    [SerializeField] private GameObject weaponPanel;
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
        _selectPanelDict.Add(SelectWindowState.WeaponSelect, weaponPanel);
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
        weaponPanel.SetActive(false);
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
                weaponPanel.SetActive(false);
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
            selectBtn.SelectBtn.onClick.AddListener( () => ShowStageSelect(charID, weaponID));
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

    private void ShowWeaponDescription(int weaponID) //재활용??
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
        
        sb.AppendHeadString("치명타:");
        sb.Append("X").Append(weaponData.WeaponStat.critDamage[0]);
        sb.Append(" (").Append(weaponData.WeaponStat.critChance[0]).AppendLine("% 확률)");
        
        sb.AppendHeadString("쿨타운:");
        sb.Append(weaponData.WeaponStat.attackSpeed[0]).AppendLine("s");
        
        var knockback = weaponData.WeaponStat.knockBack[0];
        if (knockback > 0)
        {
            sb.AppendHeadString("넉백:");
            sb.Append(knockback).AppendLine();
        }
        
        sb.AppendHeadString("범위:");
        sb.Append(weaponData.WeaponStat.range[0]).Append("(");
        sb.AppendLine(weaponData.WeaponStat.isMelee ? "근거리)" : "원거리)");

        sb.Append("•").AppendLine(weaponData.WeaponStat.description); 
        //고유 효과 -> 데이터는 어떤방식으로???
        // 최소 0개(없음부터) ~ 5?개(상한은 없이?) - 티어 수 만큼의 스탯 배수값...
        
        
        weaponDescription.SetText(sb);
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
        
        ShowStageSelect(charID, weaponID);
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
    
    private void ShowStageSelect(int charID, int weaponID)
    {
        ShowWeaponDescription(weaponID);
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
        //개선필요.
        PlayerSelected.WeaponIDList.Add(weaponID);
        PlayerSelected.StageID = stage;
        
        SceneChanger.Instance.LoadScene(SceneName.MainScene);
        SceneChanger.Instance.SetTitleSelected(PlayerSelected);
        
    }
}
