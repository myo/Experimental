namespace MySharpSupport.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    public class AutoBushRevealer
    {
        public AutoBushRevealer(Menu menu)
        {
            this._enemyInfo = HeroManager.Enemies.Select(x => new EnemyInfo(x)).ToList();

            this._menu = menu.AddSubMenu(new Menu("Auto Bush Revealer", "AutoBushRevealerMenu"));
            this._menu.AddItem(new MenuItem("Auto Bush Revealer", "AutoBushRevealer").SetValue(true));
            this._menu.AddItem(new MenuItem("AutoBushEnabled", "Enabled").SetValue(true));

            var useWardsMenu = this._menu.AddSubMenu(new Menu("Use Wards: ", "AutoBushUseWards"));

            foreach (var ward in Wards)
            {
                useWardsMenu.AddItem(new MenuItem("AutoBush" + ward.Key, ward.Value).SetValue(true));
            }

            //Game.OnGameUpdate += Game_OnGameUpdate; // TODO: wait for update
        }

        private readonly List<EnemyInfo> _enemyInfo;

        private readonly Menu _menu;

        private static readonly List<KeyValuePair<int, string>> Wards = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(3340, "Warding Totem Trinket"),
                new KeyValuePair<int, string>(3361, "Greater Stealth Totem Trinket"),
                new KeyValuePair<int, string>(3205, "Quill Coat"),
                new KeyValuePair<int, string>(3207, "Spirit Of The Ancient Golem"),
                new KeyValuePair<int, string>(3154, "Wriggle's Lantern"),
                new KeyValuePair<int, string>(2049, "Sight Stone"),
                new KeyValuePair<int, string>(2045, "Ruby Sightstone"),
                new KeyValuePair<int, string>(3160, "Feral Flare"),
                new KeyValuePair<int, string>(2050, "Explorer's Ward"),
                new KeyValuePair<int, string>(2044, "Stealth Ward")
            };

        private int _lastTimeWarded;

        private void Game_OnGameUpdate(EventArgs args)
        {
            var time = Environment.TickCount;

            foreach (var enemyInfo in this._enemyInfo.Where(x => x.Player.IsVisible))
            {
                enemyInfo.LastSeen = time;
            }

            if (PluginBase.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                return;
            }

            if (this._menu.Item("AutoBushEnabled").GetValue<bool>())
            {
                foreach (var enemy in
                    this._enemyInfo.Where(
                        x =>
                        x.Player.IsValid && !x.Player.IsVisible && !x.Player.IsDead
                        && x.Player.Distance(ObjectManager.Player.ServerPosition) < 1000 && time - x.LastSeen < 2500)
                        .Select(x => x.Player))
                {
                    var bestWardPos = this.GetWardPos(enemy.ServerPosition, 165, 2);

                    if (bestWardPos != enemy.ServerPosition && bestWardPos != Vector3.Zero
                        && bestWardPos.Distance(ObjectManager.Player.ServerPosition) <= 600)
                    {
                        var timedif = Environment.TickCount - this._lastTimeWarded;

                        if (timedif > 1250
                            && !(timedif < 2500 && this.GetNearObject("SightWard", bestWardPos, 200) != null))
                            //no near wards
                        {
                            var wardSlot = this.GetWardSlot();

                            if (wardSlot != null && wardSlot.Id != ItemId.Unknown)
                            {
                                //wardSlot.UseItem(bestWardPos); // TODO fix for 4.21
                                this._lastTimeWarded = Environment.TickCount;
                            }
                        }
                    }
                }
            }
        }

        private Obj_AI_Base GetNearObject(string name, Vector3 pos, int maxDistance)
        {
            return ObjectManager.Get<Obj_AI_Base>()
                                .FirstOrDefault(x => x.Name == name && x.Distance(pos) <= maxDistance);
        }

        private Vector3 GetWardPos(Vector3 lastPos, int radius = 165, int precision = 3)
        {
            return new Vector3();
            //    var count = precision;

            //    while (count > 0)
            //    {
            //        var vertices = radius;

            //        var wardLocations = new WardLocation[vertices];
            //        var angle = 2*Math.PI/vertices;

            //        for (var i = 0; i < vertices; i++)
            //        {
            //            var th = angle*i;
            //            var pos = new Vector3((float) (lastPos.X + radius*Math.Cos(th)),
            //                (float) (lastPos.Y + radius*Math.Sin(th)), 0);
            //            wardLocations[i] = new WardLocation(pos, NavMesh.IsWallOfGrass(pos, 50)); // TODO: check later
            //        }

            //        var grassLocations = new List<GrassLocation>();

            //        for (var i = 0; i < wardLocations.Length; i++)
            //        {
            //            if (!wardLocations[i].Grass) continue;
            //            if (i != 0 && wardLocations[i - 1].Grass)
            //                grassLocations.Last().Count++;
            //            else
            //                grassLocations.Add(new GrassLocation(i, 1));
            //        }

            //        var grassLocation = grassLocations.OrderByDescending(x => x.Count).FirstOrDefault();

            //        if (grassLocation != null) //else: no pos found. increase/decrease radius?
            //        {
            //            var midelement = (int) Math.Ceiling(grassLocation.Count/2f);
            //            lastPos = wardLocations[grassLocation.Index + midelement - 1].Pos;
            //            radius = (int) Math.Floor(radius/2f);
            //        }

            //        count--;
            //    }

            //    return lastPos;
        }

        private InventorySlot GetWardSlot()
        {
            return
                Wards.Select(x => x.Key)
                     .Where(id => this._menu.Item("AutoBush" + id).GetValue<bool>() && Items.CanUseItem(id))
                     .Select(
                         wardId => ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId))
                     .FirstOrDefault();
        }

        private class EnemyInfo
        {
            public EnemyInfo(Obj_AI_Hero player)
            {
                this.Player = player;
            }

            public int LastSeen { get; set; }

            public Obj_AI_Hero Player { get; set; }
        }

        private class GrassLocation
        {
            public GrassLocation(int index, int count)
            {
                this.Index = index;
                this.Count = count;
            }

            public readonly int Index;

            public int Count;
        }

        private class WardLocation
        {
            public WardLocation(Vector3 pos, bool grass)
            {
                this.Pos = pos;
                this.Grass = grass;
            }

            public readonly bool Grass;

            public readonly Vector3 Pos;
        }
    }
}