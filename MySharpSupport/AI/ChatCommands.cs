using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace MySharpSupport.AI
{
    public static class ChatCommands
    {
        public static void OnChat(GameChatEventArgs args)
        {
            if (args.Sender.ChampionName == Core.MainMenu.Item("mycarryis").GetValue<StringList>().SelectedValue)
            {
                if (args.Message.Contains("buy") || args.Message.Contains("get") || args.Message.Contains("rush") ||
                    args.Message.Contains("purchase"))
                {
                    Shop.RushItem = Shop.GetItemByName(args.Message);
                    return;
                }

                if (Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift && args.Message.ToLower() == "b" ||
                    args.Message.Contains("back") || args.Message.Contains("recall"))
                {
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall);
                    return;
                }
            }
        }
    }
}
