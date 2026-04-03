using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponClassInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject infoPanel;

    private void Awake()
    {
        infoPanel.SetActive(false);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter: WeaponClass");
        infoPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        infoPanel.SetActive(false);
    }
}
