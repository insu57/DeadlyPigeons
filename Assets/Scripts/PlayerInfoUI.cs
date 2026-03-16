using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;
    private StringBuilder sb;
    [Serializable] 
    public struct StatsText
    {
        public TMP_Text label;
        public TMP_Text value;
    }

    [Header("Main Stats")] 
    [SerializeField] private StatsText currentLevelTxt;
    [SerializeField] private StatsText maxHPTxt;
    [SerializeField] private StatsText healthRegenTxt;
    [SerializeField] private StatsText healthAbsorbTxt;
    [SerializeField] private StatsText armorTxt;
    [SerializeField] private StatsText dodgeChanceTxt;
    [SerializeField] private StatsText speedTxt;
    [SerializeField] private StatsText damageTxt;
    [SerializeField] private StatsText meleeTxt;
    [SerializeField] private StatsText rangedTxt;
    [SerializeField] private StatsText elementalTxt;
    [SerializeField] private StatsText engineeringTxt;
    [SerializeField] private StatsText tacticalTxt;
    [SerializeField] private StatsText attackSpeedTxt;
    [SerializeField] private StatsText critChanceTxt;
    [SerializeField] private StatsText rangeTxt;
    [SerializeField] private StatsText luckTxt;
    [SerializeField] private StatsText harvestTxt;
    private Dictionary<MainStats, StatsText> mainStatTxtDict = new();

    private void Awake()
    {
        mainStatTxtDict[MainStats.MaxHP] = maxHPTxt;
        mainStatTxtDict[MainStats.HealthRegen] = healthRegenTxt;
        mainStatTxtDict[MainStats.HealthAbsorb] = healthAbsorbTxt;
        mainStatTxtDict[MainStats.Armor] = armorTxt;
        mainStatTxtDict[MainStats.DodgeChance] = dodgeChanceTxt;
        mainStatTxtDict[MainStats.Speed] = speedTxt;
        mainStatTxtDict[MainStats.Damage] = damageTxt;
        mainStatTxtDict[MainStats.Melee] = meleeTxt;
        mainStatTxtDict[MainStats.Ranged] = rangedTxt;
        mainStatTxtDict[MainStats.Elemental] = elementalTxt;
        mainStatTxtDict[MainStats.Engineering] = engineeringTxt;
        mainStatTxtDict[MainStats.Tactical] = tacticalTxt;
        mainStatTxtDict[MainStats.AttackSpeed] = attackSpeedTxt;
        mainStatTxtDict[MainStats.CritChance] = critChanceTxt;
        mainStatTxtDict[MainStats.Range] = rangeTxt;
        mainStatTxtDict[MainStats.Luck] = luckTxt;
        mainStatTxtDict[MainStats.Harvest] = harvestTxt;

        sb = new StringBuilder();
    }
    
    private void Start()
    {
        InputManager.Instance.Input.Global.Menu.performed += ShowInfoUI; //상태창
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
        var labelTxt = mainStatTxtDict[stat].label;
        var valueTxt = mainStatTxtDict[stat].value;

        sb.Clear();
        sb.Append(value);
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
}
