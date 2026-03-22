using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private SpriteRenderer weaponSprite;
    private WeaponData _weaponData;
    private Transform _target;
    [SerializeField] private GameObject bullet; //풀링으로 변경
    
    private CapsuleCollider2D _collider;

    private void Awake()
    {
        TryGetComponent(out _collider);
    }
    
    private void Update()
    {
        RotateWeapon();
    }

    public void SetWeaponData(WeaponData weaponData)
    {
        _weaponData = weaponData;

        weaponSprite.sprite = weaponData.Sprite;
        weaponSprite.transform.localPosition = weaponData.SpriteOffset;
        weaponSprite.transform.localEulerAngles = weaponData.SpriteAngle;
        weaponSprite.transform.localScale = weaponData.SpriteScale;

        _collider.size = weaponData.ColliderSize;
        _collider.offset = weaponData.ColliderOffset;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void RotateWeapon()
    {
        //타겟
    }
}
