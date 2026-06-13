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

public struct StoreData
{
    //최대 4개
    public int Money;
    public List<(ItemData itemData, int price, int idx)> Items;
    public List<(CurrentWeaponStat weaponStat, int price, int idx)> Weapons;
    public int RerollPrice;
    public List<bool> IsSold;
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
    [SerializeField] private PlayerStatInfo playerStatInfo;
    
    [Header("Upgrade")]
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private GridLayoutGroup upgradePanelGrid;
    [SerializeField] private UpgradePanel upgradePanelPrefab;
    [SerializeField] private Button rerollUpgradeBtn;
    [SerializeField] private TextMeshProUGUI upgradeRerollPriceTxt;
    private  int _upgradeOptionCount; //고정?
    private UpgradePanel[] _upgradePanels;
    public event Action<int> OnSelectStatUpgrade;
    public event Action OnRerollUpgrade;
    
    
    [Header("Crate")]
    [SerializeField] private GameObject crateUI;
    [SerializeField] private InfoPanel crateInfoPanel;
    [SerializeField] private Button crateItemUseBtn;
    [SerializeField] private Button crateItemSellBtn;
    [SerializeField] private TextMeshProUGUI crateItemPriceTxt;
    public event Action OnUseCrateItem;
    public event Action OnSellCrateItem;
    
    [Header("Store")]
    [SerializeField] private GameObject storeUI;
    [SerializeField] private ItemGridUI itemGridUI;
    [SerializeField] private GridLayoutGroup storePanelGrid;
    private StorePanel[] _storePanels;
    [SerializeField] private StorePanel storePanelPrefab;
    [SerializeField] private Button nextWaveBtn;
    [SerializeField] private Button storeRerollBtn;
    [SerializeField] private TextMeshProUGUI storeRerollPriceTxt;
    [SerializeField] private ClassInfoPanel storeClassInfoPanel;
    [SerializeField] private TextMeshProUGUI playerMoney;
    public event Action<int> OnStorePanelShowClassInfo;
    public event Action OnRerollStore;
    public event Action<int> OnBuyStore;
    public event Action OnNextWave;
    public event Action<int> OnCombineWeapon; //이벤트 버스로 수정?
    public event Action<int> OnRecycleWeapon;
    [SerializeField] private StageEndPanel stageEndPanel;
    
    private StringBuilder _sb;
    
    private void Awake()
    {
        _sb = StatUtil.StringBuilder;
        waveEndUI.SetActive(false);
        
        waveEndDict[WaveEndState.Upgrade] = upgradeUI;
        waveEndDict[WaveEndState.Crate] = crateUI;
        waveEndDict[WaveEndState.Store] =  storeUI;
        
        rerollUpgradeBtn.onClick.AddListener( () => OnRerollUpgrade?.Invoke() );
        crateItemUseBtn.onClick.AddListener( () => OnUseCrateItem?.Invoke() );
        crateItemSellBtn.onClick.AddListener( () => OnSellCrateItem?.Invoke() );

        storeRerollBtn.onClick.AddListener(() => OnRerollStore?.Invoke());
        itemGridUI.OnCombineWeapon += HandleOnCombineWeapon;
        itemGridUI.OnRecycleWeapon += HandleOnRecycleWeapon;
        
        nextWaveBtn.onClick.AddListener(NextWave);
    }

    public void Init(PlayerManager playerManager, int upgradeOptionCount, int storeOptionCount)
    {
        playerStatInfo.InitStatGrid(playerManager);
        itemGridUI.Init(playerManager, InfoPanelType.Store);
        stageEndPanel.PlayerStatInfo.InitStatGrid(playerManager);
        stageEndPanel.ItemGridUI.Init(playerManager, InfoPanelType.Main);
        
        _upgradeOptionCount = upgradeOptionCount;
        _upgradePanels = new UpgradePanel[_upgradeOptionCount];
        
        for (int i = 0; i < _upgradeOptionCount; i++)
        {
            var upgradePanel = Instantiate(upgradePanelPrefab, upgradePanelGrid.transform);
            _upgradePanels[i] = upgradePanel;
            int panelIdx = i;
            _upgradePanels[i].SelectBtn.onClick.AddListener( () => OnSelectStatUpgrade?.Invoke(panelIdx) );
        }

        _storePanels = new StorePanel[storeOptionCount];
        
        for (int i = 0; i < storeOptionCount; i++)
        {
            var storePanel = Instantiate(storePanelPrefab, storePanelGrid.transform);
            storePanel.InitStorePanel(i);
            storePanel.InitClassInfo(storeClassInfoPanel);
            storePanel.OnShowClassInfoPanel += idx => OnStorePanelShowClassInfo?.Invoke(idx);
            var storePanelIdx = i;
            storePanel.BuyBtn.onClick.AddListener(() => OnBuyStore?.Invoke(storePanelIdx));
            _storePanels[i] = storePanel;
        }
        
        storeClassInfoPanel.SetClassBonusDict(playerManager.WeaponClassDict);
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

    public void UpdateMoney(int money)
    {
        _sb.Clear();
        _sb.Append(money);
        playerMoney.SetText(_sb);
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

    private void GetRerollPriceTxt(int rerollPrice, bool canReroll)
    {
        _sb.Clear();
        if (canReroll)
        {
            _sb.Append('-').Append(rerollPrice);
        }
        else
        {
            _sb.AppendColor("-", -1);
            _sb.AppendColor(rerollPrice, -1);
        } 
    }
    
    public void UpdateUpgradeRerollPrice(int rerollPrice, bool canReroll)
    {
        GetRerollPriceTxt(rerollPrice, canReroll);
        
        upgradeRerollPriceTxt.SetText(_sb);
    }
    
    public void OpenCrateUI(ItemData itemData, int price)
    {
        ShowWaveEndPanel(WaveEndState.Crate);
        
        _sb.Clear();
        _sb.Append("판매").Append("(+").Append(price).Append(')');
        crateItemPriceTxt.SetText(_sb);
        
        crateInfoPanel.ShowItemInfo(itemData);
    }

    public void OpenStoreUI(StoreData storeData)
    {
        ShowWaveEndPanel(WaveEndState.Store);

        UpdateMoney(storeData.Money);
        
        var canReroll = storeData.RerollPrice <= storeData.Money;
        GetRerollPriceTxt(storeData.RerollPrice, canReroll);
        storeRerollPriceTxt.SetText(_sb);

        foreach (var storePanel in _storePanels)
        {
            storePanel.ActiveStorePanel(true);
        }

        foreach (var (itemData, price, idx) in storeData.Items)
        {
            var canBuy = price <= storeData.Money;
            _storePanels[idx].SetStorePanel(itemData, price,  canBuy);
        }

        foreach (var (weaponStat, price, idx) in storeData.Weapons)
        {
            var canBuy = price <= storeData.Money;
            _storePanels[idx].SetStorePanel(weaponStat, price, canBuy);
        }
        
        // 무기 리스트, 아이템 리스트. 각 wave+itemPrice 반영된 구매가격


        //상점...
    }

    public void DisableStorePanelOnBuy(int idx)
    {
        _storePanels[idx].ActiveStorePanel(false);
    }

    public void UpdateStorePanelCanBuy(int idx, int price, bool canBuy)
    {
        _storePanels[idx].UpdateStorePanelCanBuy(price, canBuy);
    }

    public void ShowStorePanelClassInfo(List<ItemClass> classes, int idx)
    {
        _storePanels[idx].ShowClassInfoPanel(classes);
    }
    
    public void ShowStorePanelClassInfo(List<WeaponClasses> classes, int idx)
    {
        _storePanels[idx].ShowClassInfoPanel(classes);
    }

    private void HandleOnCombineWeapon(int idx)
    {
        OnCombineWeapon?.Invoke(idx);
    }

    private void HandleOnRecycleWeapon(int idx)
    {
        OnRecycleWeapon?.Invoke(idx);   
    }
    
    private void NextWave()
    {
        waveEndUI.SetActive(false);
        OnNextWave?.Invoke();
    }

    public void StageEnd(bool isClear)
    {
        stageEndPanel.gameObject.SetActive(true);
    }
}
