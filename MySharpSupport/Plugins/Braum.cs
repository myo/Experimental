namespace MySharpSupport.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using MySharpSupport.Evade;
    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;
    using SpellData = LeagueSharp.SpellData;

    public class Braum : PluginBase
    {
        public Braum()
        {
            this.Q = new Spell(SpellSlot.Q, 1000);
            this.W = new Spell(SpellSlot.W, 650);
            this.E = new Spell(SpellSlot.E, 0);
            this.R = new Spell(SpellSlot.R, 1200);

            this.Q.SetSkillshot(0.25f, 60f, 1700f, true, SkillshotType.SkillshotLine);
            this.R.SetSkillshot(0.5f, 115f, 1400f, false, SkillshotType.SkillshotLine);
            Protector.OnSkillshotProtection += this.ProtectorOnSkillshotProtection;
            Protector.OnTargetedProtection += this.ProtectorOnTargetedProtection;
        }

        private bool IsShieldActive { get; set; }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.R", "Use R", true);
            config.AddSlider("Combo.R.Count", "Targets hit by R", 2, 1, 5);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "Use Q", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.Q", "Use Q to Interrupt Gapcloser", true);
            config.AddBool("Interrupt.R", "Use R to Interrupt Spells", true);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("Misc.Shield.Skill", "Shield Skillshots", true);
            config.AddBool("Misc.Shield.Target", "Shield Targeted", true);
            config.AddSlider("Misc.Shield.Health", "Shield AA below HP", 30, 1, 100);
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (this.Q.CastCheck(gapcloser.Sender, "Gapcloser.Q"))
            {
                this.Q.Cast(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (this.R.CastCheck(target, "Interrupt.R"))
            {
                this.R.Cast(target);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (this.ComboMode)
            {
                if (this.Q.CastCheck(this.Target, "Combo.Q"))
                {
                    this.Q.Cast(this.Target);
                }

                if (this.R.CastCheck(this.Target, "Combo.R"))
                {
                    this.R.CastIfWillHit(this.Target, this.ConfigValue<Slider>("Combo.R.Count").Value - 1);
                }
            }

            if (this.HarassMode)
            {
                if (this.Q.CastCheck(this.Target, "Harass.Q"))
                {
                    this.Q.Cast(this.Target);
                }
            }
        }

        private void CastShield(Vector3 v)
        {
            if (!this.E.IsReady())
            {
                return;
            }

            this.E.Cast(v);
            this.IsShieldActive = true;
            Utility.DelayAction.Add(4000, () => this.IsShieldActive = false);
        }

        private void ProtectorOnSkillshotProtection(Obj_AI_Hero target, List<Skillshot> skillshots)
        {
            try
            {
                if (!this.ConfigValue<bool>("Misc.Shield.Skill"))
                {
                    return;
                }

                // get most dangerous skillshot
                var max = skillshots.First();
                foreach (var spell in
                    skillshots.Where(
                        s =>
                        s.SpellData.Type == SkillShotType.SkillshotMissileLine
                        || s.SpellData.Type == SkillShotType.SkillshotMissileCone))
                {
                    if (spell.Unit.GetSpellDamage(target, spell.SpellData.SpellName)
                        > max.Unit.GetSpellDamage(target, max.SpellData.SpellName))
                    {
                        max = spell;
                    }
                }

                if (target.IsMe && this.E.IsReady())
                {
                    this.CastShield(max.Start.To3D());
                }

                if (!target.IsMe && this.W.IsReady() && this.W.IsInRange(target)
                    && (this.IsShieldActive || this.E.IsReady()))
                {
                    var jumpTime = (this.Player.Distance(target) * 1000 / this.W.Instance.SData.MissileSpeed)
                                   + (this.W.Instance.SData.SpellCastTime * 1000);
                    var missileTime = target.Distance(max.MissilePosition) * 1000 / max.SpellData.MissileSpeed;

                    if (jumpTime > missileTime)
                    {
                        Console.WriteLine("Abort Jump - Missile too Fast: {0} {1}", jumpTime, missileTime);
                        return;
                    }

                    this.W.CastOnUnit(target);
                    Utility.DelayAction.Add((int)jumpTime, () => this.CastShield(max.Start.To3D()));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ProtectorOnTargetedProtection(Obj_AI_Base caster, Obj_AI_Hero target, SpellData spell)
        {
            try
            {
                if (!this.ConfigValue<bool>("Misc.Shield.Target"))
                {
                    return;
                }

                if (Orbwalking.IsAutoAttack(spell.Name)
                    && target.HealthPercent > this.ConfigValue<Slider>("Misc.Shield.Health").Value)
                {
                    return;
                }

                if (spell.MissileSpeed > 2000 || spell.MissileSpeed == 0)
                {
                    return;
                }

                // TODO: blacklist FiddleQ, FioraQ/R, LeonaE, VladQ, ZileanQ

                if (target.IsMe && this.E.IsReady())
                {
                    this.CastShield(caster.Position);
                }

                if (!target.IsMe && this.W.IsReady() && this.W.IsInRange(target)
                    && (this.IsShieldActive || this.E.IsReady()))
                {
                    var jumpTime = (this.Player.Distance(target) * 1000 / this.W.Instance.SData.MissileSpeed)
                                   + (this.W.Instance.SData.SpellCastTime * 1000);
                    var missileTime = caster.Distance(target) * 1000 / spell.MissileSpeed;

                    if (jumpTime > missileTime)
                    {
                        Console.WriteLine("Abort Jump - Missile too Fast: {0} {1}", jumpTime, missileTime);
                        return;
                    }

                    this.W.CastOnUnit(target);
                    Utility.DelayAction.Add((int)jumpTime, () => this.CastShield(caster.Position));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}