namespace MySharpSupport.Evade
{
    using System;
    using System.Collections.Generic;

    using ClipperLib;

    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    /// <summary>
    ///     Class that contains the geometry related methods.
    /// </summary>
    public static class Geometry
    {
        private const int CircleLineSegmentN = 22;

        public static List<List<IntPoint>> ClipPolygons(List<Polygon> polygons)
        {
            var subj = new List<List<IntPoint>>(polygons.Count);
            var clip = new List<List<IntPoint>>(polygons.Count);

            foreach (var polygon in polygons)
            {
                subj.Add(polygon.ToClipperPath());
                clip.Add(polygon.ToClipperPath());
            }

            var solution = new List<List<IntPoint>>();
            var c = new Clipper();
            c.AddPaths(subj, PolyType.ptSubject, true);
            c.AddPaths(clip, PolyType.ptClip, true);
            c.Execute(ClipType.ctUnion, solution, PolyFillType.pftPositive, PolyFillType.pftEvenOdd);

            return solution;
        }

        /// <summary>
        ///     Returns the position on the path after t milliseconds at speed speed.
        /// </summary>
        public static Vector2 PositionAfter(this List<Vector2> self, int t, int speed, int delay = 0)
        {
            var distance = Math.Max(0, t - delay) * speed / 1000;
            for (var i = 0; i <= self.Count - 2; i++)
            {
                var from = self[i];
                var to = self[i + 1];
                var d = (int)to.Distance(from);
                if (d > distance)
                {
                    return from + distance * (to - from).Normalized();
                }
                distance -= d;
            }
            return self[self.Count - 1];
        }

        public static Vector3 SwitchYZ(this Vector3 v)
        {
            return new Vector3(v.X, v.Z, v.Y);
        }

        public static Polygon ToPolygon(this List<IntPoint> v)
        {
            var polygon = new Polygon();
            foreach (var point in v)
            {
                polygon.Add(new Vector2(point.X, point.Y));
            }
            return polygon;
        }

        //Clipper
        public static List<Polygon> ToPolygons(this List<List<IntPoint>> v)
        {
            var result = new List<Polygon>();

            foreach (var path in v)
            {
                result.Add(path.ToPolygon());
            }

            return result;
        }

        public class Circle
        {
            public Circle(Vector2 center, float radius)
            {
                this.Center = center;
                this.Radius = radius;
            }

            public Vector2 Center;

            public float Radius;

            public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
            {
                var result = new Polygon();
                var outRadius = (overrideWidth > 0
                                     ? overrideWidth
                                     : (offset + this.Radius) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentN));

                for (var i = 1; i <= CircleLineSegmentN; i++)
                {
                    var angle = i * 2 * Math.PI / CircleLineSegmentN;
                    var point = new Vector2(
                        this.Center.X + outRadius * (float)Math.Cos(angle),
                        this.Center.Y + outRadius * (float)Math.Sin(angle));
                    result.Add(point);
                }

                return result;
            }
        }

        public class Polygon
        {
            public List<Vector2> Points = new List<Vector2>();

            public void Add(Vector2 point)
            {
                this.Points.Add(point);
            }

            public void Draw(Color color, int width = 1)
            {
                for (var i = 0; i <= this.Points.Count - 1; i++)
                {
                    var nextIndex = (this.Points.Count - 1 == i) ? 0 : (i + 1);
                    Utils.DrawLineInWorld(this.Points[i].To3D(), this.Points[nextIndex].To3D(), width, color);
                }
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, this.ToClipperPath()) != 1;
            }

            public List<IntPoint> ToClipperPath()
            {
                var result = new List<IntPoint>(this.Points.Count);

                foreach (var point in this.Points)
                {
                    result.Add(new IntPoint(point.X, point.Y));
                }

                return result;
            }
        }

        public class Rectangle
        {
            public Rectangle(Vector2 start, Vector2 end, float width)
            {
                this.RStart = start;
                this.REnd = end;
                this.Width = width;
                this.Direction = (end - start).Normalized();
                this.Perpendicular = this.Direction.Perpendicular();
            }

            public Vector2 Direction;

            public Vector2 Perpendicular;

            public Vector2 REnd;

            public Vector2 RStart;

            public float Width;

            public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
            {
                var result = new Polygon();

                result.Add(
                    this.RStart + (overrideWidth > 0 ? overrideWidth : this.Width + offset) * this.Perpendicular
                    - offset * this.Direction);
                result.Add(
                    this.RStart - (overrideWidth > 0 ? overrideWidth : this.Width + offset) * this.Perpendicular
                    - offset * this.Direction);
                result.Add(
                    this.REnd - (overrideWidth > 0 ? overrideWidth : this.Width + offset) * this.Perpendicular
                    + offset * this.Direction);
                result.Add(
                    this.REnd + (overrideWidth > 0 ? overrideWidth : this.Width + offset) * this.Perpendicular
                    + offset * this.Direction);

                return result;
            }
        }

        public class Ring
        {
            public Ring(Vector2 center, float radius, float ringRadius)
            {
                this.Center = center;
                this.Radius = radius;
                this.RingRadius = ringRadius;
            }

            public Vector2 Center;

            public float Radius;

            public float RingRadius; //actually radius width.

            public Polygon ToPolygon(int offset = 0)
            {
                var result = new Polygon();

                var outRadius = (offset + this.Radius + this.RingRadius)
                                / (float)Math.Cos(2 * Math.PI / CircleLineSegmentN);
                var innerRadius = this.Radius - this.RingRadius - offset;

                for (var i = 0; i <= CircleLineSegmentN; i++)
                {
                    var angle = i * 2 * Math.PI / CircleLineSegmentN;
                    var point = new Vector2(
                        this.Center.X - outRadius * (float)Math.Cos(angle),
                        this.Center.Y - outRadius * (float)Math.Sin(angle));
                    result.Add(point);
                }

                for (var i = 0; i <= CircleLineSegmentN; i++)
                {
                    var angle = i * 2 * Math.PI / CircleLineSegmentN;
                    var point = new Vector2(
                        this.Center.X + innerRadius * (float)Math.Cos(angle),
                        this.Center.Y - innerRadius * (float)Math.Sin(angle));
                    result.Add(point);
                }

                return result;
            }
        }

        public class Sector
        {
            public Sector(Vector2 center, Vector2 direction, float angle, float radius)
            {
                this.Center = center;
                this.Direction = direction;
                this.Angle = angle;
                this.Radius = radius;
            }

            public float Angle;

            public Vector2 Center;

            public Vector2 Direction;

            public float Radius;

            public Polygon ToPolygon(int offset = 0)
            {
                var result = new Polygon();
                var outRadius = (this.Radius + offset) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentN);

                result.Add(this.Center);
                var Side1 = this.Direction.Rotated(-this.Angle * 0.5f);

                for (var i = 0; i <= CircleLineSegmentN; i++)
                {
                    var cDirection = Side1.Rotated(i * this.Angle / CircleLineSegmentN).Normalized();
                    result.Add(
                        new Vector2(this.Center.X + outRadius * cDirection.X, this.Center.Y + outRadius * cDirection.Y));
                }

                return result;
            }
        }
    }
}