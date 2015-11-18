using UnityEngine;
using System.Collections.Generic;

public abstract class HolographicMenuUI : MenuUI {
	protected Color panelAvailableColor = new Color(0.686f, 0.839f, 0.898f, 0.392f);
	protected Color panelNonAvailableColor = new Color(0.75f, 0.75f, 0.75f, 0.392f);
	
	public static Color White = new Color(1.0f, 1.0f, 1.0f, 0.878f);
	public static Color SallowGrey = new Color(0.8f, 0.8f, 0.8f, 0.878f);
	public static Color Red = new Color(1.0f, 0.0f, 0.0f, 0.878f);
	public static Color Yellow = new Color(1.0f, 1.0f, 0.0f, 0.878f);

	protected List<FocusableGameObject> focusableGameObjects;

	protected virtual void Start() {
		focusableGameObjects = new List<FocusableGameObject>();
	}

	protected override void Update () {
		UpdateFocusableObjects();
		base.Update();
	}

	protected abstract void UpdateFocusableObjects();
	
	protected override void GetFocusedObject(out int focus) {
		focus = -1;
		for (int i = 0;i < focusableGameObjects.Count;i++) {
			if (focusableGameObjects[i] != null)
				focus = focusableGameObjects[i].IsFocused ? i : focus;
		}
	}
}
