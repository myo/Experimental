using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Auto.HowlingAbyss.ARAMShopAI
{
    class Main
    {
        private static Item _lastItem;
        private static int _priceAddup;
        private static readonly List<Item> ItemList = new List<Item>();

        public static void ItemSequence(Item item, Queue<Item> shopListQueue)
        {
            if (item.From == null)
                shopListQueue.Enqueue(item);
            else
            {
                foreach (int itemDescendant in item.From)
                    ItemSequence(GetItemById(itemDescendant), shopListQueue);
                shopListQueue.Enqueue(item);
            }
        }
        public static Item GetItemById(int id)
        {
            return ItemList.Single(x => x.Id.Equals(id));
        }
        public static Item GetItemByName(string name)
        {
            return ItemList.FirstOrDefault(x => x.Name.Equals(name));
        }
        public static string Request(string url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            Debug.Assert(dataStream != null, "dataStream != null");
            StreamReader reader = new StreamReader(stream: dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return responseFromServer;
        }
        public static string[] List = new[] {"","","","","",""};
        public static string[] MageNames =
        {
            "Ahri", "Anivia", "Annie", "Azir", "Brand", "FiddleSticks", "Heimerdinger",
            "Karma", "Karthus", "Kassadin", "KogMaw", "Leblanc", "Lissandra", "Lulu", "Lux", "Malzahar", "Morgana",
            "Nidalee", "Orianna", "Swain", "Syndra", "Veigar", "Velkoz", "Viktor", "Xerath", "Ziggs", "Zilean", "Zyra", "TwistedFate", "Ekko"
        };
        public static string[] Mage =
        {
            "Athene's Unholy Grail", "Morellonomicon",
            "Sorcerer's Shoes", "Void Staff", "Rabadon's Deathcap", "Zhonya's Hourglass"
        };

        public static string[] MarksmenNames =
        {
            "Ashe", "Caitlyn", "Draven", "Graves", "Jinx", "Kalista", "Lucian",
            "MissFortune", "Quinn", "Sivir", "Tristana", "Twitch", "Varus", "Vayne", "Gangplank", "Yasuo"
        };
        public static string[] Marksmen =
        {
            "Statikk Shiv","The Bloodthirster",
            "Berserker's Greaves", "Infinity Edge", "Last Whisper", "Banshee's Veil"
        };

        public static string[] UtilitySupportsNames = { "Bard", "Janna", "Nami", "Nunu", "Sona", "Soraka", "TahmKench" };
        public static string[] UtilitySupport =
        {
            "Frost Queen's Claim", "Mikael's Crucible",
            "Randuin's Omen", "Ionian Boots of Lucidity", "Locket of the Iron Solari",
            "Ardent Censer"
        };

        public static string[] TankSupportsNames = { "Braum", "Blitzcrank", "Alistar", "Leona", "Taric", "Thresh" };
        public static string[] TankSupport =
        {
            "Face of the Mountain", "Frozen Heart",
            "Mercury's Treads", "Thornmail", "Locket of the Iron Solari", "Banshee's Veil"
        };

        public static string[] AssassinNames =
        {
            "Aatrox", "Khazix", "MasterYi", "Nocturne", "Rengar", "Shaco", "Talon",
            "Zed", "JarvanIV", "Fiora", "Jayce", "Pantheon", "LeeSin", "Riven", "Tryndamere", "MonkeyKing", "XinZhao"
        };
        public static string[] Assassin =
        {
            "Youmuu's Ghostblade", "Blade of the Ruined King",
            "Ionian Boots of Lucidity", "Last Whisper", "Ravenous Hydra (Melee Only)",
            "The Bloodthirster"
        };

        public static string[] TankNames =
        {
            "Amumu", "Chogath", "Evelynn", "Malphite", "Maokai", "Nautilus", "Rammus",
            "Sejuani", "Shen", "Singed", "Zac", "Darius", "DrMundo", "Garen", "Gnar", "Gragas", "Hecarim", "Irelia",
            "Jax", "Nasus", "Olaf", "Poppy", "RekSai", "Renekton", "Shyvana", "Sion", "Skarner", "Trundle", "Udyr", "Vi",
            "Volibear", "Warwick", "Yorick"
        };
        public static string[] Tank =
        {
            "Randuin's Omen", "Frozen Heart", "Mercury's Treads",
            "Thornmail", "Locket of the Iron Solari", "Banshee's Veil"
        };
        public static string[] MageAssassinsNames = { "Fizz", "Akali", "Diana" };

        public static string[] MageAssassins =
        {
            "Lich Bane", "Void Staff", "Rabadon's Deathcap",
            "Sorcerer's Shoes", "Zhonya's Hourglass", "Luden's Echo"
        };

        public static string[] Katarina =
        {
            "Liandry's Torment", "Void Staff", "Rabadon's Deathcap",
            "Sorcerer's Shoes", "Zhonya's Hourglass", "Luden's Echo"
        };

        public static string[] MageSpellvampNames = { "Mordekaiser", "Rumble", "Vladimir" };
        public static string[] MageSpellvamp =
        {
            "Will of the Ancients", "Rabadon's Deathcap",
            "Sorcerer's Shoes", "Spirit Visage", "Zhonya's Hourglass", "Liandry's Torment"
        };

        public static string[] Cassiopeia =
        {
            "Archangel's Staff", "Liandry's Torment",
            "Sorcerer's Shoes", "Rabadon's Deathcap", "Zhonya's Hourglass", "Luden's Echo"
        };

        public static string[] Galio =
        {
            "Athene's Unholy Grail", "Abyssal Scepter",
            "Mercury's Treads", "Zhonya's Hourglass", "Spirit Visage", "Thornmail"
        };

        public static string[] Urgot =
        {
            "Manamune", "The Black Cleaver", "Mercury's Treads",
            "Frozen Heart", "Last Whisper", "Maw of Malmortius"
        };

        public static string[] AdUtilityNames = { "Corki", "Ezreal" };

        public static string[] AdUtility =
        {
            "Trinity Force", "Last Whisper", "Berserker's Greaves",
            "The Bloodthirster", "Infinity Edge", "Banshee's Veil"
        };

        public static string[] HybridNames = { "Kayle", "Teemo" };

        public static string[] Hybrid =
        {
            "Nashor's Tooth", "Runaan's Hurricane (Ranged Only)",
            "Sorcerer's Shoes", "Rabadon's Deathcap", "Liandry's Torment", "Void Staff"
        };

        public static string[] Ryze =
        {
            "Archangel's Staff", "Rod of Ages", "Frozen Heart", "Sorcerer's Shoes",
            "Void Staff", "Will of the Ancients"
        };

        public static Queue<Item> Queue = new Queue<Item>();
        public static bool CanBuy = true;
        public static void Init()
        {
            string itemJson = "https://raw.githubusercontent.com/myo/Experimental/master/item.json";
            string itemsData = Request(itemJson);
            string itemArray = itemsData.Split(new[] { "data" }, StringSplitOptions.None)[1];
            MatchCollection itemIdArray = Regex.Matches(itemArray, "[\"]\\d*[\"][:][{].*?(?=},\"\\d)");
            foreach (Item item in from object iItem in itemIdArray select new Item(iItem.ToString()))
                ItemList.Add(item);
            Console.WriteLine("Auto Buy Activated");
            LeagueSharp.Common.CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            CustomEvents.OnSpawn += CustomEvents_OnSpawn;
        }

        static void CustomEvents_OnSpawn(Obj_AI_Hero sender, EventArgs args)
        {
            if (sender.NetworkId == ObjectManager.Player.NetworkId)
                BuyItems();
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            var name = ObjectManager.Player.ChampionName;
            if (MageNames.Contains(name))
                List = Mage;
            if (MarksmenNames.Contains(name))
                List = Marksmen;
            if (UtilitySupportsNames.Contains(name))
                List = UtilitySupport;
            if (TankSupportsNames.Contains(name))
                List = TankSupport;
            if (AssassinNames.Contains(name))
                List = Assassin;
            if (TankNames.Contains(name))
                List = Tank;
            if (MageAssassinsNames.Contains(name))
                List = MageAssassins;
            if (name.Equals("Katarina"))
                List = Katarina;
            if (MageSpellvampNames.Contains(name))
                List = MageSpellvamp;
            if (name.Equals("Cassiopeia"))
                List = Cassiopeia;
            if (name.Equals("Galio"))
                List = Galio;
            if (name.Equals("Urgot"))
                List = Urgot;
            if (name.Equals("Ryze"))
                List = Ryze;
            if (AdUtilityNames.Contains(name))
                List = AdUtility;
            if (HybridNames.Contains(name))
                List = Hybrid;
            if (List[1] == "")
                List = TankSupport;
            Queue = ShoppingQueue();
            AlterInventory();

            Game.PrintChat("[{0}] Autobuy Loaded", ObjectManager.Player.ChampionName);
            BuyItems();
        }
        public static Queue<Item> ShoppingQueue()
        {
            var shoppingItems = new Queue<Item>();
            foreach (string indexItem in List)
            {
                var macroItems = new Queue<Item>();
                ItemSequence(GetItemByName(indexItem), macroItems);
                foreach (Item secondIndexItem in macroItems)
                    shoppingItems.Enqueue(secondIndexItem);
            }
            return shoppingItems;
        }
        public static void BuyItems()
        {
            while ((Queue.Peek() != null && InventoryFull()) && (Queue.Peek().From == null ||(Queue.Peek().From != null && !Queue.Peek().From.Contains(_lastItem.Id))))
            {
                var y = Queue.Dequeue();
                _priceAddup += y.Goldbase;
            }
            var x = 0;
                while ( Queue.Peek().Goldbase <= ObjectManager.Player.Gold - x - _priceAddup && Queue.Count > 0 &&
                       ObjectManager.Player.InShop())
                {
                    var y = Queue.Dequeue();
                    ObjectManager.Player.BuyItem((ItemId) y.Id);
                    _lastItem = y;
                    _priceAddup = 0;
                    x += y.Goldbase;
                }
        }
        public static int FreeSlots()
        {
            return -1 + ObjectManager.Player.InventoryItems.Count(y => !y.DisplayName.Contains("Poro"));
        }

        public static bool InventoryFull()
        {
            return FreeSlots() == 6;
        }

        public static void AlterInventory()
        {
            var y = 0;
            var z = ObjectManager.Player.InventoryItems.ToList().OrderBy(i => i.Slot).Select(item => item.Id).ToList();
            for(int i = 0; i < z.Count - 2; i++)
            {
                var x = GetItemById((int) z[i]);
                Queue<Item> temp = new Queue<Item>();
                ItemSequence(x, temp);
                y += temp.Count;
            }
            for (int i = 0; i < y; i++)
                Console.WriteLine(Queue.Dequeue());
        }
    }
}
