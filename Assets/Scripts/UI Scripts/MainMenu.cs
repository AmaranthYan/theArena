using UnityEngine;
using System.Collections;

public class MainMenu : StandardMenuUI {
	private enum Items {
		Start = 0, Options, About
	};

	[SerializeField]
	private GameObject optionMenu;
	[SerializeField]
	private GameObject aboutMenu;
	
	protected override void TriggerUIEvent() {
		if (focus == -1)
			return;
		if (Input.GetMouseButtonDown(0)) {
			focusableUIObjects[focus].TriggerSelectEvent(true);
			Items item = (Items)focus;
			switch (item) {
			case Items.Start :
				Application.LoadLevel("AssemblyWorkshop");
				break;
			case Items.Options :
				gameObject.SetActive(false);
				optionMenu.SetActive(true);
				break;
			case Items.About :
				gameObject.SetActive(false);
				aboutMenu.SetActive(true);
				break;
			}
		}
	}
}
