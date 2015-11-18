using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public abstract class MenuUI : MonoBehaviour {
	protected int focus = -1;

	protected virtual void Update() {
		GetFocusedObject(out focus);
		TriggerUIEvent();
	}

	protected abstract void GetFocusedObject(out int focus);

	protected abstract void TriggerUIEvent();
}
