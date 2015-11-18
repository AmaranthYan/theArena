using UnityEngine;
using System.Collections;

public abstract class SceneInitializer : MonoBehaviour {
	protected const float SIGHT_DISTANCE = 2.5f;
	public Material sceneSkybox;
	[SerializeField]
	protected Material[] vacuumshaderMaterials;
	public float initialOffset;
	public float finalOffset;
	public float velocity = -2.0f;
	protected float offset;
	public float sceneLoadTime;

	protected virtual void Start () {
		LoadConfig();
		InitializeCamera();
		offset = initialOffset;
		UpdateOffset(initialOffset);
		Invoke("DisplaySkybox", sceneLoadTime);
	}
	

	protected virtual void Update () {
		if (offset > finalOffset) 
			offset += Time.deltaTime * velocity;
		else
			offset = finalOffset;
		UpdateOffset(offset);
	}

	protected virtual void OnDestroy() {
		UpdateOffset(initialOffset);
	}

	protected abstract void LoadConfig();

	protected virtual void InitializeSkybox() {
		if (sceneSkybox == null)
			return;
		Skybox[] skyboxes = Object.FindObjectsOfType<Skybox>();
		foreach (Skybox skybox in skyboxes) {
			skybox.material = sceneSkybox;
		}
	}

	protected virtual void InitializeCamera() {
		GameObject[] cameras = GameObject.FindGameObjectsWithTag("OVRCamera");
		foreach (GameObject c in cameras) {
			c.GetComponent<OVRCameraRayCast>().EyeSightDistance = SIGHT_DISTANCE;
		}
	}

	private void UpdateOffset(float offset) {
		foreach (Material m in vacuumshaderMaterials)
			m.SetFloat("_V_WIRE_GradientOffset", offset);
	}

	protected virtual void DisplaySkybox() {
		InitializeSkybox();
	}
}
