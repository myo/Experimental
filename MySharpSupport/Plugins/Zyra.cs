namespace MySharpSupport.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Zyra : PluginBase
    {
        public Zyra()
        {
            this.Q = new Spell(SpellSlot.Q, 800);
            this.W = new Spell(SpellSlot.W, 825);
            this.E = new Spell(SpellSlot.E, 1100);
            this.R = new Spell(SpellSlot.R, 700);
            this.Passive = new Spell(SpellSlot.Q, 1470);

            this.Q.SetSkillshot(0.8f, 60f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            this.E.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);
            this.R.SetSkillshot(0.5f, 500f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            this.Passive.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);
        }

        private Spell Passive { get; set; }

        private int WCount
        {
            get
            {
                return this.W.Instance.Level > 0 ? this.W.Instance.Ammo : 0;
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.E", "Use E", true);
            config.AddBool("Combo.R", "Use R", true);
            config.AddSlider("Combo.R.Count", "Targets hit to Ult", 3, 0, 5);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "Use Q", true);
            config.AddBool("Harass.E", "Use E", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.E", "Use E to Interrupt Gapcloser", true);
            config.AddBool("Interrupt.R", "Use R to Interrupt Spells", true);
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (this.E.CastCheck(gapcloser.Sender, "Gapcloser.E"))
            {
                if (this.E.Cast(this.Target) == Spell.CastStates.SuccessfullyCasted)
                {
                    this.CastW(this.E.GetPrediction(this.Target).CastPosition);
                }
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (this.R.CastCheck(target, "Interrupt.R"))
            {
                this.R.Cast(target);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (this.ZyraisZombie())
                {
                    this.CastPassive();
                    return;
                }

                if (this.ComboMode)
                {
                    if (this.Q.CastCheck(this.Target, "Combo.Q"))
                    {
                        if (this.Q.Cast(this.Target) == Spell.CastStates.SuccessfullyCasted)
                        {
                            this.CastW(this.Q.GetPrediction(this.Target).CastPosition);
                        }
                    }

                    if (this.E.CastCheck(this.Target, "Combo.E"))
                    {
                        if (this.E.Cast(this.Target) == Spell.CastStates.SuccessfullyCasted)
                        {
                            this.CastW(this.E.GetPrediction(this.Target).CastPosition);
                        }
                    }

                    if (this.R.CastCheck(this.Target, "Combo.R"))
                    {
                        this.R.CastIfWillHit(this.Target, this.ConfigValue<Slider>("Combo.R.Count").Value);
                    }
                }

                if (this.HarassMode)
                {
                    if (this.Q.CastCheck(this.Target, "Harass.Q"))
                    {
                        if (this.Q.Cast(this.Target) == Spell.CastStates.SuccessfullyCasted)
                        {
                            this.CastW(this.Q.GetPrediction(this.Target).CastPosition);
                        }
                    }

                    if (this.E.CastCheck(this.Target, "Harass.E"))
                    {
                        if (this.E.Cast(this.Target) == Spell.CastStates.SuccessfullyCasted)
                        {
                            this.CastW(this.E.GetPrediction(this.Target).CastPosition);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void CastPassive()
        {
            if (!this.Passive.IsReady())
            {
                return;
            }
            if (!this.Target.IsValidTarget(this.E.Range))
            {
                return;
            }
            this.Passive.CastIfHitchanceEquals(this.Target, HitChance.High);
        }

        private void CastW(Vector3 v)
        {
            if (!this.W.IsReady())
            {
                return;
            }

            if (this.WCount == 1)
            {
                Utility.DelayAction.Add(50, () => this.W.Cast(new Vector2(v.X - 5, v.Y - 5)));
            }

            if (this.WCount == 2)
            {
                Utility.DelayAction.Add(50, () => this.W.Cast(new Vector2(v.X - 5, v.Y - 5)));
                Utility.DelayAction.Add(180, () => this.W.Cast(new Vector2(v.X - 5, v.Y - 5)));
            }
        }

        private bool ZyraisZombie()
        {
            return this.Player.Spellbook.GetSpell(SpellSlot.Q).Name == this.Player.Spellbook.GetSpell(SpellSlot.E).Name
                   || this.Player.Spellbook.GetSpell(SpellSlot.W).Name
                   == this.Player.Spellbook.GetSpell(SpellSlot.R).Name;
        }
    }
}