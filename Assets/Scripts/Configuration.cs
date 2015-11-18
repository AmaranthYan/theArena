using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;

public class Configuration {
	public static string ConfigFile = "/Xmls/Config.xml";

	public static void LoadConfig(string paramName, out int param) {
		XmlDocument xmlFile = new XmlDocument();
		xmlFile.Load(Application.dataPath + ConfigFile);
		XmlNodeList config = xmlFile.GetElementsByTagName(paramName);
		if (!int.TryParse(config[0].InnerText, out param)) {
			param = 0;
			Debug.LogError("Parameter ERROR! Do NOT modify the Config file!");
		}
	}
	
	public static void SaveConfig(string paramName, int value) {
		XmlNode param;
		XmlDocument xmlFile = new XmlDocument();
		xmlFile.Load(Application.dataPath + ConfigFile);
		XmlNodeList config = xmlFile.GetElementsByTagName(paramName);
		if (config.Count > 0) {
			param = config[0];
		} else {
			param = xmlFile.CreateElement(paramName);
			XmlElement root = xmlFile.DocumentElement;
			root.AppendChild(param);
		}
		param.InnerText = value.ToString();
		xmlFile.Save(Application.dataPath + ConfigFile);
	}
}
