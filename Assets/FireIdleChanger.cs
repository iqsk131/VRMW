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
	public GameObject enemy;

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
		startTime = 0;
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
			startTime = Time.time;
		};

		//VRMWdb.firebase.Child ("Player1").Child ("StartTime").SetValue(currentTime());
	}

	// Update is called once per frame
	void  Update ()
	{

		if (isReady && Time.time-startTime>=activeTime) {
			StartCoroutine (startAction ());
		}
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
			3*enemy.transform.position.x/5 + 2*transform.parent.position.x/5, 
			3*enemy.transform.position.y/5 + 2*transform.parent.position.y/5,
			3*enemy.transform.position.z/5 + 2*transform.parent.position.z/5);

		transform.position = newTarget;

		yield return new WaitForSeconds(0.5f);

		anim.Play ("Hikick");
		AudioClip audioClip = Resources.Load("Audio/SE/Blow1", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);

		yield return new WaitForSeconds(0.5f);

		transform.position = transform.parent.position;


		VRMWdb.firebase.Child ("Player1").Child ("State").SetValue ("idle");
		startTime = Time.time;
		VRMWdb.firebase.Child ("Player1").Child ("StartTime").SetValue(currentTime().ToString());
	}


	void OnGUI()
	{
		GUI.Label (new Rect (300, 100, 100, 100), dialog + ": " + (Time.time - startTime) + "/"+activeTime);
	}

	private double currentTime(){
		return (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds / 1000.0;
	}
}
