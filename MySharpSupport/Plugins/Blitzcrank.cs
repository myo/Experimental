namespace MySharpSupport.Plugins
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Blitzcrank : PluginBase
    {
        public Blitzcrank()
        {
            this.Q = new Spell(SpellSlot.Q, 900);
            this.W = new Spell(SpellSlot.W, 0);
            this.E = new Spell(SpellSlot.E, this.AttackRange);
            this.R = new Spell(SpellSlot.R, 600);

            this.Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);
        }

        private bool BlockQ
        {
            get
            {
                if (!this.Q.IsReady())
                {
                    return true;
                }

                if (!this.ConfigValue<bool>("Misc.Q.Block"))
                {
                    return false;
                }

                if (!this.Target.IsValidTarget())
                {
                    return true;
                }

                if (this.Target.HasBuff("BlackShield"))
                {
                    return true;
                }

                if (
                    Helpers.AllyInRange(1200)
                           .Any(ally => ally.Distance(this.Target) < ally.AttackRange + ally.BoundingRadius))
                {
                    return true;
                }

                return this.Player.Distance(this.Target) < this.ConfigValue<Slider>("Misc.Q.Block.Distance").Value;
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboR", "Use R", true);
            config.AddSlider("ComboCountR", "Targets in range to Ult", 2, 1, 5);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "Use Q", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserE", "Use E to Interrupt Gapcloser", true);
            config.AddBool("GapcloserR", "Use R to Interrupt Gapcloser", true);
            config.AddBool("InterruptQ", "Use Q to Interrupt Spells", true);
            config.AddBool("InterruptE", "Use E to Interrupt Spells", true);
            config.AddBool("InterruptR", "Use R to Interrupt Spells", true);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("Misc.Q.Block", "Block Q on close Targets", true);
            config.AddSlider("Misc.Q.Block.Distance", "Q Block Distance", 400, 0, 800);
        }

        public override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            if (!target.IsValid<Obj_AI_Hero>() && !target.Name.ToLower().Contains("ward"))
            {
                return;
            }

            if (!this.E.IsReady())
            {
                return;
            }

            if (this.E.Cast())
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

            if (this.E.CastCheck(gapcloser.Sender, "GapcloserE"))
            {
                if (this.E.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    this.Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                }
            }

            if (this.R.CastCheck(gapcloser.Sender, "GapcloserR"))
            {
                this.R.Cast();
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (this.E.CastCheck(target, "InterruptE"))
            {
                if (this.E.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    this.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }

            if (this.Q.CastCheck(this.Target, "InterruptQ"))
            {
                this.Q.Cast(target);
            }

            if (this.R.CastCheck(target, "InterruptR"))
            {
                this.R.Cast();
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (this.ComboMode)
                {
                    if (this.Q.CastCheck(this.Target, "ComboQ") && !this.BlockQ)
                    {
                        this.Q.Cast(this.Target);
                    }

                    if (this.E.CastCheck(this.Target))
                    {
                        if (this.E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            this.Player.IssueOrder(GameObjectOrder.AttackUnit, this.Target);
                        }
                    }

                    if (this.E.IsReady() && this.Target.IsValidTarget() && this.Target.HasBuff("RocketGrab"))
                    {
                        if (this.E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            this.Player.IssueOrder(GameObjectOrder.AttackUnit, this.Target);
                        }
                    }

                    if (this.W.IsReady() && this.ConfigValue<bool>("ComboW")
                        && this.Player.CountEnemiesInRange(1500) > 0)
                    {
                        this.W.Cast();
                    }

                    if (this.R.CastCheck(this.Target, "ComboR"))
                    {
                        if (Helpers.EnemyInRange(this.ConfigValue<Slider>("ComboCountR").Value, this.R.Range))
                        {
                            this.R.Cast();
                        }
                    }
                }

                if (this.HarassMode)
                {
                    if (this.Q.CastCheck(this.Target, "HarassQ") && !this.BlockQ)
                    {
                        this.Q.Cast(this.Target);
                    }

                    if (this.E.CastCheck(this.Target))
                    {
                        if (this.E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            this.Player.IssueOrder(GameObjectOrder.AttackUnit, this.Target);
                        }
                    }

                    if (this.E.IsReady() && this.Target.IsValidTarget() && this.Target.HasBuff("RocketGrab"))
                    {
                        if (this.E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            this.Player.IssueOrder(GameObjectOrder.AttackUnit, this.Target);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}