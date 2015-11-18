using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ComponentDetailMenu : HolographicMenuUI {
	private enum Items {
		Last = 0, Current, Next
	};

	[SerializeField]
	private WeaponAssembler weaponAssembler;
	[SerializeField]
	private Text componentName;
	[SerializeField]
	private Image projectionPanel;
	[SerializeField]
	private RawImage projectionPanelNA;
	[SerializeField]
	private RawImage projection;
	[SerializeField]
	private Camera projectionCamera;
	[SerializeField]
	private Image componentCategoryPanel;
	[SerializeField]
	private RawImage componentCategoryPanelNA;
	[SerializeField]
	private Text componentCategory;
	[SerializeField]
	private Text componentParameters;
	[SerializeField]
	private Text componentDescription;
	[SerializeField]
	private GameObject weaponAssemblyMenu;

	new void Update() {
		base.Update();
		//MouseScrollEvent
		int deltaWheel = (int)(Input.GetAxis("Mouse ScrollWheel") * -10);
		if (deltaWheel != 0)
			weaponAssembler.RotateComponentWheel(deltaWheel);
		WeaponAssembler.ComponentInfo cInfo = weaponAssembler.CurrentDisplayedComponentInfo;
		componentName.text = cInfo.name;
		if (cInfo.Equals(WeaponAssembler.NullComponent)) {
			projectionPanel.color = panelNonAvailableColor;
			projectionPanelNA.enabled = true;
			projection.enabled = false;
		} else {
			projectionPanel.color = panelAvailableColor;
			projectionPanelNA.enabled = false;
			projection.enabled = true;
		}
		if (cInfo.category == "") {
			componentCategoryPanel.color = panelNonAvailableColor;
			componentCategoryPanelNA.enabled = true;
			componentCategory.color = SallowGrey;
			componentCategory.text = "Not Available";	
		} else {
			componentCategoryPanel.color = panelAvailableColor;
			componentCategoryPanelNA.enabled = false;
			componentCategory.color = Yellow;
			componentCategory.text = cInfo.category;	
		}
		if (cInfo.parameters.Count > 0) {
			componentParameters.text = "";
			int index = 0;
			foreach (KeyValuePair<string, string> p in cInfo.parameters) {
				componentParameters.text += p.Key + " > " + p.Value;
				componentParameters.text += ++index < cInfo.parameters.Count ? "\n" : "";
			}
		} else {
			componentParameters.text = "No Paremeter";
		}
		componentDescription.text = cInfo.description;
	}

	void OnEnable() {
		weaponAssembler.SetAssemblerMode(WeaponAssembler.AssemblerMode.ComponentSelectionMode, true);
		projectionCamera.gameObject.SetActive(true);
	}

	void OnDisable() {
		weaponAssembler.SetAssemblerMode(WeaponAssembler.AssemblerMode.ComponentSelectionMode, false);
		projectionCamera.gameObject.SetActive(false);
	}
	
	protected override void UpdateFocusableObjects() {
		focusableGameObjects.Clear();
		for (int i = 0;i < 3;i++) {
			focusableGameObjects.Add(weaponAssembler.ComponentDisplays[i]);
		}
	}
	
	protected override void TriggerUIEvent() {
		if (focus == -1)
			return;
		if (Input.GetMouseButtonDown(0)) {
			//focusableGameObjects[focus].TriggerSelectEvent(true);
			Items item = (Items)focus;
			switch (item) {
			case Items.Last :
				weaponAssembler.RotateComponentWheel(-1);
				break;
			case Items.Current :
				weaponAssembler.AttachCurrentSelection();
				//weaponAssembler.PreviewWeaponComponents();
				gameObject.SetActive(false);
				weaponAssemblyMenu.SetActive(true);
				break;
			case Items.Next :
				weaponAssembler.RotateComponentWheel(1);
				break;
			}
		}
	}
}
