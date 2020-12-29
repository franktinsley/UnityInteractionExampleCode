#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;


public class SaveCurrentFrame : MonoBehaviour {

	public bool saveRenderNow;

	bool saveRender;

	#if UNITY_EDITOR

	public void SaveRender() {
		saveRender = true;
	}

	void Update() {
		if (saveRenderNow) {
			saveRenderNow = false;
			saveRender = true;
		}
	}

	void OnPostRender() {
		if (saveRender) {
			saveRender = false;
			Save(Render());
		}
	}

	void Save(Texture2D texture2D) {
		var bytes = texture2D.EncodeToPNG();
		var path = EditorUtility.SaveFilePanel("Save rendered frame as PNG", "", "render.png", "png");
		File.WriteAllBytes(path, bytes);
	}

	Texture2D Render() {
		var renderTexture = GetComponent<Camera>().targetTexture;
		var width = renderTexture.width;
		var height = renderTexture.height;
		var render = new Texture2D(width, height, TextureFormat.ARGB32, false);
		render.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		render.Apply();
		return render;
	}
	
	#endif
}
