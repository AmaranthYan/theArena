using UnityEngine;
using System.Collections;

public class DisplayLogo : MonoBehaviour {
	public float displayTime;

	void Start () {
		Invoke("LoadMainMenu", displayTime);	
	}

	private void LoadMainMenu() {
		Application.LoadLevel("MainMenu");
	}
}
