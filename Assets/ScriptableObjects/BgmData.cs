using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BgmKeyValue
{
    public BgmType bgmType;
    public AudioClip clip;
}
[CreateAssetMenu(fileName = "BgmData", menuName = "Scriptable Objects/BgmData")]
public class BgmData : ScriptableObject
{
    [field: SerializeField] public List<BgmKeyValue> Values { get; private set; }
}
