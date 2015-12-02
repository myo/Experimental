using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK.Core.Utils;
using SharpDX;
using SharpDX.Direct3D9;

namespace MySharpSupport.AI
{
    public static class HeatMap
    {
        public static Vector3
            BestPositionAccordingToDistanceFromCarryAndTheEnemyTeamTakingIntoAccountOnlyAttackRangeAndMenuSettingsVector3
        {
            get
            {
                //create a polygon to determine where the bot should stick at all the time
                var myLimitPolygon = new Geometry.Circle(Core.Carry.ServerPosition, Core.MaxDistanceFromCarry)
                        .ToPolygon();

                //stay out of turret range
                var turret =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .FirstOrDefault(t => t.IsEnemy && t.IsValidTarget() && t.Distance(ObjectManager.Player) <= 800);

                if (turret != null &&
                    MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Ally).Count(m => m.Distance(turret) < 800) < 3)
                {
                    return myLimitPolygon.Points.OrderByDescending(point => point.Distance(turret.ServerPosition)).FirstOrDefault().To3D();
                }
                
                //we take the enemy that is closest to us
                var enemyClosestToMe =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(h => h.IsEnemy && !h.IsDead && h.IsVisible && h.Distance(ObjectManager.Player) < 1000)
                        .OrderBy(e => e.Distance(ObjectManager.Player)).FirstOrDefault();


                //we check if hes valid
                if (enemyClosestToMe != null)
                {
                    //and then return the point farthest away from him
                    return myLimitPolygon.Points.OrderByDescending(point => point.Distance(enemyClosestToMe.ServerPosition)).FirstOrDefault().To3D();
                }

                //if he isnt valid we return a random point in our limit circle
                return myLimitPolygon.Points.OrderBy(h => new Random().Next(0, 9001)).FirstOrDefault().To3D();
            }
        }
    }
}
