using UnityEngine;
using System.Collections;

public class WeaponPart : WeaponComponent {
	//与上级组件接合点
	[SerializeField]
	protected Transform componentJoint;

	public ComponentJoint ComponentJoint {
		get {
			return componentJoint.GetComponent<ComponentJoint>();
		}
	}

	public override void DisassembleTo(Transform trans) {
		Separate();
		base.DisassembleTo(trans);
	}
	
	public override void DisassembleAndDestroy() {
		Separate();
		base.DisassembleAndDestroy();
	}

	//若存在复数个接合点,选择其中之一(默认为0)
	public bool JointTo(WeaponComponent component, int index = 0) {
		Transform[] targetJoints = IsAttachableTo(component);
		if (targetJoints == null)
			return false;
		if (index < 0 || index >= targetJoints.Length)
			return false;
		return componentJoint.GetComponent<ComponentJoint>().JointTo(targetJoints[index]);
	}
	
	public void Separate() {
		componentJoint.GetComponent<ComponentJoint>().Separate();
	}
}
