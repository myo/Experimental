using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace MySharpSupport.AI
{
    public static class Init
    {
        /// <summary>
        /// Initializes the AI with a specified delay
        /// </summary>
        public static void DoInitWithDelay(int min = 1000, int max=15000)
        {
            Utility.DelayAction.Add(new Random().Next(min, max), DoFullInitialization);
        }

        /// <summary>
        /// 0-delay initialization
        /// </summary>
        public static void DoFullInitialization()
        {
            Core.MainMenu = new Menu("MySharpSupport", "mysharpsupport", true);
            Core.MainMenu.AddItem(new MenuItem("mycarryis", "ADC to Follow").SetValue(new StringList(ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly).OrderByDescending(a => a.Spellbook.Spells.Any(s => s.Name == "SummonerHeal")).Select(hero => hero.ChampionName).ToArray())));
            Core.MainMenu.AddItem(
                new MenuItem("maxdistfromcarry", "Max Distance from Carry: ").SetValue(
                    new Slider(
                        Math.Min(
                            Math.Min((int) ObjectManager.Player.GetSpell(SpellSlot.Q).SData.CastRange,
                                (int) ObjectManager.Player.GetSpell(SpellSlot.W).SData.CastRange),
                            (int) ObjectManager.Player.GetSpell(SpellSlot.E).SData.CastRange), 100, 800)));
            Core.MainMenu.AddToMainMenu();

            Game.OnUpdate += Core.OnUpdate;
        }
    }
}
