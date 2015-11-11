namespace MySharpSupport.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Leona : PluginBase
    {
        public Leona()
        {
            this.Q = new Spell(SpellSlot.Q, this.AttackRange);
            this.W = new Spell(SpellSlot.W, this.AttackRange);
            this.E = new Spell(SpellSlot.E, 700);
            this.R = new Spell(SpellSlot.R, 1200);

            this.E.SetSkillshot(0.25f, 100f, 2000f, false, SkillshotType.SkillshotLine);
            this.R.SetSkillshot(1f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboE", "Use E without Q", false);
            config.AddBool("ComboQWE", "Use Q/W/E", true);
            config.AddBool("ComboR", "Use R", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserQ", "Use Q to Interrupt Gapcloser", true);

            config.AddBool("InterruptQ", "Use Q to Interrupt Spells", true);
            config.AddBool("InterruptR", "Use R to Interrupt Spells", true);
        }

        public override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            if (!(target is Obj_AI_Hero) && !target.Name.ToLower().Contains("ward"))
            {
                return;
            }

            if (!this.Q.IsReady())
            {
                return;
            }

            if (this.Q.Cast())
            {
                Orbwalking.ResetAutoAttackTimer();
                this.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (this.Q.CastCheck(gapcloser.Sender, "GapcloserQ"))
            {
                if (this.Q.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    this.Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                }
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (this.Q.CastCheck(target, "InterruptQ"))
            {
                if (this.Q.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    this.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                return;
            }

            if (this.R.CastCheck(target, "InterruptR"))
            {
                this.R.Cast(target);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (this.ComboMode)
            {
                if (this.Q.CastCheck(this.Target))
                {
                    Orbwalking.ResetAutoAttackTimer();
                    this.Player.IssueOrder(GameObjectOrder.AttackUnit, this.Target);
                }

                if (this.W.CastCheck(this.Target, "ComboQWE"))
                {
                    this.W.Cast();
                }

                if (this.E.CastCheck(this.Target, "ComboQWE") && this.Q.IsReady())
                {
                    // Max Range with VeryHigh Hitchance / Immobile
                    if (this.E.GetPrediction(this.Target).Hitchance >= HitChance.VeryHigh)
                    {
                        if (this.E.Cast(this.Target) == Spell.CastStates.SuccessfullyCasted)
                        {
                            this.W.Cast();
                        }
                    }

                    // Lower Range
                    if (this.E.GetPrediction(this.Target, false, 775).Hitchance >= HitChance.High)
                    {
                        if (this.E.Cast(this.Target) == Spell.CastStates.SuccessfullyCasted)
                        {
                            this.W.Cast();
                        }
                    }
                }

                if (this.E.CastCheck(this.Target, "ComboE"))
                {
                    this.E.Cast(this.Target);
                }

                if (this.R.CastCheck(this.Target, "ComboR"))
                {
                    this.R.CastIfHitchanceEquals(this.Target, HitChance.Immobile);
                }
            }
        }
    }
}