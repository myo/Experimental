using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK.Core.Extensions;
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
                //create a polygon for the enemy team
                var enemyTeamPolygon = new Geometry.Polygon();

                //fill it with circles of enemies with their attackrange as radius
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.IsEnemy && !h.IsDead && h.IsVisible && h.Distance(ObjectManager.Player) < 1000)
                    .Select(e => new LeagueSharp.Common.Geometry.Polygon.Circle(e.ServerPosition, e.AttackRange).ToClipperPath().ToPolygon()).ForEach(poly => enemyTeamPolygon.Add(poly));
                
                //create a polygon to determine where the bot should stick at all the time
                var myLimitPolygon =
                    new LeagueSharp.Common.Geometry.Polygon.Circle(Core.Carry.ServerPosition, Core.MaxDistanceFromCarry)
                        .ToClipperPath().ToPolygon();

                //try to find a position outside the enemy team polygon
                var goodPosition = myLimitPolygon.Points.FirstOrDefault(point => enemyTeamPolygon.IsOutside(point));

                //check if we there is any before returning it
                if (goodPosition != null && !goodPosition.IsZero)
                {
                    return goodPosition.To3D();
                }
                
                //if there isnt we take the enemy that is closest to us
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
