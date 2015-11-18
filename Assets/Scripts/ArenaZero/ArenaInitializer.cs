using UnityEngine;
using System.Collections;

public class ArenaInitializer : SceneInitializer {
	[SerializeField]
	protected GameObject playerController;
	public Transform playerRespawnPoint;
	private GameObject player;

	new void Start() {
		player = GameObject.Instantiate(playerController, 
		                                playerRespawnPoint.position, 
		                                playerRespawnPoint.rotation) as GameObject;
		player.GetComponent<FPSPlayerController>().SetHaltUpdateMovement(true);
		Invoke("EnablePlayer", sceneLoadTime - 0.1f);
		base.Start();
	}
	
	protected override void LoadConfig() {
		
	}
	
	protected override void DisplaySkybox() {
		base.DisplaySkybox();
		GameObject[] cameras = GameObject.FindGameObjectsWithTag("OVRCamera");
		foreach (GameObject c in cameras) {
			c.GetComponent<OVRSkybox>().EnableSkybox(true);
		}
	}

	private void EnablePlayer() {
		if (Workshop.assembledWeapons.Count > 0) {
			GameObject weapon = Workshop.assembledWeapons[0];
			weapon.SetActive(true);
			player.GetComponent<FPSPlayerController>().EquipeWeapon(weapon.GetComponent<Weapon>());
		}
		player.GetComponent<FPSPlayerController>().SetHaltUpdateMovement(false);
	}
}
