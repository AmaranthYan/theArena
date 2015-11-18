using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
	public GameObject barrel;
	public GameObject C;
	public RifleBody body;
	public Sight sight;
	public Camera OVRCam;
	public float FOV;
	public Weapon rifle;
	private float timer = 1.0f;
	public FPSPlayerController player;
	public Weapon weapon;

	void FixedUpdate () {
		//Test1();

	}

	void Start() {
		//player.EquipeWeapon (weapon);
	}

	void Update () {
		///Test1();
		Test2 ();
		//Test3();
	}

	void Test1() {
		//sight.transform.FindChild ("SightOVRCamera").GetComponent<SightOVRCamera> ().SetVerticalFOV (FOV);
		if (Input.GetKeyDown(KeyCode.A)) {
			sight.EnableAimAt(OVRCam);
		}
		if (Input.GetKeyDown(KeyCode.B)) {
			sight.DisableAim();
		}
		if (timer >= 1.0f) {
			barrel.GetComponent<Barrel>().SendMessage("Fire", C.GetComponent<Cartridge>());
			body.BroadcastMessage("Fire", C.GetComponent<Cartridge>());
			timer = 0.0f;
		}
		timer += Time.fixedDeltaTime * 2;
	}

	void Test2(){
		if (Input.GetMouseButtonDown (1)) {
			sight.EnableAimAt(OVRCam);	
		}
		if (Input.GetMouseButtonUp (1)) {
			sight.DisableAim();		
		}
		if (Input.GetAxis ("Mouse ScrollWheel") != 0.0f)
			sight.Zoom(Input.GetAxis ("Mouse ScrollWheel"));
		//if ()
	}

	void Test3(){
		if (Input.GetMouseButtonDown (0)) {
			rifle.PullTrigger = true;
		}
		if (Input.GetMouseButtonUp (0)) {
			rifle.PullTrigger = false;	
		}

	}
}
