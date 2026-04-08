using UnityEngine;

public class Explosion : MonoBehaviour, IWeaponEffect
{
    public void Execute(IDamageable damageable, int[] values)
    {
        //values = { 폭발 범위, 폭발 데미지(최종) }
    }
}
