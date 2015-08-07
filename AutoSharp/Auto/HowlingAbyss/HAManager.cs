using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;
// ReSharper disable InconsistentNaming

namespace AutoSharp.Auto.HowlingAbyss
{
    public static class HAManager
    {
        public static void Load()
        {
            Game.OnUpdate += DecisionMaker.OnUpdate;
            Obj_AI_Base.OnIssueOrder += OnIssueOrder;
            ARAMShopAI.Main.Init(); 
        }

        public static void Unload()
        {
            Game.OnUpdate -= DecisionMaker.OnUpdate;
        }

        public static void FastHalt()
        {
            Program.Orbwalker.ActiveMode = MyOrbwalker.OrbwalkingMode.None;
        }

        public static void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender == Heroes.Player && args.Order == GameObjectOrder.MoveTo)
            {
                var nearbyEnemyTurret = args.TargetPosition.GetClosestEnemyTurret();
                if (nearbyEnemyTurret != null && nearbyEnemyTurret.Position.CountNearbyAllyMinions(700) <= 2 && nearbyEnemyTurret.Distance(args.TargetPosition) < 800)
                {
                    args.Process = false;
                }
            }
        }
    }
}
