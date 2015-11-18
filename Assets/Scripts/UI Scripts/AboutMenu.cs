using UnityEngine;
using System.Collections;

public class AboutMenu : StandardMenuUI {
	private enum Items {
		Back = 0
	};

	[SerializeField]
	private GameObject mainMenu;

	protected override void TriggerUIEvent() {
		if (focus == -1)
			return;
		if (Input.GetMouseButtonDown(0)) {
			focusableUIObjects[focus].TriggerSelectEvent(true);
			Items item = (Items)focus;
			switch (item) {
			case Items.Back :
				gameObject.SetActive(false);
				mainMenu.SetActive(true);
				break;
			}
		}
	}
}
