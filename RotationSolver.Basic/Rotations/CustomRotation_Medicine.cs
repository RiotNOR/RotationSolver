﻿using Lumina.Excel.GeneratedSheets;

namespace RotationSolver.Basic.Rotations;

public abstract partial class CustomRotation
{
    #region Tincture
    public abstract MedicineType MedicineType { get; }
    public static IBaseItem TinctureOfStrength6 { get; }
        = new MedicineItem(36109, MedicineType.Strength, 196625);
    public static IBaseItem TinctureOfDexterity6 { get; }
        = new MedicineItem(36110, MedicineType.Dexterity);
    public static IBaseItem TinctureOfIntelligence6 { get; }
        = new MedicineItem(36112, MedicineType.Intelligence);
    public static IBaseItem TinctureOfMind6 { get; }
        = new MedicineItem(36113, MedicineType.Mind);
    public static IBaseItem TinctureOfStrength7 { get; }
        = new MedicineItem(37840, MedicineType.Strength);
    public static IBaseItem TinctureOfDexterity7 { get; }
        = new MedicineItem(37841, MedicineType.Dexterity);
    public static IBaseItem TinctureOfIntelligence7 { get; }
        = new MedicineItem(37843, MedicineType.Intelligence);
    public static IBaseItem TinctureOfMind7 { get; }
        = new MedicineItem(37844, MedicineType.Mind);

    public static IBaseItem TinctureOfStrength8 { get; }
    = new MedicineItem(39727, MedicineType.Strength);
    public static IBaseItem TinctureOfDexterity8 { get; }
        = new MedicineItem(39728, MedicineType.Dexterity);
    public static IBaseItem TinctureOfIntelligence8 { get; }
        = new MedicineItem(39730, MedicineType.Intelligence);
    public static IBaseItem TinctureOfMind8 { get; }
        = new MedicineItem(39731, MedicineType.Mind);

    static bool UseStrength(out IAction act)
    {
        if (TinctureOfStrength8.CanUse(out act)) return true;
        if (TinctureOfStrength7.CanUse(out act)) return true;
        if (TinctureOfStrength6.CanUse(out act)) return true;
        return false;
    }

    static bool UseDexterity(out IAction act)
    {
        if (TinctureOfDexterity8.CanUse(out act)) return true;
        if (TinctureOfDexterity7.CanUse(out act)) return true;
        if (TinctureOfDexterity6.CanUse(out act)) return true;
        return false;
    }
    static bool UseIntelligence(out IAction act)
    {
        if (TinctureOfIntelligence8.CanUse(out act)) return true;
        if (TinctureOfIntelligence7.CanUse(out act)) return true;
        if (TinctureOfIntelligence6.CanUse(out act)) return true;
        return false;
    }
    static bool UseMind(out IAction act)
    {
        if (TinctureOfMind8.CanUse(out act)) return true;
        if (TinctureOfMind7.CanUse(out act)) return true;
        if (TinctureOfMind6.CanUse(out act)) return true;
        return false;
    }
    protected bool UseBurstMedicine(out IAction act)
    {
        act = null;

        if (!(Target?.IsDummy() ?? false) && !DataCenter.InHighEndDuty) return false;

        return MedicineType switch
        {
            MedicineType.Strength => UseStrength(out act),
            MedicineType.Dexterity => UseDexterity(out act),
            MedicineType.Intelligence => UseIntelligence(out act),
            MedicineType.Mind => UseMind(out act),
            _ => false,
        };
    }
    #endregion

    public static IBaseItem EchoDrops { get; } = new BaseItem(4566);

    #region Heal Potion
    public static HealPotionItem[] Potions { get; } = Service.GetSheet<Item>()
        .Where(i => i.FilterGroup == 8 && i.ItemSearchCategory.Row == 43)
        .Select(i => new HealPotionItem(i)).ToArray();

    private bool UseHealPotion(out IAction act)
    {
        var acts = from a in Potions
                   where a.CanUse(out _)
                   orderby a.MaxHealHp
                   select a;

        act = acts.LastOrDefault();
        return act != null;
    }

    #endregion
}
