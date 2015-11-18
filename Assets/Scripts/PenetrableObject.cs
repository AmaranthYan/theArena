using UnityEngine;
using System.Collections;

public class PenetrableObject : ArenaObject {
	[Range(0.0f, 90.0f)]
	public float maxNonRicochetAngle = 75.0f;
	[Range(0.001f, 1000.0f)]
	public float penetrationDepth;
	public float minPenetrationVelocity = 100.0f;
	public float maxPenetrationVelocity = 600.0f;
	[SerializeField]
	private GameObject bulletHoleProjector;
	public GameObject penetrationParticleEffect;
	public AudioClip penetrationSoundEffect;

	//在物体内部速度衰减至0需要的距离
	public override float PenetrationDepth {
		get {
			return penetrationDepth;
		}
	}

	protected virtual void ProjectBulletHole(Vector3 hitPoint, Vector3 hitNormal, Vector3 projectileVelocity) {
		if (bulletHoleProjector == null)
			return;
		GameObject projector = GameObject.Instantiate(bulletHoleProjector, 
		                                              hitPoint, 
		                                              Quaternion.LookRotation(-hitNormal)) as GameObject;
		projector.GetComponent<BulletHoleProjector>().ApplyScale(Mathf.Min(1.0f, projectileVelocity.magnitude / maxRicochetVelocity));
		//弹孔具有Self-Destrcut
	}

	protected virtual void ApplyPenetrationParticleEffect(Vector3 hitPoint, Vector3 hitNormal, Vector3 projectileVelocity) {
		if (penetrationParticleEffect == null)
			return;
		//to be finished
	}

	protected virtual void ApplyPenetrationSoundEffect(Vector3 hitPoint, Vector3 projectileVelocity) {
		if (penetrationSoundEffect == null)
			return;
		GameObject audioSource = GameObject.Instantiate(hitAudioSource, 
		                                                hitPoint, 
		                                                Quaternion.identity) as GameObject;
		audioSource.GetComponent<AudioSource>().PlayOneShot(penetrationSoundEffect, 
		                                                    Mathf.Min(1.0f, projectileVelocity.magnitude / maxPenetrationVelocity));
		GameObject.Destroy(audioSource, SOUND_EXISTENCE_DURATION);
	}

	public override void HitByProjectile(Projectile projectile, Vector3 hitPoint, Vector3 hitNormal, out bool isRicocheted) {
		Vector3 linearVelocity = projectile.LinearVelocity;
		float angle = Vector3.Angle(-linearVelocity, hitNormal);
		if (angle <= maxNonRicochetAngle) {
			if (linearVelocity.magnitude >= minPenetrationVelocity) {
				isRicocheted = false;
				ProjectBulletHole(hitPoint, hitNormal, linearVelocity);
				ApplyPenetrationParticleEffect(hitPoint, hitNormal, linearVelocity);
				ApplyPenetrationSoundEffect(hitPoint, linearVelocity);
			} else {
				base.HitByProjectile(projectile, hitPoint, hitNormal, out isRicocheted);
			}
		} else if (angle > 90.0f) {
			isRicocheted = false;
			ProjectBulletHole(hitPoint, hitNormal, linearVelocity);
			ApplyPenetrationSoundEffect(hitPoint, linearVelocity);
		} else {
			base.HitByProjectile(projectile, hitPoint, hitNormal, out isRicocheted);
		}
	}
}
