namespace MySharpSupport.Plugins
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Nami : PluginBase
    {
        public Nami()
        {
            this.Q = new Spell(SpellSlot.Q, 875);
            this.W = new Spell(SpellSlot.W, 725);
            this.E = new Spell(SpellSlot.E, 800);
            this.R = new Spell(SpellSlot.R, 2200);

            this.Q.SetSkillshot(1f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            this.R.SetSkillshot(0.5f, 260f, 850f, false, SkillshotType.SkillshotLine);
            GameObject.OnCreate += this.RangeAttackOnCreate;
        }

        private double WHeal
        {
            get
            {
                int[] heal = { 0, 65, 95, 125, 155, 185 };
                return heal[this.W.Level] + this.Player.FlatMagicDamageMod * 0.3;
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboR", "Use R", true);
            config.AddSlider("ComboCountR", "Targets hit to Ult", 2, 1, 5);
            config.AddSlider("ComboHealthW", "Health to Heal", 20, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "Use Q", true);
            config.AddBool("HarassW", "Use W", true);
            config.AddSlider("HarassHealthW", "Health to Heal", 20, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserQ", "Use Q to Interrupt Gapcloser", true);

            config.AddBool("InterruptQ", "Use Q to Interrupt Spells", true);
            config.AddBool("InterruptR", "Use R to Interrupt Spells", true);
        }

        public override void MiscMenu(Menu config)
        {
            var sub = config.AddSubMenu(new Menu("Use E on Attacks", "Misc.E.AA.Menu"));
            foreach (var hero in HeroManager.Allies.Where(h => !h.IsMe))
            {
                sub.AddBool("Misc.E.AA." + hero.ChampionName, hero.ChampionName, true);
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
                this.Q.Cast(gapcloser.Sender);
            }

            if (this.R.CastCheck(gapcloser.Sender, "GapcloserR"))
            {
                this.R.Cast(gapcloser.Sender);
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
                this.Q.Cast(target);
            }

            if (!this.Q.IsReady() && this.R.CastCheck(target, "InterruptR"))
            {
                this.R.Cast(target);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (this.ComboMode)
                {
                    if (this.Q.CastCheck(this.Target, "ComboQ")) // TODO: add check for slowed targets by E or FrostQeen
                    {
                        this.Q.Cast(this.Target);
                    }

                    if (this.W.IsReady() && this.ConfigValue<bool>("ComboW"))
                    {
                        this.HealLogic();
                    }

                    if (this.R.CastCheck(this.Target, "ComboR"))
                    {
                        this.R.CastIfWillHit(this.Target, this.ConfigValue<Slider>("ComboCountR").Value);
                    }
                }

                if (this.HarassMode)
                {
                    if (this.Q.CastCheck(this.Target, "HarassQ"))
                    {
                        this.Q.Cast(this.Target);
                    }

                    if (this.W.IsReady() && this.ConfigValue<bool>("HarassW"))
                    {
                        this.HealLogic();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void HealLogic()
        {
            var ally = Helpers.AllyBelowHp(this.ConfigValue<Slider>("ComboHealthW").Value, this.W.Range);
            if (ally != null) // force heal low ally
            {
                this.W.CastOnUnit(ally);
                return;
            }

            if (this.Player.Distance(this.Target) > this.W.Range) // target out of range try bounce
            {
                var bounceTarget =
                    HeroManager.Enemies.SingleOrDefault(
                        hero => hero.IsValidAlly(this.W.Range) && hero.Distance(this.Target) < this.W.Range);

                if (bounceTarget != null && bounceTarget.MaxHealth - bounceTarget.Health > this.WHeal)
                    // use bounce & heal
                {
                    this.W.CastOnUnit(bounceTarget);
                }
            }
            else // target in range
            {
                this.W.CastOnUnit(this.Target);
            }
        }

        private void RangeAttackOnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>())
            {
                return;
            }

            var missile = (MissileClient)sender;

            // Caster ally hero / not me
            if (!missile.SpellCaster.IsValid<Obj_AI_Hero>() || !missile.SpellCaster.IsAlly || missile.SpellCaster.IsMe
                || missile.SpellCaster.IsMelee())
            {
                return;
            }

            // Target enemy hero
            if (!missile.Target.IsValid<Obj_AI_Hero>() || !missile.Target.IsEnemy)
            {
                return;
            }

            var caster = (Obj_AI_Hero)missile.SpellCaster;

            if (this.E.IsReady() && this.E.IsInRange(missile.SpellCaster)
                && this.ConfigValue<bool>("Misc.E.AA." + caster.ChampionName))
            {
                this.E.CastOnUnit(caster); // add delay
            }
        }
    }
}