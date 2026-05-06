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

    [SerializeField] private PlayerStatInfo[] playerStatInfos;

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
 
        foreach (var playerStatInfo in playerStatInfos)
        {
            playerStatInfo.InitStatGrid();
        }
        
        ObjectPoolingManager.Instance.InitSelectBtnPool();
    }
    
    private void Start()
    {
        InputManager.Instance.Input.Global.Menu.performed += ShowInfoUI; //상태창
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
        foreach (var playerStatInfo in playerStatInfos)
        {
            playerStatInfo.UpdateMainStat(stat, value);
        }
        
    }

    public void UpdateSubStat(SubStats stat, int value)
    {
        foreach (var playerStatInfo in playerStatInfos)
        {
            playerStatInfo.UpdateSubStat(stat, value);
        }
       
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
