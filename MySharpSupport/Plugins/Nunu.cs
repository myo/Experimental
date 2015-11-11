namespace MySharpSupport.Plugins
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Nunu : PluginBase
    {
        public Nunu()
        {
            this.Q = new Spell(SpellSlot.Q, 125);
            this.W = new Spell(SpellSlot.W, 700);
            this.E = new Spell(SpellSlot.E, 550);
            this.R = new Spell(SpellSlot.R, 650);
        }

        private int last = 0;

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.W", "Use W", true);
            config.AddBool("Combo.E", "Use E", true);
            config.AddSlider("Combo.Q.Health", "Consume below %HP", 50, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "Use Q", true);
            config.AddBool("Harass.W", "Use W", false);
            config.AddBool("Harass.E", "Use E", true);
            config.AddSlider("Harass.Q.Health", "Consume below %HP", 50, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.E", "Use E to Interrupt Gapcloser", true);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddList("Misc.Laugh", "Laugh Emote", new[] { "OFF", "ON" });
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (this.E.CastCheck(gapcloser.Sender, "Gapcloser.E"))
            {
                this.E.CastOnUnit(gapcloser.Sender);

                if (this.W.IsReady())
                {
                    this.W.CastOnUnit(this.Player);
                }
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (this.ComboMode)
            {
                if (this.Q.IsReady() && this.ConfigValue<bool>("Combo.Q")
                    && this.Player.HealthPercent < this.ConfigValue<Slider>("Combo.Q.Health").Value)
                {
                    var minion = MinionManager.GetMinions(this.Player.Position, this.Q.Range).FirstOrDefault();
                    if (minion.IsValidTarget(this.Q.Range))
                    {
                        this.Q.CastOnUnit(minion);
                    }
                }

                var allys = Helpers.AllyInRange(this.W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (this.W.IsReady() && allys.Count > 0 && this.ConfigValue<bool>("Combo.W"))
                {
                    this.W.CastOnUnit(allys.FirstOrDefault());
                }

                if (this.W.IsReady() && this.Target.IsValidTarget(this.AttackRange) && this.ConfigValue<bool>("Combo.W"))
                {
                    this.W.CastOnUnit(this.Player);
                }

                if (this.E.IsReady() && this.Target.IsValidTarget(this.E.Range) && this.ConfigValue<bool>("Combo.E"))
                {
                    this.E.CastOnUnit(this.Target);
                }
            }

            if (this.HarassMode)
            {
                if (this.Q.IsReady() && this.ConfigValue<bool>("Harass.Q")
                    && this.Player.HealthPercent < this.ConfigValue<Slider>("Harass.Q.Health").Value)
                {
                    var minion = MinionManager.GetMinions(this.Player.Position, this.Q.Range).FirstOrDefault();
                    if (minion.IsValidTarget(this.Q.Range))
                    {
                        this.Q.CastOnUnit(minion);
                    }
                }

                var allys = Helpers.AllyInRange(this.W.Range).OrderByDescending(h => h.FlatPhysicalDamageMod).ToList();
                if (this.W.IsReady() && allys.Count > 0 && this.ConfigValue<bool>("Harass.W"))
                {
                    this.W.CastOnUnit(allys.FirstOrDefault());
                }

                if (this.W.IsReady() && this.Target.IsValidTarget(this.AttackRange)
                    && this.ConfigValue<bool>("Harass.W"))
                {
                    this.W.CastOnUnit(this.Player);
                }

                if (this.E.IsReady() && this.Target.IsValidTarget(this.E.Range) && this.ConfigValue<bool>("Harass.E"))
                {
                    this.E.CastOnUnit(this.Target);
                }
            }

            if (this.ConfigValue<StringList>("Misc.Laugh").SelectedValue == "ON"
                && this.Player.CountEnemiesInRange(2000) > 0 && this.last + 4200 < Environment.TickCount)
            {
                Game.SendEmote(Emote.Laugh);
                this.last = Environment.TickCount;
            }
        }
    }
}