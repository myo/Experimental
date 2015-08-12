using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Diana : PluginBase
    {
        public Diana()
        {
            Q = new Spell(SpellSlot.Q, 830f);
            W = new Spell(SpellSlot.W, 200f);
            E = new Spell(SpellSlot.E, 420f);
            R = new Spell(SpellSlot.R, 825f);

            Q.SetSkillshot(0.35f, 200f, 1800, false, SkillshotType.SkillshotCircle);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Target.IsValidTarget(Q.Range) && Q.IsReady() && Q.GetPrediction(Target).Hitchance >= HitChance.High)
                {
                    Q.CastIfHitchanceEquals(Target, HitChance.High);
                }

                if (Target.IsValidTarget(R.Range) && R.IsReady() && (Target.HasBuff("dianamoonlight")))
                {
                    R.Cast(Target);
                }

                if (Target.IsValidTarget(W.Range) && W.IsReady() && !Q.IsReady())
                {
                    W.Cast();
                }
                if (Target.IsValidTarget(E.Range) && Player.Distance(Target) >= W.Range && E.IsReady() && !W.IsReady())
                {
                    E.Cast();
                }
                if (Target.IsValidTarget(R.Range) && R.IsReady() && !W.IsReady() && !Q.IsReady())
                {
                    R.Cast(Target);
                }
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboE", "Use E", true);
            config.AddBool("ComboR", "Use R", true);
        }
    }
}