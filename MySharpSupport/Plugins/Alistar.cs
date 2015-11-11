namespace MySharpSupport.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Alistar : PluginBase
    {
        public Alistar()
        {
            this.Q = new Spell(SpellSlot.Q, 365);
            this.W = new Spell(SpellSlot.W, 650);
            this.E = new Spell(SpellSlot.E, 575);
            this.R = new Spell(SpellSlot.R, 0);

            this.W.SetTargetted(0.5f, float.MaxValue);
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.W", "Use WQ", true);
            config.AddBool("Combo.E", "Use E", true);
            config.AddSlider("Combo.E.Health", "Health to Heal", 20, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "Use Q", true);
            config.AddBool("Harass.E", "Use E", true);
            config.AddSlider("Harass.E.Health", "Health to Heal", 20, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.W", "Use W to Interrupt Gapcloser", true);

            config.AddBool("Interrupt.Q", "Use Q to Interrupt Spells", true);
            config.AddBool("Interrupt.W", "Use W to Interrupt Spells", true);
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (this.W.CastCheck(gapcloser.Sender, "Gapcloser.W"))
            {
                this.W.CastOnUnit(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (this.Q.CastCheck(target, "Interrupt.Q"))
            {
                this.Q.Cast();
            }

            if (this.W.CastCheck(target, "Interrupt.W"))
            {
                this.W.CastOnUnit(target);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (this.ComboMode)
            {
                if (this.Q.CastCheck(this.Target, "Combo.Q"))
                {
                    this.Q.Cast();
                }

                if (this.Q.IsReady() && this.W.CastCheck(this.Target, "Combo.W"))
                {
                    this.W.CastOnUnit(this.Target);
                    var jumpTime = Math.Max(0, this.Player.Distance(this.Target) - 500) * 10 / 25 + 25;
                    Utility.DelayAction.Add((int)jumpTime, () => this.Q.Cast());
                }

                var ally = Helpers.AllyBelowHp(this.ConfigValue<Slider>("Combo.E.Health").Value, this.E.Range);
                if (this.E.CastCheck(ally, "Combo.E", true, false))
                {
                    this.E.Cast();
                }
            }

            if (this.HarassMode)
            {
                if (this.Q.CastCheck(this.Target, "Harass.Q"))
                {
                    this.Q.Cast();
                }

                var ally = Helpers.AllyBelowHp(this.ConfigValue<Slider>("Harass.E.Health").Value, this.E.Range);
                if (this.E.CastCheck(ally, "Harass.E", true, false))
                {
                    this.E.Cast();
                }
            }
        }
    }
}