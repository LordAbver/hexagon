using UnityEngine;
using System;

[Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private Int32 _x, _z;

    public int X
    {
        get
        {
            return _x;
        }
    }

    public int Z
    {
        get
        {
            return _z;
        }
    }

    public int Y
    {
        get
        {
            return -_x - _z;
        }
    }

    public HexCoordinates(int x, int z)
    {
       _x = x;
       _z = z;
    }

    public static HexCoordinates FromOffsetCoordinates(Int32 x, Int32 z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    public static HexCoordinates FromPosition(Vector3 position)
    {
        var x = position.x / (HexMetrics.INNER_RADIUS * 2f);
        var y = -x;
        var offset = position.z / (HexMetrics.OUTER_RADIUS * 3f);
        x -= offset;
        y -= offset;

        var iX = Mathf.RoundToInt(x);
        var iY = Mathf.RoundToInt(y);
        var iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            var dX = Mathf.Abs(x - iX);
            var dY = Mathf.Abs(y - iY);
            var dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
    }

    public HexDirection GetRelatedDirection(HexCoordinates other)
    {
        if (other.Z > Z && other.Y == Y)
            return HexDirection.NW;

        if (other.Z > Z && other.Y < Y)
            return HexDirection.NE;

        if (other.Z == Z && other.X < X)
            return HexDirection.W;

        if (other.Z == Z && other.X > X)
            return HexDirection.E;

        if (other.Z < Z && other.Y > Y)
            return HexDirection.SW;

        if (other.Z < Z && other.Y == Y)
            return HexDirection.SE;

        return HexDirection.SE; //Default state
    }

    public int DistanceTo(HexCoordinates other)
    {
        return 
            ((X < other.X ? other.X - X : X - other.X) +
            (Y < other.Y ? other.Y - Y : Y - other.Y) +
            (Z < other.Z ? other.Z - Z : Z - other.Z)) / 2;
    }

    public override string ToString()
    {
        return String.Format("({0},{1},{2})", X, Y, Z);
    }

    public string ToStringOnSeparateLines()
    {
        return String.Format("{0} \n {1} \n {2}", X, Y, Z);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == typeof(HexCoordinates) && Equals((HexCoordinates)obj);
    }

    public bool Equals(HexCoordinates other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(other.X, X) && Equals(other.Y, Y) && Equals(other.Z, Z);
    }

    public static bool operator ==(HexCoordinates a, HexCoordinates b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(HexCoordinates a, HexCoordinates b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X.GetHashCode() * 397) ^ (Y.GetHashCode() * 397) ^ (Z.GetHashCode() * 397);
        }
    }
}