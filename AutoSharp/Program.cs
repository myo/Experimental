using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoSharp.Auto;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

// ReSharper disable ObjectCreationAsStatement

namespace AutoSharp
{
    class Program
    {
        public static Utility.Map.MapType Map;
        public static Menu Config;
        public static MyOrbwalker.Orbwalker Orbwalker;
        private static bool _loaded = false;

        public static void Init()
        {
            Map = Utility.Map.GetMap().Type; 
            Config = new Menu("AutoSharp: " + ObjectManager.Player.ChampionName, "autosharp." + ObjectManager.Player.ChampionName, true);
            Config.AddItem(new MenuItem("autosharp.mode", "Mode").SetValue(new StringList(new[] {"AUTO", "SBTW"}))).ValueChanged +=
                (sender, args) =>
                {
                    if (Config.Item("autosharp.mode").GetValue<StringList>().SelectedValue == "AUTO")
                    {
                        Autoplay.Load();
                    }
                    else
                    {
                        Autoplay.Unload();
                        Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                    }
                };
            Config.AddItem(new MenuItem("autosharp.humanizer", "Humanize Movement by ").SetValue(new Slider(175, 125, 350)));
            Config.AddItem(new MenuItem("autosharp.quit", "Quit after Game End").SetValue(true));
            Config.AddItem(new MenuItem("autosharp.shop", "AutoShop?").SetValue(true));
            var options = Config.AddSubMenu(new Menu("Options: ", "autosharp.options"));
            options.AddItem(new MenuItem("autosharp.options.healup", "Take Heals?").SetValue(true));
            options.AddItem(new MenuItem("onlyfarm", "Only Farm").SetValue(false));
            if (Map == Utility.Map.MapType.SummonersRift)
            {
                options.AddItem(new MenuItem("recallhp", "Recall if Health% <").SetValue(new Slider(30, 0, 100)));
            }
            var randomizer = Config.AddSubMenu(new Menu("Randomizer", "autosharp.randomizer"));
            var orbwalker = Config.AddSubMenu(new Menu("Orbwalker", "autosharp.orbwalker"));
            randomizer.AddItem(new MenuItem("autosharp.randomizer.minrand", "Min Rand By").SetValue(new Slider(0, 0, 90)));
            randomizer.AddItem(new MenuItem("autosharp.randomizer.maxrand", "Max Rand By").SetValue(new Slider(100, 100, 300)));
            randomizer.AddItem(new MenuItem("autosharp.randomizer.playdefensive", "Play Defensive?").SetValue(true));
            randomizer.AddItem(new MenuItem("autosharp.randomizer.auto", "Auto-Adjust? (ALPHA)").SetValue(true));

            new PluginLoader();

                Cache.Load(); 
                Game.OnUpdate += Positioning.OnUpdate;
                Autoplay.Load();
                Game.OnEnd += OnEnd;
                Obj_AI_Base.OnIssueOrder += AntiShrooms;
                Game.OnUpdate += AntiShrooms2;
                Spellbook.OnCastSpell += OnCastSpell;
                Obj_AI_Base.OnDamage += OnDamage;


            Orbwalker = new MyOrbwalker.Orbwalker(orbwalker);

            Utility.DelayAction.Add(
                    new Random().Next(1000, 10000), () =>
                    {
                        new LeagueSharp.Common.AutoLevel(Utils.AutoLevel.GetSequence().Select(num => num - 1).ToArray());
                        LeagueSharp.Common.AutoLevel.Enable();
                        Console.WriteLine("AutoLevel Init Success!");
                    });
        }

        public static void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (sender == null) return;
            if (args.TargetNetworkId == ObjectManager.Player.NetworkId && (sender is Obj_AI_Turret || sender is Obj_AI_Minion))
            {
                    Orbwalker.ForceOrbwalkingPoint(Heroes.Player.Position.Extend(Wizard.GetFarthestMinion().Position, 500).RandomizePosition());
            }
        }

        private static void AntiShrooms2(EventArgs args)
        {
            if (Map == Utility.Map.MapType.SummonersRift && !Heroes.Player.InFountain() &&
                Heroes.Player.HealthPercent < Config.Item("recallhp").GetValue<Slider>().Value)
            {
                if (Heroes.Player.HealthPercent > 0 && Heroes.Player.CountEnemiesInRange(1800) == 0 &&
                    !Turrets.EnemyTurrets.Any(t => t.Distance(Heroes.Player) < 950) &&
                    !Minions.EnemyMinions.Any(m => m.Distance(Heroes.Player) < 950))
                {
                    Orbwalker.ActiveMode = MyOrbwalker.OrbwalkingMode.None;
                    if (!Heroes.Player.HasBuff("Recall"))
                    {
                        Heroes.Player.Spellbook.CastSpell(SpellSlot.Recall);
                    }
                }
            }

            var turretNearTargetPosition =
                    Turrets.EnemyTurrets.FirstOrDefault(t => t.Distance(Heroes.Player.ServerPosition) < 950);
            if (turretNearTargetPosition != null && turretNearTargetPosition.CountNearbyAllyMinions(950) < 3)
            {
                Orbwalker.ForceOrbwalkingPoint(Heroes.Player.Position.Extend(HeadQuarters.AllyHQ.Position, 950));
            }
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if (sender.Owner.IsDead)
                {
                    args.Process = false;
                    return;
                }
                if (Map == Utility.Map.MapType.SummonersRift)
                {
                    if (Config.Item("onlyfarm").GetValue<bool>() && args.Target.IsValid<Obj_AI_Hero>() &&
                        args.Target.IsEnemy)
                    {
                        args.Process = false;
                        return;
                    }
                    if (Heroes.Player.InFountain() && args.Slot == SpellSlot.Recall)
                    {
                        args.Process = false;
                        return;
                    }
                    if (Heroes.Player.HasBuff("Recall"))
                    {
                        args.Process = false;
                        return;
                    }
                }
                if (Heroes.Player.UnderTurret(true) && args.Target.IsValid<Obj_AI_Hero>())
                {
                    args.Process = false;
                    return;
                }
            }
        }

        private static void OnEnd(GameEndEventArgs args)
        {
            if (Config.Item("autosharp.quit").GetValue<bool>())
            {
                Thread.Sleep(30000);
                Game.Quit();
            }
        }

        public static void AntiShrooms(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender != null && sender.IsMe)
            {
                if (sender.IsDead)
                {
                    args.Process = false;
                    return;
                }
                var turret = Turrets.ClosestEnemyTurret;
                if (Map == Utility.Map.MapType.SummonersRift && Heroes.Player.HasBuff("Recall") && Heroes.Player.CountEnemiesInRange(1800) == 0 &&
                    turret.Distance(Heroes.Player) > 950 && !Minions.EnemyMinions.Any(m => m.Distance(Heroes.Player) < 950))
                {
                    args.Process = false;
                    return;
                }

                if (args.Order == GameObjectOrder.MoveTo)
                {
                    if (args.TargetPosition.IsZero)
                    {
                        args.Process = false;
                        return;
                    }
                    if (!args.TargetPosition.IsValid())
                    {
                        args.Process = false;
                        return;
                    }
                    if (Map == Utility.Map.MapType.SummonersRift && Heroes.Player.InFountain() &&
                        Heroes.Player.HealthPercent < 100)
                    {
                        args.Process = false;
                        return;
                    }
                    if (turret != null && turret.Distance(args.TargetPosition) < 950 &&
                        turret.CountNearbyAllyMinions(950) < 3)
                    {
                        args.Process = false;
                        return;
                    }
                }

                #region BlockAttack

                if (args.Target != null && args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackTo)
                {
                    if (Config.Item("onlyfarm").GetValue<bool>() && args.Target.IsValid<Obj_AI_Hero>())
                    {
                        args.Process = false;
                        return;
                    }
                    if (args.Target.IsValid<Obj_AI_Hero>())
                    {
                        if (Minions.AllyMinions.Count(m => m.Distance(Heroes.Player) < 900) <
                            Minions.EnemyMinions.Count(m => m.Distance(Heroes.Player) < 900))
                        {
                            args.Process = false;
                            return;
                        }
                        if (((Obj_AI_Hero) args.Target).UnderTurret(true))
                        {
                            args.Process = false;
                            return;
                        }
                    }
                    if (Heroes.Player.UnderTurret(true) && args.Target.IsValid<Obj_AI_Hero>())
                    {
                        args.Process = false;
                        return;
                    }
                    if (turret != null && turret.Distance(ObjectManager.Player) < 950 && turret.CountNearbyAllyMinions(950) < 3)
                    {
                        args.Process = false;
                        return;
                    }
                    if (Heroes.Player.HealthPercent < Config.Item("recallhp").GetValue<Slider>().Value)
                    {
                        args.Process = false;
                        return;
                    }
                }

                #endregion
            }
            if (sender != null && args.Target != null && args.Target.IsMe)
            {
                if (sender is Obj_AI_Turret || sender is Obj_AI_Minion)
                {
                    var minion = Wizard.GetClosestAllyMinion();
                    if (minion != null)
                    {
                        Orbwalker.ForceOrbwalkingPoint(
                            Heroes.Player.Position.Extend(Wizard.GetClosestAllyMinion().Position, Heroes.Player.Distance(minion) + 100));
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            Game.OnUpdate += AdvancedLoading;
        }

        private static void AdvancedLoading(EventArgs args)
        {
            if (!_loaded)
            {
                if (ObjectManager.Player.Gold > 0)
                {
                    _loaded = true;
                    Utility.DelayAction.Add(new Random().Next(3000, 25000), Init);
                }
            }
        }
    }
}
