using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour {
	protected const float MAGNITUDE_SCALE = 0.5f;
	protected const float KINETIC_ATTENUATION = 0.7f;
	[SerializeField]
	protected AudioClip soundEffect;
	public float existenceDuration = 10.0f;
	
	void Start() {
		Invoke("SelfDestruct", existenceDuration);
	}

	void OnCollisionEnter(Collision collision) {
		GetComponent<AudioSource>().PlayOneShot(soundEffect, Mathf.Max(1.0f, collision.relativeVelocity.magnitude * MAGNITUDE_SCALE));
		GetComponent<Rigidbody>().velocity *= 1 - KINETIC_ATTENUATION;
		GetComponent<Rigidbody>().angularVelocity *= 1 - KINETIC_ATTENUATION;
	}

	private void SelfDestruct() {
		GameObject.Destroy(gameObject);
	}
}
