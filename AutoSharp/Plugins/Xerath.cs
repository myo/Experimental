using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Utils = LeagueSharp.Common.Utils;

namespace AutoSharp.Plugins
{
    class Xerath : PluginBase
    {
        public const string ChampionName = "Xerath";

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static Obj_AI_Hero Player;

        private static bool AttacksEnabled
        {
            get
            {
                if (IsCastingR)
                    return false;

                if (Q.IsCharging)
                    return false;

                return IsPassiveUp || (!Q.IsReady() && !W.IsReady() && !E.IsReady());
            }
        }

        public static bool IsPassiveUp
        {
            get { return ObjectManager.Player.HasBuff("xerathascended2onhit", true); }
        }

        public static bool IsCastingR
        {
            get
            {
                return ObjectManager.Player.HasBuff("XerathLocusOfPower2", true) ||
                       (ObjectManager.Player.LastCastedSpellName() == "XerathLocusOfPower2" &&
                        LeagueSharp.Common.Utils.TickCount - ObjectManager.Player.LastCastedSpellT() < 500);
            }
        }

        public static class RCharge
        {
            public static int CastT;
            public static int Index;
            public static Vector3 Position;
            public static bool TapKeyPressed;
        }

        public override void OnLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.ChampionName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 1550);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 1150);
            R = new Spell(SpellSlot.R, 675);

            Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 60f, 1400f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.7f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Add the events we are going to use:
            Game.OnUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Game.OnWndProc += Game_OnWndProc;
            Game.PrintChat(ChampionName + " Loaded!");
            Orbwalking.BeforeAttack += OrbwalkingOnBeforeAttack;
            Obj_AI_Hero.OnIssueOrder += Obj_AI_Hero_OnIssueOrder;
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {                  
            if (Player.Distance(sender) < E.Range)
            {
                E.Cast(sender);
            }
        }

        static void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (IsCastingR)
            {
                args.Process = false;
            }
        }

        private static void OrbwalkingOnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = AttacksEnabled;
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {

            if (Player.Distance(gapcloser.Sender) < E.Range)
            {
                E.Cast(gapcloser.Sender);
            }
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)WindowsMessages.WM_KEYUP)
                RCharge.TapKeyPressed = true;
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "XerathLocusOfPower2")
                {
                    RCharge.CastT = 0;
                    RCharge.Index = 0;
                    RCharge.Position = new Vector3();
                    RCharge.TapKeyPressed = false;
                }
                else if (args.SData.Name == "xerathlocuspulse")
                {
                    RCharge.CastT = LeagueSharp.Common.Utils.TickCount;
                    RCharge.Index++;
                    RCharge.Position = args.End;
                    RCharge.TapKeyPressed = false;
                }
            }
        }

        private static void Combo()
        {
            if (R.IsReady() && Heroes.EnemyHeroes.Any(h => h.HealthPercent < 30)) R.Cast();
            UseSpells(true, true, true);
        }

        private static void UseSpells(bool useQ, bool useW, bool useE)
        {
            var qTarget = TargetSelector.GetTarget(Q.ChargedMaxRange, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range + W.Width * 0.5f, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            Hacks.DisableCastIndicator = Q.IsCharging && useQ;

            if (eTarget != null && useE && E.IsReady())
            {
                if (Player.Distance(eTarget) < E.Range * 0.4f)
                    E.Cast(eTarget);
                else if ((!useW || !W.IsReady()))
                    E.Cast(eTarget);
            }

            if (useQ && Q.IsReady() && qTarget != null)
            {
                if (Q.IsCharging)
                {
                    Q.Cast(qTarget, false, false);
                }
                else if (!useW || !W.IsReady() || Player.Distance(qTarget) > W.Range)
                {
                    Q.StartCharging();
                }
            }

            if (wTarget != null && useW && W.IsReady())
                W.Cast(wTarget, false, true);
        }

        private static Obj_AI_Hero GetTargetNearMouse(float distance)
        {
            Obj_AI_Hero bestTarget = null;
            var bestRatio = 0f;

            if (TargetSelector.SelectedTarget.IsValidTarget() && !TargetSelector.IsInvulnerable(TargetSelector.SelectedTarget, TargetSelector.DamageType.Magical, true) &&
                (Game.CursorPos.Distance(TargetSelector.SelectedTarget.ServerPosition) < distance && ObjectManager.Player.Distance(TargetSelector.SelectedTarget) < R.Range))
            {
                return TargetSelector.SelectedTarget;
            }

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsValidTarget(R.Range) || TargetSelector.IsInvulnerable(hero, TargetSelector.DamageType.Magical, true) || Game.CursorPos.Distance(hero.ServerPosition) > distance)
                {
                    continue;
                }

                var damage = (float)ObjectManager.Player.CalcDamage(hero, Damage.DamageType.Magical, 100);
                var ratio = damage / (1 + hero.Health) * TargetSelector.GetPriority(hero);

                if (ratio > bestRatio)
                {
                    bestRatio = ratio;
                    bestTarget = hero;
                }
            }

            return bestTarget;
        }

        private static void WhileCastingR()
        {
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            if (rTarget != null)
            {
                //Wait at least 0.6f if the target is going to die or if the target is to far away
                if (rTarget.Health - R.GetDamage(rTarget) < 0)
                    if (LeagueSharp.Common.Utils.TickCount - RCharge.CastT <= 700) return;

                if ((RCharge.Index != 0 && rTarget.Distance(RCharge.Position) > 1000))
                    if (LeagueSharp.Common.Utils.TickCount - RCharge.CastT <=
                        Math.Min(2500, rTarget.Distance(RCharge.Position) - 1000)) return;

                R.Cast(rTarget, true);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            Program.Orbwalker.SetMovement(true);

            R.Range = 1850 + R.Level*1050;

            if (IsCastingR)
            {
                Program.Orbwalker.SetMovement(false);
                WhileCastingR();
                return;
            }
            Combo();
        }
    }
}
