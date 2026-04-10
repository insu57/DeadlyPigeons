using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour, IDamageable
{
    //tmp
    [SerializeField] private int health;
    [SerializeField] private int maxHealth;
    
    private Coroutine _activeDotCoroutine;
    
    public void Damage(int damage)
    {
        
        health -= damage;
        
        Debug.Log(health);

        if (health <= 0)
        {
            Debug.Log("DEAD");
        }
    }

    public void Heal(int healAmount)
    {
        
    }

    public void DotDamage(int duration, int damage, float tick)
    {
        if (_activeDotCoroutine != null)
        {
            StopCoroutine(_activeDotCoroutine);
        }
        
        _activeDotCoroutine = StartCoroutine(DotDamageCoroutine(duration, damage, tick));
    }

    private IEnumerator DotDamageCoroutine(int duration, int damage, float tick)
    {
        float elapsedTime = 0f;
        
        WaitForSeconds waitTick = new WaitForSeconds(tick);

        while (elapsedTime < duration)
        {
            if (!this)
            {
                yield break;
            }
            
            Damage(damage);
            
            yield return waitTick;

            elapsedTime += tick;
        }
        
        _activeDotCoroutine = null;
    }
}
