using System;
using System.Linq;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Kogmaw : PluginBase
    {

        public Kogmaw()
        {

            Q = new Spell(SpellSlot.Q, 1000f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1360f);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 70f, 1650f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.2f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }
        public override void OnUpdate(EventArgs args)
        {

            if (ComboMode)
            {
                if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
                {
                    Q.CastIfHitchanceEquals(Target, HitChance.Medium, UsePackets);
                }
                if (W.IsReady() && Orbwalking.InAutoAttackRange(Target))
                {
                    W.Cast();
                }
                if (E.IsReady() && Heroes.Player.Distance(Target) < E.Range)
                {
                    E.Cast(Target, UsePackets);
                }
                if (R.IsReady() && GetUltimateBuffStacks() < 3)
                {
                    var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                    if (t != null)
                    R.Cast(t, false, true);
                }

            }


        }

        private int GetUltimateBuffStacks()
        {
            return (from buff in ObjectManager.Player.Buffs
                    where buff.DisplayName.ToLower() == "kogmawlivingartillery"
                    select buff.Count).FirstOrDefault();
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
