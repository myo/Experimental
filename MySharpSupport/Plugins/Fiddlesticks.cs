namespace MySharpSupport.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class FiddleSticks : PluginBase
    {
        public FiddleSticks()
        {
            this.Q = new Spell(SpellSlot.Q, 575);
            this.W = new Spell(SpellSlot.W, 575);
            this.E = new Spell(SpellSlot.E, 750);
            this.R = new Spell(SpellSlot.R, 800);
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.E", "Use E", true);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.E", "Use E", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.Q", "Use Q to Interrupt Gapcloser", true);

            config.AddBool("Interrupt.Q", "Use Q to Interrupt Spells", true);
            config.AddBool("Interrupt.E", "Use E to Interrupt Spells", true);
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (this.Q.CastCheck(gapcloser.Sender, "Gapcloser.Q"))
            {
                this.Q.CastOnUnit(gapcloser.Sender);
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
                this.Q.CastOnUnit(target);
                return;
            }

            if (this.E.CastCheck(target, "Interrupt.E"))
            {
                this.E.CastOnUnit(target);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (this.ComboMode)
            {
                if (this.Q.CastCheck(this.Target, "Combo.Q"))
                {
                    this.Q.CastOnUnit(this.Target);
                }

                if (this.E.CastCheck(this.Target, "Combo.E"))
                {
                    this.E.CastOnUnit(this.Target);
                }
            }

            if (this.HarassMode)
            {
                if (this.E.CastCheck(this.Target, "Harass.E"))
                {
                    this.E.CastOnUnit(this.Target);
                }
            }
        }
    }
}