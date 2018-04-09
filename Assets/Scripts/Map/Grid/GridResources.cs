using UnityEngine;

public static class GridResources {

    public static Sprite[] HpNumberSprites;
    public static Sprite[] TeamNumberSprites;
    public static Sprite[] TerrainSprites;

    public static void LoadAll()
    {
        HpNumberSprites = Resources.LoadAll<Sprite>("Ui\\HpNumbers");
        TeamNumberSprites = Resources.LoadAll<Sprite>("Ui\\TeamNumbers");
        TerrainSprites = Resources.LoadAll<Sprite>("Ui\\Terrain");
    }
}
