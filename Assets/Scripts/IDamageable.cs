using UnityEngine;

public interface IDamageable
{
    public void Damage(int damage, bool isCrit);
    public void Heal(int healAmount);
    public void DotDamage(int duration, int damage, float tick);
}
