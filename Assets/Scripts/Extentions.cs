using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extentions {

    public static Vector3 SetY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, y, vector.z);
    }
}
