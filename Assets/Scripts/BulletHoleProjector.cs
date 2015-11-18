using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projector))]
public class BulletHoleProjector : MonoBehaviour {
	private Projector projector;
	public float minHoleSize = 0.03f;
	public float maxHoleSize = 0.08f;
	public Material[] bulletHoles;
	public LayerMask projectedLayers;
	public float existenceDuration = 5.0f;

	void Awake() {
		projector = this.GetComponent<Projector>();
	}

	void Start() {
		projector.ignoreLayers = ~projectedLayers;
		projector.material = bulletHoles[Random.Range(0, bulletHoles.Length)];
		Invoke("SelfDestruct", existenceDuration);
	}

	private void SelfDestruct() {
		GameObject.Destroy(gameObject);
	}

	public void ApplyScale(float scale) {
		projector.orthographicSize = Mathf.Max(minHoleSize, Mathf.Min(maxHoleSize, projector.orthographicSize * scale));
	}
}
