using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Evelynn : PluginBase
    {
        public Evelynn()
        {
            Q = new Spell(SpellSlot.Q, 500f);
            W = new Spell(SpellSlot.W, Q.Range);
            E = new Spell(SpellSlot.E, 225f + 2 * 65f);
            R = new Spell(SpellSlot.R, 650f);

            R.SetSkillshot(0.25f, 350f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Player.CountEnemiesInRange(Q.Range) > 0)
                {
                    Q.Cast();
                }
                if (W.IsReady() && ObjectManager.Player.HasBuffOfType(BuffType.Slow))
                {
                    W.Cast();
                }

                if (E.IsReady() && Heroes.Player.Distance(Target) < E.Range)
                {
                    E.Cast(Target);
                }
                if (R.IsReady())
                {
                    R.CastIfWillHit(Target, 1);
                }
				if ( Q.IsReady())
				{
					Q.Cast();
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