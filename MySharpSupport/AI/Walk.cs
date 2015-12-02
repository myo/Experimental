using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace MySharpSupport.AI
{
    public static class Walk
    {
        public static void OnUpdate()
        {
            if (!ObjectManager.Player.IsRecalling() && !ObjectManager.Player.IsChannelingImportantSpell() && !ObjectManager.Player.IsDead)
            {
                Core.Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.Combo;
            }
            else
            {
                Core.Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;
            }
            if (RecallManager.ShouldRecall)
            {
                Core.Orbwalker.SetOrbwalkingPoint(RecallManager.SafestRecallPosition);
            }
            else
            {
                Core.Orbwalker.SetOrbwalkingPoint(
                    HeatMap
                        .BestPositionAccordingToDistanceFromCarryAndTheEnemyTeamTakingIntoAccountOnlyAttackRangeAndMenuSettingsVector3);
            }
        }
    }
}
