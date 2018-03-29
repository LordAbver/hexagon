// Decompile from assembly: Assembly-CSharp.dll

using System;
using UnityEngine;

public class CameraCircle : MonoBehaviour
{
	private Vector3 center;

	private int radius = 10;

	private float height = 10f;

	private void Start()
	{
		this.center = new Vector3(32f, 0f, 34f);
		base.transform.position = new Vector3(this.center.x + (float)this.radius, this.height, this.center.z);
	}

	private void Update()
	{
		base.transform.Translate(Vector3.left * Time.deltaTime * 10f);
		base.transform.LookAt(this.center, Vector3.up);
	}
}
