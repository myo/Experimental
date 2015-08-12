using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Aatrox : PluginBase
    {
        public Aatrox()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, Player.AttackRange + 25);
            E = new Spell(SpellSlot.E, 950); //1000?
            R = new Spell(SpellSlot.R, 550); //300?

            Q.SetSkillshot(0.5f, 180f, 1800f, false, SkillshotType.SkillshotCircle); //width tuned
            E.SetSkillshot(0.5f, 150f, 1200f, false, SkillshotType.SkillshotCone);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
                {
                    Q.Cast(Target);
                }
                if (W.IsReady() && W.Instance.ToggleState == 1 && Player.HealthPercent > 30)
                {
                    W.Cast();
                }
                if (W.IsReady() && W.Instance.ToggleState == 2 && Player.HealthPercent < 30)
                {
                    W.Cast();
                }

                if (E.IsReady() && Heroes.Player.Distance(Target) < E.Range)
                {
                    E.Cast(Target, UsePackets);
                }

                if (R.IsReady() && Heroes.Player.Distance(Target) < R.Range)
                {
                    R.Cast(Target);
                }
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (spell.DangerLevel < Interrupter2.DangerLevel.High || unit.IsAlly)
            {
                return;
            }

            if (Q.CastCheck(unit, "Interrupt.Q"))
            {
                Q.Cast(unit);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboE", "Use E", true);
            config.AddBool("ComboR", "Use R", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Interrupt.Q", "Use Q to Interrupt Spells", true);
        }
    }
}