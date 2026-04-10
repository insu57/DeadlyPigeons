using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStat : MonoBehaviour, IDamageable
{
    //기본스탯. 
    //캐릭터 패시브, 아이템
    //무기 클래스 조합 효과
    //레벨업 시 최대체력 증가 + 메인스탯 하나 선택(무작위, 등급 존재)

    [field: SerializeField] private int currentLevel;
    [field: SerializeField] private int currentHP;
    [field: SerializeField] private int money;
    [field: SerializeField] private int defaultMaxHP = 10;
    private Dictionary<MainStats, int> _defaultMainStats = new();
    private Dictionary<SubStats, int> _defaultSubStats = new();
    //체력회복은 초당 얼만큼?? 1부터 ~, 패시브, 아이템 등으로 깎인다면? -> 두 개는 별개로?
    public event Action<MainStats, int> OnChangeMainStats;
    public event Action<SubStats, int> OnChangeSubStats;
        
    private void Start()
    {
        currentLevel = 1;
    }

    public void InitStat(CharacterData charData)
    {
        for (int i = 0; i < (int)MainStats.None; i++) //None -> 마지막 항목
        {
            MainStats mainStat = (MainStats)i;
            _defaultMainStats.Add(mainStat, 0);
        }

        for (int i = 0; i < (int)SubStats.None; i++)
        {
            SubStats subStat = (SubStats)i;
            _defaultSubStats.Add(subStat, 0);
        }
        
        _defaultMainStats[MainStats.MaxHP] = defaultMaxHP; //기본 최대 체력
        currentHP = defaultMaxHP;
        
        var initStatsList = charData.InitStatsList;

        foreach (var initStat in initStatsList) //패시브 적용. 아이템으로 변경 시 달라짐.
        {
            if (initStat.mainStats != MainStats.None)
            {
                _defaultMainStats[initStat.mainStats] += initStat.amount;
            }
            else if (initStat.subStats != SubStats.None)
            {
                _defaultSubStats[initStat.subStats] += initStat.amount;
            }
        }

        foreach (var (mainStat, value) in _defaultMainStats)
        {
            OnChangeMainStats?.Invoke(mainStat, value);
        }

        foreach (var (subStat, value) in _defaultSubStats)
        {
            OnChangeSubStats?.Invoke(subStat, value);
        }
    }

    public void UpdateStat(MainStats mainStats, int amount)
    {
        _defaultMainStats[mainStats] += amount;
        OnChangeMainStats?.Invoke(mainStats, _defaultMainStats[mainStats]);
    }

    public void UpdateStat(SubStats subStats, int amount)
    {
        _defaultSubStats[subStats] += amount;
        OnChangeSubStats?.Invoke(subStats, _defaultSubStats[subStats]);
    }

    public void SyncStatData()
    {
        for (int i = 0; i < (int)MainStats.None; i++)
        {
            var mainStat = (MainStats)i;
            OnChangeMainStats?.Invoke(mainStat, _defaultMainStats[mainStat]);
        }

        for (int i = 0; i < (int)SubStats.None; i++)
        {
            var subStat = (SubStats)i;
            OnChangeSubStats?.Invoke(subStat, _defaultSubStats[subStat]);
        }
    }
    
    public void Damage(int damage)
    {
        
    }

    public void Heal(int healAmount)
    {
        
    }

    public void DotDamage(int duration, int damage, float tick)
    {
        
    }
}
