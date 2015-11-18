using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeaponAssemblyMenu : HolographicMenuUI {
	private enum Items {
		Disassemble = 0, Assemble
	};
	
	[SerializeField]
	private Transform[] sideButtonAnchors = new Transform[2];
	private FocusableGameObject[] sideButtons = new FocusableGameObject[2];
	[SerializeField]
	private GameObject holographicButton;
	[SerializeField]
	private WeaponAssembler weaponAssembler;
	[SerializeField]
	private Image projectionPanel;
	[SerializeField]
	private RawImage projectionPanelNA;
	[SerializeField]
	private RawImage projection;
	[SerializeField]
	private Camera projectionCamera;
	[SerializeField]
	private Image missingTypePanel;
	[SerializeField]
	private RawImage missingTypePanelNA;
	[SerializeField]
	private Text missingTypeTitle;
	[SerializeField]
	private Text missingTypeBoard;
	[SerializeField]
	private GameObject powerMenu;
	[SerializeField]
	private GameObject componentDetailMenu;

	new void Start() {
		base.Start();
		for (int i = 0;i < 2;i++) {
			GameObject button = GameObject.Instantiate(holographicButton, 
			                                           sideButtonAnchors[i].position,
			                                           sideButtonAnchors[i].rotation
			                                           ) as GameObject;
			button.name = "HolographicButton";
			button.transform.parent = transform;
			sideButtons[i] = button.GetComponent<FocusableGameObject>();
		}
	}

	new void Update() {
		base.Update();
		List<string> missingTypes = new List<string>();
		Weapon weapon = weaponAssembler.Weapon.GetComponent<Weapon>();
		if (weapon != null) {
			projectionPanel.color = panelAvailableColor;
			projectionPanelNA.enabled = false;
			projection.enabled = true;
			missingTypePanel.color = panelAvailableColor;
			missingTypePanelNA.enabled = false;
			missingTypeTitle.color = Red;
			weapon.CheckIntegrity(ref missingTypes);
			if (missingTypes.Count > 0) {
				missingTypeBoard.color = Red;
				missingTypeBoard.text = missingTypes[0];
				for (int i = 1;i < missingTypes.Count;i++)
					missingTypeBoard.text += '\n' + missingTypes[i];
			} else {
				missingTypeBoard.color = White;
				missingTypeBoard.text = "Weapon Is Assembled";
			}
		} else {
			projectionPanel.color = panelNonAvailableColor;
			projectionPanelNA.enabled = true;
			projection.enabled = false;
			missingTypePanel.color = panelNonAvailableColor;
			missingTypePanelNA.enabled = true;
			missingTypeTitle.color = SallowGrey;
			missingTypeBoard.color = SallowGrey;
			missingTypeBoard.text = "Weapon Not Available";
		}
	}

	void OnEnable() {
		weaponAssembler.SetAssemblerMode(WeaponAssembler.AssemblerMode.WeaponOverviewMode, true);
		projectionCamera.gameObject.SetActive(true);
	}

	void OnDisable() {
		weaponAssembler.SetAssemblerMode(WeaponAssembler.AssemblerMode.WeaponOverviewMode, false);
		projectionCamera.gameObject.SetActive(false);
	}

	protected override void UpdateFocusableObjects() {
		focusableGameObjects.Clear();
		focusableGameObjects.AddRange(sideButtons);
		focusableGameObjects.AddRange(weaponAssembler.Weapon.GetComponentsInChildren<FocusableGameObject>());
		focusableGameObjects.AddRange(weaponAssembler.Preview.GetComponentsInChildren<FocusableGameObject>());
	}

	protected override void TriggerUIEvent() {
		if (focus == -1)
			return;
		if (Input.GetMouseButtonDown(0)) {
			//focusableGameObjects[focus].TriggerSelectEvent(true);
			switch (focus) {
			case (int)Items.Disassemble :
				weaponAssembler.StartAssembly();
				break;
			case (int)Items.Assemble :
				if (weaponAssembler.FinishAssembly()) {
					gameObject.SetActive(false);
					componentDetailMenu.SetActive(false);
					powerMenu.SetActive(true);
					weaponAssembler.Weapon.SetActive(true);
				}
				break;
			default :
				weaponAssembler.DetachAndListOtherComponentSelections(focusableGameObjects[focus].gameObject);
				gameObject.SetActive(false);
				componentDetailMenu.SetActive(true);
				break;
			}
		}
	}
}
