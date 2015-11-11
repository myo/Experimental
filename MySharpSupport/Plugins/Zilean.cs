namespace MySharpSupport.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Zilean : PluginBase
    {
        public Zilean()
        {
            this.Q = new Spell(SpellSlot.Q, 700);
            this.W = new Spell(SpellSlot.W, 0);
            this.E = new Spell(SpellSlot.E, 700);
            this.R = new Spell(SpellSlot.R, 900);
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboE", "Use E", true);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "Use Q", true);
            config.AddBool("HarassW", "Use W", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserE", "Use E to Interrupt Gapcloser", true);
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

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (this.ComboMode)
                {
                    if (this.Q.CastCheck(this.Target, "ComboQ"))
                    {
                        this.Q.Cast(this.Target);
                    }

                    if (this.W.IsReady() && !this.Q.IsReady() && this.ConfigValue<bool>("ComboW"))
                    {
                        this.W.Cast();
                    }

                    // TODO: speed adc/jungler/engage
                    if (this.E.IsReady() && this.Player.CountEnemiesInRange(2000) > 0
                        && this.ConfigValue<bool>("ComboE"))
                    {
                        this.E.Cast(this.Player);
                    }
                }

                if (this.HarassMode)
                {
                    if (this.Q.CastCheck(this.Target, "HarassQ"))
                    {
                        this.Q.Cast(this.Target);
                    }

                    if (this.W.IsReady() && !this.Q.IsReady() && this.ConfigValue<bool>("HarassW"))
                    {
                        this.W.Cast();
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