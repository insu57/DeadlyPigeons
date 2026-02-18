using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectBtn : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image CharacterImage;
    [SerializeField] private Button selectBtn;
    public Button SelectBtn => selectBtn;
    public event Action OnBtnPointerEnter;
    
    public void SetCharacterImg(Sprite sprite)
    {
        CharacterImage.sprite = sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnBtnPointerEnter?.Invoke();
    }
}
