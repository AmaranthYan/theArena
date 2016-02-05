using UnityEngine;
using System.Collections;

public class WeaponBody : WeaponComponent {
	[SerializeField]
	protected Transform weaponHandle;
	//射速(间隔单位s)
	[SerializeField]
	protected float firingRate;
	[Header("Sounds")]
	[SerializeField]
	protected AudioClip ejectingSound;
	[SerializeField]
	protected AudioClip dryingSound;
	protected Animator animator;
	[Header("Weapon Recoil")]
	[SerializeField]
	protected Bounds recoilTranslationBounds = new Bounds(Vector3.zero, Vector3.zero);
	[SerializeField]
	protected Vector2 recoilXRotationRange = Vector2.zero;
	[SerializeField]
	protected Vector2 recoilYRotationRange = Vector2.zero;
	[Header("Shell Ejection")]
	[SerializeField]
	protected Transform shellEjectionPoint;

	public Transform WeaponHandle {
		get {
			return weaponHandle;
		}
	}

	public float FiringRate {
		get {
			return firingRate;
		}
	}

	public virtual void CalculateRecoil(out Vector3 recoilTranslation, out Quaternion recoilRotation) {
		//recoilTranslation = recoilTranslationBounds.
	}
	
	void Start() {
		animator = this.GetComponent<Animator>();
	}

	protected virtual void PullTrigger(bool isPulled) {
		animator.SetBool("isTriggerPulled", isPulled);
	}

	protected override void Fire(Cartridge cartridge) {
		GameObject shell = cartridge.InstantiateShell;
		EjectShell(shell);
	}

	protected virtual void EjectShell(GameObject shell) {
		if (GetComponent<Animation>()["Eject"] != null) {
			GetComponent<Animation>()["Eject"].layer = 1;
			GetComponent<Animation>()["Eject"].speed = 1.0f / firingRate;
			GetComponent<Animation>().Play("Eject");
		}
		if (ejectingSound != null)
			GetComponent<AudioSource>().PlayOneShot(ejectingSound);
	}
	
	protected override void Dry() {
		if (dryingSound != null)
			GetComponent<AudioSource>().PlayOneShot(dryingSound);
	}
}
