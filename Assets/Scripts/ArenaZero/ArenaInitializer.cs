using UnityEngine;
using System.Collections;

public class ArenaInitializer : SceneInitializer {
	[SerializeField]
	protected GameObject playerController;
	public Transform playerRespawnPoint;
	private GameObject player;

	[SerializeField]
	protected GameObject inGameHUDPrefab;

	private int handedness = 1;

	new void Start() {
		player = GameObject.Instantiate(playerController, 
		                                playerRespawnPoint.position, 
		                                playerRespawnPoint.rotation) as GameObject;
		player.GetComponent<FPSPlayerController>().SetHaltUpdateMovement(true);
		Invoke("EnablePlayer", sceneLoadTime - 0.1f);
		base.Start();
	}
	
	protected override void LoadConfig() {
		Configuration.LoadConfig("handedness", out handedness);
		handedness = handedness == 0 ? 0 : 1;
	}
	
	protected override void DisplaySkybox() {
		base.DisplaySkybox();
		GameObject[] cameras = GameObject.FindGameObjectsWithTag("OVRCamera");
		foreach (GameObject c in cameras) {
			c.GetComponent<OVRSkybox>().EnableSkybox(true);
		}
	}

	private void EnablePlayer() {
		GameObject weapon = null;
		if (Workshop.assembledWeapons.Count > 0) {
			weapon = Workshop.assembledWeapons[0];
			weapon.SetActive(true);
			player.GetComponent<FPSPlayerController>().EquipeWeapon(weapon.GetComponent<Weapon>());
		}
		player.GetComponent<FPSPlayerController>().SetHaltUpdateMovement(false);

		//Enable InGame HUD
		Transform cameraTransform = player.GetComponentInChildren<OVRCameraController>().transform;
		GameObject inGameHUD = GameObject.Instantiate(inGameHUDPrefab) as GameObject;
		inGameHUD.transform.SetParent(cameraTransform);
		inGameHUD.transform.localPosition = Vector3.zero;
		inGameHUD.transform.localRotation = Quaternion.identity;
		inGameHUD.transform.localScale = handedness == 0 ? 
			new Vector3(-1, 1, 1) : 
			new Vector3(1, 1, 1);

		ConsumableObjectCounter ammoCounter = inGameHUD.transform.
			Find("Canvas/AmmoCounter").GetComponentInChildren<ConsumableObjectCounter>();
		weapon.GetComponent<Weapon>().AttachAmmoCounter = ammoCounter;

		TextView zoomIndicator = inGameHUD.transform.
			Find("Canvas/ZoomIndicator").GetComponent<TextView>();
		weapon.GetComponent<Weapon>().AttachZoomIndicator = zoomIndicator;
	}
}
