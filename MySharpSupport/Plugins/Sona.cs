namespace MySharpSupport.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Sona : PluginBase
    {
        public Sona()
        {
            this.Q = new Spell(SpellSlot.Q, 850);
            this.W = new Spell(SpellSlot.W, 1000);
            this.E = new Spell(SpellSlot.E, 350);
            this.R = new Spell(SpellSlot.R, 1000);

            this.R.SetSkillshot(0.5f, 125, float.MaxValue, false, SkillshotType.SkillshotLine);
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboE", "Use E", true);
            config.AddBool("ComboR", "Use R", true);
            config.AddSlider("ComboCountR", "Targets hit to Ult", 3, 1, 5);
            config.AddSlider("ComboHealthW", "Health to Heal", 80, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "Use Q", true);
            config.AddBool("HarassW", "Use W", true);
            config.AddBool("HarassE", "Use E", true);
            config.AddSlider("HarassHealthW", "Health to Heal", 60, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserR", "Use R to Interrupt Gapcloser", false);

            config.AddBool("InterruptR", "Use R to Interrupt Spells", true);
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (this.R.CastCheck(gapcloser.Sender, "GapcloserR"))
            {
                this.R.Cast(this.Target, true);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (this.R.CastCheck(target, "InterruptR"))
            {
                this.R.Cast(this.Target, true);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (this.ComboMode)
                {
                    if (this.Q.CastCheck(this.Target, "ComboQ"))
                    {
                        this.Q.Cast();
                    }

                    //if (Target.IsValidTarget(AttackRange) &&
                    //    (Player.HasBuff("sonaqprocattacker") || Player.HasBuff("sonaqprocattacker")))
                    //{
                    //    Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                    //}

                    var allyW = Helpers.AllyBelowHp(this.ConfigValue<Slider>("ComboHealthW").Value, this.W.Range);
                    if (this.W.CastCheck(allyW, "ComboW", true, false))
                    {
                        this.W.Cast();
                    }

                    if (this.E.IsReady() && Helpers.AllyInRange(this.E.Range).Count > 0
                        && this.ConfigValue<bool>("ComboE"))
                    {
                        this.E.Cast();
                    }

                    if (this.R.CastCheck(this.Target, "ComboR"))
                    {
                        this.R.CastIfWillHit(this.Target, this.ConfigValue<Slider>("ComboCountR").Value, true);
                    }
                }

                if (this.HarassMode)
                {
                    if (this.Q.CastCheck(this.Target, "HarassQ"))
                    {
                        this.Q.Cast();
                    }

                    var allyW = Helpers.AllyBelowHp(this.ConfigValue<Slider>("HarassHealthW").Value, this.W.Range);
                    if (this.W.CastCheck(allyW, "HarassW", true, false))
                    {
                        this.W.Cast();
                    }

                    if (this.E.IsReady() && Helpers.AllyInRange(this.E.Range).Count > 0
                        && this.ConfigValue<bool>("HarassE"))
                    {
                        this.E.Cast();
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