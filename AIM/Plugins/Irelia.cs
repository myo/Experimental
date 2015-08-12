using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Irelia : PluginBase
    {
        public Irelia()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player));
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                Combo(Target);
            }
        }

        private void Combo(Obj_AI_Hero target)
        {
            if (target == null || !target.IsValid)
            {
                return;
            }

            if (Q.IsReady() && Player.Distance(target) < Q.Range)
            {
                if (W.IsReady())
                {
                    W.Cast();
                }
                Q.Cast(target);
            }

            if (E.IsReady())
            {
                E.Cast(target);
            }
            if (R.IsReady() && Player.Distance(target) < Q.Range)
            {
                R.Cast(Target.Position);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboE", "Use E", true);
            config.AddBool("ComboR", "Use R1", true);
        }
    }
}