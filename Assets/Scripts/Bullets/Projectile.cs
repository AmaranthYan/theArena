using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//使用Rigidbody中的部分参数(Drag)
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {
	protected const float MIN_VELOCITY = 0.1f;
	protected const float PERSISTENCE = 4.0f;
	//对于子弹弹头而言约为0.01单位
	public float retractDistance = 0.01f;
	[SerializeField]
	protected float muzzleVelocity;
	protected float riflingTwist = Mathf.Infinity;
	protected float substancePenetrationDepth = Mathf.Infinity;
	protected Vector3 linearVelocity;
	protected float angularVelocity;
	protected Vector3 previousPosition;
	protected Quaternion previousRotation;
	public LayerMask collisionLayers;

	[Header("Trail")]
	[SerializeField]
	protected HighSpeedTrailRenderer trailRenderer = null; 

	public float MuzzleVelocity {
		get {
			return muzzleVelocity;
		}
	}

	public float RiflingTwist {
		set {
			riflingTwist = value;
		}
	}

	public Vector3 LinearVelocity {
		get {
			return linearVelocity;
		}
	}

	public Vector3 AngularVelocity {
		get {
			return linearVelocity.normalized * angularVelocity;
		}
	}
	
	void Start () {
		linearVelocity = transform.forward * muzzleVelocity;
		angularVelocity =  360.0f * muzzleVelocity / riflingTwist;
		previousPosition = transform.position;
		previousRotation = transform.rotation;

		if (trailRenderer)
			trailRenderer.insertPoint(previousPosition, previousRotation);
	}

	//Physics计算
	void FixedUpdate() {
		linearVelocity += Physics.gravity * Time.fixedDeltaTime;
		//Drag计算
		linearVelocity *= Mathf.Clamp01(1.0f - GetComponent<Rigidbody>().drag * Time.fixedDeltaTime);
		angularVelocity *= Mathf.Clamp01(1.0f - GetComponent<Rigidbody>().angularDrag * Time.fixedDeltaTime);
		//Raycast碰撞检测防止TunnelEffect
		Vector3 newPosition = previousPosition + linearVelocity * Time.fixedDeltaTime;
		//Aligned with linear velocity direction
		Quaternion newRotation = Quaternion.LookRotation(linearVelocity.normalized);//previousRotation;
		Vector3 direction = linearVelocity.normalized;
		float distance = (newPosition - previousPosition).magnitude;
		//正向射线判定
		RaycastHit[] forwardHits;
		forwardHits = Physics.RaycastAll(previousPosition, direction, distance, collisionLayers.value)
			.OrderBy(h => h.distance).ToArray();
		//逆向射线判定
		RaycastHit[] backwardHits;
		backwardHits = Physics.RaycastAll(newPosition, -direction , distance, collisionLayers.value)
			.OrderByDescending(h => h.distance).ToArray();
		float preDist = 0.0f;
		float postDist = 0.0f;
		if (forwardHits.Length > 0 || backwardHits.Length > 0) {
			int[] index = {0, 0};
			if (backwardHits.Length > 0)
				index[0] -= forwardHits.Length == 0 || 
					!forwardHits[0].collider.Equals(backwardHits[0].collider) ? 1 : 0;
			while (index[0] < forwardHits.Length) {
				RaycastHit forwardHit;
				RaycastHit backwardHit;
				ArenaObject obj = null;
				preDist = 0.0f;
				postDist = 0.0f;
				if (index[0] >= 0) {
					forwardHit = forwardHits[index[0]];
					obj = forwardHit.collider.GetComponent<ArenaObject>();
					bool isRicocheted;
					obj.HitByProjectile(this, forwardHit.point, forwardHit.normal, out isRicocheted);
					if (isRicocheted) {
						//RetractDistance防止下一帧FixedUpdate重复计算当前Collider
						newPosition = forwardHit.point - direction * retractDistance;
						distance = (newPosition - previousPosition).magnitude;
						//法线反转Projectile速度
						linearVelocity = Vector3.Reflect(linearVelocity, forwardHit.normal);
						//更新Rotation
						newRotation = Quaternion.LookRotation(linearVelocity.normalized);
						//动能衰减
						ApplyKineticAttenuation(obj.ricochetKineticAttenuationRatio);

						//在尾迹中加入反射造成的遗漏点
						if (trailRenderer)
							trailRenderer.insertPoint(newPosition, newRotation);
						break;
					} else {
						preDist = forwardHit.distance;
						substancePenetrationDepth = obj.PenetrationDepth;
					}
				}
				if (index[1] < backwardHits.Length) {
					backwardHit = backwardHits[index[1]];
					obj = backwardHit.collider.GetComponent<ArenaObject>();
					postDist = backwardHit.distance;
					float attenuationRatio = Mathf.Min(1.0f, (distance - preDist - postDist) / substancePenetrationDepth);
					ApplyKineticAttenuation(attenuationRatio);
					substancePenetrationDepth = Mathf.Infinity;
					if (linearVelocity.magnitude > MIN_VELOCITY) {
						bool isRicocheted;
						obj.HitByProjectile(this, backwardHit.point, backwardHit.normal, out isRicocheted);
					}
				} else {
					float attenuationRatio = Mathf.Min(1.0f, (distance - preDist - postDist) / substancePenetrationDepth);
					ApplyKineticAttenuation(attenuationRatio);
				}
				index[0]++;
				index[1]++;
			}
		} else {
			if (substancePenetrationDepth != Mathf.Infinity) {
				float attenuationRatio = Mathf.Min(1.0f, (distance - preDist - postDist) / substancePenetrationDepth);
				ApplyKineticAttenuation(attenuationRatio);
			}
		}
		if (linearVelocity.magnitude < MIN_VELOCITY) {
			GameObject.Destroy(gameObject, PERSISTENCE);
			this.enabled = false;
		}
		//更新Position和Rotation
		transform.position = newPosition;
		transform.rotation = newRotation;

		//Apply Rotation
		transform.Rotate(Vector3.forward, angularVelocity % 360.0f, Space.Self);

		previousPosition = transform.position;
		previousRotation = transform.rotation;
	}

	protected void ApplyKineticAttenuation(float attenuationRatio) {
		linearVelocity *= 1 - attenuationRatio;
		angularVelocity *= 1 - attenuationRatio;
	}
}