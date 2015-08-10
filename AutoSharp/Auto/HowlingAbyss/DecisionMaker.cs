using System;
using System.Linq;
using AutoSharp.Auto.SummonersRift;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Auto.HowlingAbyss
{
    internal static class DecisionMaker
    {
        private static int _lastUpdate = 0;
        public static void OnUpdate(EventArgs args)
        {
            if (Environment.TickCount - _lastUpdate < 150) return;
            _lastUpdate = Environment.TickCount;

            var player = Heroes.Player;

            if (Decisions.ImSoLonely())
            {
                return;
            }

            if (Program.Config.Item("autosharp.options.healup").GetValue<bool>() && Decisions.HealUp())
            {
                return;
            }

            if (player.UnderTurret(true) && Wizard.GetClosestEnemyTurret().CountNearbyAllyMinions(700) <= 3 && Wizard.GetClosestEnemyTurret().CountAlliesInRange(700) == 0)
            {
                Program.Orbwalker.ActiveMode = MyOrbwalker.OrbwalkingMode.Mixed;
                player.IssueOrder(GameObjectOrder.MoveTo, player.Position.Extend(HeadQuarters.AllyHQ.Position.RandomizePosition(), 800));
                return;
            }

            if (Heroes.Player.InFountain())
            {
                Shopping.Shop();
                Wizard.AntiAfk();
            }

            if (Decisions.Farm())
            {
                return;
            }
            Decisions.Fight();

            if (Program.Orbwalker.GetOrbwalkingPoint().IsZero ||
                Program.Orbwalker.GetOrbwalkingPoint() == Game.CursorPos)
            {
                Decisions.ImSoLonely();
            }
        }
    }
}
