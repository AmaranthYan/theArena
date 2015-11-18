using UnityEngine;
using System.Collections;

public class PowerMenu : StandardMenuUI {
	private enum Items {
		PowerButton = 0
	};

	[SerializeField]
	private WeaponAssembler weaponAssembler;
	[SerializeField]
	private GameObject weaponAssemblyMenu;
	
	protected override void TriggerUIEvent() {
		if (focus == -1)
			return;
		if (Input.GetMouseButtonDown(0)) {
			focusableUIObjects[focus].TriggerSelectEvent(true);
			Items item = (Items)focus;
			switch (item) {
			case Items.PowerButton :
				gameObject.SetActive(false);
				weaponAssemblyMenu.SetActive(true);
				break;
			}
		}
	}
}
