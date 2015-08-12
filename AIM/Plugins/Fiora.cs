using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Fiora : PluginBase
    {
        public Fiora()
        {
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 350f);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
                {
                    Q.Cast(Target);
                    W.Cast();
                    E.Cast();
                }
                if (R.IsReady() && R.GetDamage(Target) > Target.Health)
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