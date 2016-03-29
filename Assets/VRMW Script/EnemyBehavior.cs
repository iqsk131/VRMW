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

	private float localStartTime;
	private double globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
	private string playAnim="";

	// Use this for initialization
	void Start ()
	{

		//Enable Canvas and Bars
		canvas.enabled = true;
		activeTimeBar.SetActive(true);
		HPBar.SetActive (true);

		//Inital StartTime
		globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
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
			globalStartTime = double.Parse (VRMWdb.getEnemyInfoString ("StartTime"));
			localStartTime = Time.time - (float)( VRMWdb.currentTime () - globalStartTime)/1000;
		}

		//Check if Active Time Bar Reset or not
		if (double.Parse(VRMWdb.getEnemyInfoString ("StartTime")) != globalStartTime) {
			localStartTime = Time.time;
			VRMWdb.setEnemyInfo ("StartTime",  VRMWdb.currentTime ().ToString());
			globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
		}

		//Check if Enemey got damaged or not
		if (VRMWdb.getEnemyInfoInt ("Attacked/Damage") != 0) {
			VRMWdb.setEnemyInfo ("HP", VRMWdb.getEnemyInfoInt ("HP") - VRMWdb.getEnemyInfoInt ("Attacked/Damage"));
			playAnim = "Damaged";
			VRMWdb.setEnemyInfo ("Attacked/Damage", 0);
		}

		//Check if animation end or not
		if (stillPlaying && VRMWdb.getEnemyInfoString ("State") == "idle") {
			stillPlaying = false;
			playAnim = "";
		}

		//Do some animations only if Enemey is idle
		//Also do when Enemy is stuck at action
		if (VRMWdb.getEnemyInfoString("State")=="idle" || (VRMWdb.getEnemyInfoString("State")=="action" && !stillPlaying)) {

			//Play Misc Animation if it has
			if (playAnim!="") {
				
				if (playAnim == "Damaged") {
					stillPlaying = true;
					transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().damaged(0);
				}

				if (playAnim == "Attack") {
					stillPlaying = true;
					transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().attack (enemy.transform.FindChild ("Model"),0);
				}

			}

			//If Active time circle full, and player are idle or ready, Attack player
			if (Time.time - localStartTime >= VRMWdb.getEnemyInfoFloat("ActiveTime")
				&& (VRMWdb.getPlayerInfoString (1, "State") == "idle" || VRMWdb.getPlayerInfoString (1, "State") == "ready")) {
				playAnim="Attack";
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
		
}
