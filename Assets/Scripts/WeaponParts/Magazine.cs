using UnityEngine;
using System.Collections;

public class Magazine : WeaponPart {
	[SerializeField]
	protected GameObject cartridge;
	[SerializeField] 
	protected int magazineSize;
	protected int ammunitionLoad;

	public GameObject InstantiateMagazine {
		get {
			GameObject m = GameObject.Instantiate(gameObject) as GameObject;
			m.name = gameObject.name;
			return m;
		}
	}

	public Cartridge FeedCartridge {
		get {
			if (ammunitionLoad > 0) {
				ammunitionLoad--;
				return cartridge.GetComponent<Cartridge>();
			} else {
				return null;
			}
		}
	}

	public int AmmunitionLoad {
		get {
			return ammunitionLoad;
		}
	}

	void Start() {
		ammunitionLoad = magazineSize;
	}
}
