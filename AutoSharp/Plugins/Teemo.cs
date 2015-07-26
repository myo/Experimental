using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace AutoSharp.Plugins
{
	public class Teemo : PluginBase
	{
		private readonly Random _rand = new Random((42 / 13 * DateTime.Now.Millisecond) + DateTime.Now.Second);
		private Vector2 _pos;

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
		var targetteemo = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);
		if (targetteemo == null) return;
            if (ComboMode)
            {
                if (Q.CastCheck(targetteemo, "ComboQ"))
                {
                    Q.Cast(targetteemo);
                }

                if (R.CastCheck(targetteemo, "ComboR"))
                {
                    R.Cast(targetteemo.ServerPosition);
                }

                if (R.IsReady())
                {
                    var randRange = _rand.Next(-100, 100);

                    _pos.X = Player.Position.X + randRange;
                    _pos.Y = Player.Position.Y + randRange;
                    R.Cast(_pos.To3D());
                }

                if (Orbwalking.InAutoAttackRange(Target) && Player.HealthPercent > 30)
                {
                    if (W.IsReady())
                    {
                        W.Cast();
                    }
                    Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                }
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