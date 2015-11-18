using UnityEngine;
using System.Collections;

public class Sight : WeaponPart {
	protected static float unitFoV;
	protected Camera targetOVRCamera;
	[SerializeField]
	protected Camera sightOVRCamera;
	protected Skybox skybox;
	[SerializeField]
	protected Transform eyePosition;
	protected float currentMagnificationRatio;
	protected float targetMagnificationRatio;
	[Range(1.0f, 100.0f)]
	public float minMagnificationRatio;
	[Range(1.0f, 100.0f)]
	public float maxMagnificationRatio;
	public float zoomInterval;
	public float zoomVelocity;

	public Transform EyePosition {
		get {
			return eyePosition;
		}
	}

	void Awake() {
		unitFoV = OVRDevice.VerticalFOV();
		targetOVRCamera = null;
		skybox = sightOVRCamera.GetComponent<Skybox>();
		maxMagnificationRatio = minMagnificationRatio > maxMagnificationRatio ? minMagnificationRatio : maxMagnificationRatio;
	}

	void Start() {
		currentMagnificationRatio = minMagnificationRatio;
		targetMagnificationRatio = currentMagnificationRatio;
		UpdateVerticalFoV();
	}

	void Update() {
		if (currentMagnificationRatio != targetMagnificationRatio) {
			UpdateCurrentMagnificationRatio();
			UpdateVerticalFoV();
		}
	}

	protected void UpdateCurrentMagnificationRatio() {
		float delta = Time.deltaTime * zoomVelocity;
		if (Mathf.Abs(targetMagnificationRatio - currentMagnificationRatio) <= delta)
			currentMagnificationRatio = targetMagnificationRatio;
		else
			currentMagnificationRatio += targetMagnificationRatio > currentMagnificationRatio ? delta : -delta;
	}

	protected void UpdateVerticalFoV() {
		sightOVRCamera.GetComponent<SightOVRCamera>().SetVerticalFOV(
			ConvertMagnificationRatioToVerticalFoV(currentMagnificationRatio));
	}

	protected float ConvertMagnificationRatioToVerticalFoV(float multiplierFactor) {
		return Mathf.Atan(Mathf.Sqrt(1 / multiplierFactor) * Mathf.Tan(unitFoV / 2 * Mathf.Deg2Rad)) * 2 * Mathf.Rad2Deg;
	}

	public float SightEyeAngle() {
		Quaternion eyeRotation = Quaternion.identity;
		Quaternion sightRotation = Quaternion.identity;
		if (targetOVRCamera != null)
			eyeRotation = targetOVRCamera.transform.rotation;
		if (sightOVRCamera != null)
			sightRotation = sightOVRCamera.transform.rotation;
		return Quaternion.Angle(eyeRotation, sightRotation);
	}
	
	public void Zoom(float z) {
		targetMagnificationRatio += z * zoomInterval;
		if (targetMagnificationRatio < minMagnificationRatio)
			targetMagnificationRatio = minMagnificationRatio;
		if (targetMagnificationRatio > maxMagnificationRatio)
			targetMagnificationRatio = maxMagnificationRatio;
	}

	public void EnableAimAt(Camera OVRCamera) {
		if (sightOVRCamera == null)
			return;
		if (targetOVRCamera != null)
			return;
		targetOVRCamera = OVRCamera;
		Skybox targetSkybox = OVRCamera.GetComponent<Skybox>();
		skybox.material = targetSkybox != null ? targetSkybox.material : null;
		sightOVRCamera.rect = targetOVRCamera.rect;
		sightOVRCamera.enabled = true;
	}

	public void DisableAim() {
		if (sightOVRCamera == null)
			return;
		sightOVRCamera.enabled = false;
		skybox.material = null;
		targetOVRCamera = null;
	}
}
