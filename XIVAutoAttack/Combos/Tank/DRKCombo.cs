using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Collections.Generic;
using XIVAutoAttack.Actions;
using XIVAutoAttack.Actions.BaseAction;
using XIVAutoAttack.Combos.CustomCombo;
using XIVAutoAttack.Configuration;
using XIVAutoAttack.Data;
using XIVAutoAttack.Helpers;
using XIVAutoAttack.Updaters;

namespace XIVAutoAttack.Combos.Tank;

internal sealed class DRKCombo : JobGaugeCombo<DRKGauge>
{
    internal override uint JobID => 32;
    internal override bool HaveShield => Player.HaveStatus(ObjectStatus.Grit);
    private protected override BaseAction Shield => Actions.Grit;
    protected override bool CanHealSingleAbility => false;

    //private static bool openerFinished = false;

    /// <summary>
    /// 在4人本的道中已经聚好怪可以使用相关技能(不移动且身边有大于3只小怪)
    /// </summary>
    private static bool CanUseSpellInDungeonsMiddle => TargetUpdater.PartyMembers.Length is > 1 and <= 4 && !Target.IsBoss() && !IsMoving
                                                    && TargetFilter.GetObjectInRadius(TargetUpdater.HostileTargets, 5).Length >= 3;

    /// <summary>
    /// 在4人本的道中
    /// </summary>
    private static bool InDungeonsMiddle => TargetUpdater.PartyMembers.Length is > 1 and <= 4 && !Target.IsBoss();

    internal struct Actions
    {
        public static readonly BaseAction
            //重斩
            HardSlash = new (3617),

            //吸收斩
            SyphonStrike = new (3623),

            //释放
            Unleash = new (3621),

            //深恶痛绝
            Grit = new (3629, shouldEndSpecial: true),

            //伤残
            Unmend = new (3624)
            {
                FilterForTarget = b => TargetFilter.ProvokeTarget(b),
            },

            //噬魂斩
            Souleater = new (3632),

            //暗黑波动
            FloodofDarkness = new (16466),

            //暗黑锋
            EdgeofDarkness = new (16467)
            {
                OtherCheck = b => !IsLastAction(true, EdgeofDarkness, FloodofDarkness) && Player.CurrentMp >= 3000,
            },

            //嗜血
            BloodWeapon = new (3625)
            {
                OtherCheck = b => JobGauge.DarksideTimeRemaining > 0,
            },

            //暗影墙
            ShadowWall = new (3636)
            {
                BuffsProvide = new [] { ObjectStatus.ShadowWall },
                OtherCheck = BaseAction.TankDefenseSelf,
            },

            //弃明投暗
            DarkMind = new(3634)
            {
                OtherCheck = BaseAction.TankDefenseSelf,
            },

            //行尸走肉
            LivingDead = new (3638)
            {
                OtherCheck = BaseAction.TankBreakOtherCheck,
            },

            //腐秽大地
            SaltedEarth = new (3639),

            //跳斩
            Plunge = new (3640, shouldEndSpecial:true)
            {
                ChoiceTarget = TargetFilter.FindTargetForMoving
            },

            //吸血深渊
            AbyssalDrain = new (3641),

            //精雕怒斩
            CarveandSpit = new (3643),

            //血溅
            Bloodspiller = new (7392)
            {
                OtherCheck = b => JobGauge.Blood >= 50 || Player.HaveStatus(ObjectStatus.Delirium),
            },

            //寂灭
            Quietus = new (7391),

            //血乱
            Delirium = new (7390)
            {
                OtherCheck = b => JobGauge.DarksideTimeRemaining > 0,
            },

            //至黑之夜
            TheBlackestNight = new (7393)
            {
                ChoiceTarget = TargetFilter.FindAttackedTarget,
            },

            //刚魂
            StalwartSoul = new (16468),

            //暗黑布道
            DarkMissionary = new (16471, true),

            //掠影示现
            LivingShadow = new (16472)
            {
                OtherCheck = b => JobGauge.Blood >= 50 && JobGauge.DarksideTimeRemaining > 1,
            },

            //献奉
            Oblation = new (25754, true)
            {
                ChoiceTarget = TargetFilter.FindAttackedTarget,
            },
            
            //暗影使者
            Shadowbringer = new (25757)
            {
                OtherCheck = b => JobGauge.DarksideTimeRemaining > 1 && !IsLastAction(true, Shadowbringer),
            },

            //腐秽黑暗
            SaltandDarkness = new (25755)
            {
                BuffsNeed = new [] { ObjectStatus.SaltedEarth },
            };
    }
    internal override SortedList<DescType, string> Description => new ()
    {
        {DescType.单体治疗, $"{Actions.TheBlackestNight}，目标为被打的小可怜"},
        {DescType.范围防御, $"{Actions.DarkMissionary}"},
        {DescType.单体防御, $"{Actions.Oblation}, {Actions.ShadowWall}, {Actions.DarkMind}"},
        {DescType.移动技能, $"{Actions.Plunge}"},
    };

    private protected override ActionConfiguration CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetBool("TheBlackestNight", true, "留3000蓝");
    }

    private protected override bool HealSingleAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.TheBlackestNight.ShouldUse(out act)) return true;

        return false;
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.DarkMissionary.ShouldUse(out act)) return true;
        if (GeneralActions.Reprisal.ShouldUse(out act, mustUse: true)) return true;

        return false;
    }

    private protected override bool GeneralGCD(uint lastComboActionID, out IAction act)
    {
        ///起手判断
        //if (!InCombat) openerFinished = false;
        //if (IsLastWeaponSkill(true, Actions.Souleater) || Actions.Unleash.ShouldUse(out _) || !Actions.Shadowbringer.EnoughLevel) openerFinished = true;

        //寂灭
        if (JobGauge.Blood >= 80 || Player.HaveStatus(ObjectStatus.Delirium))
        {
            if (Actions.Quietus.ShouldUse(out act)) return true;
        }

        //血溅
        if (Actions.Bloodspiller.ShouldUse(out act)) 
        {
            if (Player.HaveStatus(ObjectStatus.Delirium) && Player.FindStatusStack(ObjectStatus.BloodWeapon) <= 3) return true;

            if ((JobGauge.Blood >= 50 && Actions.BloodWeapon.WillHaveOneChargeGCD(1)) || (JobGauge.Blood >= 90 && !Player.HaveStatus(ObjectStatus.Delirium))) return true;

            if (!Actions.Delirium.EnoughLevel) return true;

        }

        //AOE
        if (Actions.StalwartSoul.ShouldUse(out act, lastComboActionID)) return true;
        if (Actions.Unleash.ShouldUse(out act)) return true;

        //单体
        if (Actions.Souleater.ShouldUse(out act, lastComboActionID)) return true;
        if (Actions.SyphonStrike.ShouldUse(out act, lastComboActionID)) return true;
        if (Actions.HardSlash.ShouldUse(out act, lastComboActionID)) return true;

        if (CommandController.Move && MoveAbility(1, out act)) return true;
        if (Actions.Unmend.ShouldUse(out act))
        {
            if (InDungeonsMiddle && Target.DistanceToPlayer() < 5) return false;
            return true;
        }

        return false;
    }
    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        if (SettingBreak && (InDungeonsMiddle && CanUseSpellInDungeonsMiddle || !InDungeonsMiddle))
        {
            //嗜血
            if (Actions.BloodWeapon.ShouldUse(out act)) return true;

            //血乱
            if (Actions.Delirium.ShouldUse(out act)) return true;
        }

        //掠影示现
        if (Actions.LivingShadow.ShouldUse(out act)) return true;

        //暗黑波动
        if (Actions.FloodofDarkness.ShouldUse(out act))
        {
            if ((Player.CurrentMp >= 6000 || JobGauge.HasDarkArts) && Actions.Unleash.ShouldUse(out _)) return true;
        }

        //暗黑锋
        if (Actions.EdgeofDarkness.ShouldUse(out act))
        {do{
            if (InDungeonsMiddle && TargetFilter.GetObjectInRadius(TargetUpdater.HostileTargets, 25).Length >= 3) break;

            //是否留3000蓝开黑盾
            if (Config.GetBoolByName("TheBlackestNight") && Player.CurrentMp < 6000) break;
            
            //爆发期打完
            if (Actions.Delirium.IsCoolDown && Actions.Delirium.ElapsedAfterGCD(1) && !Actions.Delirium.ElapsedAfterGCD(7)) return true;

            //非爆发期防止溢出+续buff
            if (JobGauge.HasDarkArts || (Player.CurrentMp > 8500 && openerFinished) || JobGauge.DarksideTimeRemaining < 10) return true;

            } while (false);
        }

        if (!IsMoving && Actions.SaltedEarth.ShouldUse(out act, mustUse: true)) return true;

        if (Actions.Delirium.ElapsedAfterGCD(1) && !Actions.Delirium.ElapsedAfterGCD(8))
        {
            //暗影使者
            if (Actions.Shadowbringer.ShouldUse(out act)) return true;

            //吸血深渊+精雕怒斩
            if (Actions.AbyssalDrain.ShouldUse(out act)) return true;
            if (Actions.CarveandSpit.ShouldUse(out act)) return true;

            if (Actions.Shadowbringer.ShouldUse(out act, mustUse: true)) return true;

        }
        //吸血深渊+精雕怒斩
        if (!Actions.Delirium.EnoughLevel && Actions.AbyssalDrain.ShouldUse(out act)) return true;
        if (!Actions.Delirium.EnoughLevel && Actions.CarveandSpit.ShouldUse(out act)) return true;

        //腐秽大地+腐秽黑暗
        if (Actions.SaltandDarkness.ShouldUse(out act)) return true;

        //搞搞攻击
        if (Actions.Plunge.ShouldUse(out act) && !IsMoving)
        {
            if (TargetFilter.DistanceToPlayer(Actions.Plunge.Target) < 1) return true;
        }

        return false;
    }

    private protected override bool EmergercyAbility(byte abilityRemain, IAction nextGCD, out IAction act)
    {
        //行尸走肉
        if (Actions.LivingDead.ShouldUse(out act)) return true;

        return base.EmergercyAbility(abilityRemain, nextGCD, out act);
    }

    private protected override bool DefenceSingleAbility(byte abilityRemain, out IAction act)
    {
        //上黑盾
        if (Actions.TheBlackestNight.ShouldUse(out act)) return true;

        if (abilityRemain == 2)
        {
            //减伤10%
            if (Actions.Oblation.ShouldUse(out act)) return true;

            //减伤30%
            if (Actions.ShadowWall.ShouldUse(out act)) return true;

            //减伤20%
            if (GeneralActions.Rampart.ShouldUse(out act)) return true;
            if (Actions.DarkMind.ShouldUse(out act)) return true;
        }
        //降低攻击
        //雪仇
        if (GeneralActions.Reprisal.ShouldUse(out act)) return true;

        act = null;
        return false;
    }

    private protected override bool MoveAbility(byte abilityRemain, out IAction act)
    {
        if (Actions.Plunge.ShouldUse(out act, emptyOrSkipCombo: true)) return true;

        return false;
    }
}
