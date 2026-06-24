using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SfxKeyValue
{
    public SfxType type;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "SfxData", menuName = "Scriptable Objects/SfxData")]
public class SfxData : ScriptableObject
{
    [field: SerializeField] public List<SfxKeyValue> Values { get; private set; }
}
