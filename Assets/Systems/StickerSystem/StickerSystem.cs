using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StickerSystem : MonoBehaviour {

	public GameObject futureWorldStickerPicker;
	public GameObject finalSpaceStickerPicker;
	public GameObject snlStickerPicker;
	public GameObject shoppingStickerPicker;
	public GameObject everythingStickerPicker;
	public GameObject surfboardConfigurator;
	public GameObject deleteButton;
	public GameObject pickerToggleButton;
	public GameObject cameraViewButton;
	public Toggle pickerToggle;
	public RectTransform deleteButtonRectTransform;
	public Image deleteButtonDot;
	public float maxRayDistance = 1000f;
	public LayerMask stickerLayer = 1 << 8;
	public LayerMask arKitPlaneLayer = 1 << 10;
	public Mask arCameraFeedMask;
	public Transform overworldCameraTransform;
	public Transform directionalLightTransform;
	public Vector3 overworldCameraTweenEndPosition;
	public Vector3 directionalLightTweenEndRotation;
	public float showMainTableTweenTime = 0.4f;
	public float setSunTweenTime = 2f;
	public AnimationCurve showMainTableTweenCurve;
	public AnimationCurve setSunTweenCurve;
	public Camera arCamera;
	public Camera overworldCamera;
	public GameObject futureWorldOverlay;
	public GameObject finalSpaceOverlay;
	public GameObject snlOverlay;
	public GameObject surfboardOverlay;
	public GameObject futureWorldStickerParent;
	public GameObject finalSpaceStickerParent;
	public GameObject snlStickerParent;
	public GameObject shoppingStickerParent;
	public GameObject everythingStickerParent;
	public GameObject surfboardParent;
	public GameObject normalPostProcessVolume;
	public GameObject vaporGemsPostProcessVolume;
	public GameObject pickerToggleButtonGO;
	public string arDetectedPlaneTag = "ARPlane";
	public GameObject surfboard;
	public TouchCollider surfboardTouchCollider;
	public Renderer surfboardRenderer;
	public Transform lighting;
	public Transform resetLightingTarget;
	public Mask screenCornerMask;
	
	TouchCollider selectedTouchCollider;
	Vector3 screenCenter;
	Vector3 lastHitPoint;
	Vector2 touchOffset;
	GoTween overworldCameraTween;
	GoTween directionalLightTween;
	int draggingFingerId = -1;
	bool wasTwoFinger;
	bool _manipulating;
	bool manipulating {
		get {
			return _manipulating;
		}
        set {
			if (selectedTouchCollider) {
				selectedTouchCollider.colliderRenderer.enabled = value;
			}
			deleteButton.SetActive(value);
			pickerToggleButton.SetActive(!value);
			_manipulating = value;
        }
    }
	GameObject currentStickerParent;
	GameObject currentStickerPicker;
	GameObject currentOverlay;
	GameObject currentPostProcessVolume;

	const string stickerTag = "Sticker";

    void Start() {
		Application.targetFrameRate = 60;
		currentStickerParent = futureWorldStickerParent;
		currentStickerPicker = futureWorldStickerPicker;
		currentPostProcessVolume = normalPostProcessVolume;
		currentOverlay = futureWorldOverlay;
		screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
		var pinchRecognizer = new TKPinchRecognizer();
		pinchRecognizer.gestureRecognizedEvent += OnPinchRecognized;
		pinchRecognizer.gestureCompleteEvent += OnGestureComplete;
		TouchKit.addGestureRecognizer(pinchRecognizer);
		var rotationRecognizer = new TKRotationRecognizer();
		rotationRecognizer.gestureRecognizedEvent += OnRotationRecognized;
		TouchKit.addGestureRecognizer(rotationRecognizer);
		var tapRecognizer = new TKTapRecognizer();
		tapRecognizer.gestureRecognizedEvent += OnTapRecognized;
		TouchKit.addGestureRecognizer(tapRecognizer);
		SetupShowMainTableTweens();
	}
    
	void Update() {
		if (Input.touchCount > 0) {
			if (Input.touchCount < 2) {
				var fingerId = Input.touches[0].fingerId;
				switch (Input.touches[0].phase) {
					case TouchPhase.Began:
					RaycastHit stickerHit;
					var ray = arCamera.ScreenPointToRay(Input.touches[0].position);
					if (Physics.Raycast(ray, out stickerHit, maxRayDistance, stickerLayer)) {
						selectedTouchCollider = stickerHit.transform.GetComponent<TouchCollider>();
						draggingFingerId = fingerId;
						touchOffset = Input.touches[0].position - RemoveZVector(arCamera.WorldToScreenPoint(selectedTouchCollider.sticker.position));
					}
					ray = arCamera.ScreenPointToRay(Input.touches[0].position - touchOffset);
					var planeHits = Physics.RaycastAll(ray, maxRayDistance, arKitPlaneLayer);
					RaycastHit? highestPriorityHit = null;
					foreach (var planeHit in planeHits) {
						highestPriorityHit = planeHit;
						if (planeHit.transform.tag == arDetectedPlaneTag) {
							break;
						}
					}
					if (highestPriorityHit.HasValue) {
						lastHitPoint = highestPriorityHit.Value.point;
					}
					break;
					case TouchPhase.Moved:
					case TouchPhase.Stationary:
					ray = arCamera.ScreenPointToRay(Input.touches[0].position - touchOffset);
					planeHits = Physics.RaycastAll(ray, maxRayDistance, arKitPlaneLayer);
					highestPriorityHit = null;
					foreach (var planeHit in planeHits) {
						highestPriorityHit = planeHit;
						if (planeHit.transform.tag == arDetectedPlaneTag) {
							break;
						}
					}
					if (draggingFingerId == fingerId && selectedTouchCollider && highestPriorityHit.HasValue) {
						var hitPoint = highestPriorityHit.Value.point;
						if (!wasTwoFinger) {
							selectedTouchCollider.sticker.Translate((hitPoint - lastHitPoint), Space.World);
						}
						lastHitPoint = hitPoint;
						manipulating = true;
						wasTwoFinger = false;
					}
					break;
				}
			}
		} else {
			draggingFingerId = -1;
			if (selectedTouchCollider) {
				manipulating = false;
			}
		}
	}

    public void OnPickerToggle(bool on) {
        currentStickerPicker.SetActive(on);
    }

    public void OnClickStickerButton(GameObject stickerPrefab) {
        pickerToggle.isOn = false;
		AddNew(stickerPrefab);
    }

	public void OnClickCameraView() {
		HideMainTable();
	}

	void OnPinchRecognized(TKPinchRecognizer pinchRecognizer) {
		wasTwoFinger = true;
		if (selectedTouchCollider && selectedTouchCollider.scalable) {
			selectedTouchCollider.sticker.localScale += Vector3.one * pinchRecognizer.deltaScale * 0.5f;
			manipulating = true;
		}
	}

	void OnRotationRecognized(TKRotationRecognizer rotationRecognizer) {
		wasTwoFinger = true;
		if (selectedTouchCollider) {
			selectedTouchCollider.sticker.Rotate(Vector3.up, rotationRecognizer.deltaRotation * 2.0f);
			manipulating = true;
		}
	}

	void OnGestureComplete(TKAbstractGestureRecognizer _) {
		if (Input.touchCount < 1 && selectedTouchCollider) {
			manipulating = false;
		}
	}

	void OnTapRecognized(TKAbstractGestureRecognizer _) {
		if (currentStickerParent == surfboardParent && !surfboardRenderer.isVisible) {
			ResetSurfboard();
		}
	}

	public void OnPointerEnterDeleteButton(BaseEventData pointerEventData) {
		deleteButtonDot.enabled = true;
		((PointerEventData)pointerEventData).pointerPress = deleteButton;
	}

	public void OnPointerExitDeleteButton(BaseEventData pointerEventData) {
		deleteButtonDot.enabled = false;
		((PointerEventData)pointerEventData).pointerPress = null;
	}

	public void OnPointerUpDeleteButton() {
		if (selectedTouchCollider) {
			Destroy(selectedTouchCollider.sticker.gameObject);
			deleteButtonDot.enabled = false;
			selectedTouchCollider = null;
			manipulating = false;
		}
	}

	public void OnClickMainTableButton() {
		ShowMainTable();
	}

	public void OnClickFutureWorldCell() {
		HideMainTable();
		ChangeARE(futureWorldStickerPicker, futureWorldStickerParent, normalPostProcessVolume, null, true);
	}

	public void OnClickFinalSpaceCell() {
		HideMainTable();
		ChangeARE(finalSpaceStickerPicker, finalSpaceStickerParent, normalPostProcessVolume, finalSpaceOverlay, true);
	}

	public void OnClickSevenSurfboardsCell() {
		HideMainTable();
		ChangeARE(surfboardConfigurator, surfboardParent, normalPostProcessVolume, surfboardOverlay, false);
		ResetSurfboard();
	}

	void ResetSurfboard() {
		ResetLightingRotation();
		selectedTouchCollider = surfboardTouchCollider;
		var sticker = surfboardTouchCollider.sticker;
		var ray = arCamera.ScreenPointToRay(screenCenter);
		var hits = Physics.RaycastAll(ray, maxRayDistance, arKitPlaneLayer);
		RaycastHit? highestPriorityHit = null;
		foreach (var hit in hits) {
			highestPriorityHit = hit;
			if (hit.transform.tag == arDetectedPlaneTag) {
				break;
			}
		}
		if (highestPriorityHit.HasValue) {
			sticker.position = highestPriorityHit.Value.point;
			var arCameraPosition = arCamera.transform.position;
			sticker.LookAt(new Vector3(arCameraPosition.x, sticker.position.y, arCameraPosition.z));
		}
	}

	public void OnClickVaporGemsCell() {
		HideMainTable();
		ChangeARE(null, null, vaporGemsPostProcessVolume, null, false);
	}

	public void OnClickSNLCell() {
		HideMainTable();
		ChangeARE(snlStickerPicker, snlStickerParent, normalPostProcessVolume, snlOverlay, true);
	}

	public void OnClickShoppingCell() {
		HideMainTable();
		ChangeARE(shoppingStickerPicker, shoppingStickerParent, normalPostProcessVolume, null, true);
	}

	public void OnClickEverythingCell() {
		HideMainTable();
		ChangeARE(everythingStickerPicker, everythingStickerParent, normalPostProcessVolume, null, true);
	}

	public void ChangeARE(GameObject stickerPicker, GameObject stickerParent, GameObject postProcessVolume, GameObject overlay, bool pickerToggleActive) {
		currentStickerPicker = stickerPicker;
		ChangeActiveGameObject(ref currentStickerParent, stickerParent);
		ChangeActiveGameObject(ref currentPostProcessVolume, postProcessVolume);
		ChangeActiveGameObject(ref currentOverlay, overlay);
		pickerToggleButtonGO.SetActive(pickerToggleActive);
	}

	void ResetLightingRotation() {
		lighting.position = resetLightingTarget.position;
		lighting.rotation = resetLightingTarget.rotation;
	}

	void ChangeActiveGameObject(ref GameObject currentGameObject, GameObject newGameObject) {
		if (currentGameObject) {
			currentGameObject.SetActive(false);
		}
		currentGameObject = newGameObject;
		if (currentGameObject) {
			currentGameObject.SetActive(true);
		}
	}

	public void OnClickOverworld() {
		if (overworldCamera.clearFlags == CameraClearFlags.Skybox) {
			overworldCamera.clearFlags = CameraClearFlags.SolidColor;
		} else {
			overworldCamera.clearFlags = CameraClearFlags.Skybox;
		}
	}

	public void OnClickSurfboardColorButton(Texture2D texture) {
		surfboardRenderer.material.mainTexture = texture;
	}

	void ShowMainTable() {
		overworldCameraTween.playForward();
		cameraViewButton.SetActive(true);
		pickerToggle.isOn = false;
		screenCornerMask.enabled = true;
		//directionalLightTween.playForward();
	}

	void HideMainTable() {
		overworldCameraTween.playBackwards();
		cameraViewButton.SetActive(false);
		//directionalLightTween.playBackwards();
	}

	void AddNew(GameObject stickerPrefab) {
		if (selectedTouchCollider) {
			manipulating = false;
		}
		selectedTouchCollider = Instantiate(stickerPrefab).GetComponentInChildren<TouchCollider>();
		var sticker = selectedTouchCollider.sticker;
		selectedTouchCollider.sticker.parent = currentStickerParent.transform;
		var ray = arCamera.ScreenPointToRay(screenCenter);
		var hits = Physics.RaycastAll(ray, maxRayDistance, arKitPlaneLayer);
		RaycastHit? highestPriorityHit = null;
		foreach (var hit in hits) {
			highestPriorityHit = hit;
			if (hit.transform.tag == arDetectedPlaneTag) {
				break;
			}
		}
		if (highestPriorityHit.HasValue) {
			sticker.position = highestPriorityHit.Value.point;
			var arCameraPosition = arCamera.transform.position;
			sticker.LookAt(new Vector3(arCameraPosition.x, sticker.position.y, arCameraPosition.z));
		}
	}

	void SetupShowMainTableTweens() {
		Go.defaultEaseType = GoEaseType.AnimationCurve;
		var overworldCameraPositionProperty = new PositionTweenProperty(overworldCameraTweenEndPosition, false, true);
		var overworldCameraTweenConfig = new GoTweenConfig();
		overworldCameraTweenConfig.addTweenProperty(overworldCameraPositionProperty);
		overworldCameraTweenConfig.startPaused();
		overworldCameraTweenConfig.easeCurve = showMainTableTweenCurve;
		overworldCameraTweenConfig.onCompleteHandler = OnOverworldCameraTweenComplete;
		overworldCameraTween = new GoTween(overworldCameraTransform, showMainTableTweenTime, overworldCameraTweenConfig);
		overworldCameraTween.autoRemoveOnComplete = false;
		Go.addTween(overworldCameraTween);
		var directionalLightRotationProperty = new RotationTweenProperty(directionalLightTweenEndRotation);
		var directionalLightTweenConfig = new GoTweenConfig();
		directionalLightTweenConfig.addTweenProperty(directionalLightRotationProperty);
		directionalLightTweenConfig.startPaused();
		directionalLightTweenConfig.easeCurve = setSunTweenCurve;
		directionalLightTween = new GoTween(directionalLightTransform, setSunTweenTime, directionalLightTweenConfig);
		directionalLightTween.autoRemoveOnComplete = false;
		Go.addTween(directionalLightTween);
	}

	void OnOverworldCameraTweenComplete(AbstractGoTween tween) {
		if (tween.isReversed) {
			screenCornerMask.enabled = false;
		}
	}

	Vector3 AddZeroZVector(Vector2 vector2) {
		return new Vector3(vector2.x, vector2.y, 0f);
	}

	Vector2 RemoveZVector(Vector3 vector3) {
		return new Vector2(vector3.x, vector3.y);
	}
}
