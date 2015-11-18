using UnityEngine;
using System.Collections;

public class ArenaTerrain : PenetrableObject {
	public override void HitByProjectile(Projectile projectile, Vector3 hitPoint, Vector3 hitNormal, out bool isRicocheted) {
		base.HitByProjectile(projectile, hitPoint, hitNormal, out isRicocheted);
		if (!isRicocheted)
			GameObject.Destroy(projectile.gameObject);
	}
}
