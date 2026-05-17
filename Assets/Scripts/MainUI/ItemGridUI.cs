using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ItemGridUI : MonoBehaviour
{
    [SerializeField] private SelectButton selectBtnPrefab;
    [SerializeField] private GridLayoutGroup weaponGrid;
    [SerializeField] private GridLayoutGroup itemGrid;
    [SerializeField] private InfoPanel infoPanel;
    [SerializeField] private ClassInfo classInfo;
    
    public event Action<int, SelectButton, ItemGridUI> OnShowWeaponInfo; //ID
    
    public void Init(PlayerManager playerManager)
    {
        playerManager.OnAddWeapon += AddWeapon;
        playerManager.OnAddItem +=  AddItem;
       
        playerManager.OnSetWeaponClassBonus += SetWeaponClassBonus;
        OnShowWeaponInfo += playerManager.HandleOnShowWeaponInfo;
        
        infoPanel.Init();
    }
    
    private void AddWeapon(Sprite sprite, int index)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(sprite);
        selectBtn.transform.SetParent(weaponGrid.transform);
        
        selectBtn.OnBtnPointerEnter += () => OnShowWeaponInfo?.Invoke(index, selectBtn, this);
        selectBtn.OnBtnPointerExit += CloseInfoPanel;
    }
    
    private void AddItem(ItemData item, int idx)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(item.Icon);
        selectBtn.transform.SetParent(itemGrid.transform);
        
        selectBtn.OnBtnPointerEnter += () => ShowItemInfo(item, selectBtn, idx);
        selectBtn.OnBtnPointerExit += CloseInfoPanel;
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

    public void GetCurrentWeaponInfo(CurrentWeaponStat currentWeaponStat, SelectButton selectButton, int weaponIdx) //무기 정보 표시
    {
        SetInfoPanel(weaponGrid.constraintCount, weaponIdx, selectButton);//피봇 설정 관련 개선?
        infoPanel.ShowWeaponInfo(currentWeaponStat);
        var classes = currentWeaponStat.WeaponData.WeaponStat.classes;
        infoPanel.ShowWeaponClassInfo(classes);
    }

    private void ShowItemInfo(ItemData item, SelectButton selectButton,int idx)
    {
        SetInfoPanel(itemGrid.constraintCount, idx, selectButton);
        infoPanel.ShowItemInfo(item);
    }
    
    private void CloseInfoPanel()
    {
        infoPanel.gameObject.SetActive(false);
    }
    
    private void SetWeaponClassBonus(Dictionary<WeaponClasses, int> weaponsBonusDict)
    {
        classInfo.SetWeaponClassBonusDict(weaponsBonusDict);
    }
}
