using UnityEngine;
using System.Collections;

public class CameraFadeIn : MonoBehaviour {
	private float alpha = 1.0f;
	public float fadeTime = 2.0f;
	public Texture fadeTexture = null;

	void OnGUI() {	
		if (Event.current.type != EventType.Repaint)
			return;
		if (alpha > 0.0f) {
			alpha -= Mathf.Clamp01(Time.deltaTime / fadeTime);
			if (alpha < 0.0f) {
				alpha = 0.0f;	
			} else {
				GUI.color = new Color(0, 0, 0, alpha);
				GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);
			}
		}
	}
}
