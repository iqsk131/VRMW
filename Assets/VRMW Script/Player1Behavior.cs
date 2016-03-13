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



public class Player1Behavior : MonoBehaviour
{

	public GameObject activeTimeBar;
	public GameObject HPBar;
	public Canvas canvas;
	public GameObject enemy;

	private Animator anim;	
	private double globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
	private float localStartTime;
	private string playAnim = "", dialog="";
	private bool stillPlaying=false;

	// Use this for initialization
	void Start ()
	{
		//Set Animation Component from Model into anim
		anim = transform.FindChild("Model").GetComponentInChildren<Animator> ();

		//Hide Ready Symbol
		this.transform.FindChild ("Ready").GetComponent<Renderer>().enabled = false;

		//Enable Canvas and Bars
		canvas.enabled = true;
		activeTimeBar.SetActive(true);
		HPBar.SetActive (true);

		//Inital StartTime
		globalStartTime=double.Parse(VRMWdb.getPlayerInfoString(1,"StartTime"));
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
		
		//Check if Active Time Bar Reset or not

		//If globalStartTime is not initialized, initialize it
		if (globalStartTime == 0) {
			globalStartTime = double.Parse (VRMWdb.getPlayerInfoString (1, "StartTime"));
			localStartTime = Time.time - (float)(currentTime() - globalStartTime)/1000;
		}

		//Check if Active Time Bar Reset or not
		if (double.Parse(VRMWdb.getPlayerInfoString (1,"StartTime")) != globalStartTime) {
			dialog = double.Parse (VRMWdb.getPlayerInfoString (1, "StartTime")) + " and " + globalStartTime;
			localStartTime = Time.time;
			VRMWdb.setPlayerInfo (1,"StartTime", currentTime ().ToString());
			globalStartTime=double.Parse(VRMWdb.getPlayerInfoString (1,"StartTime"));
		}

		//Check if Player got damaged or not
		if (VRMWdb.getPlayerInfoInt (1, "Attacked/Damage") != 0) {
			VRMWdb.setPlayerInfo (1,"HP", VRMWdb.getPlayerInfoInt (1,"HP") - VRMWdb.getPlayerInfoInt (1,"Attacked/Damage"));
			playAnim = "DamageDown";
			VRMWdb.setPlayerInfo (1,"Attacked/Damage", 0);
		}

		//Do some animations only if Player is idle or ready
		//Also do when Player is stuck at action
		if (VRMWdb.getPlayerInfoString(1,"State")=="idle" 
			|| VRMWdb.getPlayerInfoString(1,"State")=="ready"  
			|| (VRMWdb.getPlayerInfoString(1,"State")=="action" && !stillPlaying)) {

			//Play Misc Animation if it has
			if (playAnim!="") {

				//Backup Current State
				string currentState = VRMWdb.getPlayerInfoString (1, "State");

				//Change Player State to action
				VRMWdb.setPlayerInfo (1,"State", "action");
				stillPlaying = true;

				//Damage Animation
				if (playAnim == "DamageDown") {
					anim.Play (playAnim);
					anim.Play ("Headspring");
				}

				//Reset Misc Animation
				playAnim = "";

				//Change Player to backup state after action
				VRMWdb.setPlayerInfo (1,"State", currentState);
				stillPlaying = false;
			}

			//If Active time circle full, Player is ready, and Enemy are idle or ready, Attack Ememy
			//Also attack when both Enemy and Player stuck at action state, giving priority to player.
			if (Time.time - localStartTime >= VRMWdb.getPlayerInfoFloat(1,"ActiveTime")
				&& (VRMWdb.getPlayerInfoString(1,"State")=="ready" || (VRMWdb.getPlayerInfoString(1,"State")=="action" && !stillPlaying))
				&& (VRMWdb.getEnemyInfoString ("State") == "idle" 
				    || VRMWdb.getEnemyInfoString("State")=="action" && !enemy.GetComponent<EnemyBehavior>().stillPlaying) ) {
				StartCoroutine (startAction ());
			}
		}

		//Update HP Bar
		ProgressBarBehaviour HPBarBehavior = HPBar.GetComponent<ProgressBarBehaviour> ();
		HPBarBehavior.Value = (float)VRMWdb.getPlayerInfoInt(1,"HP")*100f/VRMWdb.getPlayerInfoInt(1,"MaxHP");

		//Update Active Time Circle
		ProgressRadialBehaviour bar = activeTimeBar.GetComponent<ProgressRadialBehaviour>();
		if (Time.time - localStartTime >= VRMWdb.getPlayerInfoFloat(1,"ActiveTime")) {
			bar.Value = 100;
		} else {
			bar.Value = (Time.time - localStartTime)*100 / VRMWdb.getPlayerInfoFloat(1,"ActiveTime");
		}

		//Update Ready Symbol
		if (VRMWdb.getPlayerInfoString (1, "State") == "ready") {
			this.transform.FindChild ("Ready").GetComponent<Renderer> ().enabled = true;
		} else {
			this.transform.FindChild ("Ready").GetComponent<Renderer> ().enabled = false;
		}


	}


	private IEnumerator startAction(){

		//Change Player State to action
		VRMWdb.setPlayerInfo (1,"State", "action");
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
		VRMWdb.setEnemyInfo ("Attacked/Damage", VRMWdb.getEnemyInfoInt ("Attacked/Damage") + 1);
		
		//Wait for 0.5 sec
		yield return new WaitForSeconds(0.5f);


		//Warp back
		transform.FindChild("Model").position = transform.position;

		//Change Player to Idle after action
		VRMWdb.setPlayerInfo (1,"State", "idle");
		stillPlaying = false;

		//Reset the Active Time Circle
		localStartTime = Time.time;
		VRMWdb.setPlayerInfo (1,"StartTime", currentTime ().ToString());

	}


	void OnGUI()
	{
		GUI.Label (new Rect (300, 100, 100, 100), dialog);
	}

	/// Return current System time as millisecond
	private double currentTime(){
		return (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
	}
}
