using UnityEngine;
using System.Collections.Generic;

public class Workshop : MonoBehaviour {
	public static List<GameObject> assembledWeapons = new List<GameObject>();
	[SerializeField]
	private WeaponAssembler assembler;

	void Update () {
		if (Input.GetKeyDown(KeyCode.D)) {
			GameObject assembledWeapon = null;
			assembler.DeliverWeapon(ref assembledWeapon);
			assembledWeapon.transform.parent = null;
			GameObject.DontDestroyOnLoad(assembledWeapon);
			assembledWeapon.SetActive(false);
			assembledWeapons.Add(assembledWeapon);
		}
		if (Input.GetKeyDown(KeyCode.Space)) {
			if (assembledWeapons.Count > 0)
				Application.LoadLevel("ArenaZero");
		}
	}
}
