using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;

public class Configuration {
	public static string ConfigFile = "/Config.xml";

	public static void LoadConfig(string paramName, out int param) {
		XmlDocument xmlFile = new XmlDocument();
		try {
			xmlFile.Load(Application.persistentDataPath + ConfigFile);
			XmlNodeList config = xmlFile.GetElementsByTagName(paramName);
			if (!int.TryParse(config[0].InnerText, out param)) {
				param = 0;
				Debug.LogError("Parameter ERROR! Do NOT modify the Config file!");
			}
		} catch(DirectoryNotFoundException e) {
			param = 0;
			Debug.LogError("Unity data directory MISSING!");
		} catch(FileNotFoundException e) {
			param = 0;
			Debug.LogError("Config file MISSING! Use default value.");
		}
	}
	
	public static void SaveConfig(string paramName, int value) {
		XmlNode param;
		XmlDocument xmlFile = new XmlDocument();
		try {
			xmlFile.Load(Application.persistentDataPath + ConfigFile);
		} catch(DirectoryNotFoundException e) {
			Directory.CreateDirectory(Application.persistentDataPath);
		} catch(FileNotFoundException e) {
		}
		XmlNodeList config = xmlFile.GetElementsByTagName(paramName);
		if (config.Count > 0) {
			param = config[0];
		} else {
			param = xmlFile.CreateElement(paramName);
			xmlFile.AppendChild(param);
		}
		param.InnerText = value.ToString();
		xmlFile.Save(Application.persistentDataPath + ConfigFile);
	}
}
