using UnityEngine;
using System.Collections;

public class ComponentJoint : MonoBehaviour {
	private Transform parentObject;
	//当被其他Joint接合时,将接合对象存入SourceJoint
	private ComponentJoint sourceJoint = null;
	//当接合其他Joint时,将接合对象存入TargetJoint
	private ComponentJoint targetJoint = null;
	private bool isJointed = false;

	public Transform ParentObject {
		get {
			return parentObject;
		}
	}

	public ComponentJoint SourceJoint {
		get {
			return sourceJoint;
		}
		set {
			sourceJoint = value;
		}
	}

	public WeaponComponent SourceComponent {
		get {
			return sourceJoint.transform.parent.GetComponent<WeaponComponent>();
		}
	}

	public ComponentJoint TargetJoint {
		get {
			return targetJoint;
		}
		set {
			targetJoint = value;
		}
	}

	public WeaponComponent TargetComponent {
		get {
			return targetJoint.transform.parent.GetComponent<WeaponComponent>();
		}
	}

	public bool IsJointed {
		get {
			return isJointed;
		}
		set {
			isJointed = value;
		}
	}

	void Awake() {
		parentObject = transform.parent;
	}

	void Update() {
		if (parentObject == null)
			return;
		UpdateTransform();
	}

	private void UpdateTransform() {
		Vector3 targetPosition = transform.position;
		Vector3 targetOrientation = transform.forward;
		if (targetJoint != null) {
			targetPosition = targetJoint.transform.position;
			targetOrientation = targetJoint.transform.forward;		
		}
		parentObject.rotation = Quaternion.FromToRotation(transform.forward, targetOrientation) * parentObject.rotation;
		parentObject.position += targetPosition - transform.position;
	}

	public bool JointTo(Transform target) {
		ComponentJoint componentjoint = target.GetComponent<ComponentJoint>();
		if (componentjoint == null || componentjoint.isJointed)
			return false;
		if (isJointed)
			Separate();
		targetJoint = componentjoint;
		isJointed = true;
		componentjoint.SourceJoint = this;
		componentjoint.IsJointed = true;
		return true;
	}

	//从TargetJoint上分离
	public void Separate() {
		if (targetJoint != null) {
			targetJoint.IsJointed = false;
			targetJoint.SourceJoint = null;
		}
		isJointed = false;
		targetJoint = null;
	}
}
