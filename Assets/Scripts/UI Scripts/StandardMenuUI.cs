using UnityEngine;
using System.Collections;

public abstract class StandardMenuUI : MenuUI {
	[SerializeField]
	protected FocusableUIObject[] focusableUIObjects;

	protected override void GetFocusedObject(out int focus) {
		focus = -1;
		for (int i = 0;i < focusableUIObjects.Length;i++)
			focus = focusableUIObjects[i].IsFocused ? i : focus;
	}
}
