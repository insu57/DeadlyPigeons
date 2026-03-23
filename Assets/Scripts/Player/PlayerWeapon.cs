using UnityEngine;

public enum AttackType
{
    None,
    Sweep,
    Thrust
}

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
        Attack();
    }

    public void SetWeaponData(WeaponData weaponData)
    {
        _weaponData = weaponData;

        weaponSprite.sprite = weaponData.Sprite;
        weaponSprite.transform.localPosition = weaponData.SpriteOffset;
        weaponSprite.transform.localRotation = Quaternion.Euler(weaponData.SpriteAngle);
        weaponSprite.transform.localScale = weaponData.SpriteScale;

        _collider.size = weaponData.ColliderSize;
        _collider.offset = weaponData.ColliderOffset;
        
        if(!weaponData.WeaponStat.isMelee) _collider.enabled = false; //원거리면 비활성.
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void RotateWeapon()
    {
        //타겟
        if (!_target)
        {
            transform.rotation = Quaternion.identity;
            return;
        }
        
        Vector3 dir =  _target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        //Y-flip
        if (Mathf.Abs(angle) > 90)
        {
            transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void Attack()
    {
        if(_weaponData.WeaponStat.isMelee) MeleeAttack();
        else RangedAttack();
    }

    private void MeleeAttack()
    {
        //공격 유형
        //None Sweep Thrust..
        //특수한 공격(근접무기) 인 경우 None으로
    }

    private void RangedAttack()
    {
        //투사체 만큼 발사.
        
        
    }
}
