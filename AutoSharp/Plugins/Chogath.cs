using System;
using System.Linq;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
	public class Chogath : PluginBase
	{
		public Chogath()
		{
			Q = new Spell(SpellSlot.Q, 950);
			W = new Spell(SpellSlot.W, 675);
			R = new Spell(SpellSlot.R, 175);

			Q.SetSkillshot(0.75f, 175f, 1000f, false, SkillshotType.SkillshotCircle);
			W.SetSkillshot(0.60f, 300f, 1750f, false, SkillshotType.SkillshotCone);
		}

		public override void OnUpdate(EventArgs args)
		{
            ExecuteAdditionals();
			var target = TargetSelector.GetTarget(675, TargetSelector.DamageType.Physical);
            if (ComboMode)
            {
                if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
                {
                    Q.Cast(target);
                }
                if (W.IsReady() && Heroes.Player.Distance(Target) < W.Range)
                {
                    W.Cast(target);
                }
                if (R.IsReady() && Heroes.Player.Distance(Target) < R.Range && R.IsKillable(target))
                {
                    R.Cast(target);
                }
                if (R.IsReady())
                {
                    var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
                    var count = 0;

                    foreach (var buffs in ObjectManager.Player.Buffs.Where(buffs => buffs.DisplayName == "Feast"))
                    {
                        count = buffs.Count;
                    }
                    foreach (
                        var minion in
                            allMinions.Where(
                                minion =>
                                    minion.IsValidTarget(R.Range) &&
                                    (ObjectManager.Player.GetSpellDamage(minion, SpellSlot.R) > minion.Health))
                                .Where(minion => count < 6))
                    {
                        R.Cast(minion);
                    }
                }
            }
        }

        //From TC-Crew
        private void ExecuteAdditionals()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var count = 0;

            foreach (var buffs in ObjectManager.Player.Buffs.Where(buffs => buffs.DisplayName == "Feast"))
            {
                count = buffs.Count;
            }

            if (R.IsReady())
            {
                foreach (
                    var minion in
                        allMinions.Where(
                            minion =>
                                minion.IsValidTarget(R.Range) &&
                                (ObjectManager.Player.GetSpellDamage(minion, SpellSlot.R) > minion.Health))
                            .Where(minion => count < 6))
                {
                    R.Cast(minion);
                }
            }


            foreach (var champion in from champion in ObjectManager.Get<Obj_AI_Hero>()
                where champion.IsValidTarget(Q.Range)
                let qPrediction = Q.GetPrediction(champion)
                where (qPrediction.Hitchance == HitChance.Immobile)
                select champion)
            {
                Q.Cast(champion, true, true);
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
                return;
            }
            if (W.CastCheck(unit, "Interrupt.W"))
            {
                W.Cast(unit);
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
            config.AddBool("Interrupt.W", "Use W to Interrupt Spells", true);
        }
    }
}