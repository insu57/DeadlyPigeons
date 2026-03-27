using UnityEngine;

public class MeleeAttack : MonoBehaviour, IDealer
{
    private int _damage;
    
    public void SetDamage(int damage)
    {
        _damage = damage;
    }
}
