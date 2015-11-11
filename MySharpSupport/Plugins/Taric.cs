namespace MySharpSupport.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Taric : PluginBase
    {
        public Taric()
        {
            this.Q = new Spell(SpellSlot.Q, 750);
            this.W = new Spell(SpellSlot.W, 200);
            this.E = new Spell(SpellSlot.E, 625);
            this.R = new Spell(SpellSlot.R, 200);
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboE", "Use E", true);
            config.AddBool("ComboR", "Use R", true);
            config.AddSlider("ComboHealthQ", "Health to Heal", 20, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "Use Q", true);
            config.AddBool("HarassE", "Use E", true);
            config.AddSlider("HarassHealthQ", "Health to Heal", 20, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserE", "Use E to Interrupt Gapcloser", true);

            config.AddBool("InterruptE", "Use E to Interrupt Spells", true);
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (this.E.CastCheck(gapcloser.Sender, "GapcloserE"))
            {
                this.E.Cast(gapcloser.Sender);
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
                this.E.Cast(target);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (this.ComboMode)
            {
                var ally = Helpers.AllyBelowHp(this.ConfigValue<Slider>("ComboHealthQ").Value, this.Q.Range);
                if (this.Q.CastCheck(ally, "ComboQ", true, false))
                {
                    this.Q.Cast(ally);
                }

                if (this.W.CastCheck(this.Target, "ComboW"))
                {
                    this.W.Cast();
                }

                if (this.E.CastCheck(this.Target, "ComboE"))
                {
                    this.E.Cast(this.Target);
                }

                if (this.R.CastCheck(this.Target, "ComboR"))
                {
                    this.R.Cast();
                }
            }

            if (this.HarassMode)
            {
                var ally = Helpers.AllyBelowHp(this.ConfigValue<Slider>("HarassHealthQ").Value, this.Q.Range);
                if (this.Q.CastCheck(ally, "HarassQ", true, false))
                {
                    this.Q.Cast(ally);
                }

                if (this.E.CastCheck(this.Target, "HarassE"))
                {
                    this.E.Cast(this.Target);
                }
            }
        }
    }
}