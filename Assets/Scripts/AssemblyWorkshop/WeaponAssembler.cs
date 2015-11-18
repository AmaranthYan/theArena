using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class WeaponAssembler : MonoBehaviour {
	private const string RESOURCE_PATH = "WeaponComponents/";
	public enum AssemblerMode {
		WeaponOverviewMode = 0, ComponentSelectionMode
	};

	public class ComponentInfo {
		private static Transform container;

		public static void AssignContainer(Transform c) {
			container = c;
		}

		//关于Component的描述信息
		public string id;
		private GameObject instance;
		
		public GameObject Instance {
			get {
				if (instance == null) {
					if (id == "")
						return null;
					instance = GameObject.Instantiate(
						Resources.Load<GameObject>(RESOURCE_PATH + id)) as GameObject;
					instance.name = id;
					//所有的Component都最初生成在Container中
					instance.transform.parent = container;
				}
				return instance;
			}
		}

		public void Reset() {
			instance = null;
		}

		//只有Body存有Category信息
		public string category;
		public string name;
		public Dictionary<string, string> parameters;
		public string description;
		//关于Assembly的信息
		public Dictionary<string, List<ComponentInfo>> attachable;

		public ComponentInfo() {
			id = "";
			instance = null;
			category = "";
			name = "";
			parameters = new Dictionary<string, string>();
			description = "";
			attachable = new Dictionary<string, List<ComponentInfo>>();
		}
	}

	[SerializeField]
	private TextAsset assemblyInfo;
	[SerializeField]
	private Transform container;
	[SerializeField]
	private GameObject weapon;
	
	public static ComponentInfo NullComponent;
	private List<ComponentInfo> bodyInfo = new List<ComponentInfo>();
	private List<ComponentInfo> partInfo = new List<ComponentInfo>();

	private bool isAssembling = true;

	public GameObject Weapon {
		get {
			return weapon;
		}
	}

	void Awake() {
		InitializeNullComponent();
		ComponentInfo.AssignContainer(container);
	}

	void Start() {
		LoadXML();
		//test
		//PreviewWeaponComponents();
	}
	
	//Must call DeliverWeapon before destroy WeaponAssembler!
	void OnDestroy() {

	}

	private void InitializeNullComponent() {
		NullComponent = new ComponentInfo();
		NullComponent.id = "NullCube";
		NullComponent.name = "Null Component";
		NullComponent.parameters.Add("Paremeter", "Null");
		NullComponent.description = "Null component is the component you choose when you decide not to select for this component type.";
	}
	
	//从<Object>节点中读取组件信息
	private void ReadComponentInfo(XmlNode obj, ref ComponentInfo cInfo) {
		cInfo = new ComponentInfo();
		cInfo.id = obj.Attributes["id"].Value;
		if (obj.Attributes["category"] != null) 
			cInfo.category = obj.Attributes["category"].Value;
		foreach (XmlNode info in obj.ChildNodes) {
			switch (info.Name) {
			case "name" :
				cInfo.name = info.InnerText;
				break;
			case "parameter" :
				foreach (XmlNode param in info.ChildNodes)
					cInfo.parameters.Add(param.Attributes["name"].Value, param.InnerText);
				break;
			case "description" :
				cInfo.description = info.InnerText;
				break;
			}
		}
	}

	//从<Object>节点中读取装配信息
	private void ReadAssemblyInfo(XmlNode obj, ref ComponentInfo cInfo) {
		foreach (XmlNode aType in obj.ChildNodes) {
			List<ComponentInfo> attachableList = new List<ComponentInfo>();
			foreach (XmlNode aObj in aType) {
				ComponentInfo componentInfo = new ComponentInfo();
				if (FindComponentInfo(aObj.Attributes["id"].Value, partInfo, ref componentInfo))
					attachableList.Add(componentInfo);
			}
			cInfo.attachable.Add(aType.Attributes["type"].Value, attachableList);
		}
	}

	public bool FindComponentInfo(string objId, List<ComponentInfo> cList, ref ComponentInfo cInfo) {
		if (cList.Exists(c => c.id == objId)) {
			cInfo = cList.Find(c => c.id == objId);
			return true;
		} else {
			cInfo = new ComponentInfo();
			return false;
		}
	}

	public bool FindComponentInfo(GameObject gameObj, List<ComponentInfo> cList, ref ComponentInfo cInfo) {
		if (cList.Exists(c => c.Instance.Equals(gameObj))) {
			cInfo = cList.Find(c => c.Instance.Equals(gameObj));
			return true;
		} else {
			cInfo = new ComponentInfo();
			return false;
		}
	}

	public bool FindAttachablesByType(ComponentInfo cInfo, string type, ref List<ComponentInfo> cInfoList) {
		return cInfo.attachable.TryGetValue(type, out cInfoList);
	}
	
	private void LoadXML() {
		XmlDocument xmlFile = new XmlDocument();
		xmlFile.LoadXml(assemblyInfo.text);
		//读取<Component>节点
		XmlNodeList componentList = xmlFile.GetElementsByTagName("component")[0].ChildNodes;
		foreach (XmlNode component in componentList) {
			XmlNodeList objectList = component.ChildNodes;
			foreach (XmlNode obj in objectList) {
				ComponentInfo componentInfo = new ComponentInfo();
				ReadComponentInfo(obj, ref componentInfo);
				switch (component.Name) {
				case "body" :
					bodyInfo.Add(componentInfo);
					break;
				case "part" :
					partInfo.Add(componentInfo);
					break;
				}
			}
		}
		//读取<Assembly>节点
		XmlNodeList assemblyList = xmlFile.GetElementsByTagName("assembly")[0].ChildNodes;
		foreach (XmlNode obj in assemblyList) {
			ComponentInfo componentInfo = new ComponentInfo();
			if (FindComponentInfo(obj.Attributes["id"].Value, bodyInfo, ref componentInfo))
				ReadAssemblyInfo(obj, ref componentInfo);
			else if (FindComponentInfo(obj.Attributes["id"].Value, partInfo, ref componentInfo))
				ReadAssemblyInfo(obj, ref componentInfo);
		}
	}

	public void SetAssemblerMode(AssemblerMode mode, bool active) {
		switch (mode) {
		case AssemblerMode.WeaponOverviewMode :
			if (active) {
				PreviewWeaponComponents();
				weapon.gameObject.SetActive(true);
				preview.gameObject.SetActive(true);
			} else {
				weapon.gameObject.SetActive(false);
				preview.gameObject.SetActive(false);
				ClearPreviews();
			}
			break;
		case AssemblerMode.ComponentSelectionMode :
			if (active) {
				DisplayWeaponComponents();
				display.gameObject.SetActive(true);
			} else {
				display.gameObject.SetActive(false);
				ClearDisplays();
			}
			break;
		}
	}

	public bool DeliverWeapon(ref GameObject assembledWeapon) {
		if (isAssembling)
			return false;
		if (weapon.GetComponent<Weapon>() == null)
			return false;
		List<string> missingTypes = new List<string>();
		if (!weapon.GetComponent<Weapon>().CheckIntegrity(ref missingTypes))
			return false;
		assembledWeapon = weapon;
		assembledWeapon.BroadcastMessage("CleanUp");
		weapon = new GameObject("Weapon");
		weapon.transform.position = assembledWeapon.transform.position;
		weapon.transform.rotation = assembledWeapon.transform.rotation;
		weapon.transform.parent = assembledWeapon.transform.parent;
		foreach (ComponentInfo cInfo in bodyInfo) {
			cInfo.Reset();
		}
		foreach (ComponentInfo cInfo in partInfo) {
			cInfo.Reset();
		}
		return true;
	}
	
	#region WeaponOverviewMode
	[SerializeField]
	private Transform bodyAnchor;
	[SerializeField]
	private Transform preview;

	public Transform Preview {
		get {
			return preview;
		}
	}

	public void StartAssembly() {
		isAssembling = true;
		Weapon assembledWeapon = weapon.GetComponent<Weapon>();
		if (assembledWeapon != null) {
			assembledWeapon.Body.GetComponent<FocusableWeaponComponent>().RenderAllComponentsWithMaterial(1);
			assembledWeapon.Detach(assembledWeapon.Body, container);
			Object.DestroyImmediate(assembledWeapon);
		}
		PreviewWeaponComponents();
	}

	private void DetachFromWeapon(WeaponComponent c) {
		Weapon w = weapon.GetComponent<Weapon>();
		if (w != null) {
			c.GetComponent<FocusableWeaponComponent>().RenderSubComponentsWithMaterial(1);
			w.Detach(c, container);
		}
	}

	public void DetachAndListOtherComponentSelections(GameObject currentSelection) {
		ComponentInfo cInfo = new WeaponAssembler.ComponentInfo();
		List<ComponentInfo> cInfoList = new List<ComponentInfo>();
		componentDisplayList.Clear();
		componentIndex = -1;
		parentComponent = null;
		if (FindComponentInfo(currentSelection, bodyInfo, ref cInfo)) {
			cInfoList = bodyInfo;
			//Detach并放入Container
			WeaponBody weaponBody = cInfo.Instance.GetComponent<WeaponBody>();
			if (weapon.GetComponent<Weapon>() != null) {
				DetachFromWeapon(weaponBody);
				Object.Destroy(weapon.GetComponent<Weapon>());
			}
		} else if (FindComponentInfo(currentSelection, partInfo, ref cInfo)) {
			WeaponPart weaponPart = cInfo.Instance.GetComponent<WeaponPart>();
			parentComponent = weaponPart.ComponentJoint.TargetComponent;
			ComponentInfo cParentInfo = new WeaponAssembler.ComponentInfo();
			if (FindComponentInfo(parentComponent.gameObject, bodyInfo, ref cParentInfo) || 
			    FindComponentInfo(parentComponent.gameObject, partInfo, ref cParentInfo)) {
				FindAttachablesByType(cParentInfo, weaponPart.ComponentType, ref cInfoList);
			}
			//Detach并放入Container
			if (weaponPart.transform.IsChildOf(preview)) {
				weaponPart.DisassembleTo(container);
			} else if (weaponPart.transform.IsChildOf(weapon.transform)) {
				DetachFromWeapon(weaponPart);
			}
		}
		componentIndex = cInfoList.IndexOf(cInfo);
		currentComponent = componentIndex;
		componentDisplayList.AddRange(cInfoList);
		componentDisplayList.Add(NullComponent);
	}
	
	private void PreviewWeaponParts(ComponentInfo cInfo) {
		Weapon w = weapon.GetComponent<Weapon>();
		foreach (KeyValuePair<string, List<ComponentInfo>> tList in cInfo.attachable) {
			WeaponPart part = w.FindPartByType(tList.Key);
			if (part == null) {
				if (tList.Value.Count > 0) {
					tList.Value[0].Instance.GetComponent<WeaponPart>().JointTo(
						cInfo.Instance.GetComponent<WeaponComponent>());
					tList.Value[0].Instance.transform.parent = preview;
				}
			} else {
				ComponentInfo componentInfo = new ComponentInfo();
				if (FindComponentInfo(part.gameObject, partInfo, ref componentInfo))
					PreviewWeaponParts(componentInfo);
			}
		}
	}

	public void ClearPreviews() {
		foreach (Transform oldPreview in preview) {
			oldPreview.GetComponent<WeaponComponent>().DisassembleTo(container);		
		}
	}
	
	public void PreviewWeaponComponents() {
		//清理之前残余的Preview
		ClearPreviews();
		//新的Preview
		Weapon w = weapon.GetComponent<Weapon>();
		if (w == null) {
			if (bodyInfo.Count > 0) {
				bodyInfo[0].Instance.transform.position = bodyAnchor.position;
				bodyInfo[0].Instance.transform.rotation = bodyAnchor.rotation;
				bodyInfo[0].Instance.transform.parent = preview;
			}
		} else {
			ComponentInfo componentInfo = new ComponentInfo();
			if (FindComponentInfo(w.Body.gameObject, bodyInfo, ref componentInfo))
				PreviewWeaponParts(componentInfo);
		}
	}

	public bool FinishAssembly() {
		Weapon assembledWeapon = weapon.GetComponent<Weapon>();
		if (assembledWeapon == null)
			return false;
		List<string> missingTypes = new List<string>();
		if (!assembledWeapon.CheckIntegrity(ref missingTypes))
			return false;
		ClearPreviews();
		isAssembling = false;
		return true;
	}
	#endregion

	#region ComponentSelectionMode
	[SerializeField]
	private Transform[] componentAnchors = new Transform[3];
	[SerializeField]
	private Transform display;
	private FocusableGameObject[] componentDisplays = new FocusableGameObject[3];
	private List<WeaponAssembler.ComponentInfo> componentDisplayList = new List<WeaponAssembler.ComponentInfo>();
	private int componentIndex = -1;
	private int currentComponent;
	private WeaponComponent parentComponent;

	public Transform Display {
		get {
			return display;
		}
	}

	public FocusableGameObject[] ComponentDisplays {
		get {
			return componentDisplays;
		}
	}

	public ComponentInfo CurrentDisplayedComponentInfo {
		get {
			return componentDisplayList[componentIndex];
		}
	}

	public void AttachCurrentSelection() {
		//还原旧Selection的材质
		GameObject ancientSelection = componentDisplayList[currentComponent].Instance;
		ancientSelection.GetComponent<FocusableWeaponComponent>().RenderAllComponentsWithMaterial(1);
		if (componentIndex != componentDisplayList.Count - 1) {
			//把当前选择从ComponentDisplays中移除以免在OnDisable调用ClearDisplay时将组件设为Inactive
			componentDisplays[1] = null;
			GameObject currentSelection = componentDisplayList[componentIndex].Instance;
			if (parentComponent == null) {
				//UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(weapon, "Assets/Scripts/AssemblyWorkshop/WeaponAssembler.cs (401,5)", componentDisplayList[componentIndex].category);
				var weaponCategory = System.Type.GetType(componentDisplayList[componentIndex].category);
				weapon.AddComponent(weaponCategory);
				WeaponBody weaponBody = currentSelection.GetComponent<WeaponBody>();
				weapon.GetComponent<Weapon>().Body = weaponBody;
			} else {
				WeaponPart weaponPart = currentSelection.GetComponent<WeaponPart>();
				weapon.GetComponent<Weapon>().Attach(weaponPart, parentComponent);
			}
			//赋予当前Selection材质
			currentSelection.GetComponent<FocusableWeaponComponent>().RenderAllComponentsWithMaterial(2);
		}
	}

	public void RotateComponentWheel(int delta) {
		componentIndex += delta;
		if (componentIndex < 0)
			componentIndex = 0;
		if (componentIndex > componentDisplayList.Count - 1)
			componentIndex = componentDisplayList.Count - 1;
		DisplayWeaponComponents();
	}
	
	private void ClearDisplays() {
		for (int i = 0;i < 3;i++) {
			if (componentDisplays[i] != null) {
				componentDisplays[i].transform.parent = container;
			}
		}
		if (componentDisplays[1] != null) {
			WeaponComponent c = componentDisplays[1].GetComponent<WeaponComponent>();
			if (c != null)
				c.SetComponentLayer("Default");
		}
	}
	
	private void DisplayWeaponComponents() {
		//清理之前残余的Display
		ClearDisplays();
		//新的Display
		int[] index = {componentIndex - 1, componentIndex, componentIndex + 1};
		for (int i = 0;i < 3;i++) {
			if (index[i] >= 0 && index[i] <= componentDisplayList.Count - 1) {
				GameObject component = componentDisplayList[index[i]].Instance;
				component.transform.position = componentAnchors[i].position;
				component.transform.rotation = componentAnchors[i].rotation;
				component.transform.parent = display;
				componentDisplays[i] = component.GetComponent<FocusableGameObject>();
			} else {
				componentDisplays[i] = null;
			}
		}
		if (componentDisplays[1] != null) {
			WeaponComponent c = componentDisplays[1].GetComponent<WeaponComponent>();
			if (c != null)
				c.SetComponentLayer("Displayed");
		}
	}
	#endregion
}