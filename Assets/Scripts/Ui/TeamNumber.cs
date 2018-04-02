using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamNumber : MonoBehaviour {

    public Sprite[] NumberSprites;

    void Awake()
    {
        NumberSprites = Resources.LoadAll<Sprite>("Ui\\Numbers");
    }	
}
