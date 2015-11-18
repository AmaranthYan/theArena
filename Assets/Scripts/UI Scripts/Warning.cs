using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class Warning : MonoBehaviour {
	public struct WarningMessage {
		public string msgName;
		public List<string> msgText;
	}
	[SerializeField]
	private TextAsset warningMsgFile;
	private List<WarningMessage> messages = new List<WarningMessage>();
	private WarningMessage? currentMessage = null;
	[SerializeField]
	private Transform OculusRiftModel;
	private const float ANGULAR_VELOCITY = 60.0f;
	private const float AMPLITUDE = 8.0f;
	private const float THETA = Mathf.PI;
	private float timer = 0.0f;
	private bool HMDFlag = false;
	private bool sensorFlag = false;
	[SerializeField]
	private Text[] messageDisplay;

	public GameObject mainCamera;
	public GameObject OVRCamera;
	public GameObject menu;

	void Start() {
		LoadXML();
	}

	void Update() {
		DetectDevice(out currentMessage);
		DisplayWarning();
		AnimateModel();
	}

	private void LoadXML() {
		XmlDocument xmlFile = new XmlDocument();
		xmlFile.LoadXml(warningMsgFile.text);
		XmlNodeList msgList = xmlFile.GetElementsByTagName("msg");
		foreach (XmlNode msg in msgList) {
			WarningMessage warning = new WarningMessage();
			warning.msgName = msg.Attributes["name"].Value;
			warning.msgText = new List<string>();
			XmlNodeList textList = msg.ChildNodes;
			foreach (XmlNode text in textList)
				warning.msgText.Add(text.InnerText);
			messages.Add(warning);
		}
	}

	private void DetectDevice(out WarningMessage? msg) {
		HMDFlag = OVRDevice.IsHMDPresent();
		//检测HeadSensor是否连接(SensorIndex=0),其他SensorIndex目前暂未使用(见Oculus官方注释,当前SDK版本SDK1)
		sensorFlag = OVRDevice.IsSensorPresent(0);
		if (!HMDFlag) {
			msg = messages.Find(m => m.msgName == "HMD Non-Attached");
			return;
		}
		if (!sensorFlag) {
			msg = messages.Find(m => m.msgName == "Sensor Non-Detected");
			return;
		}
		msg = null;
	}

	private void DisplayWarning() {
		if (currentMessage == null) {
			mainCamera.SetActive(false);
			Cursor.visible = false; 
			Cursor.lockState = CursorLockMode.Locked;
			OVRCamera.SetActive(true);
			menu.SetActive(true);
			GameObject.DestroyObject(gameObject);	
		} else {
			WarningMessage msg = currentMessage.Value;
			DisplayMsg(msg);
			switch (msg.msgName) {
			case "HMD Non-Attached" :
				mainCamera.SetActive(true);
				OVRCamera.SetActive(false);
				break;
			case "Sensor Non-Detected" :
				mainCamera.SetActive(false);
				Cursor.visible = false; 
				Cursor.lockState = CursorLockMode.Locked;
				OVRCamera.SetActive(true);
				break;
			}
		}
	}

	public void DisplayMsg(WarningMessage msg) {
		int index = 0;
		foreach (string text in msg.msgText) {
			messageDisplay[index++].text = text;
			if (index >= messageDisplay.Length)
				return;
		}
	}

	private void AnimateModel() {
		Quaternion deltaRotation = Quaternion.Euler(0.0f, ANGULAR_VELOCITY * Time.deltaTime, 0.0f);
		OculusRiftModel.rotation *= deltaRotation;
		timer += Time.deltaTime;
		OculusRiftModel.position += Vector3.up * AMPLITUDE *
			(Mathf.Sin(THETA * timer) - Mathf.Sin(THETA * (timer - Time.deltaTime)));
	}
}
