using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RendererAlpha : MonoBehaviour {
	
	[Range(0f, 1f)]
	public float rendererAlpha = 1f;

	new Renderer renderer;

	void Start() {
		renderer = GetComponent<Renderer>();
	}

	void Update() {
		var currentColor = renderer.material.GetColor("_Color");
		var newColor = new Color(currentColor.r, currentColor.g, currentColor.b, rendererAlpha);
		renderer.material.SetColor("_Color", newColor);
	}
}
