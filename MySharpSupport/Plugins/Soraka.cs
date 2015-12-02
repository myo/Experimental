using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using MySharpSupport.Util;

namespace MySharpSupport.Plugins
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
            if (Q.IsReady())
                QLogic();
            if (W.IsReady())
                WLogic();
            if (E.IsReady())
                ELogic();
            if (R.IsReady())
                RLogic();
        }

        /// <summary>
        /// The Q Logic
        /// </summary>
        public void QLogic()
        {
            if (!Q.IsReady() || (ObjectManager.Player.Mana < 3*GetWManaCost() && CanW()))
            {
                if (ObjectManager.Player.MaxHealth - ObjectManager.Player.Health > GetQHealingAmount())
                {
                    foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(925)))
                    {
                        Q.CastIfHitchanceEquals(hero, HitChance.VeryHigh);
                    }
                }
            }
        }

        /// <summary>
        /// The W Logic
        /// </summary>
        public void WLogic()
        {
            if (!W.IsReady() || !CanW() || ObjectManager.Player.InFountain()) return;
            var bestHealingCandidate =
                HeroManager.Allies.Where(
                    a =>
                        !a.IsMe && a.Distance(ObjectManager.Player) < 550 &&
                        a.MaxHealth - a.Health > GetWHealingAmount())
                    .OrderByDescending(TargetSelector.GetPriority)
                    .ThenBy(ally => ally.Health).FirstOrDefault();
            if (bestHealingCandidate != null)
            {
                if (GetWHealingAmount() > 0.05*bestHealingCandidate.MaxHealth)
                    W.Cast(bestHealingCandidate);
            }
        }

        /// <summary>
        /// The E Logic
        /// </summary>
        public void ELogic()
        {
            if (!E.IsReady()) return;
            var goodTarget =
                HeroManager.Enemies.FirstOrDefault(e => e.IsValidTarget(900) && e.HasBuffOfType(BuffType.Knockup) || e.HasBuffOfType(BuffType.Snare) || e.HasBuffOfType(BuffType.Stun) || e.HasBuffOfType(BuffType.Suppression));
            if (goodTarget != null)
            {
                var pos = goodTarget.ServerPosition;
                if (pos.Distance(ObjectManager.Player.ServerPosition) < 900)
                {
                    E.Cast(goodTarget.ServerPosition);
                }
            }
            foreach (var enemyMinion in ObjectManager.Get<Obj_AI_Base>().Where(m => m.IsEnemy && m.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 900 && m.HasBuff("teleport_target", true) || m.HasBuff("Pantheon_GrandSkyfall_Jump", true)))
            {
                Utility.DelayAction.Add(2000, () =>
                {
                    if (enemyMinion.ServerPosition.Distance(ObjectManager.Player.ServerPosition) < 900)
                    {
                        E.Cast(enemyMinion.ServerPosition);
                    }
                });
            }
        }

        /// <summary>
        /// The R Logic
        /// </summary>
        public void RLogic()
        {
            if (!R.IsReady()) return;
            if (ObjectManager.Player.CountEnemiesInRange(800) >= 1 &&
                ObjectManager.Player.HealthPercent <= 90)
            {
                R.Cast();
            }
            var minAllyHealth = 15;
            if (minAllyHealth < 1) return;
            foreach (var ally in HeroManager.Allies)
            {
                if (ally.CountEnemiesInRange(800) >= 1 && ally.HealthPercent <= minAllyHealth && !ally.IsZombie && !ally.IsDead)
                {
                    R.Cast();
                }
            }
        }

        /// <summary>
        /// The Get Q Healing Amount
        /// </summary>
        /// <returns>The Q Healing Amount</returns>
        public static double GetQHealingAmount()
        {
            return Math.Min(
                new double[] { 25, 35, 45, 55, 65 }[ObjectManager.Player.GetSpell(SpellSlot.W).Level - 1] +
                0.4 * ObjectManager.Player.FlatMagicDamageMod +
                (0.1 * (ObjectManager.Player.MaxHealth - ObjectManager.Player.Health)),
                new double[] { 50, 70, 90, 110, 130 }[ObjectManager.Player.GetSpell(SpellSlot.W).Level - 1] +
                0.8 * ObjectManager.Player.FlatMagicDamageMod);
        }

        /// <summary>
        /// The Get W Healing Amount
        /// </summary>
        /// <returns>The W Healing Amount</returns>
        public static double GetWHealingAmount()
        {
            return new double[] { 120, 150, 180, 210, 240 }[ObjectManager.Player.GetSpell(SpellSlot.W).Level - 1] +
                   0.6 * ObjectManager.Player.FlatMagicDamageMod;
        }

        /// <summary>
        /// The Get R Healing Amount
        /// </summary>
        /// <returns>The R Healing Amount</returns>
        public static double GetRHealingAmount()
        {
            return new double[] { 120, 150, 180, 210, 240 }[ObjectManager.Player.GetSpell(SpellSlot.R).Level - 1] +
                   0.6 * ObjectManager.Player.FlatMagicDamageMod;
        }

        public static int GetWManaCost()
        {
            return new[] { 20, 25, 30, 35, 40 }[ObjectManager.Player.GetSpell(SpellSlot.W).Level - 1];
        }

        public static double GetWHealthCost()
        {
            return 0.10 * ObjectManager.Player.MaxHealth;
        }

        public static bool CanW()
        {
            return ObjectManager.Player.Health - GetWHealthCost() >
            20 / 100f * ObjectManager.Player.MaxHealth;
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