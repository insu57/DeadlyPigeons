using System;
using System.Collections.Generic;
using UnityEngine;

public static class StatUtil
{
    private static Dictionary<MainStats, string> mainStatIcons;
    
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
            MainStats.Luck => nameof(MainStats.Luck),
            MainStats.Harvest => nameof(MainStats.Harvest),
            _ => nameof(MainStats.None) //예외처리
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

    public static string GetIcons(this MainStats stat)
    {
        return mainStatIcons[stat];
    }
}
