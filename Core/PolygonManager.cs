using SampSharp.GameMode;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Core
{
    public class PolygonPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public PolygonPoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class Polygon
    {
        public List<PolygonPoint> Points { get; private set; }
        public float InteractionHeight { get; set; }

        public Polygon(float interactionHeight = 3.0f)
        {
            Points = new List<PolygonPoint>();
            InteractionHeight = interactionHeight;
        }

        public void AddPoint(float x, float y, float z)
        {
            Points.Add(new PolygonPoint(x, y, z));
        }

        public void Clear()
        {
            Points.Clear();
        }

        public bool IsPointInside(float x, float y, float z)
        {
            if (Points.Count < 3) return false;

            if (Math.Abs(z - Points[0].Z) > InteractionHeight)
                return false;

            bool oddNodes = false;
            int j = Points.Count - 1;

            for (int i = 0; i < Points.Count; i++)
            {
                if ((Points[i].Y < y && Points[j].Y >= y) || (Points[j].Y < y && Points[i].Y >= y))
                {
                    if (Points[i].X + (y - Points[i].Y) / (Points[j].Y - Points[i].Y) * (Points[j].X - Points[i].X) < x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }

        public bool IsPointInside(Vector3 position)
        {
            return IsPointInside(position.X, position.Y, position.Z);
        }
    }

    public static class PolygonManager
    {
        private const float PI = 3.14159265358979323846f;

        public static Polygon CreateCircularPolygon(float centerX, float centerY, float centerZ, float radius, int points = 8, float interactionHeight = 3.0f)
        {
            var polygon = new Polygon(interactionHeight);
            float angleStep = 2.0f * PI / points;
            float angle = 0.0f;

            for (int i = 0; i < points; i++)
            {
                float x = centerX + radius * (float)Math.Cos(angle);
                float y = centerY + radius * (float)Math.Sin(angle);
                polygon.AddPoint(x, y, centerZ);
                angle += angleStep;
            }

            return polygon;
        }

        public static Polygon CreateRectangularPolygon(float centerX, float centerY, float centerZ, float width, float height, float interactionHeight = 3.0f)
        {
            var polygon = new Polygon(interactionHeight);
            float halfWidth = width / 2.0f;
            float halfHeight = height / 2.0f;

            polygon.AddPoint(centerX - halfWidth, centerY - halfHeight, centerZ);
            polygon.AddPoint(centerX + halfWidth, centerY - halfHeight, centerZ);
            polygon.AddPoint(centerX + halfWidth, centerY + halfHeight, centerZ);
            polygon.AddPoint(centerX - halfWidth, centerY + halfHeight, centerZ);

            return polygon;
        }

        public static Polygon CreateCustomPolygon(List<Vector3> points, float interactionHeight = 3.0f)
        {
            var polygon = new Polygon(interactionHeight);
            foreach (var point in points)
            {
                polygon.AddPoint(point.X, point.Y, point.Z);
            }
            return polygon;
        }
    }
}