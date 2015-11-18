using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(RectTransform))]
public class FocusableUIObject : FocusableObject {
	//FoucusableUIObject必须是目标UI的父对象,以便分离碰撞体与渲染器/图形
	[SerializeField]
	protected Transform objectTransform;
	[HideInInspector]
	public RectTransform rectTransform;
	protected RectTransform UITransform;
	//Focused动画参数
	public float deltaZ = -60.0f;
	public float amplitude = 10.0f;
	public float theta = Mathf.PI;
	[SerializeField]
	protected AudioClip[] UIFocusedSound;
	protected Vector3 posInitial;
	protected Vector3 posFocused;
	protected float timer = 0.0f;
	[SerializeField]
	protected AudioClip UITriggeredSound;
	
	void Awake() {
		rectTransform = (RectTransform)transform;
		UITransform = (RectTransform)objectTransform;
		posInitial = UITransform.position;
		posFocused = posInitial + Vector3.forward * deltaZ;
	}

	protected override void Initialize() {
		if (UITransform != null)
			UITransform.sizeDelta = rectTransform.sizeDelta;
	}
	
	protected override void AnimateFocus() {
		timer += Time.deltaTime;
		if (objectTransform != null)
			objectTransform.position += Vector3.forward * amplitude * 
				(Mathf.Sin(theta * timer) - Mathf.Sin(theta * (timer - Time.deltaTime)));
	}

	public override void TriggerFocusEvent(bool f) {
		timer = 0.0f;
		if (objectTransform != null)
			objectTransform.position = f ? posFocused : posInitial;
		AudioClip clip;
		if (UIFocusedSound.Length > 0) {
			if (UIFocusedSound.Length < 2)
				clip = f ? UIFocusedSound[0] : null;
			else
				clip = f ? UIFocusedSound[0] : UIFocusedSound[1];
			GetComponent<AudioSource>().PlayOneShot(clip);
		}
	}

	public override void TriggerSelectEvent(bool s) {
		if (s)
			if (UITriggeredSound != null)
				GetComponent<AudioSource>().PlayOneShot(UITriggeredSound);
	}
}
