using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClassInfoPanel : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel1;
    [SerializeField] private TMP_Text classText1;
    [SerializeField] private TMP_Text infoText1;
    [SerializeField] private GameObject infoPanel2;
    [SerializeField] private TMP_Text classText2;
    [SerializeField] private TMP_Text infoText2;
    
    private IReadOnlyDictionary<WeaponClasses, int> _weaponsBonusDict;

    public void SetClassBonusDict(IReadOnlyDictionary<WeaponClasses, int> weaponBonusDict) =>
        _weaponsBonusDict = weaponBonusDict;
    
    public void ShowWeaponClassInfo(List<WeaponClasses> classes) //무기 클래스 정보
    {
        var sb = StatUtil.StringBuilder;
        
        sb.Clear();
        
        //클래스 1번
        infoPanel1.SetActive(true);
        var class1 = classes[0]; 
        classText1.text = WeaponData.WeaponClassToString(class1);
        var effectList1 = DataManager.Instance.WeaponClassBonusDict[class1];
        GetWeaponClassEffectTxt(effectList1, _weaponsBonusDict[class1]);
        infoText1.SetText(sb);

        if (classes.Count <= 1) 
        {
            infoPanel2.SetActive(false);
            return;
        }
        infoPanel2.SetActive(true); //클래스가 2개면 2번도 
        
        var class2 = classes[1];
        classText2.text = WeaponData.WeaponClassToString(class2);
        var effectList2 = DataManager.Instance.WeaponClassBonusDict[class2];
        sb. Clear();
        GetWeaponClassEffectTxt(effectList2, _weaponsBonusDict[class2]);
        infoText2.SetText(sb);
    }
    
    private void GetWeaponClassEffectTxt(List<WeaponClassBonus> effectList, int bonus)
    {
        var sb = StatUtil.StringBuilder;
        
        var statNameList = new List<string>();
        foreach (var effect in effectList)
        {
            if(effect.IsUnavailable) 
            {
                Debug.LogWarning("STAT NONE!");
                continue;
            }
            
            if (effect.IsMain)
            {
                statNameList.Add(effect.mainStat.MainStatsToString());
            }
            else
            {
                statNameList.Add(effect.subStat.SubStatsToString());
            }
        }

        int bonusIdx = bonus - 2;//인덱스에 맞게 감소
        for (int i = 0; i < 5; i++) //2~5단계 효과
        {
            if (i > bonusIdx)
            {
                sb.Append("<color=").Append(StatUtil.GrayColor).Append(">");
            }
            
            sb.Append($"({i + 2}) ");
            for (int j = 0; j < effectList.Count; j++)
            {
                int value = effectList[j].values[i];
                if(value == 0) continue;
                if (j > 0) sb.Append(", ");//스탯 증감 효과가 1개가 넘는 경우

                if (i <= bonusIdx)
                {
                    if (value > 0)
                    {
                        sb.Append("<color=").Append(StatUtil.GreenColor).Append(">+").Append(value).Append("</color>");
                    }
                    else
                    {
                        sb.Append("<color=").Append(StatUtil.RedColor).Append(">").Append(value).Append("</color>");
                    }
                }
                else
                {
                    if (value > 0) sb.Append('+').Append(value);
                    else sb.Append(value);
                }
                sb.Append(' ').Append(statNameList[j]);
            }

            if (i > bonusIdx)
            {
                sb.Append("</color>");
            }
            
            sb.AppendLine();
        }
    }
    
    public void ShowItemClassInfo(List<ItemClass> itemClasses)
    {
        //WIP
        gameObject.SetActive(false);
        //추가 설명이 필요한 클래스 구현시 변경
    }
}
