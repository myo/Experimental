using System;
using System.Linq;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
	public class Teemo : PluginBase
	{

		public Teemo()
		{
			Q = new Spell(SpellSlot.Q, 680);
			W = new Spell(SpellSlot.W);
			R = new Spell(SpellSlot.R, 230);
			Q.SetTargetted(0f, 2000f);
			R.SetSkillshot(0.1f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);
		}

	    public override void OnUpdate(EventArgs args)
	    {
	        var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);
	        if (target != null)
	        {
	            if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
	            {
	                Q.Cast(target);
	            }
	        }
	        if (Player.Position.CountEnemiesInRange(325) != 0)
	        {
	            W.Cast();
	        }
	        if (R.IsReady() &&
	            (NavMesh.IsWallOfGrass(Player.ServerPosition, 100) ||
	             HealingBuffs.AllyBuffs.Any(h => h.Position.Distance(Player.ServerPosition) < 100) ||
	             HealingBuffs.EnemyBuffs.Any(h => h.Position.Distance(Player.ServerPosition) < 100)))
	        {
	            R.Cast(Player.Position);
	        }
	    }

	    public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboR", "Use R", true);
        }
    }
}