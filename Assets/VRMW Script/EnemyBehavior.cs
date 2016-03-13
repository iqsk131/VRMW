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
	public bool stillPlaying;

	private Animator anim;		
	private float localStartTime;
	private double globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
	private string playAnim="";

	// Use this for initialization
	void Start ()
	{

		//Set Animation Component from Model into anim
		anim = transform.FindChild("Model").GetComponentInChildren<Animator> ();

		//Enable Canvas and Bars
		canvas.enabled = true;
		activeTimeBar.SetActive(true);
		HPBar.SetActive (true);

		//Inital StartTime
		globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
		localStartTime = Time.time - (float)(currentTime() - globalStartTime)/1000;

		//Initial current action
		playAnim = "";
		stillPlaying = false;

	}

	// Update is called once per frame
	void  Update ()
	{
		//Don't Start update until VRMWdb is initiated
		if (!VRMWdb.isInitiated)
			return;

		//If globalStartTime is not initialized, initialize it
		if (globalStartTime == 0) {
			globalStartTime = double.Parse (VRMWdb.getEnemyInfoString ("StartTime"));
			localStartTime = Time.time - (float)(currentTime() - globalStartTime)/1000;
		}

		//Check if Active Time Bar Reset or not
		if (double.Parse(VRMWdb.getEnemyInfoString ("StartTime")) != globalStartTime) {
			localStartTime = Time.time;
			VRMWdb.setEnemyInfo ("StartTime", currentTime ().ToString());
			globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
		}

		//Check if Enemey got damaged or not
		if (VRMWdb.getEnemyInfoInt ("Attacked/Damage") != 0) {
			VRMWdb.setEnemyInfo ("HP", VRMWdb.getEnemyInfoInt ("HP") - VRMWdb.getEnemyInfoInt ("Attacked/Damage"));
			playAnim = "DamageDown";
			VRMWdb.setEnemyInfo ("Attacked/Damage", 0);
		}

		//Do some animations only if Enemey is idle
		//Also do when Enemy is stuck at action
		if (VRMWdb.getEnemyInfoString("State")=="idle" || (VRMWdb.getEnemyInfoString("State")=="action" && !stillPlaying)) {

			//Play Misc Animation if it has
			if (playAnim!="") {
				//Change Enemy State to action
				VRMWdb.setEnemyInfo ("State", "action");
				stillPlaying = true;

				//Damage Animation
				if (playAnim == "DamageDown") {
					anim.Play (playAnim);
					anim.Play ("Headspring");
				}

				//Reset Misc Animation
				playAnim = "";

				//Change Enemy to Idle after action
				VRMWdb.setEnemyInfo ("State", "idle");
				stillPlaying = false;

			}

			//If Active time circle full, and player are idle or ready, Attack player
			if (Time.time - localStartTime >= VRMWdb.getEnemyInfoFloat("ActiveTime")
				&& (VRMWdb.getPlayerInfoString (1, "State") == "idle" || VRMWdb.getPlayerInfoString (1, "State") == "ready")) {
				StartCoroutine (startAction ());
			}
		}

		//Update HP Bar
		ProgressBarBehaviour HPBarBehavior = HPBar.GetComponent<ProgressBarBehaviour> ();
		HPBarBehavior.Value = (float)VRMWdb.getEnemyInfoInt("HP")*100f/VRMWdb.getEnemyInfoInt("MaxHP");

		//Update Active Time Circle
		ProgressRadialBehaviour bar = activeTimeBar.GetComponent<ProgressRadialBehaviour>();
		if (Time.time - localStartTime >= VRMWdb.getEnemyInfoFloat("ActiveTime")) {
			bar.Value = 100;
		} else {
			bar.Value = (Time.time - localStartTime)*100 / VRMWdb.getEnemyInfoFloat("ActiveTime");
		}
	}


	private IEnumerator startAction(){
		//Change Enemy State to action
		VRMWdb.setEnemyInfo ("State", "action");
		stillPlaying = true;

		//Warp to Player
		Vector3 newTarget = new Vector3 (
			3*enemy.transform.FindChild("Model").position.x/5 + 2*transform.position.x/5, 
			3*enemy.transform.FindChild("Model").position.y/5 + 2*transform.position.y/5,
			3*enemy.transform.FindChild("Model").position.z/5 + 2*transform.position.z/5);
		transform.FindChild("Model").position = newTarget;

		//Wait for 0.5 sec
		yield return new WaitForSeconds(0.5f);

		//Play Attack animation
		anim.Play ("Hikick");
		AudioClip audioClip = Resources.Load("Audio/SE/Blow1", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);

		//Wait for 0.1 sec
		yield return new WaitForSeconds(0.1f);

		//Give damage to player
		VRMWdb.setPlayerInfo (1, "Attacked/Damage", VRMWdb.getPlayerInfoInt (1, "Attacked/Damage") + 1);

		//Wait for 0.5 sec
		yield return new WaitForSeconds(0.5f);

		//Warp back
		transform.FindChild("Model").position = transform.position;

		//Change Enemy to Idle after action
		VRMWdb.setEnemyInfo ("State", "idle");
		stillPlaying = false;

		//Reset the Active Time Circle
		localStartTime = Time.time;
		VRMWdb.setEnemyInfo ("StartTime", currentTime ().ToString());
	}



	/// Return current System time as millisecond
	private double currentTime(){
		return (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
	}
}
