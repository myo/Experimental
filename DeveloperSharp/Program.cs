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
        private static int _lastUpdateTick = 0;
        private static int _lastMovementTick = 0;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += sD =>
            {
                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
            };
        }
        private static void OnUpdate(EventArgs args)
        {
            if (Environment.TickCount - _lastUpdateTick > 150)
            {
                _lstGameObjects = ObjectManager.Get<GameObject>().ToList();
                _lstObjCloseToMouse =
                    _lstGameObjects.Where(o => o.Position.Distance(Game.CursorPos) < 500 && !(o is Obj_Turret) && o.Name != "missile" && !(o is Obj_SpellMissile) && !(o is GrassObject) && !(o is DrawFX) && !(o is LevelPropSpawnerPoint) && !(o is Obj_GeneralParticleEmitter) && !o.Name.Contains("MoveTo")).ToList();
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
                Drawing.DrawText(X, Y, Color.DarkTurquoise, obj.Name);
                Drawing.DrawText(X, Y + 10, Color.DarkTurquoise, obj.Type.ToString());
                Drawing.DrawText(X, Y + 20, Color.DarkTurquoise, "NetworkID: " + obj.NetworkId);
                Drawing.DrawText(X, Y + 30, Color.DarkTurquoise, obj.Position.ToString());
                if (obj is Obj_AI_Hero)
                {
                    var hero = obj as Obj_AI_Hero;
                    var buffs = hero.Buffs;
                    for (var i = 0; i < buffs.Count()*10; i += 10)
                    {
                        Drawing.DrawText(X, (Y + 40 + i), Color.DarkTurquoise, buffs[i/10].Name);
                    }

                }
                if (obj is Obj_SpellMissile)
                {
                    var missile = obj as Obj_SpellMissile;
                    Drawing.DrawText(X, Y + 30, Color.DarkTurquoise, "Missile Speed: " + missile.SData.MissileSpeed);
                    Drawing.DrawText(X, Y + 30, Color.DarkTurquoise, "Cast Range: " + missile.SData.CastRange);
                }
            }
        }
    }
}
