using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Auto.SummonersRift
{
    public static class RoleSwitcher
    {
        private static int _lastSwitchedRoleTick = 0;
        private static bool _paused = false;

        public static void Load()
        {
            Utility.DelayAction.Add(new Random().Next(5000, 17000), () => Game.OnUpdate += ChooseBest);
        }

        public static void Unload()
        {
            Game.OnUpdate -= ChooseBest;
        }

        public static void ChooseBest(EventArgs args)
        {
            if (_paused) return;

            if (Environment.TickCount - _lastSwitchedRoleTick > 180000)
            {
                MyTeam.MyRole = MyTeam.Roles.Unknown;
                _lastSwitchedRoleTick = Environment.TickCount;
            }

            if (MyTeam.MyRole == MyTeam.Roles.Unknown)
            {
                if (MyTeam.Midlaner == null) MyTeam.MyRole = MyTeam.Roles.Midlaner;
                if (MyTeam.Support == null) MyTeam.MyRole = MyTeam.Roles.Support;
                if (MyTeam.ADC == null) MyTeam.MyRole = MyTeam.Roles.ADC;
                if (MyTeam.Toplaner == null) MyTeam.MyRole = MyTeam.Roles.Toplaner;
                if (MyTeam.Jungler == null) MyTeam.MyRole = MyTeam.Roles.Toplaner;
            }

            if (MyTeam.MyRole == MyTeam.Roles.Support || MyTeam.MyRole == MyTeam.Roles.ADC)
            {
                BotLaneLogic();
            }

            if (MyTeam.MyRole == MyTeam.Roles.Midlaner)
            {
                MidLaneLogic();
            }

            if (MyTeam.MyRole == MyTeam.Roles.Toplaner)
            {
                TopLaneLogic();
            }
        }

        public static void BotLaneLogic()
        {
            var redZone = Map.BottomLane.Red_Zone;
            var blueZone = Map.BottomLane.Blue_Zone;
            var myOuterTurret = Heroes.Player.Team == GameObjectTeam.Order
                ? Turrets.EnemyTurrets.FirstOrDefault(t => t.Distance(Map.BottomLane.Blue_Outer_Turret) < 950)
                : Turrets.EnemyTurrets.FirstOrDefault(t => t.Distance(Map.BottomLane.Red_Outer_Turret) < 950);

            var minionsOnMyLane =
                Minions.AllyMinions.FindAll(
                    m =>
                        redZone.Points.Any(p => p.Distance(m.Position) < 300) ||
                        blueZone.Points.Any(p => p.Distance(m.Position) < 300)).OrderByDescending(minion => minion.Distance(HeadQuarters.AllyHQ));

            var farthestMinionInLane = minionsOnMyLane.FirstOrDefault();
            var farthestMinionInGame =
                Minions.AllyMinions.OrderByDescending(m => m.Distance(HeadQuarters.AllyHQ)).FirstOrDefault();
            var farthestAlly =
                Heroes.AllyHeroes.OrderByDescending(h => h.Distance(HeadQuarters.AllyHQ)).FirstOrDefault();

            if (farthestMinionInLane != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(farthestMinionInLane.Position.RandomizePosition());
            }
            else if (farthestMinionInGame != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(farthestMinionInGame.Position.RandomizePosition());
            }
            else if (farthestAlly != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(farthestAlly.Position.RandomizePosition());
            }
            else if (myOuterTurret != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(myOuterTurret.Position.RandomizePosition());
            }
        }

        public static void MidLaneLogic()
        {

            var redZone = Map.MidLane.Red_Zone;
            var blueZone = Map.MidLane.Blue_Zone;
            var myOuterTurret = Heroes.Player.Team == GameObjectTeam.Order
                ? Turrets.EnemyTurrets.FirstOrDefault(t => t.Distance(Map.MidLane.Blue_Outer_Turret) < 950)
                : Turrets.EnemyTurrets.FirstOrDefault(t => t.Distance(Map.MidLane.Red_Outer_Turret) < 950);

            var minionsOnMyLane =
                Minions.AllyMinions.FindAll(
                    m =>
                        redZone.Points.Any(p => p.Distance(m.Position) < 300) ||
                        blueZone.Points.Any(p => p.Distance(m.Position) < 300)).OrderByDescending(minion => minion.Distance(HeadQuarters.AllyHQ));

            var farthestMinionInLane = minionsOnMyLane.FirstOrDefault();
            var farthestMinionInGame =
                Minions.AllyMinions.OrderByDescending(m => m.Distance(HeadQuarters.AllyHQ)).FirstOrDefault();
            var farthestAlly =
                Heroes.AllyHeroes.OrderByDescending(h => h.Distance(HeadQuarters.AllyHQ)).FirstOrDefault();

            if (farthestMinionInLane != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(farthestMinionInLane.Position.RandomizePosition());
            }
            else if (farthestMinionInGame != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(farthestMinionInGame.Position.RandomizePosition());
            }
            else if (farthestAlly != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(farthestAlly.Position.RandomizePosition());
            }
            else if (myOuterTurret != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(myOuterTurret.Position.RandomizePosition());
            }
        }

        public static void TopLaneLogic()
        {

            var redZone = Map.TopLane.Red_Zone;
            var blueZone = Map.TopLane.Blue_Zone;
            var myOuterTurret = Heroes.Player.Team == GameObjectTeam.Order
                ? Turrets.EnemyTurrets.FirstOrDefault(t => t.Distance(Map.TopLane.Blue_Outer_Turret) < 950)
                : Turrets.EnemyTurrets.FirstOrDefault(t => t.Distance(Map.TopLane.Red_Outer_Turret) < 950);

            var minionsOnMyLane =
                Minions.AllyMinions.FindAll(
                    m =>
                        redZone.Points.Any(p => p.Distance(m.Position) < 300) ||
                        blueZone.Points.Any(p => p.Distance(m.Position) < 300)).OrderByDescending(minion => minion.Distance(HeadQuarters.AllyHQ));

            var farthestMinionInLane = minionsOnMyLane.FirstOrDefault();
            var farthestMinionInGame =
                Minions.AllyMinions.OrderByDescending(m => m.Distance(HeadQuarters.AllyHQ)).FirstOrDefault();
            var farthestAlly =
                Heroes.AllyHeroes.OrderByDescending(h => h.Distance(HeadQuarters.AllyHQ)).FirstOrDefault();

            if (farthestMinionInLane != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(farthestMinionInLane.Position.RandomizePosition());
            }
            else if (farthestMinionInGame != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(farthestMinionInGame.Position.RandomizePosition());
            }
            else if (farthestAlly != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(farthestAlly.Position.RandomizePosition());
            }
            else if (myOuterTurret != null)
            {
                Program.Orbwalker.SetOrbwalkingPoint(myOuterTurret.Position.RandomizePosition());
            }
        }

        public static void Pause()
        {
            _paused = true;
        }

        public static void Unpause()
        {
            _paused = false;
        }
    }
}
