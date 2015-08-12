using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace AutoSharp.Plugins
{
	public class Shaco : PluginBase
	{
		public Vector2 Pos;
		public bool Rcast;

		public Shaco()
		{
			Q = new Spell(SpellSlot.Q, 400);
			W = new Spell(SpellSlot.W, 425);
			E = new Spell(SpellSlot.E, 625);
			R = new Spell(SpellSlot.R, 0);
		}

		public override void OnUpdate(EventArgs args)
		{
			var target1 = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
			if (target1==null) return;
			var rnd = new Random();
			var move = rnd.Next(100, 200);

			if (Q.IsReady() && Player.HealthPercent < 50 && Player.CountEnemiesInRange(1300) >= 1)
			{
				if (Player.Team == GameObjectTeam.Order)
				{
					Pos.X = Player.Position.X -move;
					Pos.Y = Player.Position.Y -move;
				}
				else
				{
					Pos.X = Player.Position.X +move;
					Pos.Y = Player.Position.Y +move;
				}

				Q.Cast(Pos.To3D());
			}

			if (R.IsReady() && Player.CountEnemiesInRange(1300) >= 1 && Player.HealthPercent < 80)
			{
				R.Cast();
			}

			if (W.IsReady() && Player.CountEnemiesInRange(1300) >= 2)
			{
				if (Player.Team == GameObjectTeam.Order)
				{
					Pos.X = Player.Position.X +move;
					Pos.Y = Player.Position.Y +move;
				}
				else
				{
                    Pos.X = Player.Position.X - move;
                    Pos.Y = Player.Position.Y - move;
				}				
				W.Cast(Pos.To3D());
                }
				if (E.IsReady())
				{

					E.Cast(target1);
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