using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

public class TextView : MonoBehaviour {
	[Serializable]
	public class StringEvent : UnityEvent<string>{}

	[SerializeField]
	[TextArea]
	private string textFormat = string.Empty;
	public StringEvent onTrigger = new StringEvent();

	public void SetText(string text) {
		onTrigger.Invoke(string.Format(textFormat, text));
	}
}
