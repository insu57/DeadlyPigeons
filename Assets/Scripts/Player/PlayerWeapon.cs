using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

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

    private int _finalDamage;
    private List<IWeaponEffect> _weaponEffects = new();
    
    //Melee
    private bool _isAttacking;
    private float _animTimer;
    [SerializeField] private float attackDuration = 0.3f;//근거리 - 찌르기/휩쓸기를 하는 시간 <- 무기마다 수정필요?
    [SerializeField] private float sweepAngle = 90;
    private float _startAngle;
    private float _endAngle;
    [SerializeField] private float thrustDist = 2;
    private float _thrustDist;
    private const float MinMeleeRange = .5f;
    private float _meleeRange;
    
    //Ranged
    private int _piercing = 0; //기본
    private int _piercingDmgPer = -50;
    
    //Both
    public float BurningTick { get; private set; } = 1f; //아이템 반영 생각

    public void SetPiercing(int piercing, int piercingDmg)
    {
        _piercing = piercing;
        _piercingDmgPer = piercingDmg;
    }
    //public void 
    
    private int _bounces = 0;

    public void SetBounces(int bounces)
    {
        _bounces = bounces;
    }
    
    
    private Dictionary<MainStats, int> _mainStats = new();
    private Dictionary<SubStats, int> _subStats = new();
    
    //스탯별 처리(음수 처리 따로)
    
    private void Awake()
    {
        _meleeAttack = GetComponentInChildren<MeleeAttack>();
        
        for (int i = 0; i < (int)MainStats.None; i++)
        {
            _mainStats[(MainStats)i] = 0;
        }

        for (int i = 0; i < (int)SubStats.None; i++)
        {
            _subStats[(SubStats)i] = 0;
        }
        
    }
    
    private void Update()
    {
        if(!WeaponData) return;
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
        meleeCollider.gameObject.layer = DataManager.Instance.PlayerHitboxLayer;

        if (!weaponData.WeaponStat.isMelee)
        {
            muzzle.localPosition = weaponData.MuzzleOffset;
        }
        
        _currentTierIdx = tier - weaponData.WeaponStat.initTier;//인덱스은 0부터 초기 티어 만큼 차감

        var weaponEffectList = weaponData.WeaponEffectValues;
        foreach (var weaponEffectData in weaponEffectList)
        {
            var effect =  weaponEffectData.effect.Clone();
            _weaponEffects.Add(effect);
            List<float> initValues = new();
            foreach (var effectValues in weaponEffectData.initValuesList) //무기 초기화 시 효과
            {
                var value = effectValues.values[_currentTierIdx]; //무기 효과 해당 티어 값
                int multiplier; //스탯 계수(티어 값)
                if (effectValues.mainStat != MainStats.None)
                {
                    multiplier = effectValues.multipliers[_currentTierIdx];
                    value += (int)( _mainStats[effectValues.mainStat] * multiplier / 100f);
                }
                else if (effectValues.subStat != SubStats.None)
                {
                    multiplier = effectValues.multipliers[_currentTierIdx];
                    value += (int)(_subStats[effectValues.subStat] * multiplier / 100f);
                }
                
                initValues.Add(value); //현재 티어 효과 수치 + 스탯 계수
            }
            effect.Init(this, initValues); //수치 주입
        }
        
        if(!weaponData.WeaponStat.isMelee) meleeCollider.enabled = false; //원거리면 비활성.
    }

    public void SetTarget(TargetInfo target)
    {
        _targetInfo = target;
    }

    public void UpdateMainStats(MainStats stat, int value)
    {
        _mainStats[stat] = value;
        _finalDamage = GetDamage();
    }

    public void UpdateSubStats(SubStats stat, int value)
    {
        _subStats[stat] = value;
        _finalDamage = GetDamage();
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
        //(기본 범위 + 범위 스탯 / 근거리무기 범위 배율) / 범위 조절(유닛)
        
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
            _meleeRange = MathF.Max(MinMeleeRange, _targetDist - 1); //1유닛 여유(스프라이트 크기고려) -> 개선 방안?   
            Vector3 dirToTarget = _targetInfo.Target.position - _center.position; //방향벡터
            float centerAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg; //중심각
            hitbox.localPosition = new Vector3(_meleeRange, 0, 0); //해당 위치로 이동
            
            var critChance = WeaponData.WeaponStat.critChance[_currentTierIdx] + _mainStats[MainStats.CritChance];
            bool isCrit = Random.value < critChance / 100f;
            _meleeAttack.SetDamage(_finalDamage, isCrit, _weaponEffects);
            
            var attackType = WeaponData.WeaponStat.attackType;
            if (attackType == AttackType.Sweep)
            {
                float halfAngle = sweepAngle / 2f;
                
                
                if (Mathf.Abs(centerAngle) > 90f) //시작-끝 각도 계산(중심각 기준)
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
                transform.localRotation = Quaternion.Euler(0, 0, centerAngle); //타겟 조준
            }
        }
    }

    private void MeleeAnimation()//근거리 무기 공격 애니메이션
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
        if (attackType == AttackType.Sweep) 
        {
            //대략 1유닛 정도 앞에서 호를 그리기.
            float currentAngle = Mathf.LerpAngle(_startAngle, _endAngle, percent);
            transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }
        else if (attackType == AttackType.Thrust)
        {
            float moveFactor = Mathf.PingPong(percent * 2f, attackDuration);
            hitbox.localPosition = new Vector3(moveFactor * _meleeRange, 0, 0);
        }
    }

    private void RangedAttack()
    {
        float finalRange = (float)(WeaponData.WeaponStat.range[_currentTierIdx] + _mainStats[MainStats.Range] )
                            /  RangeScaler;
        //(기본 범위 + 범위 스탯 / 근거리무기 범위 배율) / 범위 조절(유닛)
        
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
            
            List<float> currentExecuteValues = new();
            for (int i = 0; i < _weaponEffects.Count; i++)
            {
                var weaponEffect = _weaponEffects[i];
                var weaponEffectData = WeaponData.WeaponEffectValues[i];
                var effectValues = weaponEffectData.executeValuesList;
                foreach (var effectValue in effectValues)
                {
                    var value = effectValue.values[_currentTierIdx];
                    if (effectValue.mainStat != MainStats.None)
                    {
                        var multiplier = effectValue.multipliers[_currentTierIdx];
                        value += (int)(_mainStats[effectValue.mainStat] * multiplier / 100f);
                    }
                    else if (effectValue.subStat != SubStats.None)
                    {
                        var multiplier = effectValue.multipliers[_currentTierIdx];
                        value += (int)(_subStats[effectValue.subStat] * multiplier / 100f);
                    }
                    currentExecuteValues.Add(value);
                }
                weaponEffect.SetExecuteData(this, currentExecuteValues);
            }

            var critChance = WeaponData.WeaponStat.critChance[_currentTierIdx] + _mainStats[MainStats.CritChance];
            bool isCrit = Random.value < critChance / 100f;
            
            var projectileInitData = new ProjectileInitData
            {
                Damage = _finalDamage,
                Piercing =  _piercing,
                PiercingDmgPer =  _piercingDmgPer,
                Bounces = _bounces,
                HitLayer = DataManager.Instance.PlayerHitboxLayer,
                IsCrit = isCrit,
                WeaponEffects = _weaponEffects
            };
            projectile.Initialize(projectileInitData);
            projectile.Fire(dir, finalRange);

            _currentTimer = WeaponData.WeaponStat.attackSpeed[_currentTierIdx];
        }
        
    }

    private int GetDamage()
    {
        //(기본 데미지 + (스탯 * 계수 + ...)) * 데미지 배율 + 고유효과 적용
        StringBuilder sb = new StringBuilder();
        
        int baseDamage = WeaponData.WeaponStat.baseDamage[_currentTierIdx]; //기본데미지(티어별)
        int statDamageSum = 0;//총 스탯 추가 데미지
        
        foreach (var statMultiplier in WeaponData.WeaponStat.damageMultipliers)
        {
            var stat = statMultiplier.stat; //스탯 종류
            var value = statMultiplier.value[_currentTierIdx]; //계수(티어별)
            var statAmount = _mainStats[stat]; //현재 스탯
            int statDamage =  Mathf.FloorToInt(statAmount * (value / 100f)); //소수점 이하는 버림
            statDamageSum += statDamage;
        }
        
        float finalDamage = (baseDamage + statDamageSum) * (1f + _mainStats[MainStats.Damage] / 100f);
        //최종 데미지 배율 적용
        
        return Mathf.FloorToInt(finalDamage);
    }
}
