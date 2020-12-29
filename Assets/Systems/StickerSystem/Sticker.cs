using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticker : MonoBehaviour {

	public Sprite thumbnail;
	public GameObject lod0;
	public GameObject lod1;
	public GameObject lod2;
	public Renderer loadingLightRenderer;
	[Range(0f, 1f)]
	public float loadingLightAlpha = 0f;
	
	void Update() {
		var currentColor = loadingLightRenderer.material.GetColor("_Color");
		var newColor = new Color(currentColor.r, currentColor.g, currentColor.b, loadingLightAlpha);
		loadingLightRenderer.material.SetColor("_Color", newColor);
	}
}
