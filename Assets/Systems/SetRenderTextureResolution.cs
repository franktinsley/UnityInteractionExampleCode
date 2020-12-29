using UnityEngine;
using UnityEngine.UI;

public class SetRenderTextureResolution : MonoBehaviour {
	
	public Camera renderCamera;
	public RawImage cameraFeed;

	void Start() {
		var renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBHalf);
		renderCamera.targetTexture = renderTexture;
		cameraFeed.texture = renderTexture;
	}
}
