using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image btnImage;
    [SerializeField] private TMP_Text btnText;
    [SerializeField] private Button selectBtn;
    [SerializeField] private Image borderImage;
    [field: SerializeField] public RectTransform InfoPanelParentBottomLeft { get; private set; }
    [field: SerializeField] public RectTransform InfoPanelParentBottomRight { get; private set; }
    [field: SerializeField] public RectTransform InfoPanelParentTopLeft { get; private set; }
    [field: SerializeField] public RectTransform InfoPanelParentTopRight { get; private set; }
    public Button SelectBtn => selectBtn;
    public event Action OnBtnPointerEnter;
    public event Action OnBtnPointerExit;

    public void ClearSelectBtn()
    {
        ClearEvent();
        
        btnImage.enabled = false;
        btnText.enabled = false;
    }
    
    public void SetButtonImg(Sprite sprite, int tier)
    {
        btnImage.enabled = true;
        btnImage.sprite = sprite;
        if (tier <= 0)
        {
            borderImage.enabled = false;
            return;
        }
        borderImage.enabled = true;
        var color = DataManager.Instance.GetTierToColor(tier);
        borderImage.color = color;
    }

    public void SetBtnText(StringBuilder sb)
    {
        btnText.enabled = true;
        btnText.SetText(sb);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnBtnPointerEnter?.Invoke();
    }

    public void ClearEvent()
    {
        OnBtnPointerEnter = null;
        OnBtnPointerExit = null;
        SelectBtn.onClick.RemoveAllListeners();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnBtnPointerExit?.Invoke();
    }

    public void SetGrid(Transform gridTransform, Vector2 cellSize)
    {
        transform.SetParent(gridTransform, false);
        
        InfoPanelParentBottomLeft.anchoredPosition = new Vector2(0, -cellSize.y);
        InfoPanelParentBottomRight.anchoredPosition = new Vector2(0, -cellSize.y);
        InfoPanelParentTopLeft.anchoredPosition = new Vector2(0, cellSize.y);
        InfoPanelParentTopRight.anchoredPosition = new Vector2(0, cellSize.y);
    }
}
