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
	public bool stillPlaying=false;
	private double globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
	private float localStartTime;
	private string playAnim = "", dialog="";

	// Use this for initialization
	void Start ()
	{

		//Hide Ready Symbol
		this.transform.FindChild ("Ready").GetComponent<Renderer>().enabled = false;

		//Enable Canvas and Bars
		canvas.enabled = true;
		activeTimeBar.SetActive(true);
		HPBar.SetActive (true);

		//Inital StartTime
		globalStartTime=double.Parse(VRMWdb.getPlayerInfoString(1,"StartTime"));
		localStartTime = Time.time - (float)(VRMWdb.currentTime() - globalStartTime)/1000;

		//Initial current action
		playAnim = "";
		stillPlaying = false;

	}

	// Call when reactivate
	public void reActivate(){
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
			globalStartTime = double.Parse (VRMWdb.getPlayerInfoString (1, "StartTime"));
			localStartTime = Time.time - (float)(VRMWdb.currentTime() - globalStartTime)/1000;
		}

		//Check if Active Time Bar Reset or not
		if (double.Parse(VRMWdb.getPlayerInfoString (1,"StartTime")) != globalStartTime) {
			dialog = double.Parse (VRMWdb.getPlayerInfoString (1, "StartTime")) + " and " + globalStartTime;
			localStartTime = Time.time;
			VRMWdb.setPlayerInfo (1,"StartTime", VRMWdb.currentTime ().ToString());
			globalStartTime=double.Parse(VRMWdb.getPlayerInfoString (1,"StartTime"));
		}

		//Check if Player got damaged or not
		if (VRMWdb.getPlayerInfoInt (1, "Attacked/Damage") != 0) {
			
			VRMWdb.setPlayerInfo (1,"HP", VRMWdb.getPlayerInfoInt (1,"HP") - VRMWdb.getPlayerInfoInt (1,"Attacked/Damage"));
			playAnim = "Damaged";
			VRMWdb.setPlayerInfo (1,"Attacked/Damage", 0);

		}

		//Check if animation end or not
		if (stillPlaying && (VRMWdb.getPlayerInfoString (1, "State") == "idle"||VRMWdb.getPlayerInfoString (1, "State") == "ready")) {
			stillPlaying = false;
			playAnim = "";
		}

		//Do some animations only if Player is idle or ready
		//Also do when Player is stuck at action
		if (VRMWdb.getPlayerInfoString(1,"State")=="idle" 
			|| VRMWdb.getPlayerInfoString(1,"State")=="ready"  
			|| (VRMWdb.getPlayerInfoString(1,"State")=="action" && !stillPlaying)) {

			//Play Animation if it has
			if (playAnim!="") {

				if (playAnim == "Damaged") {
					stillPlaying = true;
					transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().damaged(1);
				}

				if (playAnim == "Attack") {
					stillPlaying = true;
					transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().attack (enemy.transform.FindChild ("Model"),1);
				}
			}

			//If Active time circle full, Player is ready, and Enemy are idle or ready, Attack Ememy
			//Also attack when both Enemy and Player stuck at action state, giving priority to player.
			if (Time.time - localStartTime >= VRMWdb.getPlayerInfoFloat(1,"ActiveTime")
				&& (VRMWdb.getPlayerInfoString(1,"State")=="ready" || (VRMWdb.getPlayerInfoString(1,"State")=="action" && !stillPlaying))
				&& (VRMWdb.getEnemyInfoString ("State") == "idle" 
					|| (VRMWdb.getEnemyInfoString("State")=="action" && !enemy.GetComponent<EnemyBehavior>().stillPlaying)) ) {
				playAnim="Attack";
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


	void OnGUI()
	{
		GUI.Label (new Rect (300, 100, 100, 100), dialog);
	}
		
}
