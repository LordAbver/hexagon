// Decompile from assembly: Assembly-CSharp.dll

using System;
using UnityEngine;

public class FPS : MonoBehaviour
{
	public bool Show = true;

	private float deltaTime;

	private int nfps;

	private int fps;

	private void Update()
	{
		this.deltaTime += Time.deltaTime;
		if (this.deltaTime > 1f)
		{
			this.deltaTime -= 1f;
			this.fps = this.nfps;
			this.nfps = 0;
		}
		this.nfps++;
	}

	private void OnGUI()
	{
		if (this.Show)
		{
			int width = Screen.width;
			int height = Screen.height;
			GUIStyle gUIStyle = new GUIStyle();
			Rect position = new Rect(0f, 0f, (float)width, (float)(height * 2 / 100));
			gUIStyle.alignment = TextAnchor.UpperRight;
			gUIStyle.fontSize = height * 2 / 20;
			gUIStyle.normal.textColor = new Color(0f, 0f, 0.5f, 1f);
			string text = this.fps.ToString();
			GUI.Label(position, text, gUIStyle);
		}
	}
}
