using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private TMP_Text nameTxt;
    [SerializeField] private TMP_Text classesTxt;
    [SerializeField] private Image panelBorder;
    [SerializeField] private TMP_Text descriptionTxt;
    [SerializeField] private ClassInfo classInfo;
    
    public void ShowWeaponInfo(CurrentWeaponStat weapon, int tierIdx, StringBuilder sb)
    {
        sb.Clear();

        nameTxt.text = weapon.WeaponData.Name;
        img.sprite = weapon.WeaponData.Sprite;
        
        //무기의 스탯은 초기 티어기준으로.
        //var tier = tierIdx + weaponData.WeaponStat.initTier; //현재 티어(인덱스 + 초기 티어)
        var tier = weapon.Tier;
        var colorHexStr = DataManager.Instance.TierColorDict[tier]; //티어 컬러 가져오기
        var color = DataManager.Instance.GetHexToColor(colorHexStr);
        nameTxt.color = color;
        panelBorder.color = color;

        var weaponClass = weapon.WeaponData.WeaponStat.classes;
        sb.Append(WeaponData.WeaponClassToString(weaponClass[0])); //첫 클래스
        for (int i = 1; i < weaponClass.Count; i++) //하나 이상의 클래스를 가진 무기라면
        {
            sb.Append(", ").Append(WeaponData.WeaponClassToString(weaponClass[i]));
        } 
        classesTxt.SetText(sb);
        sb.Clear();
        
        sb.AppendHeadString("데미지: ");
        int sign = 0;
        if(weapon.Damage > weapon.WeaponData.WeaponStat.baseDamage[tierIdx]) sign = 1;
        else if (weapon.Damage < weapon.WeaponData.WeaponStat.baseDamage[tierIdx]) sign = -1;
        
        sb.AppendColor(weapon.Damage, sign);
        sb.Append(" ("); //기본 데미지
        foreach (var statMultiplier in weapon.StatMultipliers) //스탯 별 데미지 계수
        {
            var stat = statMultiplier.mainStat;
            var value = statMultiplier.multiplier;
            sb.Append("+").Append(value).Append("%").Append(stat.GetIcons());
        }
        sb.AppendLine(")");
        
        sb.AppendHeadString("치명타: ");
        sb.Append("X").Append(weapon.CritDamage);
        sb.Append(" (").Append(weapon.CritChance).AppendLine("% 확률)");
        
        sb.AppendHeadString("쿨다운: ");
        sb.Append(weapon.AttackSpeed).AppendLine("s");

        var knockback = weapon.KnockBack;
        if (knockback > 0)
        {
            sb.AppendHeadString("넉백: ");
            sb.Append(knockback).AppendLine();
        }
        
        sb.AppendHeadString("범위: ");
        sb.Append(weapon.Range).Append("(");
        sb.AppendLine(weapon.IsMelee ? "근거리)" : "원거리)");

        //개선 방안?
        var weaponEffectDataList = weapon.WeaponData.WeaponEffectDataList;
        foreach (var weaponEffectData in weaponEffectDataList)
        {
            var descriptionFormat = weaponEffectData.effectDescription;
            sb.Append('•');
            List<(float param, bool isValue)> paramList = new(); //파라미터 리스트
            foreach (var effectValue in weaponEffectData.valuesList) //효과 수치 리스트
            {
                paramList.Add((effectValue.values[tierIdx], true));
                if (effectValue.multipliers.Count > 0)
                {
                    paramList.Add((effectValue.multipliers[tierIdx], false));
                }
            }
            
            for (int i = 0; i < descriptionFormat.Length; i++)
            {
                if (i % 2 == 0) //짝수: 문자열
                {
                    sb.Append(descriptionFormat[i]);
                }
                else //홀수: 파라미터
                {
                    int paramIndex = int.Parse(descriptionFormat[i]); //인덱스
                    if(paramIndex < 0 || paramIndex > paramList.Count) continue;
                    var param = paramList[paramIndex].param;
                    if (paramList[paramIndex].isValue)//스탯에 영향받기 때문에 
                    {
                        sb.AppendColorValue(param);
                    }
                    else//스탯 배수인 경우 고정이므로 색x
                    {
                        sb.Append(param);
                    }
                }
            }

            sb.AppendLine();
        }
        
        descriptionTxt.SetText(sb);
    }

    public void ShowWeaponClassInfo(List<WeaponClasses> weaponClasses, StringBuilder sb)
    {
        classInfo.ShowWeaponClassInfo(weaponClasses, sb);
    }

    public void ShowItemInfo(ItemData item, StringBuilder sb)
    {
        nameTxt.text = item.ItemName;
        img.sprite = item.Icon;
        var tier = item.Tier;
        
        var colorHexStr = DataManager.Instance.TierColorDict[tier]; //티어 컬러 가져오기
        var color = DataManager.Instance.GetHexToColor(colorHexStr);
        nameTxt.color = color;
        panelBorder.color = color;

        var itemClass = ItemData.ToString(item.ItemClass);
        classesTxt.SetText(itemClass);
        
        GetItemStatTxt(sb, item.ID);
        descriptionTxt.SetText(sb);
        
        classInfo.ShowItemClassInfo(item.ItemClass, sb);
    }

    public static void GetItemStatTxt(StringBuilder stringBuilder, int itemID)
    {
        var passiveData = DataManager.Instance.ItemDict[itemID];
        
        stringBuilder.Clear();

        foreach (var statAmount in passiveData.StatMultipliers)
        {
            if (statAmount.mainStat != MainStats.None)
            {
                stringBuilder.Append(statAmount.mainStat.GetIcons()).Append(' '); //스탯 아이콘
                string mainStat = statAmount.mainStat.MainStatsToString();
                stringBuilder.AppendMultiplier(mainStat, statAmount.amount);
                stringBuilder.AppendLine();
            }
            else if (statAmount.subStat != SubStats.None)
            {
                string subStat = statAmount.subStat.SubStatsToString();
                stringBuilder.AppendMultiplier(subStat, statAmount.amount);
                stringBuilder.AppendLine();
            }
        }

        foreach (var statAmount in passiveData.StatValues)
        {
            if (statAmount.mainStat != MainStats.None)
            {
                stringBuilder.Append(statAmount.mainStat.GetIcons()).Append(' '); //스탯 아이콘
                if(statAmount.amount > 0) stringBuilder.AppendColor("+", 1);
                stringBuilder.AppendColorValue(statAmount.amount); //증감량
                stringBuilder.AppendLine(statAmount.mainStat.MainStatsToString()); //해당 스탯명
            }
            else if (statAmount.subStat != SubStats.None)
            {
                stringBuilder.Append(' ');
                if(statAmount.amount > 0) stringBuilder.AppendColor("+", 1);
                stringBuilder.AppendColorValue(statAmount.amount);
                stringBuilder.AppendLine(statAmount.subStat.SubStatsToString());
            }
        }
    }
}
