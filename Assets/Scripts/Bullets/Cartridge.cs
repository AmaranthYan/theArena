using UnityEngine;
using System.Collections;

public class Cartridge : MonoBehaviour {
	[SerializeField]
	protected GameObject projectile;
	[SerializeField]
	protected GameObject shell;

	public GameObject InstantiateProjectile {
		get {
			GameObject p = GameObject.Instantiate(projectile) as GameObject;
			p.name = projectile.name;
			return p;
		}
	}

	public GameObject InstantiateShell {
		get {
			GameObject s = GameObject.Instantiate(shell) as GameObject;
			s.name = shell.name;
			return s;
		}
	}
}
