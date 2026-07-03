using System;
using UnityEngine;

public class PickupRange : MonoBehaviour, IPickup
{
    public event Action<int> OnGetMaterial;
    
    public void Pickup(Collectable collectable)
    {
        if (collectable.CollectableType == CollectableType.Material)
        {
            OnGetMaterial?.Invoke(collectable.Amount);
            collectable.CollectablePickUp();
            ObjectPoolingManager.Instance.ReleaseCollectable(collectable);
        }
    }
    
}
