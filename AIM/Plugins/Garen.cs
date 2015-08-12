using System;
using System.Linq;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
	public class Garen : PluginBase
	{
		public Garen()
		{
			Q = new Spell(SpellSlot.Q);
			W = new Spell(SpellSlot.W);
			E = new Spell(SpellSlot.E, 165);
			R = new Spell(SpellSlot.R, 400);
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
				if (Q.IsReady())
				{
					Q.Cast();
					Orbwalking.ResetAutoAttackTimer();
				}
			}
		}

		public override void OnUpdate(EventArgs args)
		{
			KS();
			if (ComboMode)
			{
				if (Q.IsReady() && Player.CountEnemiesInRange(R.Range) > 0)
				{
					Q.Cast();
										Orbwalking.ResetAutoAttackTimer();

					
                }
				if (W.IsReady() && Player.CountEnemiesInRange(R.Range) > 0)
				{
					W.Cast();
				}

				if (E.IsReady() && Player.CountEnemiesInRange(R.Range) > 0)
				{
					E.Cast();
					
                }
            }
        }

	    // ReSharper disable once InconsistentNaming
        public void KS()
        {
            foreach (
                var target in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => Player.Distance(x) < 900 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (target != null)
                {
                    //R
                    if (Player.Distance(target.ServerPosition) <= R.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.R)) > target.Health + 50)
                    {
                        if (R.CastCheck(Target, "ComboRKS"))
                        {
                            R.Cast(target);
                            return;
                        }
                    }
                }
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboE", "Use E", true);
            config.AddBool("ComboRKS", "Use R KS", true);
        }
    }
}