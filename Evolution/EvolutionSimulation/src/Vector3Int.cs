using System;

namespace EvolutionSimulation
{
    public class Vector3Int : IEquatable<Vector3Int>
    {        
        public Vector3Int() { x = y = z = 0; }
        public Vector3Int(int xv, int yv, int zv) { x = xv; y = yv; z = zv; }
        public Vector3Int(Vector3Int v) { x = v.x; y = v.y; z = v.z; }

        public int x, y, z;

        public static Vector3Int operator +(Vector3Int v1, Vector3Int v2) { return new Vector3Int(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z); }
        public static Vector3Int operator -(Vector3Int v1, Vector3Int v2) { return new Vector3Int(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z); }
        public static Vector3Int operator *(Vector3Int v1, Vector3Int v2) { return new Vector3Int(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z); }
        public static Vector3Int operator *(Vector3Int v1, int v) { return new Vector3Int(v1.x * v, v1.y * v, v1.z* v); }
        public static Vector3Int operator *(Vector3Int v1, float v) { return new Vector3Int((int)(v1.x * v),(int)( v1.y * v), (int)(v1.z * v)); }
        public static Vector3Int operator /(Vector3Int v1, Vector3Int v2) { return new Vector3Int(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z); }
        public static bool operator ==(Vector3Int v1, Vector3Int v2) 
        {
            if (v1 is null || v2 is null)
                return v1 is null && v2 is null;
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }
        public static bool operator !=(Vector3Int v1, Vector3Int v2) { return !(v1 == v2); }

        public override bool Equals(object obj) { return Equals(obj as Vector3Int); }
        public virtual bool Equals(Vector3Int obj) { return this == obj; }
        public override int GetHashCode() { return base.GetHashCode(); }


        public override string ToString()
        {
            return "< " + x + ", " + y + ", " + z + " >";
        }
    }
}
