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
    //private List<SelectButton> _weaponSelectButtons = new();
    private Dictionary<int, SelectButton> _weaponSelectBtnDict = new();
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
        playerManager.OnRemoveWeapon += RemoveWeapon;
        playerManager.OnUpgradeWeapon += UpgradeWeapon;
        
        _infoPanelType = infoPanelType;
        if (infoPanelType == InfoPanelType.Store)
        {
            infoPanel.StoreButtons.CombineButton.onClick.AddListener(CombineWeapon);
            infoPanel.StoreButtons.RecycleButton.onClick.AddListener(RecycleWeapon);
            infoPanel.StoreButtons.CancelButton.onClick.AddListener(UnlockPanel);
        }
        
        classInfo.SetWeaponClassBonusDict(playerManager.WeaponClassDict);
        
        OnShowWeaponInfo += playerManager.HandleOnShowWeaponInfo;
        
        
    }
    
    private void AddWeapon(Sprite sprite, int tier, int index)
    {
        var selectBtn = ObjectPoolingManager.Instance.GetSelectBtn();
        selectBtn.SetButtonImg(sprite, tier);
        selectBtn.SetGrid(weaponGrid.transform, weaponGrid.cellSize);
        //_weaponSelectButtons.Add(selectBtn);
        _weaponSelectBtnDict[index] = selectBtn;
        
        InitWeaponEvent(index, selectBtn);
    }

    private void UpgradeWeapon(int idx, int tier)
    {
        //_weaponSelectButtons[idx].SetButtonTier(tier);
        _weaponSelectBtnDict[idx].SetButtonTier(tier);
    }

    private void InitWeaponEvent(int idx, SelectButton selectBtn)
    {
        selectBtn.OnBtnPointerEnter += () => OnShowWeaponInfo?.Invoke(idx, selectBtn, this);
        selectBtn.OnBtnPointerExit += CloseInfoPanel;
        if (_infoPanelType == InfoPanelType.Store)
        {
            selectBtn.SelectBtn.onClick.AddListener(() => OnSelectWeapon(idx, selectBtn));
        }
    }

    private void RemoveWeapon(int idx)
    {
        //var targetSelectBtn = _weaponSelectButtons[idx];
        var targetSelectBtn = _weaponSelectBtnDict[idx];
        targetSelectBtn.ClearEvent();
        ObjectPoolingManager.Instance.ReleaseSelectBtn(targetSelectBtn);
        
        if (_weaponSelectBtnDict.Count > 1)
        {
            for (int i = idx; i < _weaponSelectBtnDict.Count - 1; i++) //idx 당기기.
            {
                _weaponSelectBtnDict[i] = _weaponSelectBtnDict[i + 1]; 
                _weaponSelectBtnDict[i].ClearEvent();
            
                InitWeaponEvent(i, _weaponSelectBtnDict[i]);
            }
        } 
        
        _weaponSelectBtnDict.Remove(_weaponSelectBtnDict.Count - 1);
    }

    private void OnSelectWeapon(int index, SelectButton selectBtn)
    {
        _isLocked = true;
        background.SetActive(true);
        _weaponIdx =  index;
        OnShowWeaponInfo?.Invoke(index, selectBtn, this);
    }

    private void CombineWeapon()
    {
        OnCombineWeapon?.Invoke(_weaponIdx);
        UnlockPanel();
    }

    private void RecycleWeapon()
    {
        OnRecycleWeapon?.Invoke(_weaponIdx);
        UnlockPanel();
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
        selectBtn.SetButtonImg(item.Icon, item.Tier);
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

    public void ShowWeaponInfo(CurrentWeaponStat currentWeaponStat, SelectButton selectButton, 
        int weaponIdx, bool canCombine) //무기 정보 표시
    {
        SetInfoPanel(weaponGrid.constraintCount, weaponIdx, selectButton);//피봇 설정 관련 개선?
        infoPanel.ShowWeaponInfo(currentWeaponStat);
        var classes = currentWeaponStat.WeaponData.WeaponStat.classes;
        infoPanel.ShowWeaponClassInfo(classes);

        if (_infoPanelType == InfoPanelType.Store)
        {
            infoPanel.ShowWeaponStoreButtons(currentWeaponStat.RecyclePrice, canCombine);
        }
        else
        {
            infoPanel.HideWeaponStoreButtons();
        }
    }

    private void ShowItemInfo(ItemData item, SelectButton selectButton,int idx)
    {
        SetInfoPanel(itemGrid.constraintCount, idx, selectButton);
        infoPanel.ShowItemInfo(item);
        infoPanel.HideWeaponStoreButtons();
    }
    
    private void CloseInfoPanel()
    {
        if (_isLocked) return;
        infoPanel.gameObject.SetActive(false);
    }

}
