using UnityEngine;
using System.Collections;

public class EveTheBunny : MonoBehaviour {
	private enum State {
		LookOut = 0,
		Run = 1
	}

	private const float MAX_TURNING_ANGLE = 150.0f;
	private const float TURNING_SPEED = 0.8f; 
	private const float RUNNING_SPEED = 0.3f; 
	private const float MIN_RUNNING_TIME = 6.0f; 
	private const float MAX_RUNNING_TIME = 12.0f; 
	[SerializeField]
	private Animation bunnyAnimation;
	private State currentState;
	private Vector3 targetOrientation;
	private float turnTimer = 0.0f;
	private float moveTimer = 0.0f;
	public float speed = 0.3f;
	public Transform orientedTransform;

	void Start() {
		Run();
	}

	void Update() {
		switch (currentState) {
		case State.LookOut :
			Turn();
			break;
		case State.Run :
			Move();
			break;
		}
	}

	void OnCollisionEnter(Collision collision) {
		StopAndLook();
	}

	private void Turn() {
		if (turnTimer > 0.0f) {
			Quaternion deltaRotation = Quaternion.FromToRotation(transform.forward,
			                                                     Vector3.Slerp(transform.forward, targetOrientation, Mathf.Min(1.0f, Time.deltaTime / turnTimer)));
			transform.rotation = deltaRotation * transform.rotation;
			turnTimer -= Time.deltaTime;
		}
	}

	private void Move() {
		if (moveTimer > 0.0f) {
			transform.position += speed * Time.deltaTime * transform.forward;
			moveTimer -= Time.deltaTime;
		} else {
			StopAndLook();
		}
	}

	private void CalculateOrientation() {
		Vector3 newOrientation = transform.forward; 
		if (orientedTransform != null) {
			newOrientation = Vector3.ProjectOnPlane(
				orientedTransform.position - transform.position, Vector3.up).normalized;
			float k = Random.value;
			newOrientation = Vector3.Slerp(transform.forward, newOrientation, k);
		}
		Quaternion rotation = Quaternion.Euler(0.0f, Random.Range(-MAX_TURNING_ANGLE, MAX_TURNING_ANGLE), 0.0f);
		newOrientation = rotation * newOrientation;
		targetOrientation = newOrientation;
		turnTimer = TURNING_SPEED;
	}

	private void StopAndLook() {
		CancelInvoke();
		currentState = State.LookOut;
		bunnyAnimation.CrossFade("lookOut", 0.2f);
		CalculateOrientation();
		Invoke("Run", bunnyAnimation["lookOut"].length);
	}

	private void Run() {
		currentState = State.Run;
		bunnyAnimation.CrossFade("run", 0.2f);
		bunnyAnimation["run"].speed = speed / RUNNING_SPEED;
		moveTimer = Random.Range(MIN_RUNNING_TIME, MAX_RUNNING_TIME);
	}
}
