using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StorePanel : MonoBehaviour
{
    [field: SerializeField] private InfoPanel infoPanel;
    [field: SerializeField] public Button BuyBtn { get; private set; }
    [field: SerializeField] private TextMeshProUGUI priceText;
    [field: SerializeField] private GameObject parent;
    
    public event Action<int> OnShowClassInfoPanel;
    
    public void InitInfoPanel(ClassInfoPanel classInfoPanel)
    {
        infoPanel.Init(classInfoPanel);
    }

    public void InitStorePanel(int idx)
    {
        infoPanel.OnShowClassInfoPanel += () => OnShowClassInfoPanel?.Invoke(idx);
    }

    public void ShowClassInfoPanel(List<WeaponClasses> classes)
    {
        infoPanel.ShowWeaponClassInfo(classes);
    }

    public void ShowClassInfoPanel(List<ItemClass> classes)
    {
        infoPanel.ShowItemClassInfo(classes);
    }
    
    public void SetStorePanel(ItemData itemData, int price, bool canBuy)
    {
        var sb = StatUtil.StringBuilder;
        
        infoPanel.ShowItemInfo(itemData);
        
        sb.Clear();
        if (canBuy) sb.Append(price);
        else sb.AppendColor(price, -1);
        priceText.SetText(sb);
    }

    public void SetStorePanel(CurrentWeaponStat currentWeaponStat, int price, bool canBuy)
    {
        var sb = StatUtil.StringBuilder;
        
        infoPanel.ShowWeaponInfo(currentWeaponStat);
        
        sb.Clear();
        if (canBuy) sb.Append(price);
        else sb.AppendColor(price, -1);
        priceText.SetText(sb);
    }

    public void ActiveStorePanel(bool isActive)
    {
        parent.SetActive(isActive);
    }
}
