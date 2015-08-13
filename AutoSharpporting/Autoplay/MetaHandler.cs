/*
 * GPL License (c) LeagueSharp 2014-2015
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Support
{
    internal static class MetaHandler
    {

        public static List<Obj_AI_Turret> AllTurrets;
        public static List<Obj_AI_Turret> AllyTurrets;
        public static List<Obj_AI_Turret> EnemyTurrets;
        public static List<Obj_AI_Hero> AllHeroes;
        public static List<Obj_AI_Hero> AllyHeroes;
        public static List<Obj_AI_Hero> EnemyHeroes;

        public static string[] Supports =
        {
            "Alistar", "Annie", "Blitzcrank", "Braum", "Fiddlesticks", "Janna", "Karma",
            "Kayle", "Leona", "Lulu", "Morgana", "Nunu", "Nami", "Soraka", "Sona", "Taric", "Thresh", "Zilean", "Zyra"
        };

        public static void DoChecks()
        {
            var map = Utility.Map.GetMap();

            if (map != null &&
                (map.Type == Utility.Map.MapType.SummonersRift || map.Type == Utility.Map.MapType.TwistedTreeline))
            {
                if (Autoplay.Bot.InFountain() && Autoplay.NearestAllyTurret != null)
                {
                    Autoplay.NearestAllyTurret = null;
                }
            }
        }

        public static bool HasSmite(Obj_AI_Hero hero)
        {
            //return hero.GetSpellSlot("SummonerSmite", true) != SpellSlot.Unknown; //obsolete, use the one below.
            return hero.GetSpellSlot("SummonerSmite") != SpellSlot.Unknown;
        }

        public static void LoadObjects()
        {
            AutoSharpporting.SRShopAI.Main.Init();   //SRShopAI
            //Heroes
            AllHeroes = ObjectManager.Get<Obj_AI_Hero>().ToList();
            AllyHeroes = (Utility.Map.GetMap().Type != Utility.Map.MapType.HowlingAbyss)
                ? AllHeroes.FindAll(hero => hero.IsAlly && !IsSupport(hero) && !hero.IsMe && !HasSmite(hero)).ToList()
                : AllHeroes.FindAll(hero => hero.IsAlly && !hero.IsMe).ToList();
            EnemyHeroes = AllHeroes.FindAll(hero => !hero.IsAlly).ToList();
            UpdateObjects();
        }

        public static void UpdateObjects()
        {

            //Heroes
            AllHeroes = AllHeroes.OrderBy(hero => hero.Distance(Autoplay.Bot)).ToList();
            AllyHeroes = AllyHeroes.OrderBy(hero => hero.Distance(Autoplay.Bot)).ToList();
            EnemyHeroes = EnemyHeroes.OrderBy(hero => hero.Distance(Autoplay.Bot)).ToList();


            //Turrets
            AllTurrets = ObjectManager.Get<Obj_AI_Turret>().ToList();
            AllyTurrets = AllTurrets.FindAll(turret => turret.IsAlly).ToList();
            EnemyTurrets = AllTurrets.FindAll(turret => !turret.IsAlly).ToList();
            AllTurrets = AllTurrets.OrderBy(turret => turret.Distance(Autoplay.Bot)).ToList();
            AllyTurrets = AllyTurrets.OrderBy(turret => turret.Distance(Autoplay.Bot)).ToList();
            EnemyTurrets = EnemyTurrets.OrderBy(turret => turret.Distance(Autoplay.Bot)).ToList();

        }

        public static bool IsSupport(this Obj_AI_Hero hero)
        {
            return Supports.Any(support => hero.CharData.BaseSkinName.ToLower() == support.ToLower());
        }

        public static bool HasSixItems()
        {
            return Autoplay.Bot.InventoryItems.ToList().Count >= 6;
        }

        public static int NearbyAllyMinions(Obj_AI_Base x, int distance)
        {
            return ObjectManager.Get<Obj_AI_Minion>()
                .Count(minion => minion.IsAlly && !minion.IsDead && minion.Distance(x) < distance);
        }

        public static int NearbyAllies(Obj_AI_Base x, int distance)
        {
            return AllyHeroes.Count(
                    hero => hero.IsAlly && !hero.IsDead && !HasSmite(hero) && !hero.IsMe && hero.Distance(x) < distance);
        }

        public static int NearbyAllies(Vector3 x, int distance)
        {
            return AllyHeroes
                .Count(
                    hero => hero.IsAlly && !hero.IsDead && !HasSmite(hero) && !hero.IsMe && hero.Distance(x) < distance);
        }

        public static bool ShouldSupportTopLane
        {
            get
            {
                return NearbyAllies(Autoplay.BotLanePos, 4500) > 1 &&
                       NearbyAllies(Autoplay.TopLanePos, 4500) == 1;
            }
        }
    }
}