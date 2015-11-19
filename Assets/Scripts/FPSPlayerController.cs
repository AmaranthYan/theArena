using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour {
	private enum PlayerState {
		Normal = 0,
		TransToAiming,
		Aiming,
		TransToNormal
	};

	private const float MAX_WEAPON_X_NORMAL_ROTATION_ANGLE = 60.0f;
	private const float MAX_WEAPON_X_AIMING_ROTATION_ANGLE = 30.0f;
	private const float STATE_TRANSITION_TIME = 0.2f;
	private const float AIMING_MOVEMENT_SCALE = 0.4f;
	//左眼0,右眼1
	[SerializeField]
	private Transform[] eyePositions = new Transform[2];
	[SerializeField]
	private Camera[] eyeViews = new Camera[2];
	[SerializeField]
	private Transform[] weaponHandles = new Transform[2];
	[SerializeField]
	private Weapon weapon = null;
	public float weaponRotationAmount = 1.5f;
	private PlayerState currentState = PlayerState.Normal;
	private float transitionTimer = 0.0f;
	private int handedness = 1;
	private float rotateScale = 1.0f;
	private Vector3 recoilPosition = Vector3.zero;
	private Quaternion recoilRotation = Quaternion.identity;
	private float weaponXRotation = 0.0f;
	private Vector3 deltaWeaponPosition;
	private Quaternion deltaWeaponRotation;

	//注释请参看Occulus官方Doc
	#region Inherited or Altered OVR Variables
	protected float DeltaTime = 1.0f;
	protected CharacterController Controller = null;
	protected OVRCameraController CameraController = null;
	public float Acceleration = 0.1f;
	public float Damping = 0.15f;
	public float BackAndSideDampen = 0.5f;
	public float JumpForce = 0.3f;
	public float RotationAmount = 1.5f;
	public float GravityModifier = 0.379f;
	private float MoveScale = 1.0f;
	private Vector3 MoveThrottle = Vector3.zero;
	private float FallSpeed = 0.0f;
	private Quaternion OrientationOffset = Quaternion.identity;			
	private float YRotation = 0.0f;
	protected Transform DirXform = null;
	private float MoveScaleMultiplier = 1.0f; 
	private float RotationScaleMultiplier = 1.0f;
	//Set to false by default to enable mouse rotation
	private bool AllowMouseRotation = false;
	private bool HaltUpdateMovement = false;
	private float YfromSensor2 = 0.0f;
	#endregion

	public void LoadConfig() {
		Configuration.LoadConfig("handedness", out handedness);
		handedness = handedness == 0 ? 0 : 1;
	}

	public void EquipeWeapon(Weapon w) {
		if (w.Body == null)
			return;
		weapon = w;
		weapon.Body.GetComponent<Rigidbody>().isKinematic = true;
		weapon.transform.parent = DirXform;
		weaponXRotation = 0.0f;
		deltaWeaponPosition = weaponHandles[handedness].position - weapon.WeaponHandle.position;
		deltaWeaponRotation = Quaternion.FromToRotation(weapon.WeaponHandle.forward, weaponHandles[handedness].forward);
		UpdateWeaponTransform();
	}

	//弹药反冲力计算
	public virtual void CalculateRecoil() {
		//recoilRotation
		recoilPosition;
		recoilRotation;
	}

	//根据玩家状态计算Weapon位置信息+反冲力计算
	public virtual void CalculateWeaponMovement() {
		CalculateRecoil();
		if(HaltUpdateMovement == true)
			return;
		Vector3 targetWeaponPosition;
		Vector3 targetWeaponOrientation;
		float transitionRatio;
		float rotateInfluence;
		float deltaRotation;
		switch (currentState) {
		case PlayerState.Normal :
			targetWeaponPosition = weaponHandles[handedness].position;
			targetWeaponOrientation = weaponHandles[handedness].forward;
			rotateInfluence = DeltaTime * weaponRotationAmount * RotationScaleMultiplier;
			deltaRotation = 0.0f;
			if(AllowMouseRotation == false)
				deltaRotation = -Input.GetAxis("Mouse Y") * rotateInfluence * 3.25f;
			weaponXRotation += deltaRotation;
			weaponXRotation = Mathf.Abs(weaponXRotation) > MAX_WEAPON_X_NORMAL_ROTATION_ANGLE ? 
				Mathf.Sign(weaponXRotation) * MAX_WEAPON_X_NORMAL_ROTATION_ANGLE : weaponXRotation;
			targetWeaponOrientation = Quaternion.AngleAxis(weaponXRotation, weaponHandles[handedness].right) * targetWeaponOrientation;
			deltaWeaponPosition = targetWeaponPosition - weapon.WeaponHandle.position;
			deltaWeaponRotation = Quaternion.FromToRotation(weapon.WeaponHandle.forward, targetWeaponOrientation);
			break;
		case PlayerState.TransToAiming :
			transitionRatio = Mathf.Min(1.0f, Time.deltaTime / transitionTimer);
			targetWeaponPosition = Vector3.Lerp(weapon.EyePosition.position,
			                                    eyePositions[handedness].position,
			                                    transitionRatio);
			targetWeaponOrientation = Vector3.Slerp(weapon.EyePosition.forward,
			                                        eyePositions[handedness].forward,
			                                        transitionRatio);
			deltaWeaponPosition = targetWeaponPosition - weapon.EyePosition.position;
			deltaWeaponRotation = Quaternion.FromToRotation(weapon.EyePosition.forward, targetWeaponOrientation);
			weaponXRotation *= (1 - transitionRatio);
			transitionTimer -= Time.deltaTime;
			if (transitionTimer <= 0.0f) {
				transitionTimer = 0.0f;
				currentState = PlayerState.Aiming;
				weapon.EnableAim(eyeViews[handedness]);
			}	
			break;
		case PlayerState.Aiming :
			targetWeaponPosition = eyePositions[handedness].position;
			targetWeaponOrientation = eyePositions[handedness].forward;
			rotateInfluence = DeltaTime * weaponRotationAmount * AIMING_MOVEMENT_SCALE * RotationScaleMultiplier;
			deltaRotation = 0.0f;
			if(AllowMouseRotation == false)
				deltaRotation = -Input.GetAxis("Mouse Y") * rotateInfluence * 3.25f;
			weaponXRotation += deltaRotation;
			weaponXRotation = Mathf.Abs(weaponXRotation) > MAX_WEAPON_X_AIMING_ROTATION_ANGLE ? 
				Mathf.Sign(weaponXRotation) * MAX_WEAPON_X_AIMING_ROTATION_ANGLE : weaponXRotation;
			targetWeaponOrientation = Quaternion.AngleAxis(weaponXRotation, eyePositions[handedness].right) * targetWeaponOrientation;
			deltaWeaponPosition = targetWeaponPosition - weapon.EyePosition.position;
			deltaWeaponRotation = Quaternion.FromToRotation(weapon.EyePosition.forward, targetWeaponOrientation);
			break;
		case PlayerState.TransToNormal :
			transitionRatio = Mathf.Min(1.0f, Time.deltaTime / transitionTimer);
			targetWeaponPosition = Vector3.Lerp(weapon.WeaponHandle.position,
			                                    weaponHandles[handedness].position,
			                                    transitionRatio);
			targetWeaponOrientation = Vector3.Slerp(weapon.WeaponHandle.forward,
			                                        weaponHandles[handedness].forward,
			                                        transitionRatio);
			deltaWeaponPosition = targetWeaponPosition - weapon.WeaponHandle.position;
			deltaWeaponRotation = Quaternion.FromToRotation(weapon.WeaponHandle.forward, targetWeaponOrientation);
			weaponXRotation *= (1 - transitionRatio);
			transitionTimer -= Time.deltaTime;
			if (transitionTimer <= 0.0f) {
				transitionTimer = 0.0f;
				currentState = PlayerState.Normal;
			}	
			break;
		}
	}

	//更新Weapon的Transform信息
	public virtual void UpdateWeaponTransform() {
		weapon.transform.rotation = deltaWeaponRotation * weapon.transform.rotation;
		weapon.transform.position += deltaWeaponPosition;
		weapon.transform.rotation = recoilRotation * weapon.transform.rotation;
		weapon.transform.position += recoilPosition;
	}

	protected virtual void TransToNormal() {
		weapon.DisableAim();
		currentState = PlayerState.TransToNormal;
		transitionTimer = STATE_TRANSITION_TIME - transitionTimer;
		weapon.PullTrigger = false;	
	}

	protected virtual void TransToAiming() {
		currentState = PlayerState.TransToAiming;
		transitionTimer = STATE_TRANSITION_TIME - transitionTimer;
		weapon.PullTrigger = false;
	}

	protected virtual void FireWeapon() {
		weapon.PullTrigger = true;
	}

	protected virtual void HaltWeapon() {
		weapon.PullTrigger = false;
	}

	protected virtual void ReplaceMagazine() {
		Magazine oldMagazine = (Magazine)weapon.EjectMagazine();
		Magazine newMagazine = oldMagazine.InstantiateMagazine.GetComponent<Magazine>();
		weapon.InsertMagazine(newMagazine);
	}

	protected virtual void ZoomSight() {
		float zoom = Input.GetAxis("Mouse ScrollWheel");
		if (zoom != 0.0f)
			weapon.ZoomAim(zoom);
	}

	//执行射击,瞄准等用户命令
	public virtual void HandleUserWeaponEvents() {
		if (Input.GetMouseButtonDown(1)) {
			TransToAiming();
		}
		if (Input.GetMouseButtonUp(1)) {
			TransToNormal();
		}
		if ((currentState == PlayerState.TransToAiming) || (currentState == PlayerState.TransToNormal))
			return;
		if (Input.GetMouseButtonDown(0)) {
			FireWeapon();
		}
		if (Input.GetMouseButtonUp(0)) {
			HaltWeapon();	
		}
		if (Input.GetKeyDown(KeyCode.R)) {
			if (currentState != PlayerState.Normal) {
				TransToNormal();
			}
			Invoke("ReplaceMagazine", transitionTimer);
			return;
		}
		if (currentState == PlayerState.Aiming) {
			ZoomSight();
		}
	}

	#region Inherited or Altered OVR Functions
	void Awake() {
		Controller = gameObject.GetComponent<CharacterController>();
		OVRCameraController[] CameraControllers;
		CameraControllers = gameObject.GetComponentsInChildren<OVRCameraController>();
		CameraController = CameraControllers[0];	
		DirXform = null;
		Transform[] Xforms = gameObject.GetComponentsInChildren<Transform>();
		for(int i = 0; i < Xforms.Length; i++) {
			if(Xforms[i].name == "ForwardDirection") {
				DirXform = Xforms[i];
				break;
			}
		}
	}

	void Start() {
		LoadConfig();
		InitializeInputs();	
		SetCameras();
	}

	void Update() {
		DeltaTime = (Time.deltaTime * 60.0f);
		if (OVRDevice.SensorCount == 2) {
			Quaternion q = Quaternion.identity;
			OVRDevice.GetPredictedOrientation(1, ref q);
			YfromSensor2 = q.eulerAngles.y;
		}
		UpdatePlayerMovement();
		if (weapon != null) {
			CalculateWeaponMovement();
			//更新武器Transform
			UpdateWeaponTransform();
			HandleUserWeaponEvents();
		}
		Vector3 moveDirection = Vector3.zero;
		float motorDamp = (1.0f + (Damping * DeltaTime));
		MoveThrottle.x /= motorDamp;
		MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
		MoveThrottle.z /= motorDamp;
		moveDirection += MoveThrottle * DeltaTime;
		if (Controller.isGrounded && FallSpeed <= 0)
			FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));	
		else
			FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * DeltaTime);	
		moveDirection.y += FallSpeed * DeltaTime;
		float bumpUpOffset = 0.0f;
		if (Controller.isGrounded && MoveThrottle.y <= 0.001f) {
			bumpUpOffset = Mathf.Max(Controller.stepOffset, 
			                         new Vector3(moveDirection.x, 0, moveDirection.z).magnitude); 
			moveDirection -= bumpUpOffset * Vector3.up;
		}			
		Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), 
		                                    new Vector3(1, 0, 1));
		Controller.Move(moveDirection);
		Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));
		if (predictedXZ != actualXZ)
			MoveThrottle += (actualXZ - predictedXZ) / DeltaTime; 
		UpdatePlayerForwardDirTransform();
	}

	static float sDeltaRotationOld = 0.0f;
	
	public virtual void UpdatePlayerMovement() {
		if(HaltUpdateMovement == true)
			return;
		bool moveForward = false;
		bool moveLeft = false;
		bool moveRight = false;
		bool moveBack = false;
		MoveScale = 1.0f;
		if (Input.GetKey(KeyCode.W)) moveForward = true;
		if (Input.GetKey(KeyCode.A)) moveLeft = true;
		if (Input.GetKey(KeyCode.S)) moveBack = true; 
		if (Input.GetKey(KeyCode.D)) moveRight = true; 		
		if ((moveForward && moveLeft) || (moveForward && moveRight) ||
		    (moveBack && moveLeft) || (moveBack && moveRight))
			MoveScale = 0.70710678f;
		if (!Controller.isGrounded)	
			MoveScale = 0.0f;
		MoveScale *= DeltaTime;
		//在瞄准状态下适当减缓玩家移动速度(提高瞄准精度)
		MoveScale *= currentState == PlayerState.Aiming ? AIMING_MOVEMENT_SCALE : 1.0f;
		float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			moveInfluence *= 2.0f;
		if(DirXform != null) {
			if (moveForward)
				MoveThrottle += DirXform.TransformDirection(Vector3.forward * moveInfluence);
			if (moveBack)
				MoveThrottle += DirXform.TransformDirection(Vector3.back * moveInfluence) * BackAndSideDampen;
			if (moveLeft)
				MoveThrottle += DirXform.TransformDirection(Vector3.left * moveInfluence) * BackAndSideDampen;
			if (moveRight)
				MoveThrottle += DirXform.TransformDirection(Vector3.right * moveInfluence) * BackAndSideDampen;
		}
		rotateScale = 1.0f;
		//在瞄准状态下适当减缓玩家旋转速度(提高瞄准精度)
		rotateScale *= currentState == PlayerState.Aiming ? AIMING_MOVEMENT_SCALE : 1.0f;
		float rotateInfluence = DeltaTime * RotationAmount * rotateScale * RotationScaleMultiplier;
		if (Input.GetKey(KeyCode.Q)) 
			YRotation -= rotateInfluence * 0.5f;  
		if (Input.GetKey(KeyCode.E)) 
			YRotation += rotateInfluence * 0.5f; 
		//鼠标
		float deltaRotation = 0.0f;
		if(AllowMouseRotation == false)
			deltaRotation = Input.GetAxis("Mouse X") * rotateInfluence * 3.25f;
		float filteredDeltaRotation = (sDeltaRotationOld * 0.0f) + (deltaRotation * 1.0f);
		YRotation += filteredDeltaRotation;
		sDeltaRotationOld = filteredDeltaRotation;
		SetCameras();	
	}

	public virtual void UpdatePlayerForwardDirTransform() {
		if ((DirXform != null) && (CameraController != null)) {
			Quaternion q = Quaternion.identity;
			q = Quaternion.Euler(0.0f, YfromSensor2, 0.0f);
			DirXform.rotation = q * CameraController.transform.rotation;
		}
	}

	public bool Jump() {
		if (!Controller.isGrounded)
			return false;
		MoveThrottle += new Vector3(0, JumpForce, 0);
		return true;
	}

	public void Stop() {
		Controller.Move(Vector3.zero);
		MoveThrottle = Vector3.zero;
		FallSpeed = 0.0f;
	}	

	public void InitializeInputs() {
		OrientationOffset = transform.rotation;
		YRotation = 0.0f;
	}

	public void SetCameras() {
		if(CameraController != null) {
			CameraController.SetOrientationOffset(OrientationOffset);
			CameraController.SetYRotation(YRotation);
		}
	}

	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier) {
		moveScaleMultiplier = MoveScaleMultiplier;
	}

	public void SetMoveScaleMultiplier(float moveScaleMultiplier) {
		MoveScaleMultiplier = moveScaleMultiplier;
	}

	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier) {
		rotationScaleMultiplier = RotationScaleMultiplier;
	}

	public void SetRotationScaleMultiplier(float rotationScaleMultiplier) {
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	public void GetAllowMouseRotation(ref bool allowMouseRotation) {
		allowMouseRotation = AllowMouseRotation;
	}

	public void SetAllowMouseRotation(bool allowMouseRotation) {
		AllowMouseRotation = allowMouseRotation;
	}

	public void GetHaltUpdateMovement(ref bool haltUpdateMovement) {
		haltUpdateMovement = HaltUpdateMovement;
	}

	public void SetHaltUpdateMovement(bool haltUpdateMovement) {
		HaltUpdateMovement = haltUpdateMovement;
	}
	#endregion
}