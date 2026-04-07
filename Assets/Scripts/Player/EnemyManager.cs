using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    //tmp
    [SerializeField] private int health;
    [SerializeField] private int maxHealth;
    
    public void Damage(int damage)
    {
        Debug.Log(damage);
    }

    public void Heal(int healAmount)
    {
        
    }
}
