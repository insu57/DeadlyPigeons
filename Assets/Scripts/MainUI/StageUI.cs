using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum WaveEndState
{
    Upgrade,
    Crate,
    Store,
}

public class StageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waveTimer;
    [SerializeField] private TextMeshProUGUI currentWaveTxt;
    
    [Header("Icon")]
    [SerializeField] private GameObject crateIcon;
    [SerializeField] private TextMeshProUGUI crateCountTxt;
    [SerializeField] private GameObject lvUpIcon;
    [SerializeField] private TextMeshProUGUI lvUpCountTxt;
    
    [Header("WaveEnd")]
    [SerializeField] private GameObject waveEndUI;
    private Dictionary<WaveEndState, GameObject> waveEndDict = new();
    
    [Header("Upgrade")]
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private GridLayoutGroup upgradePanelGrid;
    [SerializeField] private UpgradePanel upgradePanelPrefab;
    private const int UpgradePanelCount = 4; //고정?
    
    [Header("Crate")]
    [SerializeField] private GameObject crateUI;
    
    [Header("Store")]
    [SerializeField] private GameObject storeUI;
    [SerializeField] private ItemGridUI itemGridUI;
    [SerializeField] private Button nextWaveBtn;

    private StringBuilder _sb;

    
    
    private void Awake()
    {
        _sb = StatUtil.StringBuilder;
        waveEndUI.SetActive(false);
        
        waveEndDict[WaveEndState.Upgrade] = upgradeUI;
        waveEndDict[WaveEndState.Crate] = crateUI;
        waveEndDict[WaveEndState.Store] =  storeUI;

        for (int i = 0; i < UpgradePanelCount; i++)
        {
            Instantiate(upgradePanelPrefab, upgradePanelGrid.transform);
        }
    }

    public void Init(PlayerManager playerManager)
    {
        itemGridUI.Init(playerManager);
    }
    
    public void SetCurrentWaveText(int wave)
    {
        _sb.Clear();
        _sb.Append("Wave ").Append(wave);
        currentWaveTxt.SetText(_sb);
    }

    public void UpdateWaveTimer(float time)
    {
        _sb.Clear();
        _sb.Append("Time\n").Append(time);
        waveTimer.SetText(_sb);
    }

    public void UpdateCrateCount(int count)
    {
        _sb.Clear();
        _sb.Append('x').Append(count);
        crateCountTxt.SetText(_sb);
        if(count > 0) crateIcon.SetActive(true);
    }

    public void UpdateLvUpCount(int count)
    {
        _sb.Clear();
        _sb.Append('x').Append(count);
        lvUpCountTxt.SetText(_sb);
        if (count > 0) lvUpIcon.SetActive(true);
    }
    
    public void OpenWaveEndUI(WaveEndState waveEndState)
    {
        waveEndUI.SetActive(true);
        foreach (var (key, ui) in waveEndDict)
        {
            waveEndDict[key].SetActive(key == waveEndState); //해당 UI만 활성화.
        }
    }
    
    public void OpenStoreUI(bool isOpen)
    {
        waveEndUI.SetActive(isOpen);
        if (isOpen)
        {
            
        }
    }
}
