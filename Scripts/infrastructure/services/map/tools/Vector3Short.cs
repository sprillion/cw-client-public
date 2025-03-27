using System;
using UnityEngine;

[Serializable]
public struct Vector3Short
{
    public short X;
    public short Y;
    public short Z;

    public Vector3Short(short x, short y, short z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public Vector3Short(int x, int y, int z)
    {
        X = (short)x;
        Y = (short)y;
        Z = (short)z;
    }

    public Vector3Short(Vector3Int vector3Int)
    {
        X = (short)vector3Int.x;
        Y = (short)vector3Int.y;
        Z = (short)vector3Int.z;
    }
    
    public override bool Equals(object obj)
    {
        if (obj is not Vector3Short other) return false;

        return X == other.X && Y == other.Y && Z == other.Z;
    }
    
    public static Vector3 operator +(Vector3 b, Vector3Short a)
    {
        return new Vector3(a.X + b.x, a.Y + b.y, a.Z + b.z);
    }
    
    public static Vector3Short operator +(Vector3Short a, Vector3Short b)
    {
        return new Vector3Short((short)(a.X + b.X), (short)(a.Y + b.Y), (short)(a.Z + b.Z));
    }
    
    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
    }
    
    public override string ToString()
    {
        return $"X: {X}, Y: {Y}, Z: {Z}";
    }
}