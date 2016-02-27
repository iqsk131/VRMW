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



public class EnemyBehavior : MonoBehaviour
{

	public GameObject activeTimeBar;
	public GameObject HPBar;
	public Canvas canvas;
	public GameObject enemy;

	private Animator anim;		
	private float activeTime,startTime;
	private bool firstInit,isAction;
	private int HP;
	private int MaxHP;
	private string playAnim;

	// Use this for initialization
	void Start ()
	{
		firstInit = false;
		anim = transform.FindChild("Model").GetComponentInChildren<Animator> ();
		startTime = 0;
		canvas.enabled = true;
		activeTimeBar.SetActive(true);
		playAnim = "";
		isAction = false;

		VRMWdb.firebase.Child("Enemy").Child("ActiveTime").ValueUpdated += (object sender, ChangedEventArgs e) => {
			activeTime = e.DataSnapshot.FloatValue;
		};

		VRMWdb.firebase.Child ("Enemy").Child("State").ValueUpdated += (object sender, ChangedEventArgs e) => {
			if(e.DataSnapshot.StringValue=="action"){
				isAction=true;
			}
			else{
				isAction=false;
			}
		};
		VRMWdb.firebase.Child ("Enemy").Child("MaxHP").ValueUpdated += (object sender, ChangedEventArgs e) => {
			MaxHP=(int)e.DataSnapshot.FloatValue;
		};
		VRMWdb.firebase.Child ("Enemy").Child("HP").ValueUpdated += (object sender, ChangedEventArgs e) => {
			HP=(int)e.DataSnapshot.FloatValue;
		};

		VRMWdb.firebase.Child ("Enemy").Child("StartTime").ValueUpdated += (object sender, ChangedEventArgs e) => {
			if(!firstInit){
				startTime = Time.time - ((float)currentTime() - e.DataSnapshot.FloatValue);
			}
			else{
				startTime = Time.time;
			}
		};

		VRMWdb.firebase.Child ("Enemy").Child("Attacked").ValueUpdated += (object sender, ChangedEventArgs e) => {
			if(e.DataSnapshot.Child("Damage").FloatValue!=0){
				HP-=(int)e.DataSnapshot.Child("Damage").FloatValue;
				VRMWdb.firebase.Child ("Enemy").Child("HP").SetValue(HP);
				playAnim="DamageDown";
				VRMWdb.firebase.Child ("Enemy").Child("Attacked").Child("Damage").SetValue(0f);
			}
		};

	}

	// Update is called once per frame
	void  Update ()
	{
		if (playAnim != "" && !isAction) {
			anim.Play (playAnim);
			if (playAnim == "DamageDown") {
				anim.Play ("Headspring");
			}
			playAnim = "";
		}
		if (!isAction && Time.time-startTime>=activeTime) {
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
	}


	private IEnumerator startAction(){
		
		VRMWdb.firebase.Child ("Enemy").Child ("State").SetValue ("action");

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
		VRMWdb.firebase.Child ("Player1").Child ("Attacked").Child ("Damage").SetValue (1f);
		yield return new WaitForSeconds(0.5f);

		transform.FindChild("Model").position = transform.position;

		VRMWdb.firebase.Child ("Enemy").Child ("State").SetValue ("idle");
		//isAction = false;
		startTime = Time.time;
		VRMWdb.firebase.Child ("Enemy").Child ("StartTime").SetValue(currentTime().ToString());
	}


	void OnGUI()
	{
		//GUI.Label (new Rect (300, 100, 100, 100), dialog + ": " + (Time.time - startTime) + "/"+activeTime);
	}

	private double currentTime(){
		return (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
	}
}
