using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;

public class WorkshopInitializer : SceneInitializer {
	[SerializeField]
	private GameObject assemblerScreen;

	new void Start() {
		base.Start();
		Invoke("TurnOnAssemblerScreen", sceneLoadTime);
	}

	protected override void LoadConfig() {

	}

	private void TurnOnAssemblerScreen() {
		assemblerScreen.SetActive(true);
	}
}
