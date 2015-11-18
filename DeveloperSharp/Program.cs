using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using SharpDX;

namespace DeveloperSharp
{
    class Program
    {
        private static List<GameObject> _lstGameObjects = new List<GameObject>(); 
        private static List<GameObject> _lstObjCloseToMouse = new List<GameObject>();
        private static Menu Config;
        private static int _lastUpdateTick = 0;
        private static int _lastMovementTick = 0;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += sD =>
            {
                InitMenu();
                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            };
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            Game.PrintChat("Detected Spell Name: " + args.SData.Name + " Missile Name: " + args.SData.MissileBoneName);
        }

        private static void InitMenu()
        {
            Config = new Menu("Developer#", "developersharp", true);
            Config.AddItem(new MenuItem("range", "Max object dist from cursor").SetValue(new Slider(400, 100, 1000)));
            Config.AddToMainMenu();
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Environment.TickCount - _lastUpdateTick > 150)
            {
                _lstGameObjects = ObjectManager.Get<GameObject>().ToList();
                _lstObjCloseToMouse =
                    _lstGameObjects.Where(o => o.Position.Distance(Game.CursorPos) < Config.Item("range").GetValue<Slider>().Value && !(o is Obj_Turret) && o.Name != "missile" && !(o is Obj_LampBulb) && !(o is Obj_SpellMissile) && !(o is GrassObject) && !(o is DrawFX) && !(o is LevelPropSpawnerPoint) && !(o is Obj_GeneralParticleEmitter) && !o.Name.Contains("MoveTo")).ToList();
                _lastUpdateTick = Environment.TickCount;
            }
            if (Environment.TickCount - _lastMovementTick > 140000)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                    ObjectManager.Player.Position.Randomize(-1000, 1000));
                _lastMovementTick = Environment.TickCount;
            }
        }

        private static void OnDraw(EventArgs args)
        {
            foreach (var obj in _lstObjCloseToMouse)
            {
                if (!obj.IsValid) return;
                var X = Drawing.WorldToScreen(obj.Position).X;
                var Y = Drawing.WorldToScreen(obj.Position).Y;
                Drawing.DrawText(X, Y, Color.DarkTurquoise, (obj is Obj_AI_Hero) ? ((Obj_AI_Hero)obj).CharData.BaseSkinName : obj.Name);
                Drawing.DrawText(X, Y + 10, Color.DarkTurquoise, obj.Type.ToString());
                Drawing.DrawText(X, Y + 20, Color.DarkTurquoise, "NetworkID: " + obj.NetworkId);
                Drawing.DrawText(X, Y + 30, Color.DarkTurquoise, obj.Position.ToString());
                if (obj is Obj_AI_Base)
                {
                    var aiobj = obj as Obj_AI_Base;
                    Drawing.DrawText(X, Y + 40, Color.DarkTurquoise, "Health: " + aiobj.Health + "/" + aiobj.MaxHealth + "(" + aiobj.HealthPercent + "%)");
                }
                if (obj is Obj_AI_Hero)
                {
                    var hero = obj as Obj_AI_Hero;
                    Drawing.DrawText(X, Y + 50, Color.DarkTurquoise, "Spells:");
                    Drawing.DrawText(X, Y + 60, Color.DarkTurquoise, "(Q): " + hero.Spellbook.Spells[0].Name);
                    Drawing.DrawText(X, Y + 70, Color.DarkTurquoise, "(W): " + hero.Spellbook.Spells[1].Name);
                    Drawing.DrawText(X, Y + 80, Color.DarkTurquoise, "(E): " + hero.Spellbook.Spells[2].Name);
                    Drawing.DrawText(X, Y + 90, Color.DarkTurquoise, "(R): " + hero.Spellbook.Spells[3].Name);
                    Drawing.DrawText(X, Y + 100, Color.DarkTurquoise, "(D): " + hero.Spellbook.Spells[4].Name);
                    Drawing.DrawText(X, Y + 110, Color.DarkTurquoise, "(F): " + hero.Spellbook.Spells[5].Name);
                    var buffs = hero.Buffs;
                    if (buffs.Any())
                    {
                        Drawing.DrawText(X, Y + 120, Color.DarkTurquoise, "Buffs:");
                    }
                    for (var i = 0; i < buffs.Count()*10; i += 10)
                    {
                        Drawing.DrawText(X, (Y + 130 + i), Color.DarkTurquoise, buffs[i/10].Count + "x " + buffs[i/10].Name);
                    }

                }
                if (obj is Obj_SpellMissile)
                {
                    var missile = obj as Obj_SpellMissile;
                    Drawing.DrawText(X, Y + 40, Color.DarkTurquoise, "Missile Speed: " + missile.SData.MissileSpeed);
                    Drawing.DrawText(X, Y + 50, Color.DarkTurquoise, "Cast Range: " + missile.SData.CastRange);
                }

                if (obj is MissileClient && obj.Name != "missile")
                {
                    var missile = obj as MissileClient;
                    Drawing.DrawText(X, Y + 40, Color.DarkTurquoise, "Missile Speed: " + missile.SData.MissileSpeed);
                    Drawing.DrawText(X, Y + 50, Color.DarkTurquoise, "Cast Range: " + missile.SData.CastRange);
                }
            }
        }
    }
}
