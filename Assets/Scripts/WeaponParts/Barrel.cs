using UnityEngine;
using System.Collections;

public class Barrel : WeaponPart {
	[SerializeField]
	protected Transform barrelDirection;
	[SerializeField]
	protected float riflingTwist;
	//射击动画与音效
	[SerializeField]
	protected Transform[] flameEmissionPoints;
	[SerializeField]
	protected GameObject flameEffect;
	[SerializeField]
	protected AudioClip firingSound;

	protected override void Fire(Cartridge cartridge) {
		GameObject projectile = cartridge.InstantiateProjectile;
		projectile.transform.position = barrelDirection.position;
		projectile.transform.rotation = barrelDirection.rotation;
		projectile.GetComponent<Projectile>().RiflingTwist = riflingTwist;
		//检查是否配置消音器
		if (HasInstalled("Silencer")) {
				Debug.LogWarning("This function is currently not available!");
				return;
		}
		//生成火花
		foreach (Transform flameEmissionPoint in flameEmissionPoints) {
			Object.Instantiate(flameEffect, flameEmissionPoint.position, flameEmissionPoint.rotation);
		}
		GetComponent<AudioSource>().PlayOneShot(firingSound);
	}
}
