using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StatUtil
{
    private static Dictionary<MainStats, string> mainStatIcons;
    public const string Tier1Color = "#FFFFFF";
    public const string Tier2Color = "#2EC5FF";
    public const string Tier3Color = "#B52EFF";
    public const string Tier4Color = "#FF0800";
    public const string YellowColor = "#FFE394";
    public const string RedColor = "#FF5555";
    public const string GreenColor = "#55FF55";
    public const string DefaultWhite = "#FFFFFF";
    
    
    static StatUtil()
    {
        mainStatIcons = new Dictionary<MainStats, string>();

        foreach (MainStats stat in Enum.GetValues(typeof(MainStats)))
        {
            string statName = MainStatsToString(stat);
            mainStatIcons[stat] = $"<sprite=\"{statName}\" index=0>";
        }
    }   
    
    public static void Initialize(){}
    
    public static MainStats StringToMainStats(this string str)
    {
        return str switch
        {
            "MaxHP" => MainStats.MaxHP,
            "HealthRegen" => MainStats.HealthRegen,
            "HealthAbsorb" => MainStats.HealthAbsorb,
            "Armor" => MainStats.Armor,
            "DodgeChance" => MainStats.DodgeChance,
            "Speed" => MainStats.Speed,
            "Damage" => MainStats.Damage,
            "Melee" => MainStats.Melee,
            "Ranged" => MainStats.Ranged,
            "Elemental" => MainStats.Elemental,
            "Engineering" => MainStats.Engineering,
            "Tactical" => MainStats.Tactical,
            "AttackSpeed" => MainStats.AttackSpeed,
            "CritChance" => MainStats.CritChance,
            "Range" =>  MainStats.Range,
            "Luck" => MainStats.Luck,
            "Harvest" => MainStats.Harvest,
            _ => MainStats.None
        };
    }

    public static string MainStatsToString(this MainStats stat)
    {
        return stat switch
        {
            MainStats.MaxHP => nameof(MainStats.MaxHP),
            MainStats.HealthRegen => nameof(MainStats.HealthRegen),
            MainStats.HealthAbsorb => nameof(MainStats.HealthAbsorb),
            MainStats.Armor => nameof(MainStats.Armor),
            MainStats.DodgeChance => nameof(MainStats.DodgeChance),
            MainStats.Speed => nameof(MainStats.Speed),
            MainStats.Damage => nameof(MainStats.Damage),
            MainStats.Melee => nameof(MainStats.Melee),
            MainStats.Ranged => nameof(MainStats.Ranged),
            MainStats.Elemental => nameof(MainStats.Elemental),
            MainStats.Engineering => nameof(MainStats.Engineering),
            MainStats.Tactical => nameof(MainStats.Tactical),
            MainStats.AttackSpeed => nameof(MainStats.AttackSpeed),
            MainStats.CritChance => nameof(MainStats.CritChance),
            MainStats.Range => nameof(MainStats.Range),
            MainStats.Luck => nameof(MainStats.Luck),
            MainStats.Harvest => nameof(MainStats.Harvest),
            _ => nameof(MainStats.None) //예외처리
        };
    }
    
    public static bool IsPercentageMainStat(this MainStats stat)
    {
        return stat switch
        {
            // 퍼센트로 계산되는 스탯들만  true 반환
            MainStats.HealthAbsorb => true,
            MainStats.Damage => true,
            MainStats.AttackSpeed => true,
            MainStats.CritChance => true,
            MainStats.DodgeChance => true,
            MainStats.Speed => true,
            // 나머지는 전부 일반(Flat) 수치이므로 false
            _ => false
        };
    }
    
    public static SubStats StringToSubStats(this string str)
    {
        return str switch
        {
            "ConsumableHeal" => SubStats.ConsumableHeal,
            "XPGain"         => SubStats.XPGain,
            "ItemPrice"      => SubStats.ItemPrice,
            "PickUpRange"    => SubStats.PickUpRange,
            "ExplosiveDamage"=> SubStats.ExplosiveDamage,
            "ExplosiveSize"  => SubStats.ExplosiveSize,
            "Bounces"        => SubStats.Bounces,
            "Piercing"       => SubStats.Piercing,
            "FreeRerolls"    => SubStats.FreeRerolls,
            "Enemies"        => SubStats.Enemies,
            "EnemiesSpeed"   => SubStats.EnemiesSpeed,
            "RerollPrice"    => SubStats.RerollPrice, 
            _ => SubStats.None,
        };
    }

    public static string SubStatsToString(this SubStats stat)
    {
        return stat switch
        {
            SubStats.ConsumableHeal  => nameof(SubStats.ConsumableHeal),
            SubStats.XPGain          => nameof(SubStats.XPGain),
            SubStats.ItemPrice       => nameof(SubStats.ItemPrice),
            SubStats.PickUpRange     => nameof(SubStats.PickUpRange),
            SubStats.ExplosiveDamage => nameof(SubStats.ExplosiveDamage),
            SubStats.ExplosiveSize   => nameof(SubStats.ExplosiveSize),
            SubStats.Bounces         => nameof(SubStats.Bounces),
            SubStats.Piercing        => nameof(SubStats.Piercing),
            SubStats.FreeRerolls     => nameof(SubStats.FreeRerolls),
            SubStats.Enemies         => nameof(SubStats.Enemies),
            SubStats.EnemiesSpeed    => nameof(SubStats.EnemiesSpeed),
            SubStats.RerollPrice     => nameof(SubStats.RerollPrice),
            _ => nameof(SubStats.None) //예외처리
        };
    }
    
    public static bool IsPercentageSubStat(this SubStats stat)
    {
        return stat switch
        {
            // 퍼센트로 계산되는 스탯들만  true 반환
            SubStats.XPGain => true,
            SubStats.ItemPrice => true,
            SubStats.EnemiesSpeed => true,
            SubStats.Enemies => true,
            SubStats.RerollPrice => true,
            SubStats.ExplosiveDamage => true,
            SubStats.ExplosiveSize => true,
            SubStats.PickUpRange => true,
            // 나머지는 전부 일반(Flat) 수치이므로 false
            _ => false
        };
    }

    public static string GetIcons(this MainStats stat)
    {
        return mainStatIcons[stat];
    }

    public static void AppendColorString(this StringBuilder sb, float value)
    {
        var format = value.ToString("F2");
        if (value > 0)
        {
            sb.Append(" <color=#55FF55>+");
            sb.Append(format);
            sb.Append("</color> ");
        }
        else if (value < 0)
        {
            sb.Append(" <color=#FF5555>");
            sb.Append(format);
            sb.Append("</color> ");

        }
        else sb.Append(0);
    }
    
    public static void AppendColorString(this StringBuilder sb, int value)
    {
        if (value > 0)
        {
            sb.Append(" <color=").Append(GreenColor).Append(">+");
            sb.Append(value);
            sb.Append("</color> ");
        }
        else if (value < 0)
        {
            sb.Append(" <color=").Append(RedColor).Append(">");
            sb.Append(value);
            sb.Append("</color> ");

        }
        else sb.Append(0);
    }

    public static void AppendHeadString(this StringBuilder sb, string txt)
    {
        sb.Append("<color=").Append(YellowColor).Append(">");
        sb.Append(txt);
        sb.Append("</color> ");
    }
}
