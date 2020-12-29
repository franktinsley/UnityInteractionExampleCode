using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSystem : MonoBehaviour {
	
	public string arSessionSceneName;
	public string contentSceneName;
	public string spinningWheelName;

	GameObject spinningWheel;

	void Start() {
		SceneManager.LoadScene(arSessionSceneName);
		SceneManager.LoadSceneAsync(contentSceneName);
		SceneManager.sceneLoaded += SceneLoadedHandler;
	}

	void OnDestroy() {
		SceneManager.sceneLoaded -= SceneLoadedHandler;
	}

	void SceneLoadedHandler(Scene scene, LoadSceneMode loadMode) {
		if (scene.name == arSessionSceneName) {
			spinningWheel = GameObject.Find(spinningWheelName);
		} else if (scene.name == contentSceneName) {
			spinningWheel.SetActive(false);
		}
	}
}
