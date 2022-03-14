﻿using System;

namespace EvolutionSimulation
{
    public class Vector2Int
    {
        static double PI = 3.14159265358979323846264338327950288;

        public Vector2Int() { x = y = 0; }
        public Vector2Int(int xv, int yv) { x = xv; y = yv; }

        public int x, y;

        public static Vector2Int operator +(Vector2Int v1, Vector2Int v2) { return new Vector2Int(v1.x + v2.x, v1.y + v2.y); }
        public static Vector2Int operator -(Vector2Int v1, Vector2Int v2) { return new Vector2Int(v1.x - v2.x, v1.y - v2.y); }
        public static Vector2Int operator *(Vector2Int v1, Vector2Int v2) { return new Vector2Int(v1.x * v2.x, v1.y * v2.y); }
        public static Vector2Int operator *(Vector2Int v1, int v) { return new Vector2Int(v1.x * v, v1.y * v); }
        public static Vector2Int operator /(Vector2Int v1, Vector2Int v2) { return new Vector2Int(v1.x / v2.x, v1.y / v2.y); }


        public double Magnitude() { return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)); }
        public double Angle() { return Math.Atan(y / x); }
        public double Angle(Vector2Int v)
        {
            double a2 = Math.Atan2(v.x, v.y);
            double a1 = Math.Atan2(x, y);
            double sign = a1 > a2 ? 1 : -1;
            double angle = a1 - a2;
            double K = -sign * PI * 2;
            angle = (Math.Abs(K + angle) < Math.Abs(angle)) ? K + angle : angle;
            return angle * 180f / PI;
        }
    }
}