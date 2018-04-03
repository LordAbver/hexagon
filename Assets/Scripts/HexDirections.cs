using System;

public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

public static class HexDirectionExtensions
{

    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }

    public static String GetName(this HexDirection direction)
    {
        switch (direction)
        {
            case HexDirection.NW:
                return "NorthWest";
            case HexDirection.NE:
                return "NorthEast";
            case HexDirection.W:
                return "West";
            case HexDirection.E:
                return "East";
            case HexDirection.SW:
                return "SouthWest";
            case HexDirection.SE:
                return "SouthEast";
            default:
                return "SouthEast";
        }
    }
}