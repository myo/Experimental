namespace MySharpSupport.Evade
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    public enum SkillShotType
    {
        SkillshotCircle,

        SkillshotLine,

        SkillshotMissileLine,

        SkillshotCone,

        SkillshotMissileCone,

        SkillshotRing
    }

    public enum DetectionType
    {
        RecvPacket,

        ProcessSpell
    }

    public struct SafePathResult
    {
        public SafePathResult(bool isSafe, FoundIntersection intersection)
        {
            this.IsSafe = isSafe;
            this.Intersection = intersection;
        }

        public FoundIntersection Intersection;

        public bool IsSafe;
    }

    public struct FoundIntersection
    {
        public FoundIntersection(float distance, int time, Vector2 point, Vector2 comingFrom)
        {
            this.Distance = distance;
            this.ComingFrom = comingFrom;
            this.Valid = (point.X != 0) && (point.Y != 0);
            this.Point = point + Config.GridSize * (this.ComingFrom - point).Normalized();
            this.Time = time;
        }

        public Vector2 ComingFrom;

        public float Distance;

        public Vector2 Point;

        public int Time;

        public bool Valid;
    }

    public class Skillshot
    {
        public Skillshot(
            DetectionType detectionType,
            SpellData spellData,
            int startT,
            Vector2 start,
            Vector2 end,
            Obj_AI_Base unit)
        {
            this.DetectionType = detectionType;
            this.SpellData = spellData;
            this.StartTick = startT;
            this.Start = start;
            this.End = end;
            this.MissilePosition = start;
            this.Direction = (end - start).Normalized();

            this.Unit = unit;

            //Create the spatial object for each type of skillshot.
            switch (spellData.Type)
            {
                case SkillShotType.SkillshotCircle:
                    this.Circle = new Geometry.Circle(this.CollisionEnd, spellData.Radius);
                    break;
                case SkillShotType.SkillshotLine:
                    this.Rectangle = new Geometry.Rectangle(this.Start, this.CollisionEnd, spellData.Radius);
                    break;
                case SkillShotType.SkillshotMissileLine:
                    this.Rectangle = new Geometry.Rectangle(this.Start, this.CollisionEnd, spellData.Radius);
                    break;
                case SkillShotType.SkillshotCone:
                    this.Sector = new Geometry.Sector(
                        start,
                        this.CollisionEnd - start,
                        spellData.Radius * (float)Math.PI / 180,
                        spellData.Range);
                    break;
                case SkillShotType.SkillshotRing:
                    this.Ring = new Geometry.Ring(this.CollisionEnd, spellData.Radius, spellData.RingRadius);
                    break;
            }

            this.UpdatePolygon(); //Create the polygon.
        }

        public Geometry.Circle Circle;

        public DetectionType DetectionType;

        public Vector2 Direction;

        public Geometry.Polygon DrawingPolygon;

        public Vector2 End;

        public bool ForceDisabled;

        public Vector2 MissilePosition;

        public Geometry.Polygon Polygon;

        public Geometry.Rectangle Rectangle;

        public Geometry.Ring Ring;

        public Geometry.Sector Sector;

        public SpellData SpellData;

        public Vector2 Start;

        public int StartTick;

        private Vector2 _collisionEnd;

        private int _lastCollisionCalc;

        public Vector2 CollisionEnd
        {
            get
            {
                if (this._collisionEnd.IsValid())
                {
                    return this._collisionEnd;
                }

                if (this.IsGlobal)
                {
                    return this.GlobalGetMissilePosition(0)
                           + this.Direction * this.SpellData.MissileSpeed
                           * (0.5f + this.SpellData.Radius * 2 / ObjectManager.Player.MoveSpeed);
                }

                return this.End;
            }
        }

        public Geometry.Polygon EvadePolygon { get; set; }

        public bool IsGlobal
        {
            get
            {
                return this.SpellData.RawRange == 20000;
            }
        }

        public Vector2 Perpendicular
        {
            get
            {
                return this.Direction.Perpendicular();
            }
        }

        public Obj_AI_Base Unit { get; set; }

        //public bool Evade()
        //{
        //    if (ForceDisabled)
        //    {
        //        return false;
        //    }
        //    if (Environment.TickCount - _cachedValueTick < 100)
        //    {
        //        return _cachedValue;
        //    }

        //    if (!ConfigValue<bool>("IsDangerous") && Config.Menu.Item("OnlyDangerous").ConfigValue<KeyBind>().Active)
        //    {
        //        _cachedValue = false;
        //        _cachedValueTick = Environment.TickCount;
        //        return _cachedValue;
        //    }

        //    _cachedValue = ConfigValue<bool>("Enabled");
        //    _cachedValueTick = Environment.TickCount;

        //    return _cachedValue;
        //}

        public void Game_OnGameUpdate()
        {
            //Even if it doesnt consume a lot of resources with 20 updatest second works k
            if (this.SpellData.CollisionObjects.Count() > 0 && this.SpellData.CollisionObjects != null
                && Environment.TickCount - this._lastCollisionCalc > 50)
            {
                this._lastCollisionCalc = Environment.TickCount;
                this._collisionEnd = Collision.GetCollisionPoint(this);
            }

            //Update the missile position each time the game updates.
            if (this.SpellData.Type == SkillShotType.SkillshotMissileLine)
            {
                this.Rectangle = new Geometry.Rectangle(
                    this.GetMissilePosition(0),
                    this.CollisionEnd,
                    this.SpellData.Radius);
                this.UpdatePolygon();
            }

            //Spells that update to the unit position.
            if (this.SpellData.MissileFollowsUnit)
            {
                if (this.Unit.IsVisible)
                {
                    this.End = this.Unit.ServerPosition.To2D();
                    this.Direction = (this.End - this.Start).Normalized();
                    this.UpdatePolygon();
                }
            }
        }

        /// <summary>
        ///     Returns the missile position after time time.
        /// </summary>
        public Vector2 GetMissilePosition(int time)
        {
            var t = Math.Max(0, Environment.TickCount + time - this.StartTick - this.SpellData.Delay);

            var x = 0;

            //Missile with acceleration = 0.
            if (this.SpellData.MissileAccel == 0)
            {
                x = t * this.SpellData.MissileSpeed / 1000;
            }

            //Missile with constant acceleration.
            else
            {
                var t1 = (this.SpellData.MissileAccel > 0
                              ? this.SpellData.MissileMaxSpeed
                              : this.SpellData.MissileMinSpeed - this.SpellData.MissileSpeed) * 1000f
                         / this.SpellData.MissileAccel;

                if (t <= t1)
                {
                    x =
                        (int)
                        (t * this.SpellData.MissileSpeed / 1000d
                         + 0.5d * this.SpellData.MissileAccel * Math.Pow(t / 1000d, 2));
                }
                else
                {
                    x =
                        (int)
                        (t1 * this.SpellData.MissileSpeed / 1000d
                         + 0.5d * this.SpellData.MissileAccel * Math.Pow(t1 / 1000d, 2)
                         + (t - t1) / 1000d
                         * (this.SpellData.MissileAccel < 0
                                ? this.SpellData.MissileMaxSpeed
                                : this.SpellData.MissileMinSpeed));
                }
            }

            t = (int)Math.Max(0, Math.Min(this.CollisionEnd.Distance(this.Start), x));
            return this.Start + this.Direction * t;
        }

        /// <summary>
        ///     Returns the missile position after time time.
        /// </summary>
        public Vector2 GlobalGetMissilePosition(int time)
        {
            var t = Math.Max(0, Environment.TickCount + time - this.StartTick - this.SpellData.Delay);
            t = (int)Math.Max(0, Math.Min(this.End.Distance(this.Start), t * this.SpellData.MissileSpeed / 1000));
            return this.Start + this.Direction * t;
        }

        //Returns if the skillshot is about to hit the unit in the next time seconds.
        public bool IsAboutToHit(int time, Obj_AI_Base unit)
        {
            if (this.SpellData.Type == SkillShotType.SkillshotMissileLine)
            {
                var missilePos = this.GetMissilePosition(0);
                var missilePosAfterT = this.GetMissilePosition(time);

                //TODO: Check for minion collision etc.. in the future.
                var projection = unit.ServerPosition.To2D().ProjectOn(missilePos, missilePosAfterT);

                if (projection.IsOnSegment
                    && projection.SegmentPoint.Distance(unit.ServerPosition) < this.SpellData.Radius)
                {
                    return true;
                }

                return false;
            }

            if (!this.IsSafe(unit.ServerPosition.To2D()))
            {
                var timeToExplode = this.SpellData.ExtraDuration + this.SpellData.Delay
                                    + (int)((1000 * this.Start.Distance(this.End)) / this.SpellData.MissileSpeed)
                                    - (Environment.TickCount - this.StartTick);
                if (timeToExplode <= time)
                {
                    return true;
                }
            }

            return false;
        }

        //public T ConfigValue<T>(string name)
        //{
        //    return Config.Menu.Item(name + SpellData.MenuItemName).ConfigValue<T>();
        //}
        /// <summary>
        ///     Returns the value from this skillshot menu.
        /// </summary>
        /// <summary>
        ///     Returns if the skillshot has expired.
        /// </summary>
        public bool IsActive()
        {
            if (this.SpellData.MissileAccel != 0)
            {
                return Environment.TickCount <= this.StartTick + 5000;
            }

            return Environment.TickCount
                   <= this.StartTick + this.SpellData.Delay + this.SpellData.ExtraDuration
                   + 1000 * (this.Start.Distance(this.End) / this.SpellData.MissileSpeed);
        }

        public bool IsDanger(Vector2 point)
        {
            return !this.IsSafe(point);
        }

        public bool IsSafe(Vector2 point)
        {
            return this.Polygon.IsOutside(point);
        }

        /// <summary>
        ///     Returns if the skillshot will hit the unit if the unit follows the path.
        /// </summary>
        public SafePathResult IsSafePath(
            List<Vector2> path,
            int timeOffset,
            int speed = -1,
            int delay = 0,
            Obj_AI_Base unit = null)
        {
            var Distance = 0f;
            timeOffset += Game.Ping / 2;

            speed = (speed == -1) ? (int)ObjectManager.Player.MoveSpeed : speed;

            if (unit == null)
            {
                unit = ObjectManager.Player;
            }

            var allIntersections = new List<FoundIntersection>();
            for (var i = 0; i <= path.Count - 2; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var segmentIntersections = new List<FoundIntersection>();

                for (var j = 0; j <= this.Polygon.Points.Count - 1; j++)
                {
                    var sideStart = this.Polygon.Points[j];
                    var sideEnd = this.Polygon.Points[j == (this.Polygon.Points.Count - 1) ? 0 : j + 1];

                    var intersection = from.Intersection(to, sideStart, sideEnd);

                    if (intersection.Intersects)
                    {
                        segmentIntersections.Add(
                            new FoundIntersection(
                                Distance + intersection.Point.Distance(from),
                                (int)((Distance + intersection.Point.Distance(from)) * 1000 / speed),
                                intersection.Point,
                                from));
                    }
                }

                var sortedList = segmentIntersections.OrderBy(o => o.Distance).ToList();
                allIntersections.AddRange(sortedList);

                Distance += from.Distance(to);
            }

            //Skillshot with missile.
            if (this.SpellData.Type == SkillShotType.SkillshotMissileLine
                || this.SpellData.Type == SkillShotType.SkillshotMissileCone)
            {
                //Outside the skillshot
                if (this.IsSafe(ObjectManager.Player.ServerPosition.To2D()))
                {
                    //No intersections -> Safe
                    if (allIntersections.Count == 0)
                    {
                        return new SafePathResult(true, new FoundIntersection());
                    }

                    for (var i = 0; i <= allIntersections.Count - 1; i = i + 2)
                    {
                        var enterIntersection = allIntersections[i];
                        var enterIntersectionProjection =
                            enterIntersection.Point.ProjectOn(this.Start, this.End).SegmentPoint;

                        //Intersection with no exit point.
                        if (i == allIntersections.Count - 1)
                        {
                            var missilePositionOnIntersection =
                                this.GetMissilePosition(enterIntersection.Time - timeOffset);
                            return
                                new SafePathResult(
                                    (this.End.Distance(missilePositionOnIntersection) + 50
                                     <= this.End.Distance(enterIntersectionProjection))
                                    && ObjectManager.Player.MoveSpeed < this.SpellData.MissileSpeed,
                                    allIntersections[0]);
                        }

                        var exitIntersection = allIntersections[i + 1];
                        var exitIntersectionProjection =
                            exitIntersection.Point.ProjectOn(this.Start, this.End).SegmentPoint;

                        var missilePosOnEnter = this.GetMissilePosition(enterIntersection.Time - timeOffset);
                        var missilePosOnExit = this.GetMissilePosition(exitIntersection.Time + timeOffset);

                        //Missile didnt pass.
                        if (missilePosOnEnter.Distance(this.End) + 50 > enterIntersectionProjection.Distance(this.End))
                        {
                            if (missilePosOnExit.Distance(this.End) <= exitIntersectionProjection.Distance(this.End))
                            {
                                return new SafePathResult(false, allIntersections[0]);
                            }
                        }
                    }

                    return new SafePathResult(true, allIntersections[0]);
                }
                //Inside the skillshot.
                if (allIntersections.Count == 0)
                {
                    return new SafePathResult(false, new FoundIntersection());
                }

                if (allIntersections.Count > 0)
                {
                    //Check only for the exit point
                    var exitIntersection = allIntersections[0];
                    var exitIntersectionProjection = exitIntersection.Point.ProjectOn(this.Start, this.End).SegmentPoint;

                    var missilePosOnExit = this.GetMissilePosition(exitIntersection.Time + timeOffset);
                    if (missilePosOnExit.Distance(this.End) <= exitIntersectionProjection.Distance(this.End))
                    {
                        return new SafePathResult(false, allIntersections[0]);
                    }
                }
            }

            if (this.IsSafe(ObjectManager.Player.ServerPosition.To2D()))
            {
                if (allIntersections.Count == 0)
                {
                    return new SafePathResult(true, new FoundIntersection());
                }

                if (this.SpellData.DontCross)
                {
                    return new SafePathResult(false, allIntersections[0]);
                }
            }
            else
            {
                if (allIntersections.Count == 0)
                {
                    return new SafePathResult(false, new FoundIntersection());
                }
            }

            var timeToExplode = (this.SpellData.DontAddExtraDuration ? 0 : this.SpellData.ExtraDuration)
                                + this.SpellData.Delay
                                + (int)(1000 * this.Start.Distance(this.End) / this.SpellData.MissileSpeed)
                                - (Environment.TickCount - this.StartTick);

            var myPositionWhenExplodes = path.PositionAfter(timeToExplode, speed, delay);

            if (!this.IsSafe(myPositionWhenExplodes))
            {
                return new SafePathResult(false, allIntersections[0]);
            }

            var myPositionWhenExplodesWithOffset = path.PositionAfter(timeToExplode, speed, timeOffset);

            return new SafePathResult(this.IsSafe(myPositionWhenExplodesWithOffset), allIntersections[0]);
        }

        /// <summary>
        ///     Returns if the skillshot will hit you when trying to blink to the point.
        /// </summary>
        public bool IsSafeToBlink(Vector2 point, int timeOffset, int delay = 0)
        {
            timeOffset /= 2;

            if (this.IsSafe(ObjectManager.Player.ServerPosition.To2D()))
            {
                return true;
            }

            //Skillshots with missile
            if (this.SpellData.Type == SkillShotType.SkillshotMissileLine)
            {
                var missilePositionAfterBlink = this.GetMissilePosition(delay + timeOffset);
                var myPositionProjection = ObjectManager.Player.ServerPosition.To2D().ProjectOn(this.Start, this.End);

                if (missilePositionAfterBlink.Distance(this.End) < myPositionProjection.SegmentPoint.Distance(this.End))
                {
                    return false;
                }

                return true;
            }

            //skillshots without missile
            var timeToExplode = this.SpellData.ExtraDuration + this.SpellData.Delay
                                + (int)(1000 * this.Start.Distance(this.End) / this.SpellData.MissileSpeed)
                                - (Environment.TickCount - this.StartTick);

            return timeToExplode > timeOffset + delay;
        }

        public void UpdatePolygon()
        {
            switch (this.SpellData.Type)
            {
                case SkillShotType.SkillshotCircle:
                    this.Polygon = this.Circle.ToPolygon();
                    this.EvadePolygon = this.Circle.ToPolygon(Config.ExtraEvadeDistance);
                    this.DrawingPolygon = this.Circle.ToPolygon(
                        0,
                        !this.SpellData.AddHitbox
                            ? this.SpellData.Radius
                            : (this.SpellData.Radius - ObjectManager.Player.BoundingRadius));
                    break;
                case SkillShotType.SkillshotLine:
                    this.Polygon = this.Rectangle.ToPolygon();
                    this.DrawingPolygon = this.Rectangle.ToPolygon(
                        0,
                        !this.SpellData.AddHitbox
                            ? this.SpellData.Radius
                            : (this.SpellData.Radius - ObjectManager.Player.BoundingRadius));
                    this.EvadePolygon = this.Rectangle.ToPolygon(Config.ExtraEvadeDistance);
                    break;
                case SkillShotType.SkillshotMissileLine:
                    this.Polygon = this.Rectangle.ToPolygon();
                    this.DrawingPolygon = this.Rectangle.ToPolygon(
                        0,
                        !this.SpellData.AddHitbox
                            ? this.SpellData.Radius
                            : (this.SpellData.Radius - ObjectManager.Player.BoundingRadius));
                    this.EvadePolygon = this.Rectangle.ToPolygon(Config.ExtraEvadeDistance);
                    break;
                case SkillShotType.SkillshotCone:
                    this.Polygon = this.Sector.ToPolygon();
                    this.DrawingPolygon = this.Polygon;
                    this.EvadePolygon = this.Sector.ToPolygon(Config.ExtraEvadeDistance);
                    break;
                case SkillShotType.SkillshotRing:
                    this.Polygon = this.Ring.ToPolygon();
                    this.DrawingPolygon = this.Polygon;
                    this.EvadePolygon = this.Ring.ToPolygon(Config.ExtraEvadeDistance);
                    break;
            }
        }
    }
}