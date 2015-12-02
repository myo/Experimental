using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace MySharpSupport.AI
{
    public static class Core
    {
        //our main menu
        public static Menu MainMenu;

        //our link to the Orbwalker
        public static Orbwalking.Orbwalker Orbwalker;

        //our preffered carry
        public static Obj_AI_Base Carry
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            h => h.ChampionName == MainMenu.Item("mycarryis").GetValue<StringList>().SelectedValue);
            }
        }

        //the max distance we can get away from the carry
        public static int MaxDistanceFromCarry
        {
            get { return MainMenu.Item("maxdistfromcarry").GetValue<Slider>().Value; }
        }

        //the shopping queue
        public static List<ItemId> ShoppingList = new List<ItemId>();

        public static void OnUpdate(EventArgs args)
        {
            Shop.OnUpdate();
            Walk.OnUpdate();
        }
    }
}
