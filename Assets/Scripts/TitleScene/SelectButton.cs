using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image btnImage;
    [SerializeField] private TMP_Text btnText;
    [SerializeField] private Button selectBtn;
    [field: SerializeField] public Transform InfoPanelParentLeft { get; private set; }
    [field: SerializeField] public Transform InfoPanelParentRight { get; private set; }
    [field: SerializeField] public Transform InfoPanelParentTop { get; private set; }
    public Button SelectBtn => selectBtn;
    public event Action OnBtnPointerEnter;
    public event Action OnBtnPointerExit;

    public void ClearSelectBtn()
    {
        ClearEvent();
        
        btnImage.enabled = false;
        btnText.enabled = false;
    }
    
    public void SetButtonImg(Sprite sprite)
    {
        btnImage.enabled = true;
        btnImage.sprite = sprite;
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
}
