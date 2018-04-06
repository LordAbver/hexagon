using UnityEngine;

public class TerrainPreview : MonoBehaviour
{

    public Sprite[] TerrainSprites;

    void Awake()
    {
        TerrainSprites = Resources.LoadAll<Sprite>("Ui\\Terrain");
    }
}
