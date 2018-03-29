using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTerrain : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var mesh = GetComponent<MeshFilter>().mesh;
        var verts = mesh.vertices;
        var colors = mesh.colors;
        for(var i=0; i < colors.Length; i++)
        {
            //colors[i] = Color.red;
        }
        mesh.colors = colors;
        var search = new Vector3(transform.position.x + (10 / 2), transform.position.y, transform.position.z + (10 / 2));
        float xStep = 1;
        float zStep = 1;
        int squaresize = 11;
        for (int n = 0; n < squaresize; n++)
        {
            for (int i = 0; i < squaresize; i++)
            {
                verts[(n * squaresize) + i].y = Terrain.activeTerrain.SampleHeight(search);
                search.x -= xStep;
            }
            search.x += (((float)squaresize) * xStep);
            search.z -= zStep;
        }
        mesh.vertices = verts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
