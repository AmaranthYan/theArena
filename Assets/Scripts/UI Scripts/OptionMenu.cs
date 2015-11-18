using UnityEngine;
using System.Collections;

public class OptionMenu : StandardMenuUI {
	private enum Items {
		LeftHand = 0, RightHand, Back
	};
	
	private int handedness;
	[SerializeField]
	private GameObject mainMenu;

	void OnEnable() {
		LoadConfig();
		Initialize();
	}

	void OnDisable() {
		SaveConfig();
	}

	private void LoadConfig() {
		Configuration.LoadConfig("handedness", out handedness);
		handedness = handedness == 0 ? 0 : 1;
	}

	private void SaveConfig() {
		Configuration.SaveConfig("handedness", handedness);
	}

	private void Initialize() {
		focusableUIObjects[handedness].IsSelected = true;
	}

	protected override void TriggerUIEvent() {
		if (focus == -1)
			return;
		if (Input.GetMouseButtonDown(0)) {
			//focusableUIObjects[focus].TriggerEvent();
			Items item = (Items)focus;
			switch (item) {
			case Items.LeftHand :
				handedness = (int)Items.LeftHand;
				focusableUIObjects[(int)Items.RightHand].IsSelected = false;
				focusableUIObjects[(int)Items.LeftHand].IsSelected = true;
				SaveConfig();
				break;
			case Items.RightHand :
				handedness = (int)Items.RightHand;
				focusableUIObjects[(int)Items.LeftHand].IsSelected = false;
				focusableUIObjects[(int)Items.RightHand].IsSelected = true;
				SaveConfig();
				break;
			case Items.Back :
				gameObject.SetActive(false);
				mainMenu.SetActive(true);
				break;
			}
		}
	}
}
