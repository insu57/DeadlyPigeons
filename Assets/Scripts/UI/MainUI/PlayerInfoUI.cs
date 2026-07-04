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
    
    //Btn
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button restartBtn;
    [SerializeField] private Button optionBtn;
    [SerializeField] private Button titleBtn;
    
    [SerializeField] private OptionUI optionUI;
    
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image expBar;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI moneyText;
    
    //StatInfo
    [SerializeField] private PlayerStatInfo[] playerStatInfos;
    [SerializeField] private PlayerStatInfo playerStatInfo;
    
    
    //grid 분리...
    [SerializeField] private ItemGridUI itemGridUI;
    [Header("Weapons")] 
    [SerializeField] private SelectButton selectBtnPrefab;
    [SerializeField] private GridLayoutGroup weaponGrid;
    [SerializeField] private GridLayoutGroup itemGrid;

    [SerializeField] private InfoPanel infoPanel;
    [SerializeField] private ClassInfo classInfo;
    private int _weaponSlot;
    //public event Action<int, SelectButton> OnShowWeaponInfo; //ID
    
    private void Awake()
    {
        sb = new StringBuilder();
        
        ObjectPoolingManager.Instance.InitSelectBtnPool();
    }
    
    private void Start()
    {
        InputManager.Instance.Input.Global.Menu.performed += ShowInfoUI; //상태창
        
        optionBtn.onClick.AddListener(() => optionUI.ShowOptionPanel(true));
        
        //재시작/타이틀/종료 -> 확인창
    }

    public void Init(PlayerManager playerManager)
    {
        itemGridUI.Init(playerManager, InfoPanelType.Main);
        playerStatInfo.InitStatGrid(playerManager);
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
            if (optionUI.IsOpen)//옵션창 닫기
            {
                optionUI.ShowOptionPanel(false);
                return;
            }
            
            InputManager.Instance.Input.Player.Enable();
            InputManager.Instance.Input.UI.Disable();
            infoUI.SetActive(false);
            Time.timeScale = 1f;
        }
        else//열기
        {
            InputManager.Instance.Input.Player.Disable();
            InputManager.Instance.Input.UI.Enable();
            infoUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
