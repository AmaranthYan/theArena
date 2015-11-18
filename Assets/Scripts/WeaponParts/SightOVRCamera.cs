using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

//通过继承OVRComponent类重写部分MonoBehavior和OVR方法,合并Camera与CameraController
//实现Camera的Transform脱离CameraController的强制
[RequireComponent(typeof(Camera))]
public class SightOVRCamera : OVRComponent {
	//Camera深度应始终大于OVRCamera深度(默认左眼为1,右眼为0)以确保正确的渲染顺序
	private const float CAMERA_DEPTH = 2.0f;
	private const float MAX_FOCAL_LENGTH = 1000.0f;
	[SerializeField]
	protected GameObject renderCamera;
	public Material crosshair;

	//注释请参看Occulus官方Doc
	#region Inherited or Altered OVR Variables
	private bool UpdateCamerasDirtyFlag = false;
	//LensOffset默认为0.0f
	private float LensOffset = 0.0f;
	private float AspectRatio = 1.0f;						
	private float DistK0, DistK1, DistK2, DistK3 = 0.0f;
	[SerializeField]
	private float verticalFOV = 90.0f;
	public 	float VerticalFOV {
		get {
			return verticalFOV;
		}
		set {
			verticalFOV = value;
			UpdateCamerasDirtyFlag = true;
		}
	}
	private RenderTexture CameraTexture	= null;
	private Material ColorOnlyMaterial = null;
	private Color QuadColor = Color.red;
	private float CameraTextureScale = 1.0f;
	public bool	WireMode = false;
	public bool LensCorrection = true;
	public bool Chromatic = true;
	#endregion

	private void ConfigureRenderCamera() {
		if (renderCamera != null)
			renderCamera.GetComponent<Camera>().fieldOfView = GetComponent<Camera>().fieldOfView;
	}

	public Transform GetCameraFocus() {
		//DepthofFieldScatter是UnityScript,此处并非声明错误
		DepthOfFieldScatter DepthOfField = this.GetComponent<DepthOfFieldScatter>();
		RaycastHit focus;
		Physics.Raycast(transform.position, transform.forward, out focus, MAX_FOCAL_LENGTH);
		DepthOfField.focalLength = focus.transform != null ? focus.distance : MAX_FOCAL_LENGTH;
		//DepthOfField.focalTransform = focus.transform;
		return DepthOfField.focalTransform;
	}

	private void RenderCrosshair(RenderTexture targetTexture) {
		Graphics.Blit(targetTexture, targetTexture, crosshair);
	}

	//PreLensCorrection渲染(所有的用户ImageEffects以及GUITextures)
	//Should be integrated into OVRSDK!
	public virtual void OnPreLensCorrectionRender(RenderTexture intermediateTexture) {
		RenderCrosshair(intermediateTexture);
	}

	#region Inherited or Altered OVR Functions
	new void Awake() {
		base.Awake();
		InitCameraVariables();
		SetMaximumVisualQuality();
		if (ColorOnlyMaterial == null)
			ColorOnlyMaterial = new Material("Shader \"Solid Color\" {\n" +	"Properties {\n" + 
			                                 "_Color (\"Color\", Color) = (1,1,1)\n" + "}\n" +
			                                 "SubShader {\n" + "Color [_Color]\n" +	"Pass {}\n" +
			                                 "}\n" + "}");
	}

	new void Start() {
		base.Start();
		UpdateCamerasDirtyFlag = true;
		UpdateCameras();
		//CameraTextureScale = OVRDevice.DistortionScale();
		if ((CameraTexture == null) && (CameraTextureScale != 1.0f)) {
			int w = (int)(Screen.width / 2.0f * CameraTextureScale);
			int h = (int)(Screen.height * CameraTextureScale);
			if (GetComponent<Camera>().hdr)
				CameraTexture = new RenderTexture(w, h, 24, RenderTextureFormat.ARGBFloat);
			else
				CameraTexture = new RenderTexture(w, h, 24);
			CameraTexture.antiAliasing = (QualitySettings.antiAliasing == 0) ? 1 : QualitySettings.antiAliasing;
		}
	}

	new void Update() {
		base.Update ();
		UpdateCameras();
	}

	//在PreCull阶段修改CameraOrientation,而不是在PreRender阶段
	void OnPreCull() {
		SetCameraOrientation();
		GetCameraFocus();
	}
	
	void OnPreRender() {
		if (WireMode == true)
			GL.wireframe = true;
		if (CameraTexture != null) {
			Graphics.SetRenderTarget(CameraTexture);
			GL.Clear(true, true, GetComponent<Camera>().backgroundColor);
		}
	}
	
	void OnPostRender() {
		if(WireMode == true)
			GL.wireframe = false;
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination) {	
		RenderTexture SourceTexture = source;
		if (CameraTexture != null)
			SourceTexture = CameraTexture;
		//Any GUI effects goes here in order to apply the OVRLensCorrection correctly!
		OnPreLensCorrectionRender(SourceTexture);
		Material material = null;
		if (LensCorrection == true) {
			if (Chromatic == true)
				material = this.GetComponent<OVRLensCorrection>().GetMaterial_CA(false);
			else
				material = this.GetComponent<OVRLensCorrection>().GetMaterial(false);				
		}
		if (material != null)
			Graphics.Blit(SourceTexture, destination, material);
		else
			Graphics.Blit(SourceTexture, destination);			
		LatencyTest(destination);
	}

	public void GetVerticalFOV(ref float verticalFOV) {
		verticalFOV = VerticalFOV;
	}

	public void SetVerticalFOV(float verticalFOV) {
		VerticalFOV = verticalFOV;
		UpdateCamerasDirtyFlag = true;
	}

	public void GetAspectRatio(ref float aspecRatio) {
		aspecRatio = AspectRatio;
	}
	public void SetAspectRatio(float aspectRatio) {
		AspectRatio = aspectRatio;
		UpdateCamerasDirtyFlag = true;
	}
	
	public void GetDistortionCoefs(ref float distK0, ref float distK1, ref float distK2, ref float distK3) {
		distK0 = DistK0;
		distK1 = DistK1;
		distK2 = DistK2;
		distK3 = DistK3;
	}
	
	public void SetDistortionCoefs(float distK0, float distK1, float distK2, float distK3) {
		DistK0 = distK0;
		DistK1 = distK1;
		DistK2 = distK2;
		DistK3 = distK3;
		UpdateCamerasDirtyFlag = true;
	}
	
	public void AttachGameObjectToCamera(ref GameObject gameObject)	{	
		gameObject.transform.parent = transform;
	}
	
	public bool DetachGameObjectFromCamera(ref GameObject gameObject) {
		if (transform == gameObject.transform.parent) {
			gameObject.transform.parent = null;
			return true;
		}
		return false;
	}
	
	public void SetMaximumVisualQuality() {
		QualitySettings.softVegetation = true;
		QualitySettings.maxQueuedFrames = 0;
		QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
		QualitySettings.vSyncCount = 1;
	}
	
	public void SetPerspectiveOffset(ref Vector3 offset) {
		GetComponent<Camera>().ResetProjectionMatrix();
		Matrix4x4 om = Matrix4x4.identity;
		om.SetColumn(3, new Vector4(offset.x, offset.y, 0.0f, 1.0f));
		GetComponent<Camera>().projectionMatrix = om * GetComponent<Camera>().projectionMatrix;
	}
	
	public void InitCameraVariables()	{
		VerticalFOV = OVRDevice.VerticalFOV();
		AspectRatio = OVRDevice.CalculateAspectRatio();		
		OVRDevice.GetDistortionCorrectionCoefficients(ref DistK0, ref DistK1, ref DistK2, ref DistK3);
		//设置Camera深度
		GetComponent<Camera>().depth = CAMERA_DEPTH;
	}
	
	void UpdateCameras() {
		if (UpdateCamerasDirtyFlag == false)
			return;
		float distOffset = 0.5f + (LensOffset * 0.5f);
		float perspOffset = LensOffset;
		ConfigureCamera(distOffset, perspOffset);
		//使RenderTextureCamera的FoV与SightCamera一致
		ConfigureRenderCamera();
		UpdateCamerasDirtyFlag = false;
	}
	
	bool ConfigureCamera(float distOffset, float perspOffset) {
		Vector3 PerspOffset = Vector3.zero;
		GetComponent<Camera>().fieldOfView = VerticalFOV;
		GetComponent<Camera>().aspect = AspectRatio;
		GetComponent<Camera>().GetComponent<OVRLensCorrection>()._Center.x = distOffset;
		ConfigureCameraLensCorrection();
		PerspOffset.x = perspOffset;
		SetPerspectiveOffset(ref PerspOffset);
		return true;
	}
	
	void ConfigureCameraLensCorrection() {
		float distortionScale = 1.0f / OVRDevice.DistortionScale();
		float aspectRatio = OVRDevice.CalculateAspectRatio();
		float NormalizedWidth = 1.0f;
		float NormalizedHeight = 1.0f;
		OVRLensCorrection lc = GetComponent<Camera>().GetComponent<OVRLensCorrection>();
		lc._Scale.x = (NormalizedWidth / 2.0f) * distortionScale;
		lc._Scale.y = (NormalizedHeight / 2.0f) * distortionScale * aspectRatio;
		lc._ScaleIn.x = (2.0f / NormalizedWidth);
		lc._ScaleIn.y = (2.0f / NormalizedHeight) / aspectRatio;
		lc._HmdWarpParam.x = DistK0;
		lc._HmdWarpParam.y = DistK1;
		lc._HmdWarpParam.z = DistK2;
	}

	void SetCameraOrientation() {
		Quaternion xyRotation = Quaternion.identity;
		if (transform.parent != null) {
			Vector3 parentRotation = transform.parent.eulerAngles;
			xyRotation = Quaternion.Euler(parentRotation.x, parentRotation.y, 0.0f);
		}
		//旋转Camera的z轴与头部偏移对齐,使用GetPredictedOrientation也可替换为GetOrientation
		Quaternion cameraOrientation = Quaternion.identity;
		OVRDevice.GetPredictedOrientation(0, ref cameraOrientation);
		OVRDevice.ProcessLatencyInputs();
		Quaternion zRotation = Quaternion.Euler(0.0f, 0.0f, cameraOrientation.eulerAngles.z);
		transform.rotation = xyRotation * zRotation;
	}

	void LatencyTest(RenderTexture dest) {
		byte r = 0,g = 0, b = 0;
		string s = Marshal.PtrToStringAnsi(OVRDevice.GetLatencyResultsString());
		if (s != null) {
			string result = "\n\n---------------------\nLATENCY TEST RESULTS:\n---------------------\n";
			result += s;
			result += "\n\n\n";
			print(result);
		}
		if (OVRDevice.DisplayLatencyScreenColor(ref r, ref g, ref b) == false)
			return;
		RenderTexture.active = dest;  		
		Material material = ColorOnlyMaterial;
		QuadColor.r = (float)r / 255.0f;
		QuadColor.g = (float)g / 255.0f;
		QuadColor.b = (float)b / 255.0f;
		material.SetColor("_Color", QuadColor);
		GL.PushMatrix();
		material.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.QUADS);
		GL.Vertex3(0.3f,0.3f,0);
		GL.Vertex3(0.3f,0.7f,0);
		GL.Vertex3(0.7f,0.7f,0);
		GL.Vertex3(0.7f,0.3f,0);
		GL.End();
		GL.PopMatrix();
	}
	#endregion
}
