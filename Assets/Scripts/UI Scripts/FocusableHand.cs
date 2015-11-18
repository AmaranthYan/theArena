using UnityEngine;
using System.Collections;

public class FocusableHand : FocusableUIObject {
	[SerializeField]
	private Renderer handRenderer;
	[SerializeField]
	private Material[] UISelectedMaterial;

	void OnDestroy() {
		/*
		if (handRenderer != null)
			Material.Destroy(handRenderer.material);
			*/
	}

	public override void TriggerSelectEvent(bool s) {
		base.TriggerSelectEvent(s);
		if (UISelectedMaterial.Length < 2)
			return;
		handRenderer.material = s ? UISelectedMaterial[0] : UISelectedMaterial[1];
	}
}
