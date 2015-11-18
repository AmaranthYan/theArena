using UnityEngine;
using System.Collections;

public abstract class FocusableObject : MonoBehaviour {
	protected bool isFocused = false;
	protected bool isSelected = false;

	public virtual bool IsFocused {
		get {
			return isFocused;
		}
		set {
			if (isFocused ^ value) {
				TriggerFocusEvent(value);
			}
			isFocused = value;
		}
	}
	
	public virtual bool IsSelected {
		get {
			return isSelected;
		}
		set {
			TriggerSelectEvent(value);
			isSelected = value;
		}
	}

	void Start() {
		Initialize();
	}

	void Update() {
		if (isFocused)
			AnimateFocus();
	}

	protected abstract void Initialize();

	protected abstract void AnimateFocus();

	public abstract void TriggerFocusEvent(bool f);

	public abstract void TriggerSelectEvent(bool s);
}
