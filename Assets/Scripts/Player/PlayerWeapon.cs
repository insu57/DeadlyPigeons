using System.Collections.Generic;
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
    [SerializeField] private Transform muzzle;
    private Transform _center;
    public WeaponData WeaponData{get; private set;}
    private TargetInfo _targetInfo;
    private int _currentTierIdx;
    private float _currentTimer;
    private const int RangeScaler = 75;
    private const float MeleeRangeMultiplier = 0.5f;
    private MeleeAttack _meleeAttack;
    private bool _isAttacking;
    private float _animTimer;
    [SerializeField] private float attackDuration = 1;//근거리 - 찌르기/휩쓸기를 하는 시간 <- 무기마다 수정필요?
    [SerializeField] private float sweepAngle = 120;
    [SerializeField] private float thrustDist = 2;

    private Dictionary<MainStats, int> _mainStats = new();
    private Dictionary<SubStats, int> _subStats = new();
    //private CapsuleCollider2D _collider;

    private void Awake()
    {
        TryGetComponent(out _meleeAttack);
        _center = transform.parent;
    }
    
    private void Update()
    {
        RotateWeapon();
        Attack();
        MeleeAnimation();
    }

    public void SetWeaponData(WeaponData weaponData, int tier)
    {
        WeaponData = weaponData;

        weaponSprite.sprite = weaponData.Sprite;
        weaponSprite.transform.localPosition = weaponData.SpriteOffset;
        weaponSprite.transform.localRotation = Quaternion.Euler(weaponData.SpriteAngle);
        weaponSprite.transform.localScale = weaponData.SpriteScale;

        meleeCollider.size = weaponData.ColliderSize;
        meleeCollider.offset = weaponData.ColliderOffset;
        meleeCollider.enabled = false;

        if (!weaponData.WeaponStat.isMelee)
        {
            muzzle.position = weaponData.MuzzleOffset;
        }
        
        _currentTierIdx = tier - 1;//인덱스은 0부터(1감소)
        
        if(!weaponData.WeaponStat.isMelee) meleeCollider.enabled = false; //원거리면 비활성.
    }

    public void SetTarget(TargetInfo target)
    {
        _targetInfo = target;
    }

    public void UpdateMainStats(MainStats stat, int value)
    {
        _mainStats[stat] = value;
    }

    public void UpdateSubStats(SubStats stat, int value)
    {
        _subStats[stat] = value;
    }

    private void RotateWeapon()
    {
        //타겟
        if(_isAttacking) return; //공격 중(근거리) 회전x
         
        if (!_targetInfo.Target) //타겟이 없으면 기본 각도
        {
            weaponSprite.transform.rotation = Quaternion.identity;
            return;
        }
        
        //타겟을 바라보도록 회전
        Vector3 dir =  _targetInfo.Target.position - transform.position;
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
        if(!_targetInfo.Target) return; //타겟이 있을 때만.
        if(_isAttacking) return;
        if(WeaponData.WeaponStat.isMelee) MeleeAttack();
        else RangedAttack();
    }
    //적정 Range 조정은 테스트하면서 변경. 현재는 조금 사거리가 짧은듯

    private void MeleeAttack()
    {
        var finalRange = (WeaponData.WeaponStat.range[_currentTierIdx] + _mainStats[MainStats.Range] / 2f)
                         / RangeScaler;
        
        if(finalRange * finalRange < _targetInfo.SqrDistance) return; //범위 밖
        
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
            _currentTimer = WeaponData.WeaponStat.attackSpeed[_currentTierIdx];
            weaponSprite.transform.localPosition = Vector3.zero;
            weaponSprite.transform.localRotation = Quaternion.identity;
            meleeCollider.enabled = false;
            return;
        }

        var attackType = WeaponData.WeaponStat.attackType;
        
        //1.Range제한.
        //호를 그리기.
        if (attackType == AttackType.Sweep) //가장 가까운 적(target)
        {
            //float radius = 
            //대략 1유닛 정도 앞에서 호를 그리기.
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
        float finalRange = (float)(WeaponData.WeaponStat.range[_currentTierIdx] + _mainStats[MainStats.Range] )
                            /  RangeScaler;
        
        if (finalRange * finalRange < _targetInfo.SqrDistance)
        {
            return;
        }
        
        if (_currentTimer > 0)
        {
            _currentTimer -= Time.deltaTime;
        }
        else
        {
            //투사체 만큼 발사.
            //투사체 각도!
            var projectile = ObjectPoolingManager.Instance.GetProjectile();
            projectile.transform.position = muzzle.position;//
            var dir = _targetInfo.Target.position - muzzle.position;
            
            projectile.Fire(dir, finalRange);

            _currentTimer = WeaponData.WeaponStat.attackSpeed[_currentTierIdx];
        }
        
    }
}
