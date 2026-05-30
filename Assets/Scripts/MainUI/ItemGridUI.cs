using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public enum InfoPanelType
{
    Main,
    Store
}

public class ItemGridUI : MonoBehaviour
{
    [SerializeField] private SelectButton selectBtnPrefab;
    [SerializeField] private GridLayoutGroup weaponGrid;
    [SerializeField] private GridLayoutGroup itemGrid;
    [SerializeField] private InfoPanel infoPanel;
    private InfoPanelType _infoPanelType;
    [SerializeField] private ClassInfo classInfo;
    [SerializeField] private GameObject background;
    private bool _isLocked;
    private const int MinCols = 3;
    public event Action<int, SelectButton, ItemGridUI> OnShowWeaponInfo; //ID
    private int _weaponIdx = -1;
    public event Action<int> OnCombineWeapon;
    public event Action<int> OnRecycleWeapon;
    

    public void Init(PlayerManager playerManager, InfoPanelType infoPanelType)
    {
        playerManager.OnAddWeapon += AddWeapon;
        playerManager.OnAddItem +=  AddItem;
        
        _infoPanelType = infoPanelType;
        if (infoPanelType == InfoPanelType.Store)
        {
            //Combine
            //Recycle //Remove Weapon 처리!
            infoPanel.StoreButtons.CombineButton.onClick.AddListener(() => OnCombineWeapon?.Invoke(_weaponIdx));
            infoPanel.StoreButtons.RecycleButton.onClick.AddListener(() => OnRecycleWeapon?.Invoke(_weaponIdx));
            infoPanel.StoreButtons.CancelButton.onClick.AddListener(UnlockPanel);
        }
        
        classInfo.SetWeaponClassBonusDict(playerManager.WeaponClassDict);
        
        OnShowWeaponInfo += playerManager.HandleOnShowWeaponInfo;
    }
    
    private void AddWeapon(Sprite sprite, int index)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(sprite);
        selectBtn.SetGrid(weaponGrid.transform, weaponGrid.cellSize);
        
        selectBtn.OnBtnPointerEnter += () => OnShowWeaponInfo?.Invoke(index, selectBtn, this);
        selectBtn.OnBtnPointerExit += CloseInfoPanel;
        
        if (_infoPanelType == InfoPanelType.Store)
        {
            selectBtn.SelectBtn.onClick.AddListener(() => OnSelectWeapon(index, selectBtn));
        }
    }

    private void OnSelectWeapon(int index, SelectButton selectBtn)
    {
        _isLocked = true;
        background.SetActive(true);
        _weaponIdx =  index;
        OnShowWeaponInfo?.Invoke(index, selectBtn, this);
    }

    private void UnlockPanel()
    {
        _isLocked = false;
        background.SetActive(false);
        infoPanel.gameObject.SetActive(false);
    }
    
    private void AddItem(ItemData item, int idx)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(item.Icon);
        selectBtn.SetGrid(itemGrid.transform, itemGrid.cellSize);
        
        selectBtn.OnBtnPointerEnter += () => ShowItemInfo(item, selectBtn, idx);
        selectBtn.OnBtnPointerExit += CloseInfoPanel;
    }
    
    private void SetInfoPanel(int cols, int idx, SelectButton selectButton)
    {
        Transform panelParent;
        var panelRT = (RectTransform)infoPanel.transform;

        if (_infoPanelType == InfoPanelType.Main)//메인 - pivot이 top으로
        {
            if (idx < cols / 2)
            {
                panelParent = selectButton.InfoPanelParentBottomLeft;
                panelRT.pivot = new Vector2(0, 1);//top-left
            }
            else
            {
                panelParent = selectButton.InfoPanelParentBottomRight;
                panelRT.pivot = new Vector2(1, 1); //top-right
            }
        }
        else //상점 - pivot이 bottom
        {
            if (idx < cols / 2)
            {
                panelRT.pivot = new Vector2(0, 0);//bottom-left
                panelParent = selectButton.InfoPanelParentTopLeft;
            }
            else
            {
                panelRT.pivot = new Vector2(1, 0);//bottom-right
                panelParent = selectButton.InfoPanelParentTopRight;
            }

            if (cols <= MinCols)
            {
                panelRT.pivot = new Vector2(0, 0);//bottom-left
                panelParent = selectButton.InfoPanelParentTopLeft;
            }
        }

        infoPanel.transform.position = panelParent.position;
        infoPanel.gameObject.SetActive(true);
    }

    public void ShowWeaponInfo(CurrentWeaponStat currentWeaponStat, SelectButton selectButton, int weaponIdx) //무기 정보 표시
    {
        SetInfoPanel(weaponGrid.constraintCount, weaponIdx, selectButton);//피봇 설정 관련 개선?
        infoPanel.ShowWeaponInfo(currentWeaponStat);
        var classes = currentWeaponStat.WeaponData.WeaponStat.classes;
        infoPanel.ShowWeaponClassInfo(classes);

        if (_infoPanelType == InfoPanelType.Store)
        {
            infoPanel.ShowWeaponStoreButtons(true, currentWeaponStat.RecyclePrice);
        }
        else
        {
            infoPanel.ShowWeaponStoreButtons(false,0);
        }
    }

    private void ShowItemInfo(ItemData item, SelectButton selectButton,int idx)
    {
        SetInfoPanel(itemGrid.constraintCount, idx, selectButton);
        infoPanel.ShowItemInfo(item);
        infoPanel.ShowWeaponStoreButtons(false,0);
    }
    
    private void CloseInfoPanel()
    {
        if (_isLocked) return;
        infoPanel.gameObject.SetActive(false);
    }

}
