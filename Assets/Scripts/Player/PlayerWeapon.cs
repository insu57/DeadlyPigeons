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
    public int Tier {get; private set;}
    public int TierIdx { get; private set; }
    private float _attackCoolTimer;
    public float FinalAttackSpeed { get; private set; } //최종 공격 속도
    private const float AttackSpeedScaler = 0.01f; //공격 속도 상수(스탯이 늘어나면 속도가 0에 가까워짐)
    private const int RangeScaler = 75; //실제 유니티 유닛으로 스케일링
    private const float MeleeRangeMultiplier = 0.5f; //근접 무기 스탯효율 감소치
    private float _finalRange; //실제 최종 범위(유닛 배율 적용). 범위 스탯과는 다름.
    private Hitbox _hitbox;
    private float _targetDist;

    private Dictionary<MainStats, int> _mainStats = new();
    private Dictionary<SubStats, int> _subStats = new();

    public int FinalDamage {get; private set;}

    private (bool isCrit, int damage) CritDamage()
    {
        var critChance = WeaponData.WeaponStat.critChance[TierIdx] + _mainStats[MainStats.CritChance];
        bool isCrit = Random.value < critChance / 100f;
        var damage = FinalDamage;
        if (isCrit) damage = Mathf.FloorToInt(damage * WeaponData.WeaponStat.critDamage[TierIdx]);
        
        return (isCrit, damage);
    }
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

    public int ExplosiveDmgPer => _subStats[SubStats.ExplosiveDamage];
    
    public int GetStat(MainStats stat) => _mainStats[stat];
    public int GetStat(SubStats stat) => _subStats[stat];

    public void SetPiercing(int piercing, int piercingDmg)
    {
        _piercing = piercing;
        _piercingDmgPer = piercingDmg;
    }
    
    private int _bounces = 0;
    public void SetBounces(int bounces) =>  _bounces = bounces;

    //스탯별 처리(음수 처리 따로)
    
    private void Awake()
    {
        _hitbox = GetComponentInChildren<Hitbox>();
        
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
        
        if(!WeaponData) return;

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

        Tier = tier;
        TierIdx = tier - weaponData.WeaponStat.initTier;//인덱스은 0부터 초기 티어 만큼 차감

        var weaponEffectList = weaponData.WeaponEffectDataList;
        foreach (var weaponEffectData in weaponEffectList)
        {
            var effectType = weaponEffectData.effectType;
            var effect = WeaponData.GetWeaponEffect(effectType);
            _weaponEffects.Add(effect);
            
            if(effect.IsExecuteType) continue;
            List<float> initValues = new();
            foreach (var effectValues in weaponEffectData.valuesList) //무기 초기화 시 효과
            {
                var value = GetEffectValueStatMultiplier(effectValues); //무기 효과 해당 티어 값
                initValues.Add(value); //현재 티어 효과 수치 + 스탯 계수
            }
            effect.Init(this, initValues); //수치 주입
        }
        
        FinalDamage = GetDamage();
        
        if(!weaponData.WeaponStat.isMelee) meleeCollider.enabled = false; //원거리면 비활성.
    }

    private float GetEffectValueStatMultiplier(WeaponEffectValues weaponEffectValues)
    {
        var value = weaponEffectValues.values[TierIdx];
        if (weaponEffectValues.multipliers.Count == 0) return value;
        
        var multiplier = weaponEffectValues.multipliers[TierIdx];
        //스탯 계수(티어 값)
        if (weaponEffectValues.mainStat != MainStats.None)
        {
            value += (int)( _mainStats[weaponEffectValues.mainStat] * multiplier / 100f);
        }
        else if (weaponEffectValues.subStat != SubStats.None)
        {
            value += (int)(_subStats[weaponEffectValues.subStat] * multiplier / 100f);
        }
        return value;
    }

    public void SetTarget(TargetInfo target)
    {
        _targetInfo = target;
    }

    public void UpdateMainStats(MainStats stat, int value)
    {
        _mainStats[stat] = value;
        FinalDamage = GetDamage();
     
        if (stat == MainStats.Range)
        {
            _finalRange = GetRange();
        }
        else if (stat == MainStats.AttackSpeed)
        {
            FinalAttackSpeed = GetAttackSpeed();
        }
    }

    public void UpdateSubStats(SubStats stat, int value)
    {
        _subStats[stat] = value;
        FinalDamage = GetDamage();
    }

    public void SetCenter(Transform center)
    {
        _center = center;
    }
    
    private void RotateWeapon()
    {
        if(!WeaponData) return;
        
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
        _attackCoolTimer -= Time.deltaTime; 
        
        if(!_targetInfo.Target) return; //타겟이 있을 때만.
        if(_isAttacking) return;
        if(WeaponData.WeaponStat.isMelee) MeleeAttack();
        else RangedAttack();
    }
    //적정 Range 조정은 테스트하면서 변경. 현재는 조금 사거리가 짧은듯

    private void MeleeAttack()
    {
        if(_finalRange * _finalRange < _targetInfo.SqrDistance) return; //범위 밖
        
        if (_attackCoolTimer > 0) return; //공격 쿨 타이머(무기 공격 속도 기준)

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

        var (isCrit, damage) = CritDamage();
            
        WeaponEffectsSetExecute(); //무기 효과 실행효과 데이터 주입
        _hitbox.AttackInit(damage, isCrit, _weaponEffects);
            
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

    private void MeleeAnimation()//근거리 무기 공격 애니메이션
    {
        if(!_isAttacking) return;
        
        _animTimer += Time.deltaTime;

        float percent = _animTimer / attackDuration;
        if (percent >= 1f)
        {
            _isAttacking = false;
            _attackCoolTimer = WeaponData.WeaponStat.attackSpeed[TierIdx];
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
        if (_finalRange * _finalRange < _targetInfo.SqrDistance)
        {
            return;
        }
        
        if (_attackCoolTimer > 0) return; //공격 쿨 타이머(무기 공격 속도 기준)
       
        //투사체 만큼 발사.//투사체 각도!
        var projectile = ObjectPoolingManager.Instance.GetProjectile();
        projectile.transform.position = muzzle.position;//
        var dir = _targetInfo.Target.position - muzzle.position;
            
        WeaponEffectsSetExecute(); //무기효과: 실행 효과 데이터 주입
            
        var (isCrit, damage) = CritDamage(); //최종 데미지(치명 포함)
        var projectileInitData = new ProjectileInitData //투사체 초기화 정보 
        {
            Damage = damage,
            Piercing =  _piercing + _subStats[SubStats.Piercing], //무기 관통 + 관통 스탯
            PiercingDmgPer =  _piercingDmgPer + _subStats[SubStats.PiercingDamage],//관통 데미지 + 스탯
            Bounces = _bounces + _subStats[SubStats.Bounces], //무기 도탄 + 스탯
            HitLayer = DataManager.Instance.PlayerHitboxLayer, //Layer : 플레이어 히트 박스
            IsCrit = isCrit,
            WeaponEffects = _weaponEffects //  효과 리스트
        };
        projectile.Initialize(projectileInitData);
        projectile.Fire(dir, _finalRange);

        _attackCoolTimer = WeaponData.WeaponStat.attackSpeed[TierIdx];
    }

    private void WeaponEffectsSetExecute()
    {
        for (int i = 0; i < _weaponEffects.Count; i++)
        {
            var weaponEffect = _weaponEffects[i];
            if(!weaponEffect.IsExecuteType) continue;
            
            List<float> currentExecuteValues = new();
            var weaponEffectData = WeaponData.WeaponEffectDataList[i];
            var effectValues = weaponEffectData.valuesList;
                
            foreach (var effectValue in effectValues)
            {
                var value = GetEffectValueStatMultiplier(effectValue);
                currentExecuteValues.Add(value);
            }
            
            weaponEffect.SetExecuteData(this, currentExecuteValues);
        }
    }

    private int GetDamage()
    {
        //(기본 데미지 + (스탯 * 계수 + ...)) * 데미지 배율 + 고유효과 적용
        
        int baseDamage = WeaponData.WeaponStat.baseDamage[TierIdx]; //기본데미지(티어별)
        int statDamageSum = 0;//총 스탯 추가 데미지
        
        foreach (var statMultiplier in WeaponData.WeaponStat.damageMultipliers)
        {
            var stat = statMultiplier.stat; //스탯 종류
            var value = statMultiplier.value[TierIdx]; //계수(티어별)
            var statAmount = _mainStats[stat]; //현재 스탯
            int statDamage =  Mathf.FloorToInt(statAmount * (value / 100f)); //소수점 이하 버리기
            statDamageSum += statDamage;
        }
        
        float finalDamage = (baseDamage + statDamageSum) * (1f + _mainStats[MainStats.Damage] / 100f);
        //최종 데미지 배율 적용
        
        if(finalDamage < 1) finalDamage = 1; //최소치 1 데미지.
        
        return (int)Mathf.Round(finalDamage);//반올림.
    }

    private float GetRange()
    {
        //최소 범위 처리?
        if (WeaponData.WeaponStat.isMelee)
        {
            //(기본 범위 + 범위 스탯 / 근거리무기 범위 배율) / 범위 조절(유닛)
            var finalRange = (WeaponData.WeaponStat.range[TierIdx] 
                              + _mainStats[MainStats.Range] * MeleeRangeMultiplier) / RangeScaler;
            return finalRange;
        }
        else
        {
            //(기본 범위 + 범위 스탯) / 범위 조절(유닛)
            float finalRange = (float)(WeaponData.WeaponStat.range[TierIdx] + _mainStats[MainStats.Range] )
                               /  RangeScaler;
            return finalRange;
        }
    }

    private float GetAttackSpeed()
    {
        var baseAttackSpeed = WeaponData.WeaponStat.attackSpeed[TierIdx];
        var attackSpeedStat = _mainStats[MainStats.AttackSpeed];
        var finalAttackSpeed = baseAttackSpeed / (1 + (AttackSpeedScaler * attackSpeedStat)); 
        //공격 속도 스탯이 늘어나면 0에 가까워짐.(0은 되지않음) 스탯 100이면 공격 간 시간(대기시간)이 절반으로 감소
        return finalAttackSpeed;
    }

    public CurrentWeaponStat GetCurrentWeaponStat()
    {
        var weaponStat = WeaponData.WeaponStat;
        var statMultipliers = new List<(MainStats MainStats, int multipleir)>();
        foreach (var statMultiplier in weaponStat.damageMultipliers)
        {
            var mainStat = statMultiplier.stat;
            var multiplier = statMultiplier.value[TierIdx];
            statMultipliers.Add((mainStat, multiplier));
        }
        var currentWeaponStat = new CurrentWeaponStat
        {
            WeaponData = WeaponData,
            Tier = Tier,
            Damage = FinalDamage,
            StatMultipliers = statMultipliers,
            CritDamage = weaponStat.critDamage[TierIdx],
            CritChance = weaponStat.critChance[TierIdx] + _mainStats[MainStats.CritChance],
            AttackSpeed = FinalAttackSpeed,
            KnockBack = weaponStat.knockBack[TierIdx] + _subStats[SubStats.Knockback],
            Range =  weaponStat.range[TierIdx] + _mainStats[MainStats.Range],
            IsMelee = weaponStat.isMelee,
            HealthAbsorb = weaponStat.healthAbsorb[TierIdx] + _mainStats[MainStats.HealthAbsorb],
        };

        return currentWeaponStat;
    }
}
