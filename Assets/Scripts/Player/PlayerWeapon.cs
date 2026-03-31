using System;
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
    [SerializeField] private Transform hitbox;
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
    private float _targetDist;
    private bool _isAttacking;
    private float _animTimer;
    [SerializeField] private float attackDuration = 1;//근거리 - 찌르기/휩쓸기를 하는 시간 <- 무기마다 수정필요?
    [SerializeField] private float sweepAngle = 120;
    private float _startAngle;
    private float _endAngle;
    [SerializeField] private float thrustDist = 2;
    private const float MinMeleeRange = .5f;
    
    private Dictionary<MainStats, int> _mainStats = new();
    private Dictionary<SubStats, int> _subStats = new();
    
    //스탯별 처리(음수 처리 따로)
    
    private void Awake()
    {
        TryGetComponent(out _meleeAttack);
        //_center = transform.parent;
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

    public void SetCenter(Transform center)
    {
        _center = center;
    }
    
    private void RotateWeapon()
    {
        //타겟
        if(_isAttacking) return; //공격 중(근거리) 회전x
         
        if (!_targetInfo.Target) //타겟이 없으면 기본 각도
        {
            transform.rotation = Quaternion.identity;
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
        var finalRange = (WeaponData.WeaponStat.range[_currentTierIdx] 
                          + _mainStats[MainStats.Range] * MeleeRangeMultiplier) / RangeScaler;
        
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

            _targetDist = MathF.Sqrt(_targetInfo.SqrDistance);//루트 연산(실제 거리)
            transform.position = _center.position; //중앙으로
            
            //공격 유형
            //None Sweep Thrust..
            //특수한 공격(근접무기) 인 경우 None으로
            float range = MathF.Max(MinMeleeRange, _targetDist - 1); //1유닛 여유(스프라이트 크기고려) -> 스프라이트마다 변경?            
            var attackType = WeaponData.WeaponStat.attackType;
            if (attackType == AttackType.Sweep)
            {
                Vector3 dirToTarget = _targetInfo.Target.position - transform.position;
                float centerAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
                float halfAngle = sweepAngle / 2f;
                //y축 보정
                hitbox.localPosition = new Vector3(range, 0, 0);

                if (Mathf.Abs(centerAngle) > 90f)
                {
                    _startAngle = centerAngle - halfAngle;
                    _endAngle = centerAngle + halfAngle;
                }
                else
                {
                    _startAngle = centerAngle + halfAngle;
                    _endAngle = centerAngle - halfAngle;
                }
                
                
            }

            else if (attackType == AttackType.Thrust)
            {
                
            }
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
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            hitbox.localPosition = Vector3.zero;
            hitbox.localRotation = Quaternion.identity;
            meleeCollider.enabled = false;
            return;
        }

        var attackType = WeaponData.WeaponStat.attackType;
        //1.Range제한.
        //호를 그리기.
        if (attackType == AttackType.Sweep) //가장 가까운 적(target)
        {
            
            //대략 1유닛 정도 앞에서 호를 그리기.
            float currentAngle = Mathf.LerpAngle(_startAngle, _endAngle, percent);
            transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
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
