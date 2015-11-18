using UnityEngine;
using System.Collections.Generic;

public class FocusableWeaponComponent : FocusableGameObject {
	protected struct MeshNMaterial {
		public GameObject gameObject;
		public Mesh originalMesh;
		public Material[] originalMaterials;
		public Material[] wireframeNoTexMaterials;
		public Material[] wireframeTexMaterials;
	}

	protected List<MeshNMaterial> subObjects;

	new void OnDestroy() {
		base.OnDestroy();
		//rigidbody.isKinematic = false;
		GetComponent<Collider>().enabled = true;
	}

	public override void GenerateCollider() {
		GetComponent<Collider>().enabled = false;
		base.GenerateCollider();
	}

	protected override void Initialize() {
		GenerateCollider();
		subObjects = new List<MeshNMaterial>();
		SaveMeshsAndMaterials();
		GenerateWireframeMeshsAndMaterials();
		SetMaterials(1);
	}

	private void SaveMeshsAndMaterials() {
		MeshFilter[] subMeshs = this.GetComponentsInChildren<MeshFilter>(true);
		foreach (MeshFilter m in subMeshs) {
			MeshNMaterial subObj = new MeshNMaterial();
			subObj.gameObject = m.gameObject;
			subObj.originalMesh = m.mesh;
			subObj.originalMaterials = m.GetComponent<Renderer>().materials;
			subObj.wireframeNoTexMaterials = new Material[subObj.originalMaterials.Length];
			subObj.wireframeTexMaterials = new Material[subObj.originalMaterials.Length];
			subObjects.Add(subObj);
		}
	}
	
	private void GenerateWireframeMeshsAndMaterials() {
		foreach (MeshNMaterial subObj in subObjects) {
			//添加AmazingWireframeGenerator
			subObj.gameObject.AddComponent<TheAmazingWireframeGenerator>();
			for (int i = 0;i < subObj.originalMaterials.Length;i++) {
				Material wireframeNoTexMat = new Material(
					Shader.Find("VacuumShaders/The Amazing Wireframe/Unlit/NoTex"));
				wireframeNoTexMat.SetColor("_Color", subObj.originalMaterials[i].GetColor("_Color"));
				wireframeNoTexMat.EnableKeyword("V_WIRE_ANTIALIASING_ON");
				wireframeNoTexMat.EnableKeyword("V_WIRE_LIGHT_ON");
				subObj.wireframeNoTexMaterials[i] = wireframeNoTexMat;
				Material wireframeTexMat = new Material(
						Shader.Find("VacuumShaders/The Amazing Wireframe/Deferred/Bumped Specular"));
				wireframeTexMat.SetColor("_Color", subObj.originalMaterials[i].GetColor("_Color"));
				if (subObj.originalMaterials[i].HasProperty("_SpecColor"))
					wireframeTexMat.SetColor("_SpecColor", subObj.originalMaterials[i].GetColor("_SpecColor"));
				if (subObj.originalMaterials[i].HasProperty("_Shininess"))
					wireframeTexMat.SetFloat("_Shininess", subObj.originalMaterials[i].GetFloat("_Shininess"));
				wireframeTexMat.SetTexture("_MainTex", subObj.originalMaterials[i].GetTexture("_MainTex"));
				if (subObj.originalMaterials[i].HasProperty("_BumpMap"))
					wireframeTexMat.SetTexture("_BumpMap", subObj.originalMaterials[i].GetTexture("_BumpMap"));
				wireframeTexMat.EnableKeyword("V_WIRE_ANTIALIASING_ON");
				wireframeTexMat.EnableKeyword("V_WIRE_LIGHT_ON");
				subObj.wireframeTexMaterials[i] = wireframeTexMat;
			}
		}
	}

	private void SetMaterials(int index) {
		switch (index) {
		//OriginalMaterials
		case 0 :
			foreach (MeshNMaterial subObj in subObjects)
				subObj.gameObject.GetComponent<Renderer>().materials = subObj.originalMaterials;
			break;
		//NoTexMaterials
		case 1 :
			foreach (MeshNMaterial subObj in subObjects)
				subObj.gameObject.GetComponent<Renderer>().materials = subObj.wireframeNoTexMaterials;
			break;
		//TexMaterials
		case 2 :
			foreach (MeshNMaterial subObj in subObjects)
				subObj.gameObject.GetComponent<Renderer>().materials = subObj.wireframeTexMaterials;
			break;
		}
	}

	public void CleanUp() {
		//还原Materials
		SetMaterials(0);
		foreach (MeshNMaterial subObj in subObjects) {
			//清理AmazingWireframeGenerator
			Object.DestroyImmediate(
				subObj.gameObject.GetComponent<TheAmazingWireframeGenerator>());
			//还原Mesh
			subObj.gameObject.GetComponent<MeshFilter>().mesh = subObj.originalMesh;
			//清理WireframeMeshs&VacuumShaderMaterials
			foreach (Material wNoTexMat in subObj.wireframeNoTexMaterials)
				Material.DestroyImmediate(wNoTexMat);
			foreach (Material wTexMat in subObj.wireframeTexMaterials)
				Material.DestroyImmediate(wTexMat);
		}
		Object.DestroyImmediate(this);
	}

	private void RenderFocusColor(bool f) {
		if (f)
			foreach (MeshNMaterial subObj in subObjects)
				foreach (Material material in subObj.gameObject.GetComponent<Renderer>().materials)
					material.SetColor("_Color", focusColor);
		else
			foreach (MeshNMaterial subObj in subObjects)
				for (int i = 0;i < subObj.originalMaterials.Length;i++)
					subObj.gameObject.GetComponent<Renderer>().materials[i].SetColor("_Color", 
					                                                 subObj.originalMaterials[i].GetColor("_Color"));
	}

	public override void TriggerFocusEvent(bool f) {
		RenderFocusColor(f);
	}

	//递归改变Component以及其上附加的所有物体的材质
	public virtual void RenderAllComponentsWithMaterial(int index) {
		SetMaterials(index);
		RenderFocusColor(isFocused);
		RenderSubComponentsWithMaterial(index);
	}

	//递归改变Component上附加的所有物体的材质
	public virtual void RenderSubComponentsWithMaterial(int index) {
		WeaponComponent component = this.GetComponent<WeaponComponent>();
		foreach (WeaponComponent.Attachment attachment in component.AttachableObjects)
			foreach (Transform jointTransform in attachment.attachmentJoints) {
				ComponentJoint joint = jointTransform.GetComponent<ComponentJoint>();
				if (joint.IsJointed)
					joint.SourceComponent.GetComponent<FocusableWeaponComponent>().
						RenderAllComponentsWithMaterial(index);
		}
	}
}
