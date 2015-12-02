using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace MySharpSupport.AI
{
    public static class RecallManager
    {
        public static bool ShouldRecall { get { return !ObjectManager.Player.InFountain() && (Core.Carry.IsRecalling() || Core.Carry.InFountain() || Core.Carry.IsDead || ObjectManager.Player.HealthPercent < 25); } }

        public static Vector3 SafestRecallPosition
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(turret => turret.IsAlly && turret.CountEnemiesInRange(800) == 0)
                        .OrderBy(t => t.Distance(ObjectManager.Player))
                        .FirstOrDefault()
                        .ServerPosition.Randomize(-100, 100);
            }
        }

        public static void OnUpdate()
        {
            if (ShouldRecall)
            {
                if (ObjectManager.Player.Distance(SafestRecallPosition) < 300)
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall);
            }
        }
    }
}
