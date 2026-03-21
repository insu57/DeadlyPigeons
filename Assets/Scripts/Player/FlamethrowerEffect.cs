using System;
using UnityEngine;

[Serializable]
public class FlamethrowerEffect : IWeaponEffect
{
    public void Execute()
    {
        Debug.Log("FlamethrowerEffect Execute");
        
    }
}
