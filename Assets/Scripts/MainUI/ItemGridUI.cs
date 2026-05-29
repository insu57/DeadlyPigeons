using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public enum InfoPanelPivot
{
    Top,
    Bottom
}

public class ItemGridUI : MonoBehaviour
{
    [SerializeField] private SelectButton selectBtnPrefab;
    [SerializeField] private GridLayoutGroup weaponGrid;
    [SerializeField] private GridLayoutGroup itemGrid;
    [SerializeField] private InfoPanel infoPanel;
    private InfoPanelPivot _infoPanelPivot;
    [SerializeField] private ClassInfo classInfo;
    [SerializeField] private GameObject background;
    private const int MinCols = 3;
    
    public event Action<int, SelectButton, ItemGridUI> OnShowWeaponInfo; //ID
    
    public void Init(PlayerManager playerManager, InfoPanelPivot infoPanelPivot)
    {
        playerManager.OnAddWeapon += AddWeapon;
        playerManager.OnAddItem +=  AddItem;
        
        _infoPanelPivot = infoPanelPivot;
        
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
    }
    
    private void AddItem(ItemData item, int idx)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(item.Icon);
        selectBtn.SetGrid(itemGrid.transform, itemGrid.cellSize);
        
        selectBtn.OnBtnPointerEnter += () => ShowItemInfo(item, selectBtn, idx);
        selectBtn.OnBtnPointerExit += CloseInfoPanel;
    }
    
    //피봇조절???
    private void SetInfoPanel(int cols, int idx, SelectButton selectButton)
    {
        Transform panelParent;
        var panelRT = (RectTransform)infoPanel.transform;

        if (_infoPanelPivot == InfoPanelPivot.Top)//pivot이 top으로
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
        else //pivot이 bottom
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

}
