using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace MySharpSupport.AI
{
    public static class Shop
    {
        public static int CurrentItem = 0;
        public static bool AttemptedToShop = false;
        public static ItemId RushItem = ItemId.Unknown;

        public static void OnUpdate()
        {
            if (ObjectManager.Player.InFountain())
            {
                UpdateCurrentItem();
                TryToBuy();
            }
            UpdateShopAttemptState();
        }

        public static void UpdateShopAttemptState()
        {
            if (AttemptedToShop && !ObjectManager.Player.InShop())
            {
                AttemptedToShop = false;
            }
        }
        public static void TryToBuy()
        {
            if (!AttemptedToShop)
            {
                AttemptedToShop = true;
                if (RushItem != ItemId.Unknown && !ObjectManager.Player.InventoryItems.Any(i => i.Id == RushItem))
                {
                    ObjectManager.Player.BuyItem(RushItem);
                }
                else
                {
                    ObjectManager.Player.BuyItem(GetItemByName(
                        Core.MainMenu.Item(GetMenuItemNameByCurrentItemNumber(CurrentItem))
                            .GetValue<StringList>()
                            .SelectedValue));
                }
            }
        }
        public static void UpdateCurrentItem()
        {
            if ((ObjectManager.Player.Gold < 500 || ObjectManager.Player.Gold > 500) && CurrentItem == 0)
            {
                CurrentItem++;
            }
            if (
                ObjectManager.Player.InventoryItems.Any(
                    i =>
                        i.Id ==
                        GetItemByName(
                            Core.MainMenu.Item(GetMenuItemNameByCurrentItemNumber(CurrentItem))
                                .GetValue<StringList>()
                                .SelectedValue)))
            {
                CurrentItem++;
            }
        }
        public static string GetMenuItemNameByCurrentItemNumber(int item)
        {
            switch (item)
            {
                case 0:
                    return "startingitem";
                case 1:
                    return "firstitem";
                case 2:
                    return "seconditem";
                case 3:
                    return "thirditem";
                case 4:
                    return "fourthitem";
                case 5:
                    return "fifthitem";
                case 6:
                    return "sixthitem";
                default:
                    return null;
            }
        }
        public static ItemId GetItemByName(string name)
        {
            var nameToLower = name.ToLower();

            if (nameToLower.Contains("coin"))
            {
                return ItemId.Ancient_Coin;
            }
            if (nameToLower.Contains("shield"))
            {
                return ItemId.Relic_Shield;
            }
            if (nameToLower.Contains("spellthief"))
            {
                return ItemId.Spellthiefs_Edge;
            }
            if (nameToLower.Contains("sightstone"))
            {
                return ItemId.Sightstone;
            }
            if (nameToLower.Contains("talisman"))
            {
                return ItemId.Talisman_of_Ascension;
            }
            if (nameToLower.Contains("frostqueen") || nameToLower.Contains("frost queen"))
            {
                return ItemId.Frost_Queens_Claim;
            }
            if (nameToLower.Contains("face"))
            {
                return ItemId.Face_of_the_Mountain;
            }
            if (nameToLower.Contains("zeke"))
            {
                return ItemId.Zekes_Herald;
            }
            if (nameToLower.Contains("locket"))
            {
                return ItemId.Locket_of_the_Iron_Solari;
            }
            if (nameToLower.Contains("mikael") || nameToLower.Contains("crucible"))
            {
                return ItemId.Mikaels_Crucible;
            }
            if (nameToLower.Contains("zhonya"))
            {
                return ItemId.Zhonyas_Hourglass;
            }
            if (nameToLower.Contains("morello"))
            {
                return ItemId.Morellonomicon;
            }
            if (nameToLower.Contains("rod") || nameToLower.Contains("roa"))
            {
                return ItemId.Rod_of_Ages;
            }
            return ItemId.Rod_of_Ages;
        }
    }
}