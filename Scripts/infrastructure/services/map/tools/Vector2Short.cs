using System;
using UnityEngine;

[Serializable]
public struct Vector2Short
{
    public short X;
    public short Y;

    public Vector2Short(short x, short y)
    {
        X = x;
        Y = y;
    }
    
    public Vector2Short(int x, int y)
    {
        X = (short)x;
        Y = (short)y;
    }
        
    public static Vector2Short operator +(Vector2Short a, Vector2Short b)
    {
        return new Vector2Short((short)(a.X + b.X), (short)(a.Y + b.Y));
    }
        
    public static Vector2Short operator -(Vector2Short a, Vector2Short b)
    {
        return new Vector2Short((short)(a.X - b.X), (short)(a.Y - b.Y));
    }
    
    public static bool operator ==(Vector2Short a, Vector2Short b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Vector2Short a, Vector2Short b)
    {
        return !(a == b);
    }
    
    public override bool Equals(object obj)
    {
        if (obj is not Vector2Short other) return false;

        return X == other.X && Y == other.Y;
    }
    
    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }

    public override string ToString()
    {
        return $"X: {X}, Y: {Y}";
    }
    
    public readonly Vector2 ToUnityVector()
    {
        return new Vector2(X, Y);
    }
}