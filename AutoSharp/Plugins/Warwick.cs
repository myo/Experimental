using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
	public class Warwick : PluginBase
	{
		public Warwick()
		{
			Q = new Spell(SpellSlot.Q, 400);
			W = new Spell(SpellSlot.W, 1000);
			E = new Spell(SpellSlot.E, 1500);
			R = new Spell(SpellSlot.R, 700);
		}

		public override void OnUpdate(EventArgs args)
		{
		//	if (ComboMode)
		//	{
					 if (Target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

                if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
                {
                    Q.Cast(Target);
                }
                if (R.IsReady() && Heroes.Player.Distance(Target) < R.Range && R.IsKillable(Target))
                {
                    R.Cast(Target);
                }
                if (Player.HealthPercent > 20 && Player.Distance(Target) < 1000)
                {
                    if (W.IsReady())
                    {
                        W.Cast();
                    }
                }
				var allminions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
				foreach(var minion in allminions)
                {
                    if (minion.Health < Player.GetSpellDamage(minion, SpellSlot.Q)) Q.Cast(minion);
                    return;
                }
          //  }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (spell.DangerLevel < Interrupter2.DangerLevel.High || unit.IsAlly)
            {
                return;
            }
            if (R.CastCheck(unit, "Interrupt.R"))
            {
                R.Cast(unit);
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
            config.AddBool("Interrupt.R", "Use R to Interrupt Spells", true);
        }
    }
}