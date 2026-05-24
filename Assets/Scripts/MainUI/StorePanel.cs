using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorePanel : MonoBehaviour
{
    [field: SerializeField] private InfoPanel infoPanel;
    [field: SerializeField] private Button buyBtn;
    [field: SerializeField] private TextMeshProUGUI priceText;

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
    
    public void SetStorePanel(ItemData itemData, int price)
    {
        var sb = StatUtil.StringBuilder;
        
        infoPanel.ShowItemInfo(itemData);
        
        sb.Clear();
        sb.Append(price);
        priceText.SetText(sb);
    }

    public void SetStorePanel(CurrentWeaponStat currentWeaponStat, int price)
    {
        var sb = StatUtil.StringBuilder;
        
        infoPanel.ShowWeaponInfo(currentWeaponStat);
        
        sb.Clear();
        sb.Append(price);
        priceText.SetText(sb);
    }
    
    
}
