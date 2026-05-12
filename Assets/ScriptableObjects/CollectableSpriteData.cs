using UnityEngine;

[CreateAssetMenu(fileName = "CollectableSpriteData", menuName = "Scriptable Objects/CollectableSpriteData")]
public class CollectableSpriteData : ScriptableObject
{
    [field: SerializeField] public CollectableSprite[] CollectableSprites { get; private set; }
}
