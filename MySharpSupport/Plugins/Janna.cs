namespace MySharpSupport.Plugins
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Janna : PluginBase
    {
        public Janna()
        {
            this.Q = new Spell(SpellSlot.Q, 850);
            this.W = new Spell(SpellSlot.W, 600);
            this.E = new Spell(SpellSlot.E, 800);
            this.R = new Spell(SpellSlot.R, 550);

            this.Q.SetSkillshot(0.25f, 120f, 900f, false, SkillshotType.SkillshotLine);
            GameObject.OnCreate += this.TowerAttackOnCreate;
            GameObject.OnCreate += this.RangeAttackOnCreate;
            Obj_AI_Base.OnProcessSpellCast += this.OnProcessSpellCast;
        }

        private bool IsUltChanneling { get; set; }

        private int LastQInterrupt { get; set; }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.W", "Use W", true);
            config.AddBool("Combo.R", "Use R", true);
            config.AddSlider("Combo.R.Health", "Health to Ult", 15, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.W", "Use W", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.Q", "Use Q to Interrupt Gapcloser", true);
            config.AddBool("Gapcloser.W", "Use W to Interrupt Gapcloser", true);

            config.AddBool("Interrupt.Q", "Use Q to Interrupt Spells", true);
            config.AddBool("Interrupt.R", "Use R to Interrupt Spells", true);
        }

        public override void ManaMenu(Menu config)
        {
            config.AddSlider("Mana.E.Priority.1", "E Priority 1", 65, 0, 100);
            config.AddSlider("Mana.E.Priority.2", "E Priority 2", 35, 0, 100);
            config.AddSlider("Mana.E.Priority.3", "E Priority 3", 10, 0, 100);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("Misc.E.Tower", "Use E on Towers", true);

            // build aa menu
            var aa = config.AddSubMenu(new Menu("Use E on Attacks", "Misc.E.AA.Menu"));
            foreach (var hero in HeroManager.Allies)
            {
                aa.AddBool("Misc.E.AA." + hero.ChampionName, hero.ChampionName, true);
            }

            // build spell menu
            var dmg = config.AddSubMenu(new Menu("Use E on Spell", "Misc.E.Spell.Menu"));
            foreach (var spell in
                HeroManager.Allies.Where(h => !h.IsMe)
                           .SelectMany(hero => DamageBoostDatabase.Spells.Where(s => s.Champion == hero.ChampionName)))
            {
                dmg.AddSlider("Misc.E.Spell." + spell.Spell, spell.Champion + " " + spell.Slot, spell.Priority, 0, 3);
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (this.Q.CastCheck(gapcloser.Sender, "Gapcloser.Q"))
            {
                var pred = this.Q.GetPrediction(gapcloser.Sender);
                if (pred.Hitchance >= HitChance.Medium)
                {
                    this.Q.Cast(pred.CastPosition);
                    this.Q.Cast();
                }
            }

            if (this.W.CastCheck(gapcloser.Sender, "Gapcloser.W"))
            {
                this.W.CastOnUnit(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if ((args.DangerLevel < Interrupter2.DangerLevel.High && target.IsAlly))
            {
                return;
            }

            if (this.Q.CastCheck(target, "Interrupt.Q"))
            {
                var pred = this.Q.GetPrediction(target);
                if (pred.Hitchance >= HitChance.Medium)
                {
                    this.Q.Cast(pred.CastPosition);
                    this.Q.Cast();
                    this.LastQInterrupt = Environment.TickCount;
                    return;
                }
            }

            if (!this.Q.IsReady() && Environment.TickCount - this.LastQInterrupt > 1000
                && this.R.CastCheck(target, "Interrupt.R"))
            {
                this.R.Cast();
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (this.Player.IsChannelingImportantSpell())
                {
                    return;
                }

                if (this.IsUltChanneling)
                {
                    this.Orbwalker.SetAttack(true);
                    this.Orbwalker.SetMovement(true);
                    this.IsUltChanneling = false;
                }

                if (this.ComboMode)
                {
                    if (this.Q.CastCheck(this.Target, "Combo.Q"))
                    {
                        var pred = this.Q.GetPrediction(this.Target);
                        if (pred.Hitchance >= HitChance.High)
                        {
                            this.Q.Cast(pred.CastPosition);
                            this.Q.Cast();
                        }
                    }

                    if (this.W.CastCheck(this.Target, "Combo.W"))
                    {
                        this.W.CastOnUnit(this.Target);
                    }

                    var ally = Helpers.AllyBelowHp(this.ConfigValue<Slider>("Combo.R.Health").Value, this.R.Range);
                    if (this.R.CastCheck(ally, "Combo.R", true, false) && this.Player.CountEnemiesInRange(1000) > 0)
                    {
                        this.R.Cast();
                    }
                }

                if (this.HarassMode)
                {
                    if (this.W.CastCheck(this.Target, "Harass.W"))
                    {
                        this.W.CastOnUnit(this.Target);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "ReapTheWhirlwind")
            {
                this.Orbwalker.SetAttack(false);
                this.Orbwalker.SetMovement(false);
                this.IsUltChanneling = true;
            }

            if (!this.E.IsReady() || !this.E.IsInRange(sender))
            {
                return;
            }

            // Boost Damage
            // Caster ally / target enemy hero
            if (sender.IsValid<Obj_AI_Hero>() && sender.IsAlly && !sender.IsMe)
            {
                var spell = args.SData.Name;
                var caster = (Obj_AI_Hero)sender;

                if (DamageBoostDatabase.Spells.Any(s => s.Spell == spell) && caster.CountEnemiesInRange(2000) > 0)
                {
                    switch (this.ConfigValue<Slider>("Misc.E.Spell." + args.SData.Name).Value) // prio 0 = disabled
                    {
                        case 1:
                            if (this.Player.ManaPercent > this.ConfigValue<Slider>("Mana.E.Priority.1").Value)
                            {
                                this.E.CastOnUnit(caster);
                                Console.WriteLine("DMG Boost " + spell);
                            }
                            break;
                        case 2:
                            if (this.Player.ManaPercent > this.ConfigValue<Slider>("Mana.E.Priority.2").Value)
                            {
                                this.E.CastOnUnit(caster);
                                Console.WriteLine("DMG Boost " + spell);
                            }
                            break;
                        case 3:
                            if (this.Player.ManaPercent > this.ConfigValue<Slider>("Mana.E.Priority.3").Value)
                            {
                                this.E.CastOnUnit(caster);
                                Console.WriteLine("DMG Boost " + spell);
                            }
                            break;
                    }
                }
            }
        }

        private void RangeAttackOnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>() && !this.E.IsReady())
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

            Console.WriteLine("Target:{0} Caster:{1}", missile.Target.Name, missile.SpellCaster.Name);

            // Target enemy hero
            if (!missile.Target.IsValid<Obj_AI_Hero>() || !missile.Target.IsEnemy)
            {
                return;
            }

            var caster = (Obj_AI_Hero)missile.SpellCaster;

            if (this.E.IsInRange(caster) && this.ConfigValue<bool>("Misc.E.AA." + caster.ChampionName))
            {
                this.E.CastOnUnit(caster);
                Console.WriteLine("AA Boost " + missile.SData.Name);
            }
        }

        private void TowerAttackOnCreate(GameObject sender, EventArgs args)
        {
            if (!this.E.IsReady() || !this.ConfigValue<bool>("Misc.E.Tower"))
            {
                return;
            }

            if (sender.IsValid<MissileClient>())
            {
                var missile = (MissileClient)sender;

                // Ally Turret -> Enemy Hero
                if (missile.SpellCaster.IsValid<Obj_AI_Turret>() && missile.SpellCaster.IsAlly
                    && missile.Target.IsValid<Obj_AI_Hero>() && missile.Target.IsEnemy)
                {
                    var turret = (Obj_AI_Turret)missile.SpellCaster;

                    if (this.E.IsInRange(turret))
                    {
                        this.E.CastOnUnit(turret);
                    }
                }
            }
        }
    }
}
