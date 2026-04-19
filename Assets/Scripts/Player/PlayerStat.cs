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
    private readonly Dictionary<MainStats, int> _baseMainStatDict = new();
    private readonly Dictionary<MainStats, int> _mainStatMultiDict = new();
    private readonly Dictionary<MainStats, int> _finalMainStatDict = new();
    private readonly Dictionary<SubStats, int> _subStatDict = new();
   
    //체력회복은 초당 얼만큼?? 1부터 ~, 패시브, 아이템 등으로 깎인다면? -> 두 개는 별개로?
    public event Action<MainStats, int> OnChangeMainStats;
    public event Action<SubStats, int> OnChangeSubStats;
        
    private void Start()
    {
        currentLevel = 1;
    }

    public void InitStat()
    {
        for (int i = 0; i < (int)MainStats.None; i++) //None -> 마지막 항목
        {
            MainStats mainStat = (MainStats)i;
            _baseMainStatDict.Add(mainStat, 0);
            _mainStatMultiDict.Add(mainStat, 0);
            _finalMainStatDict.Add(mainStat, 0);
        }
        
        for (int i = 0; i < (int)SubStats.None; i++)
        {
            SubStats subStat = (SubStats)i;
            _subStatDict.Add(subStat, 0);
        }
        
        _baseMainStatDict[MainStats.MaxHP] = defaultMaxHP; //기본 최대 체력
        currentHP = defaultMaxHP;
        
        foreach (var (mainStat, value) in _baseMainStatDict)
        {
            OnChangeMainStats?.Invoke(mainStat, value);
        }

        foreach (var (subStat, value) in _subStatDict)
        {
            OnChangeSubStats?.Invoke(subStat, value);
        }
    }

    public void AddItem(ItemData itemData)
    {
        if (itemData.StatMultipliers != null) //스탯 배수
        {
            foreach (var statAmount in itemData.StatMultipliers)
            {
                if(statAmount.mainStat == MainStats.None) continue;
                _mainStatMultiDict[statAmount.mainStat] += statAmount.amount;
                UpdateStat(statAmount.mainStat, 0);
            }
        }

        if (itemData.StatValues != null)
        {
            foreach (var statAmount in itemData.StatValues)
            {
                if (statAmount.mainStat != MainStats.None)
                {
                    UpdateStat(statAmount.mainStat, statAmount.amount);
                }
                else if (statAmount.subStat != SubStats.None)
                {
                    UpdateStat(statAmount.subStat, statAmount.amount);
                }
            }
        }
    }
    
    public void UpdateStat(MainStats mainStats, int amount)
    {
        _baseMainStatDict[mainStats] += amount;
        int currentAmount =  _baseMainStatDict[mainStats];
        int multiplier = _mainStatMultiDict[mainStats];
        _finalMainStatDict[mainStats] = Mathf.FloorToInt(currentAmount * (1f + multiplier / 100f));
        if(mainStats== MainStats.MaxHP)
        {
            Debug.Log("MaxHP:" + _baseMainStatDict[mainStats]);
            Debug.Log($"{mainStats}: {_finalMainStatDict[mainStats]}");
        }
        OnChangeMainStats?.Invoke(mainStats, _finalMainStatDict[mainStats]);
    }

    public void UpdateStat(SubStats subStats, int amount)
    {
        _subStatDict[subStats] += amount;
        OnChangeSubStats?.Invoke(subStats, _subStatDict[subStats]);
    }

    public void SyncStatData()
    {
        for (int i = 0; i < (int)MainStats.None; i++)
        {
            var mainStat = (MainStats)i;
            OnChangeMainStats?.Invoke(mainStat, _finalMainStatDict[mainStat]);
        }

        for (int i = 0; i < (int)SubStats.None; i++)
        {
            var subStat = (SubStats)i;
            OnChangeSubStats?.Invoke(subStat, _subStatDict[subStat]);
        }
    }
    
    public void Damage(int damage, bool isCrit)
    {
        
    }

    public void Heal(int healAmount)
    {
        
    }

    public void DotDamage(int duration, int damage, float tick)
    {
        
    }
    
    public Transform GetTransform() => transform;
}
