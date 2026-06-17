using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct WeaponTransform
{
    public Vector3 spriteOffset;
    public Vector3 muzzleOffset;
    public Vector2 colliderOffset;
    public Vector2 colliderSize;
}

public class WeaponTransformDataSync : MonoBehaviour
{
    [SerializeField] private PlayerWeapon playerWeapon;
    [SerializeField] private WeaponData weaponData;
    
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

        //Transform
        var weaponTransform = new WeaponTransform
        {
            spriteOffset = playerWeapon.WeaponSprite.transform.localPosition,
            muzzleOffset = playerWeapon.Muzzle.localPosition,
            colliderOffset = playerWeapon.MeleeCollider.offset,
            colliderSize = playerWeapon.MeleeCollider.size
        };
        
        weaponData.SetWeaponTransform(weaponTransform);
        
        EditorUtility.SetDirty(weaponData);
        AssetDatabase.SaveAssets();
        Debug.Log($"Update Complete : {weaponData.Name}");
    }
    
#endif


#if UNITY_EDITOR
    [ContextMenu("SetTransformToWeapon")]
    public void SetTransformToWeapon()
    {
        if (!weaponData)
        {
            Debug.LogError("Weapon Data Not Found");
            return;
        }

        if (!playerWeapon)
        {
            Debug.LogError("Player Weapon Not Found");
            return;
        }

        var spriteTransform = playerWeapon.WeaponSprite.transform;
        var muzzle = playerWeapon.Muzzle;
        var meleeCollider = playerWeapon.MeleeCollider;

        //되돌리기 등록(씬 오브젝트 수정)
        Undo.RecordObject(spriteTransform, "Set Transform To Weapon");
        Undo.RecordObject(muzzle, "Set Transform To Weapon");
        Undo.RecordObject(meleeCollider, "Set Transform To Weapon");

        //weaponData의 transform 데이터를 playerWeapon에 적용(런타임 SetWeaponData와 동일)
        spriteTransform.localPosition = weaponData.WeaponTransform.spriteOffset;
        playerWeapon.WeaponSprite.sprite = weaponData.Sprite;

        muzzle.localPosition = weaponData.WeaponTransform.muzzleOffset;

        meleeCollider.offset = weaponData.WeaponTransform.colliderOffset;
        meleeCollider.size = weaponData.WeaponTransform.colliderSize;

        Debug.Log($"Set Transform Complete : {weaponData.Name}");
    }
#endif
}
