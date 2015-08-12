using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Auto.SummonersRift
{
    public static class SRManager
    {
        public static void Load()
        {
            RoleSwitcher.Load();
            SRShopAI.Main.Init();
            RoleSwitcher.Unpause();
        }

        public static void Unload()
        {
            //RoleSwitcher.Unload(); #TODO OR NOT TODO: SHIT WILL GO CRAZY YO
            RoleSwitcher.Pause();
        }

        public static void FastHalt()
        {
            Program.Orbwalker.ActiveMode = MyOrbwalker.OrbwalkingMode.None;
            RoleSwitcher.Pause();
        }

        public static void OnUpdate(EventArgs args)
        {
            if (Heroes.Player.InFountain() && !Heroes.Player.IsDead)
            {
                Shopping.Shop();
                Wizard.AntiAfk();
                Program.Orbwalker.ActiveMode = MyOrbwalker.OrbwalkingMode.Mixed;
            }
        }
    }
}
