using UnityEditor;
using UnityEngine;


public struct WeaponTransform
{
    public Vector3 SpriteScale;
    public Vector3 SpriteOffset;
    public Vector3 SpriteAngle;
    public Vector3 MuzzleOffest;
    public Vector2 ColliderOffset;
    public Vector2 ColliderSize;
}

public class WeaponTransformDataSync : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private CapsuleCollider2D weaponCollider;
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private Transform muzzlePosition;
    
#if UNITY_EDITOR
    [ContextMenu("Update WeaponTransformData")]
    public void UpdateWeaponTransformData()
    {
        if (!weaponData)
        {
            Debug.LogError("Weapon Data Not Found");
            return;
        }
        
        Undo.RecordObject(weaponData, "Update WeaponTransformData");

        var weaponTransform = new WeaponTransform
        {
            SpriteScale = spriteTransform.localScale,
            SpriteOffset = spriteTransform.localPosition,
            SpriteAngle = spriteTransform.localEulerAngles,
            MuzzleOffest = muzzlePosition.position,
            ColliderOffset = weaponCollider.offset,
            ColliderSize = weaponCollider.size
        };
        
        weaponData.SetWeaponTransform(weaponTransform);
        
        EditorUtility.SetDirty(weaponData);
        AssetDatabase.SaveAssets();
        Debug.Log($"Update Complete : {weaponData.Name}");
    }
    
#endif
    
}
