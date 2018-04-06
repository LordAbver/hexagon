using UnityEngine;

public class TeamNumber : MonoBehaviour {

    public Sprite[] NumberSprites;

    void Awake()
    {
        NumberSprites = Resources.LoadAll<Sprite>("Ui\\Numbers");
    }	
}
