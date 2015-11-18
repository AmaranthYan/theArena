using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class ProjectionCamera : MonoBehaviour {
	[SerializeField]
	private Transform focusTransform;
	private Vector3 initPosition;
	private Quaternion initRotaiton;
	private const float ANGULAR_VELOCITY = 36.0f;
 
	void Awake() {
		initPosition = transform.position;
		initRotaiton = transform.rotation;
	}

	void OnEnable() {
		transform.position = initPosition;
		transform.rotation = initRotaiton;
	}

	void Update() {
		transform.RotateAround(focusTransform.position, 
		                       focusTransform.up, 
		                       Time.deltaTime * ANGULAR_VELOCITY);
	}
}
