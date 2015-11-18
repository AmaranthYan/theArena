using UnityEngine;
using System.Collections;

[RequireComponent(typeof(OVRCameraController))]
public class OVRCameraRayCast : MonoBehaviour {
	private float eyeSightDistance = Mathf.Infinity;
	private FocusableObject currentFocus = null;

	public float EyeSightDistance {
		set {
			if (value > 0.0f)
				eyeSightDistance = value;
			else
				eyeSightDistance = Mathf.Infinity;
		}
	}

	void Update() {
		CheckEyeFocus();
	}

	private void CheckEyeFocus() {
		//以Camera为原点生成视线方向的射线判定
		OVRCameraController cameraController = this.GetComponent<OVRCameraController>();
		Vector3 headPosition = Vector3.zero;
		cameraController.GetCameraPosition(ref headPosition);
		Vector3 eyePosition = Vector3.zero;
		cameraController.GetEyeCenterPosition(ref eyePosition);
		eyePosition += headPosition;
		Vector3 sightOrientation = Vector3.zero;
		cameraController.GetCameraOrientationEulerAngles(ref sightOrientation);
		RaycastHit focus;
		Physics.Raycast(eyePosition, Quaternion.Euler(sightOrientation) * Vector3.forward, out focus, eyeSightDistance);
		Transform focusTransform = focus.transform;
		FocusableObject newFocus = focusTransform != null ? 
			focusTransform.GetComponent<FocusableObject>() : null;
		//如果NewFocus不是CurrentFocus,则更新CurrentFocus
		if (currentFocus != newFocus) {
			if (currentFocus != null) 
				currentFocus.IsFocused = false;
			currentFocus = newFocus;
			if (currentFocus != null)
				currentFocus.IsFocused = true;
		}
	}
}
