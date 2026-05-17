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
    
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image expBar;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI moneyText;
    
    //StatInfo
    [SerializeField] private PlayerStatInfo[] playerStatInfos;
    public event Action<int> OnUpdateLevel;
    public event Action<MainStats, int> OnUpdateMainStat;
    public event Action<SubStats, int> OnUpdateSubStat;
    
    //grid 분리...
    [SerializeField] private ItemGridUI itemGridUI;
    [Header("Weapons")] 
    [SerializeField] private SelectButton selectBtnPrefab;
    [SerializeField] private GridLayoutGroup weaponGrid;
    [SerializeField] private GridLayoutGroup itemGrid;
    //[SerializeField] private Transform selectBtnParent;
    //private Transform _weaponInfoPanelParent;
    [SerializeField] private InfoPanel infoPanel;
    [SerializeField] private ClassInfo classInfo;
    private int _weaponSlot;
    public event Action<int, SelectButton> OnShowWeaponInfo; //ID
    
    
    
    private void Awake()
    {
        sb = new StringBuilder();
 
        foreach (var playerStatInfo in playerStatInfos)
        {
            playerStatInfo.InitStatGrid(this);
        }
        
        ObjectPoolingManager.Instance.InitSelectBtnPool();
    }
    
    private void Start()
    {
        InputManager.Instance.Input.Global.Menu.performed += ShowInfoUI; //상태창
    }

    public void Init(PlayerManager playerManager)
    {
        itemGridUI.Init(playerManager);
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        sb.Clear();
        sb.Append(currentHealth + " / " + maxHealth);
        healthText.SetText(sb);
        healthBar.fillAmount = (float)currentHealth / maxHealth;
    }

    public void UpdateExpBar(int lv, float currentExp, float targetExp)
    {
        sb.Clear();
        sb.Append("Lv " + lv);
        expText.SetText(sb);
        expBar.fillAmount = currentExp / targetExp;

        OnUpdateLevel?.Invoke(lv);
    }

    public void UpdateMoney(int money)
    {
        sb.Clear();
        sb.Append(money);
        moneyText.SetText(sb);
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
        OnUpdateMainStat?.Invoke(stat, value);
    }

    public void UpdateSubStat(SubStats stat, int value)
    {
        OnUpdateSubStat?.Invoke(stat, value);
    }
    
}
