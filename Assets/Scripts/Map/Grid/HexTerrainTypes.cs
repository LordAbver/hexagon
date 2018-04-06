using System;
using System.Collections.Generic;

public enum HexTerrainTypes
{
    Flat = 2,
    Hill = 4,
    Lake = 5,
    Road = 7,
    Forest = 3
}

public static class HexTerrainExtentions{
    public static String GetName(this HexTerrainTypes type)
    {
        switch (type)
        {
            case HexTerrainTypes.Flat:
                return "Flat";
            case HexTerrainTypes.Hill:
                return "Hill";
            case HexTerrainTypes.Lake:
                return "Lake";
            case HexTerrainTypes.Road:
                return "Road";
            case HexTerrainTypes.Forest:
                return "Forest";
            default:
                return "Flat";
        }
    }

}
