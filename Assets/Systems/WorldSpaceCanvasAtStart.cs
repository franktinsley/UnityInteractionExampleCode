using UnityEngine;

public class WorldSpaceCanvasAtStart : MonoBehaviour {
	public Canvas canvas;

	void Start() {
		canvas.renderMode = RenderMode.WorldSpace;
	}
}
