using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClassInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject infoPanelParent;
    [SerializeField] private ClassInfoPanel classInfoPanel;

    public event Action OnShowClassInfoPanel;
    
    private void Awake()
    {
        classInfoPanel.gameObject.SetActive(false);
    }

    public void Init(ClassInfoPanel infoPanel)
    {
        classInfoPanel = infoPanel;
        classInfoPanel.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //infoPanelParent.SetActive(true);
        OnShowClassInfoPanel?.Invoke();
        classInfoPanel.transform.position = infoPanelParent.transform.position;
        //classInfoPanel.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //infoPanelParent.SetActive(false);
        classInfoPanel.gameObject.SetActive(false);
    }

    public void ShowWeaponClassInfo(List<WeaponClasses> classes)
    {
        classInfoPanel.gameObject.SetActive(true);
        classInfoPanel.ShowWeaponClassInfo(classes);
    }

    public void ShowItemClassInfo(List<ItemClass> classes)
    {
        classInfoPanel.gameObject.SetActive(true);
        classInfoPanel.ShowItemClassInfo(classes);
    } 
    
    public void SetWeaponClassBonusDict(Dictionary<WeaponClasses, int> weaponsBonusDict)
    {
        classInfoPanel.SetClassBonusDict(weaponsBonusDict);
    }
}
