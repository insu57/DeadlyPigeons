using System;
using UnityEngine;

public enum CollectableType
{
    Material,
    Meat,
    Crate,
}

[Serializable]
public struct CollectableSprite
{
    public CollectableType collectableType;
    public Sprite sprite;
}

public class Collectable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CollectableSprite[]  collectableSprites;
    public event Action<Collectable, CollectableType> OnPickup;
    
    public CollectableType CollectableType { get; private set; }
    public int Amount { get; private set; }

    public void SetType(CollectableType collectableType, int amount)
    {
        CollectableType = collectableType;
        Amount = amount;
        if (DataManager.Instance.CollectableSpriteDict.TryGetValue(collectableType, out var sprite))
        {
            spriteRenderer.sprite = sprite;
        }
        else
        {
            spriteRenderer.sprite = DataManager.Instance.PlaceHolderSprite;
        }
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IPickup pickup))
        {
            pickup.Pickup(this);
        }
    }

    public void CollectablePickUp()
    {
        OnPickup?.Invoke(this, CollectableType);
    }
}
