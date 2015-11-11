using System;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;

namespace Support.Plugins
{
    public class Soraka : PluginBase
    {
        public Soraka()
        {
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.5f, 300, 1750, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (ComboMode)
                {
                    if (Q.CastCheck(Target, "Combo.Q"))
                    {
                        Q.Cast(Target);
                    }

                    if (E.CastCheck(Target, "Combo.E"))
                    {
                        E.Cast(Target);
                    }
                }

                if (HarassMode)
                {
                    if (Q.CastCheck(Target, "Harass.Q"))
                    {
                        Q.Cast(Target);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "Use Q", true);
            config.AddBool("Combo.E", "Use E", true);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.Q", "Use Q", true);
        }
    }
}