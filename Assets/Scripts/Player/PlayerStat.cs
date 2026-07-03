using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct PlayerLevelInfo
{
    public int currentLevel;
    public bool hasLevelUp;
    public float currentExp;
    public float targetExp;
}

public class PlayerStat : MonoBehaviour
{
    //기본스탯. 
    //캐릭터 패시브, 아이템
    //무기 클래스 조합 효과
    //레벨업 시 최대체력 증가 + 메인스탯 하나 선택(무작위, 등급 존재)

    public int CurrentLevel { get; private set; }
    private const int StartLevel = 0;
    [field: SerializeField] private int currentHP;
    [field: SerializeField] private float currentExp;
    private int TargetExp => (CurrentLevel + 3) * (CurrentLevel + 3);
    private int MaxHp => _finalMainStatDict[MainStats.MaxHP];
    public int ItemPrice => _finalSubStatDict[SubStats.ItemPrice];
    
    private const int DefaultMaxHP = 10;
    private const int DefaultFoodHeal = 3;

    //체력 재생: 틱마다 1 회복. 틱 간격(초) = RegenTickNumerator / (RegenTickBase + healthRegen)
    private const float RegenTickNumerator = 11.25f;
    private const float RegenTickBase = 1.25f;
    private float _regenTimer;
    public int Money { get; private set; }
    public int Stock { get; private set; }
    public void GetStock() => Stock++;
    private readonly Dictionary<MainStats, int> _baseMainStatDict = new();
    private readonly Dictionary<MainStats, int> _mainStatClassBonus = new();//무기 클래스 보너스
    private readonly Dictionary<MainStats, int> _mainStatMultiDict = new();//패시브 스탯 배수
    private readonly Dictionary<MainStats, int> _finalMainStatDict = new(); //최종 스탯
    private readonly Dictionary<SubStats, int> _subStatDict = new();
    private readonly Dictionary<SubStats, int> _subStatClassBonus = new();
    private readonly Dictionary<SubStats, int> _finalSubStatDict = new();

    public IReadOnlyDictionary<MainStats, int> FinalMainStat => _finalMainStatDict;
    public IReadOnlyDictionary<SubStats, int> FinalSubStat => _finalSubStatDict;
   
    //Weapons Stat 
    private const float AttackSpeedScaler = 0.01f; //공격 속도 상수(스탯이 늘어나면 속도가 0에 가까워짐)
    private const int RangeScaler = 75; //실제 유니티 유닛으로 스케일링
    private const float MeleeRangeMultiplier = 0.5f; //근접 무기 스탯효율 감소치
    
    //체력회복은 초당 얼만큼?? 1부터 ~, 패시브, 아이템 등으로 깎인다면? -> 두 개는 별개로?
    public event Action<MainStats, int> OnChangeMainStats;
    public event Action<SubStats, int> OnChangeSubStats;

    public event Action<int, int> OnChangeHealth;
    public event Action<PlayerLevelInfo> OnChangeExp;
    public event Action<int> OnChangeMoney;
    
    public event Action OnPlayerDeath;

    public void InitStat() //스탯 초기화.
    {
        for (int i = 0; i < (int)MainStats.None; i++) //None -> 마지막 항목
        {
            MainStats mainStat = (MainStats)i;
            _baseMainStatDict.Add(mainStat, 0);
            _mainStatClassBonus.Add(mainStat, 0);
            _mainStatMultiDict.Add(mainStat, 0);
            _finalMainStatDict.Add(mainStat, 0);
        }
        
        for (int i = 0; i < (int)SubStats.None; i++)
        {
            SubStats subStat = (SubStats)i;
            _subStatDict.Add(subStat, 0);
            _subStatClassBonus.Add(subStat, 0);
            _finalSubStatDict.Add(subStat, 0);
        }
        
        ChangeStat(MainStats.MaxHP, DefaultMaxHP); //기본 최대 체력
        currentHP = DefaultMaxHP;
        
        foreach (var (mainStat, value) in _finalMainStatDict)
        {
            OnChangeMainStats?.Invoke(mainStat, value);
        }

        foreach (var (subStat, value) in _finalSubStatDict)
        {
            OnChangeSubStats?.Invoke(subStat, value);
        }
        
        CurrentLevel = StartLevel;
        
        OnChangeHealth?.Invoke(currentHP, MaxHp);

        var lvInfo = new PlayerLevelInfo()
        {
            currentLevel = CurrentLevel,
            hasLevelUp = false,
            currentExp = currentExp,
            targetExp = TargetExp
        };
        
        OnChangeExp?.Invoke(lvInfo);
        
        OnChangeMoney?.Invoke(Money);
    }

    public void WaveStatInit()
    {
        currentHP = MaxHp;
        OnChangeHealth?.Invoke(currentHP, MaxHp);
    }

    public void AddItem(ItemData itemData)
    {
        if (itemData.StatMultipliers != null) //스탯 배수
        {
            foreach (var statAmount in itemData.StatMultipliers)
            {
                if(statAmount.mainStat == MainStats.None) continue;
                _mainStatMultiDict[statAmount.mainStat] += statAmount.amount;
                
                ChangeStat(statAmount.mainStat, 0);
            }
        }

        if (itemData.StatValues != null)
        {
            foreach (var statAmount in itemData.StatValues)
            {
                if (statAmount.mainStat != MainStats.None)
                {
                    ChangeStat(statAmount.mainStat, statAmount.amount);
                }
                else if (statAmount.subStat != SubStats.None)
                {
                    ChangeStat(statAmount.subStat, statAmount.amount);
                }
            }
        }
    }

    public void ResetStatClassBonus()
    {
        for (int i = 0; i < (int)MainStats.None; i++)
        {
            var key = (MainStats)i;
            _mainStatClassBonus[key] = 0;
        }

        for (int i = 0; i < (int)SubStats.None; i++)
        {
            var key =  (SubStats)i;
            _subStatClassBonus[key] = 0;
        }
    }
    
    public void UpdateStatClassBonus(MainStats mainStat, int amount)
    {
        _mainStatClassBonus[mainStat] += amount;
        ChangeStat(mainStat, 0);
    }

    public void UpdateStatClassBonus(SubStats subStat, int amount)
    {
        _subStatClassBonus[subStat] += amount;
        ChangeStat(subStat, 0);
    }

    public void ChangeStat(MainStats mainStat, int amount)
    {
        _baseMainStatDict[mainStat] += amount;
        int currentAmount =  _baseMainStatDict[mainStat] + _mainStatClassBonus[mainStat];
        int multiplier = _mainStatMultiDict[mainStat];
        _finalMainStatDict[mainStat] = Mathf.FloorToInt(currentAmount * (1f + multiplier / 100f));
    
        OnChangeMainStats?.Invoke(mainStat, _finalMainStatDict[mainStat]);
        if (mainStat == MainStats.MaxHP)
        {
            currentHP += amount;
            OnChangeHealth?.Invoke(currentHP, MaxHp);
        }
    }

    private void ChangeStat(SubStats subStat, int amount)
    {
        _subStatDict[subStat] += amount;
        int currentAmount =  _subStatDict[subStat] + _subStatClassBonus[subStat];
        _finalSubStatDict[subStat] = currentAmount;
        
        OnChangeSubStats?.Invoke(subStat, _finalSubStatDict[subStat]);
    }

    public void SyncStat()
    {
        foreach (var (mainStat, value) in _finalMainStatDict)
        {
            OnChangeMainStats?.Invoke(mainStat, value);
        }

        foreach (var (subStat, value) in _finalSubStatDict)
        {
            OnChangeSubStats?.Invoke(subStat, value);
        }
    }
    
    public void Damage(int damage)
    {
        var armor = _finalMainStatDict[MainStats.Armor];
        var ratio =  CalculateArmorRatio(armor);
        var finalDamage = Mathf.FloorToInt(damage * ratio + 0.5f);
       
        currentHP -= finalDamage;
        OnChangeHealth?.Invoke(currentHP, MaxHp);
        if (currentHP <= 0)
        {
            OnPlayerDeath?.Invoke();
        }
    }

    private static float CalculateArmorRatio(float armor)
    {
        if (armor >= 0)
        {
            // 양수 방어도 — 감쇠
            return 1f / (1f + armor / 15f);
        }
        else
        {
            // 음수 방어도 — 피해 증가
            return (15f - 2f * armor) / (15f - armor);
        }
    }

    public void Heal(int healAmount)
    {
        currentHP += healAmount;
        if(currentHP>= MaxHp) currentHP = MaxHp;
        OnChangeHealth?.Invoke(currentHP, MaxHp);
    }

    private void Update()
    {
        HealthRegenTick();
    }

    private void HealthRegenTick() //HealthRegen 스탯에 따라 틱마다 1 회복
    {
        if (_finalMainStatDict.Count == 0) return; //InitStat 이전(스탯 미초기화)
        if(_finalMainStatDict[MainStats.HealthRegen] <= 0f) return;
        if (currentHP >= MaxHp) //풀피면 누적 리셋(피해 직후 즉시 회복 방지)
        {
            _regenTimer = 0f;
            return;
        }
        
        var tickInterval = RegenTickNumerator / (RegenTickBase + _finalMainStatDict[MainStats.HealthRegen]);

        _regenTimer += Time.deltaTime;
        if (_regenTimer >= tickInterval)
        {
            _regenTimer = 0f;
            Heal(1);
        }
    }

    public void ChangeMoney(int amount) //상점에서 구매/판매 시
    {
        if (Money + amount < 0)
        {
            Debug.LogWarning("Not enough money");
        }
        
        Money += amount;
        
        OnChangeMoney?.Invoke(Money);
    }

    public void GetMaterial(int amount)
    {
        //스탯(아이템) 계산 후 돈,경험치 추가.
        ChangeMoney(amount);
        var expAmount = amount * (1 + _finalSubStatDict[SubStats.XPGain] / 100f);
        currentExp += expAmount;

        var hasLvUp = false;
        
        if(currentExp >= TargetExp) //레벨 업. 한번에 여러번가능하도록 수정?
        {
            AudioManager.Instance.PlaySFX(SfxType.LevelUp);
            
            currentExp = TargetExp - currentExp;
            CurrentLevel++;
            hasLvUp = true;
            
            ChangeStat(MainStats.MaxHP, 1);
        }

        var lvInfo = new PlayerLevelInfo()
        {
            currentLevel = CurrentLevel,
            hasLevelUp = hasLvUp,
            currentExp = currentExp,
            targetExp = TargetExp
        };
        
        OnChangeExp?.Invoke(lvInfo);
    }

    public void GetMeat()
    {
        //스탯(아이템) 계산 후 회복.
        var healAmount = DefaultFoodHeal + _finalSubStatDict[SubStats.FoodHeal];
        Heal(healAmount);
    }

    public CurrentWeaponStat GetWeaponStat(WeaponData weaponData, int tier)
    {
        return WeaponStatCalculator.GetCurrentWeaponStat(weaponData, tier, _finalMainStatDict, _finalSubStatDict);
    }
}


