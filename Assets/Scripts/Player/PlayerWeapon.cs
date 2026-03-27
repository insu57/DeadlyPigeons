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
    [SerializeField] private CapsuleCollider2D meleeCollider;
    private WeaponData _weaponData;
    private Transform _target;
    private int _currentTier;
    private float _currentTimer;
    private const int RangeScaler = 100;
    private const float MeleeRangeMultiplier = 0.5f;
    private MeleeAttack _meleeAttack;
    private bool _isAttacking;
    private float _animTimer;
    [SerializeField] private float attackDuration;//근거리 - 찌르기/휩쓸기를 하는 시간 <- 무기마다 수정필요?
    [SerializeField] private float sweepAngle;
    [SerializeField] private float thrustDist;
    
    //private CapsuleCollider2D _collider;

    private void Awake()
    {
        TryGetComponent(out _meleeAttack);
    }
    
    private void Update()
    {
        RotateWeapon();
        Attack();
        MeleeAnimation();
    }

    public void SetWeaponData(WeaponData weaponData, int tier)
    {
        _weaponData = weaponData;

        weaponSprite.sprite = weaponData.Sprite;
        weaponSprite.transform.localPosition = weaponData.SpriteOffset;
        weaponSprite.transform.localRotation = Quaternion.Euler(weaponData.SpriteAngle);
        weaponSprite.transform.localScale = weaponData.SpriteScale;

        meleeCollider.size = weaponData.ColliderSize;
        meleeCollider.offset = weaponData.ColliderOffset;
        meleeCollider.enabled = false;
        
        _currentTier = tier;
        
        if(!weaponData.WeaponStat.isMelee) meleeCollider.enabled = false; //원거리면 비활성.
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void RotateWeapon()
    {
        //타겟
        if(_isAttacking) return; //공격 중(근거리) 회전x
         
        if (!_target) //타겟이 없으면 기본 각도
        {
            weaponSprite.transform.rotation = Quaternion.identity;
            return;
        }
        
        //타겟을 바라보도록 회전
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
        if(!_target) return; //타겟이 있을 때만.
        if(_isAttacking) return;
        if(_weaponData.WeaponStat.isMelee) MeleeAttack();
        else RangedAttack();
    }

    private void MeleeAttack()
    {
        if (_currentTimer > 0)
        {
            _currentTimer -= Time.deltaTime;
        }
        else
        {
            _isAttacking = true;
            _animTimer = 0f;
            meleeCollider.enabled = true;
            //공격 유형
            //None Sweep Thrust..
            //특수한 공격(근접무기) 인 경우 None으로
        }
        
        
    }

    private void MeleeAnimation() //수정 필요...
    {
        if(!_isAttacking) return;
        
        _animTimer += Time.deltaTime;

        float percent = _animTimer / attackDuration;
        if (percent >= 1f)
        {
            _isAttacking = false;
            _currentTimer = _weaponData.WeaponStat.attackSpeed[_currentTier];
            weaponSprite.transform.localPosition = Vector3.zero;
            weaponSprite.transform.localRotation = Quaternion.identity;
            meleeCollider.enabled = false;
            return;
        }

        var attackType = _weaponData.WeaponStat.attackType;
        if (attackType == AttackType.Sweep)
        {
            float startAngle = sweepAngle / 2f;
            float endAngle = -sweepAngle / 2f;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, percent);
            
            weaponSprite.transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }
        else if (attackType == AttackType.Thrust)
        {
            
        }
    }

    private void RangedAttack()
    {
        if (_currentTimer > 0)
        {
            _currentTimer -= Time.deltaTime;
        }
        else
        {
            //투사체 만큼 발사.
            var projectile = ObjectPoolingManager.Instance.GetProjectile();
            projectile.transform.position = transform.position;//
            var dir = _target.position - transform.position;
            float range = (float)_weaponData.WeaponStat.range[_currentTier] /  RangeScaler;
            projectile.Fire(dir, range);

            _currentTimer = _weaponData.WeaponStat.attackSpeed[_currentTier];
        }
        
    }
}
