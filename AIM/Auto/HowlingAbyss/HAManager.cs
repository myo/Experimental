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
    }
}
