using ECommons.DalamudServices;
using ECommons.ExcelServices;
using RotationSolver.Basic.Traits;
using System.Collections.Generic;

namespace RotationSolver.Basic.Rotations.Basic;

public abstract class MNK_Base : CustomRotation
{
    public override MedicineType MedicineType => MedicineType.Strength;

    public sealed override Job[] Jobs => new [] { ECommons.ExcelServices.Job.MNK, ECommons.ExcelServices.Job.PGL };

    #region Job Gauge
    static MNKGauge JobGauge => Svc.Gauges.Get<MNKGauge>();

    protected static BeastChakra[] BeastChakras => JobGauge.BeastChakra;

    protected static byte Chakra => JobGauge.Chakra;

    protected static bool HasSolar => JobGauge.Nadi.HasFlag(Nadi.SOLAR);
    protected static bool HasLunar => JobGauge.Nadi.HasFlag(Nadi.LUNAR);
    #endregion

    #region Attack Single
    public static IBaseAction BootShine { get; } = new BaseAction(ActionID.BootShine);

    public static IBaseAction DragonKick { get; } = new BaseAction(ActionID.DragonKick)
    {
        StatusProvide = new[] { StatusID.LeadenFist },
    };

    public static IBaseAction TwinSnakes { get; } = new BaseAction(ActionID.TwinSnakes, ActionOption.Dot);
    public static IBaseAction TrueStrike { get; } = new BaseAction(ActionID.TrueStrike);
    public static IBaseAction Demolish { get; } = new BaseAction(ActionID.Demolish, ActionOption.Dot)
    {
        TargetStatus = new StatusID[] { StatusID.Demolish },
        GetDotGcdCount = () => 3,
    };
    public static IBaseAction SnapPunch { get; } = new BaseAction(ActionID.SnapPunch);

    public static IBaseAction SteelPeak { get; } = new BaseAction(ActionID.SteelPeak)
    {
        ActionCheck = (b, m) => InCombat && Chakra == 5,
    };
    #endregion

    #region Attack Area
    public static IBaseAction ArmOfTheDestroyer { get; } = new BaseAction(ActionID.ArmOfTheDestroyer);

    [Obsolete("Please use ArmOfTheDestroyer instead.")]
    public static IBaseAction ShadowOfTheDestroyer { get; } = new BaseAction(ActionID.ShadowOfTheDestroyer);

    public static IBaseAction FourPointFury { get; } = new BaseAction(ActionID.FourPointFury);

    public static IBaseAction RockBreaker { get; } = new BaseAction(ActionID.RockBreaker);

    public static IBaseAction HowlingFist { get; } = new BaseAction(ActionID.HowlingFist)
    {
        ActionCheck = SteelPeak.ActionCheck,
    };

    public static IBaseAction ElixirField { get; } = new BaseAction(ActionID.ElixirField);

    public static IBaseAction FlintStrike { get; } = new BaseAction(ActionID.FlintStrike);

    public static IBaseAction RisingPhoenix { get; } = new BaseAction(ActionID.RisingPhoenix);

    public static IBaseAction CelestialRevolution { get; } = new BaseAction(ActionID.CelestialRevolution);


    public static IBaseAction TornadoKick { get; } = new BaseAction(ActionID.TornadoKick);

    public static IBaseAction PhantomRush { get; } = new BaseAction(ActionID.PhantomRush);

    #endregion

    #region Support
    public static IBaseAction Mantra { get; } = new BaseAction(ActionID.Mantra, ActionOption.Heal);

    public static IBaseAction RiddleOfEarth { get; } = new BaseAction(ActionID.RiddleOfEarth, ActionOption.Defense)
    {
        StatusProvide = new[] { StatusID.RiddleOfEarth },
    };

    public static IBaseAction RiddleOfWind { get; } = new BaseAction(ActionID.RiddleOfWind);

    public static IBaseAction PerfectBalance { get; } = new BaseAction(ActionID.PerfectBalance)
    {
        ActionCheck = (b, m) => InCombat,
    };

    public static IBaseAction Meditation { get; } = new BaseAction(ActionID.Meditation);

    public static IBaseAction FormShift { get; } = new BaseAction(ActionID.FormShift, ActionOption.Buff)
    {
        StatusProvide = new[] { StatusID.FormlessFist, StatusID.PerfectBalance },
    };

    public static IBaseAction Brotherhood { get; } = new BaseAction(ActionID.Brotherhood, ActionOption.Buff);

    public static IBaseAction RiddleOfFire { get; } = new BaseAction(ActionID.RiddleOfFire);
    #endregion

    #region Traits
    protected static IBaseTrait DeepMeditation { get; } = new BaseTrait(160);
    protected static IBaseTrait DeepMeditation2 { get; } = new BaseTrait(245);
    protected static IBaseTrait GreasedLightning { get; } = new BaseTrait(364);
    protected static IBaseTrait EnhancedGreasedLightning    { get; } = new BaseTrait(365);
    protected static IBaseTrait EnhancedGreasedLightning2    { get; } = new BaseTrait(366);
    protected static IBaseTrait EnhancedGreasedLightning3    { get; } = new BaseTrait(367);
    protected static IBaseTrait SteelPeakMastery    { get; } = new BaseTrait(428);
    protected static IBaseTrait HowlingFistMastery    { get; } = new BaseTrait(429);
    protected static IBaseTrait ArmOfTheDestroyerMastery    { get; } = new BaseTrait(430);
    protected static IBaseTrait EnhancedThunderclap    { get; } = new BaseTrait(431);
    protected static IBaseTrait EnhancedBrotherhood    { get; } = new BaseTrait(432);
    protected static IBaseTrait EnhancedPerfectBalance    { get; } = new BaseTrait(433);
    protected static IBaseTrait FlintStrikeMastery    { get; } = new BaseTrait(512);
    protected static IBaseTrait TornadoKickMastery    { get; } = new BaseTrait(513);
    protected static IBaseTrait MeleeMastery    { get; } = new BaseTrait(518);
    #endregion
    public static IBaseAction Thunderclap { get; } = new BaseAction(ActionID.Thunderclap, ActionOption.EndSpecial)
    {
        ChoiceTarget = TargetFilter.FindTargetForMoving,
    };

    [RotationDesc(ActionID.Thunderclap)]
    protected sealed override bool MoveForwardAbility(out IAction act)
    {
        if (Thunderclap.CanUse(out act)) return true;
        return base.MoveForwardAbility(out act);
    }

    [RotationDesc(ActionID.Feint)]
    protected sealed override bool DefenseAreaAbility(out IAction act)
    {
        if (Feint.CanUse(out act)) return true;
        return base.DefenseAreaAbility(out act);
    }

    [RotationDesc(ActionID.Mantra)]
    protected sealed override bool HealAreaAbility(out IAction act)
    {
        if (Mantra.CanUse(out act)) return true;
        return base.HealAreaAbility(out act);
    }

    [RotationDesc(ActionID.RiddleOfEarth)]
    protected sealed override bool DefenseSingleAbility(out IAction act)
    {
        if (RiddleOfEarth.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        return base.DefenseSingleAbility(out act);
    }
}
