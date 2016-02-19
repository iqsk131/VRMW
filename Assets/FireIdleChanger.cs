using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
// ↑↓キーでループアニメーションを切り替えるスクリプト（ランダム切り替え付き）Ver.3
// 2014/04/03 N.Kobayashi
//

// Require these components when using this script
using ProgressBar;


[RequireComponent(typeof(Animator))]



public class FireIdleChanger : MonoBehaviour
{

	public GameObject activeTimeBar;
	public Canvas canvas;

	private Animator anim;		
	private bool isReady;
	private float activeTime,startTime;
	private string dialog;

	// Use this for initialization
	void Start ()
	{
		anim = GetComponent<Animator> ();
		dialog= "Intial..";
		isReady = false;
		this.transform.FindChild ("Ready").GetComponent<Renderer>().enabled = false;
		startTime = Time.time;
		canvas.enabled = true;
		activeTimeBar.SetActive(true);

		VRMWdb.firebase.Child("Player1").Child("ActiveTime").ValueUpdated += (object sender, ChangedEventArgs e) => {
			activeTime = e.DataSnapshot.FloatValue;
		};

		VRMWdb.firebase.Child ("Player1").Child("State").ValueUpdated += (object sender, ChangedEventArgs e) => {
			if(e.DataSnapshot.StringValue=="ready"){
				isReady=true;
				anim.Play ("Land");
				dialog="Change!!";
			}
		};
		VRMWdb.firebase.Child ("Player1").Child("StartTime").ValueUpdated += (object sender, ChangedEventArgs e) => {
			startTime = e.DataSnapshot.FloatValue;
		};

		VRMWdb.firebase.Child ("Player1").Child ("StartTime").SetValue(startTime);
	}

	// Update is called once per frame
	void  Update ()
	{

		activeTimeBar.SetActive(true);
		if (isReady && Time.time-startTime>=activeTime) {
			anim.Play ("Hikick");
			AudioClip audioClip = Resources.Load("Blow1", typeof(AudioClip)) as AudioClip;
			AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			isReady=false;
			VRMWdb.firebase.Child ("Player1").Child ("State").SetValue ("idle");
			startTime = Time.time;
			VRMWdb.firebase.Child ("Player1").Child ("StartTime").SetValue((float) startTime);
		}
		ProgressRadialBehaviour bar = activeTimeBar.GetComponent<ProgressRadialBehaviour>();
		if (Time.time - startTime >= activeTime) {
			bar.Value = 100;
		} else {
			bar.Value = (Time.time - startTime)*100 / activeTime;
		}
		this.transform.FindChild ("Ready").GetComponent<Renderer>().enabled = isReady;
	}


	void OnGUI()
	{
		GUI.Label (new Rect (300, 100, 100, 100), dialog + ": " + (Time.time - startTime) + "/"+activeTime);
	}


}
