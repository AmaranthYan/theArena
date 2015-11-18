using UnityEngine;
using System.Collections;

public class WeaponBody : WeaponComponent {
	[SerializeField]
	protected Transform weaponHandle;
	//射速(间隔单位s)
	[SerializeField]
	protected float firingRate;
	[SerializeField]
	protected AudioClip ejectingSound;
	[SerializeField]
	protected AudioClip dryingSound;
	protected Animator animator;

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
