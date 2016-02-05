using UnityEngine;
using System.Collections;

public class RifleBody : WeaponBody {
	//弹壳弹射时围绕EjectionPoint坐标z轴的旋转角度(空间角)
	protected const float ANGULAR_EJECTION_VARIATION = 4.5f;
	protected const float EJECTION_FORCE = 0.6f;
	protected const float EJECTION_TORQUE = 2.0f;

	protected override void EjectShell(GameObject shell) {
		shell.transform.position = shellEjectionPoint.transform.position;
		shell.transform.rotation = shellEjectionPoint.transform.rotation;
		//叠加空间角偏移量
		Vector2 angularVariation = Random.insideUnitCircle * ANGULAR_EJECTION_VARIATION;
		shell.transform.eulerAngles += new Vector3(angularVariation.x, angularVariation.y, 0.0f);
		//施加Force将壳抛出
		shell.GetComponent<Rigidbody>().AddForce(shell.transform.right * EJECTION_FORCE);
		//施加Torque使其旋转
		shell.GetComponent<Rigidbody>().AddTorque(shell.transform.up * EJECTION_TORQUE);
		base.EjectShell(shell);
	}
}
