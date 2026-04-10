using System.Collections.Generic;
using UnityEngine;

public interface IWeaponEffect
{
    public IWeaponEffect Clone();
    public void Init(PlayerWeapon playerWeapon, List<float> value);
    public void Execute(IDamageable target, List<float> values);
}
