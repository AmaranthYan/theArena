﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LayoutGroup))]
public class ConsumableObjectCounter : MonoBehaviour {
	[Header("Injection")]
	[SerializeField]
	private GameObject unitIndicatorPrefab = null;
	[Header("Animator")]
	[SerializeField]
	private string consumeAnimatorParam = string.Empty;
	[SerializeField]
	private string refillAnimatorParam = string.Empty;

	public UnityEvent onNoObject = new UnityEvent();
	public UnityEvent onRefill = new UnityEvent();

	private List<GameObject> indicators = new List<GameObject>();
	private int remaining = 0;

	// Use this for initialization
	void Start () {
		Initialize(20);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.X)) {
			Consume(2);}
		if (Input.GetKey (KeyCode.C)) {
			Refill();}
	}

	public void Initialize(int amount) {
		foreach (GameObject indicator in indicators) {
			GameObject.Destroy(indicator);		
		}
		indicators.Clear();
		for (int i = 0; i < amount; i++) {
			GameObject unit = GameObject.Instantiate(unitIndicatorPrefab) as GameObject;
			unit.transform.SetParent(this.transform);
			unit.transform.localPosition = Vector3.zero;
			unit.transform.localRotation = Quaternion.identity;
			indicators.Add(unit);
		}

		remaining = amount;
	}

	public void Consume(int amount = 1) {
		if (remaining <= 0) {
			onNoObject.Invoke();
			return;		
		}
		for (int i = remaining - 1; i >= Mathf.Max(remaining - amount, 0); i--) {
			Animator animator = indicators[i].GetComponentInChildren<Animator>();
			animator.SetTrigger(consumeAnimatorParam);
		}
		remaining = Mathf.Max(remaining - amount, 0);
		if (remaining <= 0) {
			onNoObject.Invoke();		
		}
	}

	public void Refill(int amount = short.MaxValue) {
		if (amount <= 0) {
			return;		
		}
		for (int i = remaining; i < Mathf.Min(remaining + amount, indicators.Count); i++) {
			Animator animator = indicators[i].GetComponentInChildren<Animator>();
			animator.SetTrigger(refillAnimatorParam);
		}
		remaining = Mathf.Min(remaining + amount, indicators.Count);
		onRefill.Invoke();
	}
}
