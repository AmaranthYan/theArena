using UnityEngine;
using System.Collections.Generic;

public class Rifle : Weapon {
	protected static new string[] NecessaryTypes = {"Barrel", "Magazine", "Sight"};

	public override Transform EyePosition {
		get {
			Sight sight = (Sight)FindPartByType("Sight");
			if (sight == null)
				return null;
			return sight.EyePosition;
		}
	}

	public override bool CheckIntegrity(ref List<string> missingTypes) {
		missingTypes = new List<string>();
		isIntegrated = (body != null);
		if (!isIntegrated) {
			missingTypes.Add("Body");
			return false;
		}
		foreach (string type in NecessaryTypes) {
			if (FindPartByType(type) == null) {
				missingTypes.Add(type);
				isIntegrated = false;
			}
		}
		return isIntegrated;
	}

	public override void SightEyeAngle(ref float angle) {
		Sight sight = (Sight)FindPartByType("Sight");
		if (sight == null)
			return;
		angle = sight.SightEyeAngle();
	}

	public override void EnableAim(Camera OVRCamera) {
		Sight sight = (Sight)FindPartByType("Sight");
		if (sight == null)
			return;
		sight.EnableAimAt(OVRCamera);
	}

	public override void ZoomAim(float zoom) {
		Sight sight = (Sight)FindPartByType("Sight");
		if (sight == null)
			return;
		sight.Zoom(zoom);
	}
	
	public override void DisableAim() {
		Sight sight = (Sight)FindPartByType("Sight");
		if (sight == null)
			return;
		sight.DisableAim();
	}

	public override WeaponPart EjectMagazine() {
		Magazine magazine = (Magazine)FindPartByType("Magazine");
		if (magazine == null)
			return null;
		Detach(magazine, null);
		magazine.GetComponent<Rigidbody>().isKinematic = false;
		GameObject.Destroy(magazine.gameObject, 10.0f);
		return magazine;
	}
	
	public override void InsertMagazine(Magazine magazine) {
		if ((Magazine)FindPartByType("Magazine") != null)
			return;
		magazine.GetComponent<Rigidbody>().isKinematic = true;
		Attach(magazine, body);
		if (ammoCounter) 
			ammoCounter.Refill();
	}

	public override int GetMagazineSize() {
		Magazine magazine = (Magazine)FindPartByType("Magazine");
		if (magazine == null)
			return -1;
		return magazine.MagazineSize;
	}
	
	public override void FireWeapon() {
		if (!isIntegrated)
			return;
		//武器未准备好
		if (cooldown > 0.0f)
			return;
		Magazine magazine = (Magazine)FindPartByType("Magazine");
		if (magazine == null)
			return;
		Cartridge cartridge = magazine.FeedCartridge;
		if (cartridge != null) {
			BroadcastMessage("Fire", cartridge);
			if (ammoCounter) 
				ammoCounter.Consume();
		} else {
			BroadcastMessage("Dry");
		}
		cooldown = body.FiringRate;
	}
}
