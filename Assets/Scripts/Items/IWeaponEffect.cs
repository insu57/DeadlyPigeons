using System.Collections.Generic;
using UnityEngine;

public interface IWeaponEffect
{
    public IWeaponEffect Clone();
    public void Init(PlayerWeapon playerWeapon, List<float> value); //무기 초기화 시 호출
    public void SetExecuteData(PlayerWeapon playerWeapon, List<float> values);
    public void Execute(IDamageable target); //공격(착탄) 시 호출(데미지) 
    public void Remove(PlayerWeapon playerWeapon);
}
