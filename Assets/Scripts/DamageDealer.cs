using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    private int _damage;
    public void SetDamage(int damage) => _damage = damage;
    private Rigidbody2D _rigidbody2D;
    
    private void Awake()
    {
        TryGetComponent(out _rigidbody2D);
    }

    public void Fire(Vector3 direction)
    {
        _rigidbody2D.linearVelocity = direction.normalized * 10f;
    }
}
