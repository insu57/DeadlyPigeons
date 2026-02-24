using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image btnImage;
    [SerializeField] private Button selectBtn;
    public Button SelectBtn => selectBtn;
    public event Action OnBtnPointerEnter;
    
    public void SetButtonImg(Sprite sprite)
    {
        btnImage.sprite = sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnBtnPointerEnter?.Invoke();
    }
}
