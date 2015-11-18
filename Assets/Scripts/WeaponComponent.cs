using UnityEngine;
using System.Collections;

public class WeaponComponent : MonoBehaviour {
	[System.Serializable]
	public struct Attachment {
		//配件类型
		public string attachmentType;
		//配件接合点
		public Transform[] attachmentJoints;
	}

	//组件类型
	[SerializeField]
	protected string componentType;
	
	//配件列表
	[SerializeField]
	protected Attachment[] attachableObjects;

	public string ComponentType {
		get {
			return componentType;
		}
	}
	
	public Attachment[] AttachableObjects {
		get {
			return attachableObjects;
		}
	}
	
	//如果不能配置该类型配件则返回Null
	public Attachment? GetAttachment(string type) {
		foreach (Attachment attachment in attachableObjects)
			if (attachment.attachmentType.Equals(type))
				return attachment;
		return null;
	}

	//检测组件是否可以被安装,若是则返回JointPoints
	public Transform[] IsAttachableTo(WeaponComponent weaponComponent) {
		Attachment? attachableObject = weaponComponent.GetAttachment(componentType);
		if (attachableObject != null)
			return attachableObject.Value.attachmentJoints;
		else
			return null;
	}

	//检查是否已安装组件
	public virtual bool HasInstalled(string type) {
		Attachment? attachment = GetAttachment(type);
		if (attachment != null)
			foreach (Transform jointTransform in attachment.Value.attachmentJoints)
				if (jointTransform.GetComponent<ComponentJoint>().IsJointed)
					return true;
		return false;
	}

	public void SetComponentLayer(string lName) {
		gameObject.layer = LayerMask.NameToLayer(lName);
		foreach (Transform childTrans in transform)
			childTrans.gameObject.layer = LayerMask.NameToLayer(lName);
	}

	//递归将Component以及其上附加的所有物体并入目标Transform
	public virtual void AssembleTo(Transform trans) {
		transform.parent = trans;
		SetComponentLayer("Assembled");
		foreach (Attachment attachment in attachableObjects)
			foreach (Transform jointTransform in attachment.attachmentJoints) {
				ComponentJoint joint = jointTransform.GetComponent<ComponentJoint>();
				if (joint.IsJointed)
					joint.SourceComponent.AssembleTo(trans);
			}
	}

	//递归将Component以及其上附加的所有物体从父物体中脱离至目标Transform
	public virtual void DisassembleTo(Transform trans) {
		transform.parent = trans;
		SetComponentLayer("Default");
		foreach (Attachment attachment in attachableObjects)
		foreach (Transform jointTransform in attachment.attachmentJoints) {
			ComponentJoint joint = jointTransform.GetComponent<ComponentJoint>();
			if (joint.IsJointed)
				joint.SourceComponent.DisassembleTo(trans);
		}
	}

	//递归将Component以及其上附加的所有物体从父物体中脱离并逐一摧毁
	public virtual void DisassembleAndDestroy() {
		SetComponentLayer("Default");
		foreach (Attachment attachment in attachableObjects)
			foreach (Transform jointTransform in attachment.attachmentJoints) {
				ComponentJoint joint = jointTransform.GetComponent<ComponentJoint>();
				if (joint.IsJointed) {
				Debug.Log(joint.SourceComponent);
					joint.SourceComponent.DisassembleAndDestroy();
				}
			}
		GameObject.Destroy(gameObject);
	}

	protected virtual void Fire(Cartridge cartridge) {
	}

	protected virtual void Dry() {
	}
}