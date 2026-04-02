using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;
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
    [SerializeField] private Transform weaponGrid;
    [SerializeField] private Transform itemGrid;
    [SerializeField] private Transform selectBtnParent;
    private int _weaponSlot;
    public event Action<int> OnShowWeaponInfo; //ID
    
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
        if (infoPanel.activeSelf) //닫기
        {
            InputManager.Instance.Input.Player.Enable();
            InputManager.Instance.Input.UI.Disable();
            infoPanel.SetActive(false);
        }
        else//열기
        {
            InputManager.Instance.Input.Player.Disable();
            InputManager.Instance.Input.UI.Enable();
            infoPanel.SetActive(true);
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

    public void SetWeaponSlots(int slots)
    {
        _weaponSlot = slots;
        if (_weaponSlot <= 0)
        {
            weaponGrid.gameObject.SetActive(false);
        }
    }
    
    public void AddWeapon(Sprite sprite, int index)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(sprite);
        selectBtn.transform.SetParent(weaponGrid);
        selectBtn.OnBtnPointerEnter += () => OnShowWeaponInfo?.Invoke(index);
    }

    public void ShowWeaponInfo(WeaponData weaponData)
    {
        Debug.Log(weaponData.Name);
        
        //
    }
    
}
