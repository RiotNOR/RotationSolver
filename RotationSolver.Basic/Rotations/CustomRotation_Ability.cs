namespace RotationSolver.Basic.Rotations;

public abstract partial class CustomRotation
{
    private bool Ability(IAction nextGCD, out IAction act, bool helpDefenseAOE, bool helpDefenseSingle)
    {
        act = DataCenter.CommandNextAction;
        if (act is IBaseAction a && a != null && !a.IsRealGCD && a.CanUse(out _,  
            CanUseOption.MustUse | CanUseOption.SkipDisable | CanUseOption.EmptyOrSkipCombo)) return true;

        if(act is IBaseItem i &&  i.CanUse(out _)) return true;

        if (!Configuration.PluginConfiguration.GetValue(SettingsCommand.UseAbility) 
            || Player.TotalCastTime > 0)
        {
            act = null;
            return false;
        }

        if (EmergencyAbility(nextGCD, out act)) return true;
        var role = ClassJob.GetJobRole();

        if (InterruptAbility(role, out act)) return true;

        var specialType = DataCenter.SpecialType;

        if (ShirkOrShield(role, specialType, out act)) return true;
        if (AntiKnockback(role, specialType, out act)) return true;

        if (specialType == SpecialCommandType.EsunaStanceNorth && role == JobRole.Melee)
        {
            if (TrueNorth.CanUse(out act)) return true;
        }

        if (GeneralHealAbility(specialType, out act)) return true;

        if (AutoDefense(role, helpDefenseAOE, helpDefenseSingle, out act)) return true;

        BaseAction.OtherOption |= CanUseOption.EmptyOrSkipCombo;
        if (MovingAbility(specialType, out act)) return true;
        BaseAction.OtherOption &= ~CanUseOption.EmptyOrSkipCombo;

        if (GeneralUsingAbility(role, out act)) return true;

        if (DataCenter.HPNotFull && InCombat)
        {
            if (DataCenter.SpecialType == SpecialCommandType.HealSingle || CanHealSingleAbility)
            {
                if (UseHealPotion(out act)) return true;
            }
        }

        if (HasHostilesInRange && AttackAbility(out act)) return true;
        if (GeneralAbility(out act)) return true;

        //Run!
        if (!InCombat && IsMoving && role == JobRole.RangedPhysical
            && Peloton.CanUse(out act, CanUseOption.MustUse | CanUseOption.IgnoreClippingCheck)) return true;

        return false;
    }

    private static bool InterruptAbility(JobRole role, out IAction act)
    {
        act = null;
        if (!DataCenter.SetAutoStatus(AutoStatus.Interrupt, DataCenter.CanInterruptTargets.Any()))
            return false;


        switch (role)
        {
            case JobRole.Tank:
                if (Interject.CanUse(out act)) return true;
                break;

            case JobRole.Melee:
                if (LegSweep.CanUse(out act)) return true;
                break;

            case JobRole.RangedPhysical:
                if (HeadGraze.CanUse(out act)) return true;
                break;
        }
        return false;
    }

    private bool ShirkOrShield(JobRole role, SpecialCommandType specialType, out IAction act)
    {
        act = null;
        if (role != JobRole.Tank)
        {
            return DataCenter.SetAutoStatus(AutoStatus.TankStance, false);
        }

        switch (specialType)
        {
            case SpecialCommandType.RaiseShirk:
                if (Shirk.CanUse(out act)) return true;
                break;

            case SpecialCommandType.EsunaStanceNorth:
                if (TankStance.CanUse(out act)) return true;
                break;
        }

        if (DataCenter.SetAutoStatus(AutoStatus.TankStance, Configuration.PluginConfiguration.GetValue(SettingsCommand.AutoTankStance)
            && !DataCenter.AllianceTanks.Any(t => t.CurrentHp != 0 && t.HasStatus(false, StatusHelper.TankStanceStatus))
            && !HasTankStance && TankStance.CanUse(out act, CanUseOption.IgnoreClippingCheck)))
        {
            return true;
        }

        return false;
    }

    private static bool AntiKnockback(JobRole role, SpecialCommandType specialType, out IAction act)
    {
        act = null;

        if (specialType != SpecialCommandType.AntiKnockback) return false;

        switch (role)
        {
            case JobRole.Tank:
            case JobRole.Melee:
                if (ArmsLength.CanUse(out act)) return true;
                break;
            case JobRole.Healer:
                if (SureCast.CanUse(out act)) return true;
                break;
            case JobRole.RangedPhysical:
                if (ArmsLength.CanUse(out act)) return true;
                break;
            case JobRole.RangedMagical:
                if (SureCast.CanUse(out act)) return true;
                break;
        }

        return false;
    }

    private bool GeneralHealAbility(SpecialCommandType specialType, out IAction act)
    {
        act = null;

        BaseAction.OtherOption |= CanUseOption.MustUse;
        switch (specialType)
        {
            case SpecialCommandType.DefenseArea:
                if (DefenseAreaAbility(out act)) return true;
                break;

            case SpecialCommandType.DefenseSingle:
                if (DefenseSingleAbility(out act)) return true;
                break;
        }
        BaseAction.OtherOption &= ~CanUseOption.MustUse;

        if ((DataCenter.HPNotFull || ClassJob.GetJobRole() != JobRole.Healer) && InCombat)
        {
            if (DataCenter.SpecialType == SpecialCommandType.HealArea || CanHealAreaAbility)
            {
                if (HealAreaAbility(out act)) return true;
            }
            if (DataCenter.SpecialType == SpecialCommandType.HealSingle || CanHealSingleAbility)
            {
                if (HealSingleAbility(out act)) return true;
            }
        }

        return false;
    }

    private bool AutoDefense(JobRole role, bool helpDefenseAOE, bool helpDefenseSingle, out IAction act)
    {
        act = null;
        if (!InCombat || !HasHostilesInRange)
        {
            DataCenter.SetAutoStatus(AutoStatus.Provoke, false);
            return false;
        }

        //Auto Provoke
        if (DataCenter.SetAutoStatus(AutoStatus.Provoke, role == JobRole.Tank
            && (Configuration.PluginConfiguration.GetValue(SettingsCommand.AutoProvokeForTank) || DataCenter.AllianceTanks.Count() < 2)
            && TargetFilter.ProvokeTarget(DataCenter.HostileTargets, true).Count() != DataCenter.HostileTargets.Count()))
        {
            if (!HasTankStance && TankStance.CanUse(out act)) return true;
            if (Provoke.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        //No using defense abilities.
        if (!Configuration.PluginConfiguration.GetValue(SettingsCommand.UseDefenseAbility)) return false;

        if (helpDefenseAOE)
        {
            if (DefenseAreaAbility(out act)) return true;
            if (role is JobRole.Melee or JobRole.RangedPhysical or JobRole.RangedMagical)
            {
                if (DefenseSingleAbility(out act)) return true;
            }
        }

        //Defense himself.
        if (role == JobRole.Tank && HasTankStance)
        {
            var tarOnMeCount = DataCenter.TarOnMeTargets.Count(t => t.DistanceToPlayer() <= 3);

            //A lot targets are targeting on me.
            if (tarOnMeCount > 1 && !IsMoving)
            {
                if (ArmsLength.CanUse(out act)) return true;
                if (DefenseSingleAbility(out act)) return true;
            }

            //Big damage casting action.
            if (tarOnMeCount == 1 && DataCenter.IsHostileCastingToTank)
            {
                if (DefenseSingleAbility(out act)) return true;
            }
        }

        if (helpDefenseSingle && DefenseSingleAbility(out act)) return true;

        return false;
    }

    private bool MovingAbility(SpecialCommandType specialType, out IAction act)
    {
        act = null;
        if (specialType == SpecialCommandType.MoveForward && MoveForwardAbility(out act))
        {
            if (act is BaseAction b && ObjectHelper.DistanceToPlayer(b.Target) > 5) return true;
        }
        else if (specialType == SpecialCommandType.MoveBack)
        {
            if (MoveBackAbility(out act)) return true;
        }
        return false;
    }

    private bool GeneralUsingAbility(JobRole role, out IAction act)
    {
        act = null;
        switch (role)
        {
            case JobRole.Tank:
                if (LowBlow.CanUse(out act)) return true;
                break;

            case JobRole.Melee:
                if (SecondWind.CanUse(out act)) return true;
                if (Bloodbath.CanUse(out act)) return true;
                break;

            case JobRole.Healer:
            case JobRole.RangedMagical:
                if (Jobs[0] == ECommons.ExcelServices.Job.BLM) break;
                if (LucidDreaming.CanUse(out act)) return true;
                break;

            case JobRole.RangedPhysical:
                if (SecondWind.CanUse(out act)) return true;
                break;
        }
        return false;
    }


    protected virtual bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (nextGCD is BaseAction action)
        {
            if (ClassJob.GetJobRole() is JobRole.Healer or JobRole.RangedMagical &&
            action.CastTime >= 5 && Swiftcast.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;

            if (Configuration.PluginConfiguration.GetValue(SettingsCommand.AutoUseTrueNorth)
                && action.EnemyPositional != EnemyPositional.None && action.Target != null)
            {
                if (action.EnemyPositional != action.Target.FindEnemyPositional() && action.Target.HasPositional())
                {
                    if (TrueNorth.CanUse(out act, CanUseOption.EmptyOrSkipCombo | CanUseOption.OnLastAbility)) return true;
                }
            }
        }

        act = null;
        return false;
    }

    [RotationDesc(DescType.MoveForwardAbility)]
    protected virtual bool MoveForwardAbility(out IAction act)
    {
        act = null; return false;
    }

    [RotationDesc(DescType.MoveBackAbility)]
    protected virtual bool MoveBackAbility(out IAction act)
    {
        act = null; return false;
    }

    [RotationDesc(DescType.HealSingleAbility)]
    protected virtual bool HealSingleAbility(out IAction act)
    {
        act = null; return false;
    }

    [RotationDesc(DescType.HealAreaAbility)]
    protected virtual bool HealAreaAbility(out IAction act)
    {
        act = null; return false;
    }

    [RotationDesc(DescType.DefenseSingleAbility)]
    protected virtual bool DefenseSingleAbility(out IAction act)
    {
        act = null; return false;
    }

    [RotationDesc(DescType.DefenseAreaAbility)]
    protected virtual bool DefenseAreaAbility(out IAction act)
    {
        act = null; return false;
    }

    protected virtual bool GeneralAbility(out IAction act)
    {
        act = null; return false;
    }

    protected abstract bool AttackAbility(out IAction act);
}
