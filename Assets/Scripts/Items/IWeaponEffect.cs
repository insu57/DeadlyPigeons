using UnityEngine;

public interface IWeaponEffect 
{
    public void Execute(IDamageable target, int[] values);
}
