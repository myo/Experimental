using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Jax : PluginBase
    {
        public Jax()
        {
            //spelldata from Mechanics-StackOverflow Galio
            Q = new Spell(SpellSlot.Q, 680f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 190f);
            R = new Spell(SpellSlot.R);

            Q.SetTargetted(0.50f, 75f);
        }

        public override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            var t = target as Obj_AI_Hero;
            if (unit.IsMe && t != null)
            {
                if (W.IsReady())
                {
                    W.Cast();
                }
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
                {
                    if (E.IsReady())
                    {
                        E.Cast();
                    }
                    if (R.IsReady())
                    {
                        R.Cast();
                    }
                    Q.Cast(Target);
                }
                if (E.IsReady() && Target.IsValidTarget(E.Range))
                {
                    E.Cast();
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