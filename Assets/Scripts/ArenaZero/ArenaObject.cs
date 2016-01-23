using UnityEngine;
using System.Collections;

public class ArenaObject : MonoBehaviour {
	protected const float PARTICLE_EXISTENCE_DURATION = 0.4f;
	protected const float FLASH_EXISTENCE_DURATION = 0.15f;
	protected const float SOUND_EXISTENCE_DURATION = 0.8f;
	public float ricochetKineticAttenuationRatio = 0.6f;
	public float minRicochetVelocity = 100.0f;
	public float maxRicochetVelocity = 600.0f;
	[SerializeField]
	protected GameObject hitAudioSource;
	public GameObject ricochetParticleEffect;
	public AudioClip ricochetSoundEffect;

	//在物体内部速度衰减至0需要的距离,对于不可穿透物体始终保持最小值(0.001)
	public virtual float PenetrationDepth {
		get {
			return 0.001f;
		}
	}

	void Start() {
		if (maxRicochetVelocity < minRicochetVelocity)
			maxRicochetVelocity = minRicochetVelocity;
	}

	protected virtual void ApplyRicochetParticleEffect(Vector3 hitPoint, Vector3 hitNormal, Vector3 projectileVelocity) {
		if (ricochetParticleEffect == null)
			return;
		GameObject spark = GameObject.Instantiate(ricochetParticleEffect, 
		                                          hitPoint,
		                                          Quaternion.LookRotation(projectileVelocity, hitNormal)) as GameObject;
		ParticleSystem[] emitters = spark.GetComponentsInChildren<ParticleSystem>();
		float scale = Mathf.Min(1.0f, projectileVelocity.magnitude / maxRicochetVelocity);
		foreach (ParticleSystem e in emitters) {
			e.startSpeed *= scale;
			e.startSize *= scale;
		}
		Light flash = spark.GetComponentInChildren<Light>();
		if (flash) 
			GameObject.Destroy(flash, FLASH_EXISTENCE_DURATION);
		GameObject.Destroy(spark, PARTICLE_EXISTENCE_DURATION);
	}

	protected virtual void ApplyRicochetSoundEffect(Vector3 hitPoint, Vector3 projectileVelocity) {
		if (ricochetSoundEffect == null)
			return;
		GameObject audioSource = GameObject.Instantiate(hitAudioSource,
		                                                hitPoint,
		                                                Quaternion.identity) as GameObject;
		audioSource.GetComponent<AudioSource>().PlayOneShot(ricochetSoundEffect, 
		                        Mathf.Min(1.0f, projectileVelocity.magnitude / maxRicochetVelocity));
		GameObject.Destroy(audioSource, SOUND_EXISTENCE_DURATION);
	}

	public virtual void HitByProjectile(Projectile projectile, Vector3 hitPoint, Vector3 hitNormal, out bool isRicocheted) {
		isRicocheted = true;
		Vector3 linearVelocity = projectile.LinearVelocity;
		if (linearVelocity.magnitude >= minRicochetVelocity) {
			ApplyRicochetParticleEffect(hitPoint, hitNormal, linearVelocity);
		}
		ApplyRicochetSoundEffect(hitPoint, linearVelocity);
	}
}
