using System;
using UnityEngine;

public enum CollectableType
{
    Material,
    Food,
    Crate,
}

public class Collectable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    private CollectableType _collectableType;
    public int Amount { get; private set; }

    public void SetType(CollectableType collectableType, int amount)
    {
        _collectableType = collectableType;
        Amount = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerManager player))
        {
            
        }
    }
}
