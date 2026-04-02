using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInfoPanel : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private TMP_Text nameTxt;
    [SerializeField] private TMP_Text classesTxt;
    [SerializeField] private Image panelBorder;
    [SerializeField] private TMP_Text descriptionTxt;

    public void ShowInfo(WeaponData weaponData, StringBuilder sb)
    {
        sb.Clear();

        nameTxt.text = weaponData.Name;
        img.sprite = weaponData.Sprite;
        
        //무기의 스탯은 초기 티어기준으로.
        var tier = weaponData.WeaponStat.initTier;
        var colorHexStr = DataManager.Instance.TierColorDict[tier]; //티어 컬러 가져오기
        var color = DataManager.Instance.GetHexToColor(colorHexStr);
        nameTxt.color = color;
        panelBorder.color = color;
        
        var weaponClass = weaponData.WeaponStat.classes;
        sb.Append(WeaponData.WeaponClassToString(weaponClass[0])); //첫 클래스
        for (int i = 1; i < weaponClass.Count; i++) //하나 이상의 클래스를 가진 무기라면
        {
            sb.Append(", ").Append(WeaponData.WeaponClassToString(weaponClass[i]));
        } 
        classesTxt.SetText(sb);
        sb.Clear();
        
        sb.AppendHeadString("데미지:");
        sb.Append(weaponData.WeaponStat.baseDamage[0]).Append(" ("); //기본 데미지
        foreach (var statMultiplier in weaponData.WeaponStat.damageMultipliers) //스탯 별 데미지 계수
        {
            var stat = statMultiplier.stat;
            var value = statMultiplier.value[0];
            sb.Append("+").Append(value).Append("%").Append(stat.GetIcons());
        }
        sb.AppendLine(")");
        
        sb.AppendHeadString("치명타:");
        sb.Append("X").Append(weaponData.WeaponStat.critDamage[0]);
        sb.Append(" (").Append(weaponData.WeaponStat.critChance[0]).AppendLine("% 확률)");
        
        sb.AppendHeadString("쿨타운:");
        sb.Append(weaponData.WeaponStat.attackSpeed[0]).AppendLine("s");
        
        var knockback = weaponData.WeaponStat.knockBack[0];
        if (knockback > 0)
        {
            sb.AppendHeadString("넉백:");
            sb.Append(knockback).AppendLine();
        }
        
        sb.AppendHeadString("범위:");
        sb.Append(weaponData.WeaponStat.range[0]).Append("(");
        sb.AppendLine(weaponData.WeaponStat.isMelee ? "근거리)" : "원거리)");

        sb.Append("•").AppendLine(weaponData.WeaponStat.description); 
        //고유 효과 -> 데이터는 어떤방식으로???
        // 최소 0개(없음부터) ~ 5?개(상한은 없이?) - 티어 수 만큼의 스탯 배수값...
        
        
        descriptionTxt.SetText(sb);
    }
}
