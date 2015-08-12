using System;
using System.Linq;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Sivir : PluginBase
    {
        public Sivir()
        {
            Q = new Spell(SpellSlot.Q, 1250);
            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 593);

            R = new Spell(SpellSlot.R);
        }

        public override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || !(target is Obj_AI_Hero))
            {
                return;
            }

            if (W.IsReady())
            {
                W.Cast();
            }

            if (R.IsReady())
            {
                R.Cast();
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (Q.IsReady())
            {
                if (
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(h => h.IsValidTarget(Q.Range))
                        .Any(enemy => Q.CastIfHitchanceEquals(enemy, HitChance.Immobile)))
                {
                    return;
                }
            }

            if (!ComboMode)
            {
                return;
            }

            if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
            {
                Q.Cast(Target);
            }

            if (R.IsReady() && Player.CountEnemiesInRange(600) > 2)
            {
                R.Cast();
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