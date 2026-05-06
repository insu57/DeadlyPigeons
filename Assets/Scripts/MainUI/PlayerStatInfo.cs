using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatInfo : MonoBehaviour
{
    [Header("Prefabs")] [SerializeField] private PlayerStatTxt playerStatTxt;

    [Header("Main Stats")] [SerializeField]
    private Button mainStatBtn;

    [SerializeField] private GameObject mainStatPanel;
    [SerializeField] private GameObject mainStatGrid;
    [SerializeField] private TMP_Text currentLevelLabel;
    [SerializeField] private TMP_Text currentLevelValue;

    [Header("Sub Stats")] [SerializeField] private GameObject subStatPanel;
    [SerializeField] private GameObject subStatGrid;
    [SerializeField] private Button subStatBtn;

    private Dictionary<MainStats, PlayerStatTxt> _mainStatDict = new();
    private Dictionary<SubStats, PlayerStatTxt> _subStatDict = new();

    private StringBuilder _sb = new();

  

    private void Start()
    {
        mainStatBtn.onClick.AddListener(ShowMainStat);
        subStatBtn.onClick.AddListener(ShowSubStat);
    }
    
    public void InitStatGrid() //스탯 텍스트 초기화
    {
        for(int i = 0; i < (int)MainStats.None; i++)
        {
            var statTxt = Instantiate(playerStatTxt, mainStatGrid.transform);
            _sb.Clear();
            var mainStat = (MainStats)i;
            _sb.Append(mainStat.GetIcons()); //스탯 아이콘
            _sb.Append(mainStat.MainStatsToString()); //스탯 명(Localization???)
            statTxt.StatLabel.SetText(_sb);
            _sb.Clear();
           
            _mainStatDict[mainStat] = statTxt;
        }

        for (int i = 0; i < (int)SubStats.None; i++)
        {
            var statTxt = Instantiate(playerStatTxt, subStatGrid.transform);
            _sb.Clear();
            var subStat = (SubStats)i;
            _sb.Append(subStat.SubStatsToString());
            statTxt.StatLabel.SetText(_sb);
            _subStatDict[subStat] = statTxt;
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
    
    public void UpdateMainStat(MainStats stat, int value)
    {
        var labelTxt = _mainStatDict[stat].StatLabel;
        var valueTxt = _mainStatDict[stat].StatValue;
        
        _sb.Clear();
        _sb.Append(value);
        if (stat.IsPercentageMainStat()) _sb.Append('%');
        valueTxt.SetText(_sb);
        
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
        var labelTxt = _subStatDict[stat].StatLabel;
        var valueTxt = _subStatDict[stat].StatValue;
        _sb.Clear();
        _sb.Append(value);
        if (stat.IsPercentageSubStat()) _sb.Append('%');
        valueTxt.SetText(_sb);
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
}
