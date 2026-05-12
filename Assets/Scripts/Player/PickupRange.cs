using UnityEngine;

public class PickupRange : MonoBehaviour, IPickup
{
    public void Pickup(Collectable collectable)
    {
        if (collectable.CollectableType == CollectableType.Material)
        {
            ObjectPoolingManager.Instance.ReleaseCollectable(collectable);
        }
    }
}
