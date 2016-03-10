﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
// ↑↓キーでループアニメーションを切り替えるスクリプト（ランダム切り替え付き）Ver.3
// 2014/04/03 N.Kobayashi
//

// Require these components when using this script
using ProgressBar;


[RequireComponent(typeof(Animator))]



public class Player1Behavior : MonoBehaviour
{

	public GameObject activeTimeBar;
	public GameObject HPBar;
	public Canvas canvas;
	public GameObject enemy;

	private Animator anim;		
	private bool isReady;
	private float activeTime,startTime;
	private string dialog;
	private bool firstInit;
	private int HP;
	private int MaxHP;
	private string playAnim;

	// Use this for initialization
	void Start ()
	{
		firstInit = false;
		anim = transform.FindChild("Model").GetComponentInChildren<Animator> ();
		dialog= "Intial..";
		isReady = false;
		this.transform.FindChild ("Ready").GetComponent<Renderer>().enabled = false;
		startTime = 0;
		canvas.enabled = true;
		activeTimeBar.SetActive(true);
		playAnim = "";

		VRMWdb.firebase.Child("Player1").Child("ActiveTime").ValueUpdated += (object sender, ChangedEventArgs e) => {
			activeTime = e.DataSnapshot.FloatValue;
		};

		VRMWdb.firebase.Child ("Player1").Child("State").ValueUpdated += (object sender, ChangedEventArgs e) => {
			if(e.DataSnapshot.StringValue=="ready"){
				isReady=true;
				dialog="Change!!";
			}
		};
		VRMWdb.firebase.Child ("Player1").Child("MaxHP").ValueUpdated += (object sender, ChangedEventArgs e) => {
			MaxHP=(int)e.DataSnapshot.FloatValue;
		};
		VRMWdb.firebase.Child ("Player1").Child("HP").ValueUpdated += (object sender, ChangedEventArgs e) => {
			HP=(int)e.DataSnapshot.FloatValue;
		};

		VRMWdb.firebase.Child ("Player1").Child("StartTime").ValueUpdated += (object sender, ChangedEventArgs e) => {
			if(!firstInit){
				startTime = Time.time - ((float)currentTime() - e.DataSnapshot.FloatValue);
			}
			else{
				startTime = Time.time;
			}
		};
		VRMWdb.firebase.Child ("Player1").Child("Attacked").ValueUpdated += (object sender, ChangedEventArgs e) => {
			if(e.DataSnapshot.Child("Damage").FloatValue!=0){
				HP-=(int)e.DataSnapshot.Child("Damage").FloatValue;
				playAnim="DamageDown";
				VRMWdb.firebase.Child ("Player1").Child("HP").SetValue(HP);
				VRMWdb.firebase.Child ("Player1").Child("Attacked").Child("Damage").SetValue(0f);
			}
		};
		//VRMWdb.firebase.Child ("Player1").Child ("StartTime").SetValue(currentTime());
	}

	// Update is called once per frame
	void  Update ()
	{
		if (playAnim != "") {
			anim.Play (playAnim);
			if (playAnim == "DamageDown") {
				anim.Play ("Headspring");
			}
			playAnim = "";
		}
		if (isReady && Time.time-startTime>=activeTime) {
			StartCoroutine (startAction ());
		}
		ProgressBarBehaviour HPBarBehavior = HPBar.GetComponent<ProgressBarBehaviour> ();
		HPBarBehavior.Value = (float)HP*100f/MaxHP;

		ProgressRadialBehaviour bar = activeTimeBar.GetComponent<ProgressRadialBehaviour>();
		if (Time.time - startTime >= activeTime) {
			bar.Value = 100;
		} else {
			bar.Value = (Time.time - startTime)*100 / activeTime;
		}
		this.transform.FindChild ("Ready").GetComponent<Renderer>().enabled = isReady;


	}


	private IEnumerator startAction(){

		isReady=false;

		Vector3 newTarget = new Vector3 (
			3*enemy.transform.FindChild("Model").position.x/5 + 2*transform.position.x/5, 
			3*enemy.transform.FindChild("Model").position.y/5 + 2*transform.position.y/5,
			3*enemy.transform.FindChild("Model").position.z/5 + 2*transform.position.z/5);

		transform.FindChild("Model").position = newTarget;

		yield return new WaitForSeconds(0.5f);

		anim.Play ("Hikick");
		AudioClip audioClip = Resources.Load("Audio/SE/Blow1", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);


		yield return new WaitForSeconds(0.1f);
		VRMWdb.firebase.Child ("Enemy").Child ("Attacked").Child ("Damage").SetValue (1f);
		yield return new WaitForSeconds(0.5f);

		transform.FindChild("Model").position = transform.position;


		VRMWdb.firebase.Child ("Player1").Child ("State").SetValue ("idle");
		startTime = Time.time;
		VRMWdb.firebase.Child ("Player1").Child ("StartTime").SetValue(currentTime().ToString());
	}


	void OnGUI()
	{
		GUI.Label (new Rect (300, 100, 100, 100), dialog + ": " + (Time.time - startTime) + "/"+activeTime);
	}

	private double currentTime(){
		return (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
	}
}