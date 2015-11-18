using UnityEngine;
using System.Collections;

public class FocusableGameObject : FocusableObject {
	//FocusableGameObject的碰撞体附着于被聚焦的物体上
	private BoxCollider boxCollider = null;
	public Vector3 colliderCenter;
	public Vector3 colliderSize;
	public Color focusColor = new Color(1.0f, 0.92f, 0.016f, 1.0f);
	private Color originalColor;
	
	public virtual void OnDestroy() {
		Collider.Destroy(boxCollider);
	}

	public virtual void GenerateCollider() {
		boxCollider = gameObject.AddComponent<BoxCollider>();
		boxCollider.center = colliderCenter;
		boxCollider.size = colliderSize;
	}

	protected override void Initialize() {
		GenerateCollider();
		originalColor = GetComponent<Renderer>().material.color;
	}

	protected override void AnimateFocus() {

	}
	
	public override void TriggerFocusEvent(bool f) {
		if (f)
			foreach (Material material in GetComponent<Renderer>().materials)
				material.SetColor("_Color", focusColor);
		else
			foreach (Material material in GetComponent<Renderer>().materials)
				material.SetColor("_Color", originalColor);
	}
	
	public override void TriggerSelectEvent(bool s) {

	}
}
