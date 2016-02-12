using UnityEngine;
using System.Collections.Generic;

public abstract class Weapon : MonoBehaviour {
	protected static string[] NecessaryTypes;
	[SerializeField]
	protected WeaponBody body = null;
	protected bool isIntegrated = false;
	protected float cooldown = 0.0f;
	protected bool isTriggerPulled = false;

	private Vector3 recoilTranslation = Vector3.zero;
	private Vector2 recoilRotation = Vector2.zero;

	//HUD
	[Header("UI Display")]
	[SerializeField]
	protected ConsumableObjectCounter ammoCounter = null;
	[SerializeField]
	protected TextView zoomIndicator = null;

	public virtual ConsumableObjectCounter AttachAmmoCounter {
		set {
			ammoCounter = value;
			ammoCounter.Initialize(GetMagazineSize());
		}
	}

	public virtual TextView AttachZoomIndicator {
		set {
			zoomIndicator = value;
		}
	}

	public virtual WeaponBody Body {
		get {
			return body;		
		}
		set {
			if (body != null)
				return;
			if (value != null) {
				body = value;
				AddWeaponComponent(body);
			}
		}
	}

	public virtual Transform WeaponHandle {
		get {
			if (body == null)
				return null;
			return body.WeaponHandle;
		}
	}

	public abstract Transform EyePosition {
		get;
	}

	public virtual bool PullTrigger {
		set {
			if (body == null)
				return;
			if (isTriggerPulled ^ value) {
				isTriggerPulled = value;
				body.SendMessage("PullTrigger", isTriggerPulled);
			}
		}
	}

	void Start() {
		List<string> missingTypes = new List<string>();
		CheckIntegrity(ref missingTypes);
	}

	void Update() {
		if (cooldown > 0.0f)
			cooldown -= Time.deltaTime;
		if (isTriggerPulled) {
			FireWeapon();
		};
		//Recover from recoil
		recoilTranslation = Vector3.Lerp(recoilTranslation, Vector3.zero, Time.deltaTime * body.RecoverSpeed);
		recoilRotation = Vector2.Lerp(recoilRotation, Vector2.zero, Time.deltaTime * body.RecoverSpeed);
	}

	protected bool AddWeaponComponent(WeaponComponent component) {
		if (HasComponent(component))
			return false;
		component.AssembleTo(transform);
		List<string> missingTypes = new List<string>();
		CheckIntegrity(ref missingTypes);
		return HasComponent(component);
	}
	
	protected bool RemoveWeaponComponent(WeaponComponent component, Transform toTrans) {
		if (!HasComponent(component))
			return false;
		if (component.Equals(body))
			body = null;
		component.DisassembleTo(toTrans);
		List<string> missingTypes = new List<string>();
		CheckIntegrity(ref missingTypes);
		return !HasComponent(component);
	}

	protected bool RemoveAndDestroyWeaponComponent(WeaponComponent component) {
		if (!HasComponent(component))
			return false;
		if (component.Equals(body))
			body = null;
		component.DisassembleAndDestroy();
		List<string> missingTypes = new List<string>();
		CheckIntegrity(ref missingTypes);
		return !HasComponent(component);
	}
	
	public bool HasComponent(WeaponComponent component) {
		if (component == null)
			return false;
		WeaponComponent[] components = transform.GetComponentsInChildren<WeaponComponent>(true);
		foreach (WeaponComponent c in components)
			if (c.Equals(component))
				return true;
		return false;
	}
	
	public WeaponPart FindPartByType(string type) {
		WeaponPart[] parts = transform.GetComponentsInChildren<WeaponPart>(true);
		foreach (WeaponPart p in parts)
			if (p.ComponentType.Equals(type))
				return p;
		return null;
	}
	
	//检查必要配件是否已安装
	public abstract bool CheckIntegrity(ref List<string> missingTypes);

	//在Body不为Null时有效
	public bool Attach(WeaponPart part, WeaponComponent component) {
		if (!HasComponent(component))
			return false;
		bool isAttached = part.JointTo(component);
		if (isAttached) {
			AddWeaponComponent(part);
		}
		return isAttached;
	}
	
	public bool Detach(WeaponComponent component, Transform toTrans = null) {
		bool isDetached = RemoveWeaponComponent(component, toTrans);
		return isDetached;
	}

	public bool DetachAndDestroy(WeaponComponent component) {
		bool isDetached = RemoveAndDestroyWeaponComponent(component);
		return isDetached;
	}

	protected virtual void CalculateRecoil() {
		Vector3 translation = Vector3.zero;
		Vector2 rotation = Vector2.zero;
		body.Recoil(out translation, out rotation);
		recoilTranslation += translation;
		recoilRotation += rotation;
	}

	public void ApplyRecoil(out Vector3 rTranslation, out Quaternion rRotation) {
		//Local translation to world translation
		rTranslation = body.transform.rotation * recoilTranslation;

		rRotation = Quaternion.identity;
		rRotation *= Quaternion.AngleAxis(recoilRotation.x, body.transform.right);
		rRotation *= Quaternion.AngleAxis(recoilRotation.y, body.transform.up);

	}

	public void ResetRecoil() {
		recoilTranslation = Vector3.zero;
		recoilRotation = Vector2.zero;
	}

	public abstract void SightEyeAngle(ref float angle);

	public abstract void EnableAim(Camera OVRCamera);

	public abstract void ZoomAim(float zoom);

	public abstract void DisableAim();

	public abstract WeaponPart EjectMagazine();
	
	public abstract void InsertMagazine(Magazine magazine);

	public abstract int GetMagazineSize();

	public abstract void FireWeapon();
}
