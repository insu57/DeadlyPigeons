using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject infoUI;
    private StringBuilder sb;
    [SerializeField] private PlayerStatTxt playerStatTxt;
    
    private Dictionary<MainStats, PlayerStatTxt>  mainStatDict = new();
    private Dictionary<SubStats, PlayerStatTxt>  subStatDict = new();
    
    [Header("Main Stats")] 
    [SerializeField] private Button mainStatBtn;
    [SerializeField] private GameObject mainStatPanel;
    [SerializeField] private GameObject mainStatGrid;
    [SerializeField] private TMP_Text currentLevelLabel;
    [SerializeField] private TMP_Text currentLevelValue;
    
    [Header("Sub Stats")]
    [SerializeField] private GameObject subStatPanel;
    [SerializeField] private GameObject subStatGrid;
    [SerializeField] private Button subStatBtn;

    [Header("Weapons")] 
    [SerializeField] private SelectButton selectBtnPrefab;
    [SerializeField] private GridLayoutGroup weaponGrid;
    [SerializeField] private GridLayoutGroup itemGrid;
    [SerializeField] private Transform selectBtnParent;
    private Transform _weaponInfoPanelParent;
    [SerializeField] private InfoPanel infoPanel;
    [SerializeField] private ClassInfo classInfo;
    private int _weaponSlot;
    public event Action<int, SelectButton> OnShowWeaponInfo; //ID
    
    private void Awake()
    {
        sb = new StringBuilder();
        InitStatGrid();
        ObjectPoolingManager.Instance.InitSelectBtnPool();
    }
    
    private void Start()
    {
        InputManager.Instance.Input.Global.Menu.performed += ShowInfoUI; //상태창
        mainStatBtn.onClick.AddListener(ShowMainStat);
        subStatBtn.onClick.AddListener(ShowSubStat);
    }

    private void InitStatGrid() //스탯 텍스트 초기화
    {
        for(int i = 0; i < (int)MainStats.None; i++)
        {
           var statTxt = Instantiate(playerStatTxt, mainStatGrid.transform);
           sb.Clear();
           var mainStat = (MainStats)i;
           sb.Append(mainStat.GetIcons()); //스탯 아이콘
           sb.Append(mainStat.MainStatsToString()); //스탯 명(Localization???)
           statTxt.StatLabel.SetText(sb);
           sb.Clear();
           
           mainStatDict[mainStat] = statTxt;
        }

        for (int i = 0; i < (int)SubStats.None; i++)
        {
            var statTxt = Instantiate(playerStatTxt, subStatGrid.transform);
            sb.Clear();
            var subStat = (SubStats)i;
            sb.Append(subStat.SubStatsToString());
            statTxt.StatLabel.SetText(sb);
            subStatDict[subStat] = statTxt;
        }
    }
    
    private void ShowInfoUI(InputAction.CallbackContext context)
    {
        if (infoUI.activeSelf) //닫기
        {
            InputManager.Instance.Input.Player.Enable();
            InputManager.Instance.Input.UI.Disable();
            infoUI.SetActive(false);
        }
        else//열기
        {
            InputManager.Instance.Input.Player.Disable();
            InputManager.Instance.Input.UI.Enable();
            infoUI.SetActive(true);
        }
    }

    public void UpdateMainStat(MainStats stat, int value)
    {
        var labelTxt = mainStatDict[stat].StatLabel;
        var valueTxt = mainStatDict[stat].StatValue;
        
        sb.Clear();
        sb.Append(value);
        if (stat.IsPercentageMainStat()) sb.Append('%');
        valueTxt.SetText(sb);
        
        if (value > 0)
        {
            labelTxt.color = DataManager.Instance.GetHexToColor(StatUtil.GreenColor);
            valueTxt.color =  DataManager.Instance.GetHexToColor(StatUtil.GreenColor);
        }
        else if(value == 0)
        {
            labelTxt.color = DataManager.Instance.GetHexToColor(StatUtil.DefaultWhite);
            valueTxt.color =  DataManager.Instance.GetHexToColor(StatUtil.DefaultWhite);
        }
        else
        {
            labelTxt.color = DataManager.Instance.GetHexToColor(StatUtil.RedColor);
            valueTxt.color = DataManager.Instance.GetHexToColor(StatUtil.RedColor);
        }
    }

    public void UpdateSubStat(SubStats stat, int value)
    {
        var labelTxt = subStatDict[stat].StatLabel;
        var valueTxt = subStatDict[stat].StatValue;
        sb.Clear();
        sb.Append(value);
        if (stat.IsPercentageSubStat()) sb.Append('%');
        valueTxt.SetText(sb);
        if (value > 0)
        {
            labelTxt.color = DataManager.Instance.GetHexToColor(StatUtil.GreenColor);
            valueTxt.color =  DataManager.Instance.GetHexToColor(StatUtil.GreenColor);
        }
        else if(value == 0)
        {
            labelTxt.color = DataManager.Instance.GetHexToColor(StatUtil.DefaultWhite);
            valueTxt.color =  DataManager.Instance.GetHexToColor(StatUtil.DefaultWhite);
        }
        else
        {
            labelTxt.color = DataManager.Instance.GetHexToColor(StatUtil.RedColor);
            valueTxt.color = DataManager.Instance.GetHexToColor(StatUtil.RedColor);
        }
    }

    private void ShowMainStat()
    {
        mainStatPanel.SetActive(true);
        subStatPanel.SetActive(false);
    }

    private void ShowSubStat()
    {
        subStatPanel.SetActive(true);
        mainStatPanel.SetActive(false);
    }

    public void InitWeaponSlots(int slots) //슬롯 초기화.
    {
        _weaponSlot = slots;
        if (_weaponSlot <= 0)
        {
            weaponGrid.gameObject.SetActive(false);
        }
    }
    
    public void AddWeapon(Sprite sprite, int index) //무기 장착(UI)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(sprite);
        selectBtn.transform.SetParent(weaponGrid.transform);
        
        selectBtn.OnBtnPointerEnter += () => OnShowWeaponInfo?.Invoke(index, selectBtn);
        selectBtn.OnBtnPointerExit += CloseInfoPanel;
    }

    public void AddItem(ItemData item, int idx)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(item.Icon);
        selectBtn.transform.SetParent(itemGrid.transform);
        
        selectBtn.OnBtnPointerEnter += () => ShowItemInfo(item, selectBtn, idx);
        selectBtn.OnBtnPointerExit += CloseInfoPanel;
    }
    
    public void SetWeaponClassBonus(Dictionary<WeaponClasses, int> weaponsBonusDict)
    {
        classInfo.SetWeaponClassBonusDict(weaponsBonusDict);
    }

    private void SetInfoPanel(int cols, int idx, SelectButton selectButton)
    {
        Transform panelParent;
        var panelRT = (RectTransform)infoPanel.transform;
        if (idx < cols / 2)
        {
            panelParent = selectButton.InfoPanelParentLeft;
            panelRT.pivot = new Vector2(0, 1);
        }
        else
        {
            panelParent = selectButton.InfoPanelParentRight;
            panelRT.pivot = new Vector2(1, 1);
        }
        infoPanel.transform.position = panelParent.position;
        infoPanel.gameObject.SetActive(true);
    }

    public void ShowWeaponInfo(CurrentWeaponStat currentWeaponStat,SelectButton selectButton, int weaponIdx) //무기 정보 표시
    {
        SetInfoPanel(weaponGrid.constraintCount, weaponIdx, selectButton);//피봇 설정 관련 개선?
        infoPanel.ShowWeaponInfo(currentWeaponStat, sb);
        var classes = currentWeaponStat.WeaponData.WeaponStat.classes;
        infoPanel.ShowWeaponClassInfo(classes, sb);
    }

    private void ShowItemInfo(ItemData item, SelectButton selectButton,int idx)
    {
        SetInfoPanel(itemGrid.constraintCount, idx, selectButton);
        infoPanel.ShowItemInfo(item, sb);
    }
    
    private void CloseInfoPanel()
    {
        infoPanel.gameObject.SetActive(false);
    }
    
}
