namespace MySharpSupport.Plugins
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Morgana : PluginBase
    {
        public Morgana()
        {
            this.Q = new Spell(SpellSlot.Q, 1175);
            this.W = new Spell(SpellSlot.W, 900);
            this.E = new Spell(SpellSlot.E, 750);
            this.R = new Spell(SpellSlot.R, 550);

            this.Q.SetSkillshot(0.25f, 80f, 1200f, true, SkillshotType.SkillshotLine);
            this.W.SetSkillshot(0.28f, 175f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public override void ComboMenu(Menu config)
        {
            var comboQ = config.AddSubMenu(new Menu("Q Settings", "Q"));
            comboQ.AddBool("ComboQ", "Use Q", true);
            comboQ.AddHitChance("ComboQHC", "Min HitChance", HitChance.Medium);

            var comboW = config.AddSubMenu(new Menu("W Settings", "W"));
            comboW.AddBool("ComboW", "Use W", true);

            var comboE = config.AddSubMenu(new Menu("E Settings", "E"));
            comboE.AddBool("ComboE", "Use E", true);

            var comboR = config.AddSubMenu(new Menu("R Settings", "R"));
            comboR.AddBool("ComboR", "Use R", true);
            comboR.AddSlider("ComboCountR", "Targets in range to Ult", 2, 1, 5);
        }

        public override void HarassMenu(Menu config)
        {
            var harassQ = config.AddSubMenu(new Menu("Q Settings", "Q"));
            harassQ.AddBool("HarassQ", "Use Q", true);
            harassQ.AddHitChance("HarassQHC", "Min HitChance", HitChance.High);

            var harassW = config.AddSubMenu(new Menu("W Settings", "W"));
            harassW.AddBool("HarassW", "Use W", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserQ", "Use Q to Interrupt Gapcloser", true);
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
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (this.ComboMode)
                {
                    if (this.Q.CastCheck(this.Target, "ComboQ") && this.Q.CastWithHitChance(this.Target, "ComboQHC"))
                    {
                        return;
                    }

                    if (this.W.CastCheck(this.Target, "ComboW"))
                    {
                        if (
                            HeroManager.Enemies.Where(
                                hero => (hero.IsValidTarget(this.W.Range) && hero.IsMovementImpaired()))
                                       .Any(enemy => this.W.Cast(enemy.Position)))
                        {
                            return;
                        }

                        if (
                            HeroManager.Enemies.Where(hero => hero.IsValidTarget(this.W.Range))
                                       .Any(enemy => this.W.CastIfWillHit(enemy, 1)))
                        {
                            return;
                        }
                    }

                    if (this.R.CastCheck(this.Target, "ComboR")
                        && Helpers.EnemyInRange(this.ConfigValue<Slider>("ComboCountR").Value, this.R.Range))
                    {
                        this.R.Cast();
                    }
                    return;
                }

                if (this.HarassMode)
                {
                    if (this.Q.CastCheck(this.Target, "HarassQ") && this.Q.CastWithHitChance(this.Target, "HarassQHC"))
                    {
                        return;
                    }

                    if (this.W.CastCheck(this.Target, "HarassW"))
                    {
                        if (
                            HeroManager.Enemies.Where(
                                hero => (hero.IsValidTarget(this.W.Range) && hero.IsMovementImpaired()))
                                       .Any(enemy => this.W.Cast(enemy.Position)))
                        {
                            return;
                        }

                        if (
                            HeroManager.Enemies.Where(hero => hero.IsValidTarget(this.W.Range))
                                       .Any(enemy => this.W.CastIfWillHit(enemy, 1)))
                        {
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