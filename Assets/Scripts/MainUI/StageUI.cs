using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum WaveEndState //늘어나면 수정.
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
    private  int _upgradeOptionCount; //고정?
    private UpgradePanel[] _upgradePanels;
    public event Action<int> OnSelectStatUpgrade;
    
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
        
    }

    public void Init(PlayerManager playerManager, int upgradeOptionCount)
    {
        itemGridUI.Init(playerManager);
        
        _upgradeOptionCount = upgradeOptionCount;
        _upgradePanels = new UpgradePanel[_upgradeOptionCount];
        
        for (int i = 0; i < _upgradeOptionCount; i++)
        {
            var upgradePanel = Instantiate(upgradePanelPrefab, upgradePanelGrid.transform);
            _upgradePanels[i] = upgradePanel;
            int panelIdx = i;
            _upgradePanels[i].SelectBtn.onClick.AddListener( () => OnSelectStatUpgrade?.Invoke(panelIdx) );
        }
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
    
    private void ShowWaveEndPanel(WaveEndState waveEndState)
    {
        waveEndUI.SetActive(true);
        upgradeUI.gameObject.SetActive(waveEndState == WaveEndState.Upgrade);
        crateUI.gameObject.SetActive(waveEndState == WaveEndState.Crate);
        storeUI.gameObject.SetActive(waveEndState == WaveEndState.Store);
    }

    public void OpenUpgradeUI((MainStats mainstat, int tier)[] upgrades)
    {
        ShowWaveEndPanel(WaveEndState.Upgrade);

        for (int i = 0; i < upgrades.Length; i++)
        {
            var upgradePanel = _upgradePanels[i];
            upgradePanel.SetUpgrade(upgrades[i].mainstat, upgrades[i].tier);
        }
    }
    
    public void OpenCrateUI(ItemData itemData, int price)
    {
        ShowWaveEndPanel(WaveEndState.Crate);
    }

    public void OpenStoreUI()
    {
        //?
    }
}
