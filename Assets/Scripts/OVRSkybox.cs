using UnityEngine;
using System.Collections;

public class OVRSkybox : MonoBehaviour {
	private Camera cameraLeft, cameraRight = null;

	void Awake() {
		Camera[] cameras = gameObject.GetComponentsInChildren<Camera>();
		for (int i = 0;i < cameras.Length;i++) {
			if (cameras[i].name == "CameraLeft")
				cameraLeft = cameras[i];
			if (cameras[i].name == "CameraRight")
				cameraRight = cameras[i];
		}
		if (cameraLeft.GetComponent<Skybox>() == null)
			cameraLeft.gameObject.AddComponent<Skybox>();
		if (cameraRight.GetComponent<Skybox>() == null)
			cameraRight.gameObject.AddComponent<Skybox>();
	}
	
	public void EnableSkybox(bool isEnabled) {
		Skybox skyboxLeft = cameraLeft.GetComponent<Skybox>();
		Skybox skyboxRight = cameraRight.GetComponent<Skybox>();
		if (isEnabled) {
			skyboxLeft.enabled = true;
			skyboxRight.enabled = true;
		} else {
			skyboxLeft.enabled = false;
			skyboxRight.enabled = false;
		}
	}
}
