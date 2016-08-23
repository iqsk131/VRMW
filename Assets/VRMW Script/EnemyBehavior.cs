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
	public GameObject Damage1;
	public GameObject Damage2;
	public GameObject Damage3;
	public Canvas canvas;
	public GameObject enemy1;
	public GameObject enemy2;
	public GameObject enemy3;
	public bool stillPlaying;

	//private float localStartTime;
	//private double globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
	private string playAnim="";
	private float latestShowDamage;
	private int targetPlayer;
	public GameObject targetEnemy;

	// Use this for initialization
	void OnEnable ()
	{

		//Enable Canvas and Bars
		canvas.enabled = true;
		activeTimeBar.SetActive(true);
		HPBar.SetActive (true);

		//Inital StartTime
		//globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
		//localStartTime = Time.time - (float)(VRMWdb.currentTime() - globalStartTime)/1000;

		//Initial current action
		playAnim = "";
		stillPlaying = false;
		latestShowDamage = Time.time - 2;
		targetPlayer = 1;

		StartCoroutine(ActiveTime());
		StartCoroutine(ActionBehavior());
		StartCoroutine(UpdateHP());
		VRMWdb.OnEnemyHPChange += (bool st) => {
			StartCoroutine(UpdateHP());
		};

	}


	private IEnumerator ActiveTime(){
		while(true){
			yield return new WaitForSeconds(0.1f);
			if (!VRMWdb.isInitiated)
				continue;
			//Behavior Field
			ProgressRadialBehaviour bar = activeTimeBar.GetComponent<ProgressRadialBehaviour>();
			Debug.Log ("Bar: " + (VRMWdb.currentTime() - double.Parse(VRMWdb.getEnemyInfoString("StartTime"))));
			if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getEnemyInfoString("StartTime")))/1000.0 >= VRMWdb.getEnemyInfoFloat("ActiveTime")) {
				bar.Value = 100;
			} else {
				bar.Value = (float)(VRMWdb.currentTime() - double.Parse(VRMWdb.getEnemyInfoString("StartTime")))/1000f*100.0f / VRMWdb.getEnemyInfoFloat("ActiveTime");
			}

			//If Active time circle full, and player are idle or ready, Attack player
			if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getEnemyInfoString("StartTime")))/1000.0 >= VRMWdb.getEnemyInfoFloat("ActiveTime")){
				//TO-DO enemy behavior
				playAnim="Attack";
			}
			////////////////
		}
		yield return 0;
	}

	private IEnumerator UpdateHP(){
		
		if (!VRMWdb.isInitiated)
			yield break;
		HPBar.transform.rotation = Quaternion.LookRotation(Camera.current.transform.position - HPBar.transform.position) * Quaternion.Euler(0, 180, 0);
		TextMesh HPBarText = HPBar.GetComponent<TextMesh> ();
		HPBarText.text = "" + VRMWdb.getEnemyInfoInt("HP");

		yield return 0;
	}

	private IEnumerator ActionBehavior(){
		while(true){
			yield return new WaitForSeconds(0.1f);
			if (!VRMWdb.isInitiated)
				continue;
			//Behavior Field

			//If Player Dead
			if (VRMWdb.getPlayerInfoString (1, "State") == "dead"
			    && VRMWdb.getPlayerInfoString (2, "State") == "dead"
			    && VRMWdb.getPlayerInfoString (3, "State") == "dead") {
				VRMWdb.setStage ("Initial");
				yield break;
			}

			//Change State
			if (VRMWdb.getEnemyInfoString ("State") == "dead" || VRMWdb.getEnemyInfoInt ("HP") <= 0) {
				if (VRMWdb.getEnemyInfoString ( "State") != "dead")
					VRMWdb.setEnemyInfo ("State", "dead");  
				if (VRMWdb.getPlayerInfoString (1, "State") != "action"
				    && VRMWdb.getPlayerInfoString (2, "State") != "action"
				    && VRMWdb.getPlayerInfoString (3, "State") != "action") {
					VRMWdb.setStage ("Initial");
					yield break;
				}
			}
			
			//If globalStartTime is not initialized, initialize it
			/*if (globalStartTime == 0) {
				globalStartTime = double.Parse (VRMWdb.getEnemyInfoString ("StartTime"));
				localStartTime = Time.time - (float)( VRMWdb.currentTime () - globalStartTime)/1000;
			}
			
			//Check if Active Time Bar Reset or not
			if (double.Parse(VRMWdb.getEnemyInfoString ("StartTime")) != globalStartTime) {
				localStartTime = Time.time;
				VRMWdb.setEnemyInfo ("StartTime",  VRMWdb.currentTime ().ToString());
				globalStartTime=double.Parse(VRMWdb.getEnemyInfoString("StartTime"));
			}*/
			
			//Check if Enemey got damaged or not
			if (VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage") != 0
			    || VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage") != 0
			    || VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage") != 0) {
				
				//Update DamageText
				if (VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage") != 0) {
					Damage1.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage1.transform.position) * Quaternion.Euler (0, 180, 0);
					TextMesh DamageText = Damage1.GetComponent<TextMesh> ();
					DamageText.text = "-" + VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage");
					Damage1.SetActive (true);
				}
				
				if (VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage") != 0) {
					Damage2.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage2.transform.position) * Quaternion.Euler (0, 180, 0);
					TextMesh DamageText = Damage2.GetComponent<TextMesh> ();
					DamageText.text = "-" + VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage");
					Damage2.SetActive (true);
				}
				
				if (VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage") != 0) {
					Damage3.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage3.transform.position) * Quaternion.Euler (0, 180, 0);
					TextMesh DamageText = Damage3.GetComponent<TextMesh> ();
					DamageText.text = "-" + VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage");
					Damage3.SetActive (true);
				}
				latestShowDamage = Time.time;
				
				VRMWdb.setEnemyInfo ("HP", Mathf.Max(VRMWdb.getEnemyInfoInt ("HP") 
				                                     - VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage") 
				                                     - VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage") 
				                                     - VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage"),0));
				playAnim = "Damaged";
				VRMWdb.setEnemyInfo ("Attacked/Player1/Damage", 0);
				VRMWdb.setEnemyInfo ("Attacked/Player2/Damage", 0);
				VRMWdb.setEnemyInfo ("Attacked/Player3/Damage", 0);
			}
			if (Time.time - latestShowDamage > 2) {
				Damage1.SetActive (false);
				Damage2.SetActive (false);
				Damage3.SetActive (false);
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
						targetPlayer = Random.Range (1, 4);
						if (targetPlayer != 1 && targetPlayer != 2)targetPlayer = 3;
						while(VRMWdb.getPlayerInfoString (targetPlayer, "State") == "dead"){
							targetPlayer = targetPlayer %3 +1;
						}
						if (targetPlayer == 1) {
							targetEnemy = enemy1;
						} else if (targetPlayer == 2) {
							targetEnemy = enemy2;
						} else {
							targetPlayer = 3;
							targetEnemy = enemy3;
						}
						if( VRMWdb.getPlayerInfoString (1, "State") != "action" 
						   && VRMWdb.getPlayerInfoString (2, "State") != "action" 
						   && VRMWdb.getPlayerInfoString (3, "State") != "action")
							transform.FindChild ("Model").GetComponentInChildren<ModelInterface>().attack(targetEnemy.transform.FindChild ("Model"),0,targetPlayer);
					}
				}
			}
			////////////////
		}
		yield return 0;
	}

	// Update is called once per frame
	/*void  Update ()
	{
		//Don't Start update until VRMWdb is initiated
		if (!VRMWdb.isInitiated)
			return;

		if (VRMWdb.getEnemyInfoString ("State") == "dead" || VRMWdb.getEnemyInfoInt ("HP") <= 0) {
			if (VRMWdb.getEnemyInfoString ( "State") != "dead")
				VRMWdb.setEnemyInfo ("State", "dead");  
			if (VRMWdb.getPlayerInfoString (1, "State") != "action"
				&& VRMWdb.getPlayerInfoString (2, "State") != "action"
				&& VRMWdb.getPlayerInfoString (3, "State") != "action") {
				VRMWdb.setStage ("Initial");
				return;
			}
		}

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
		if (VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage") != 0
			|| VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage") != 0
			|| VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage") != 0) {

			//Update DamageText
			if (VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage") != 0) {
				Damage1.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage1.transform.position) * Quaternion.Euler (0, 180, 0);
				TextMesh DamageText = Damage1.GetComponent<TextMesh> ();
				DamageText.text = "-" + VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage");
				Damage1.SetActive (true);
			}

			if (VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage") != 0) {
				Damage2.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage2.transform.position) * Quaternion.Euler (0, 180, 0);
				TextMesh DamageText = Damage2.GetComponent<TextMesh> ();
				DamageText.text = "-" + VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage");
				Damage2.SetActive (true);
			}

			if (VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage") != 0) {
				Damage3.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage3.transform.position) * Quaternion.Euler (0, 180, 0);
				TextMesh DamageText = Damage3.GetComponent<TextMesh> ();
				DamageText.text = "-" + VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage");
				Damage3.SetActive (true);
			}
			latestShowDamage = Time.time;

			VRMWdb.setEnemyInfo ("HP", Mathf.Max(VRMWdb.getEnemyInfoInt ("HP") 
				- VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage") 
				- VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage") 
				- VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage"),0));
			playAnim = "Damaged";
			VRMWdb.setEnemyInfo ("Attacked/Player1/Damage", 0);
			VRMWdb.setEnemyInfo ("Attacked/Player2/Damage", 0);
			VRMWdb.setEnemyInfo ("Attacked/Player3/Damage", 0);
		}
		if (Time.time - latestShowDamage > 2) {
			Damage1.SetActive (false);
			Damage2.SetActive (false);
			Damage3.SetActive (false);
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
					targetPlayer = Random.Range (1, 4);
					if (targetPlayer != 1 && targetPlayer != 2)targetPlayer = 3;
					while(VRMWdb.getPlayerInfoString (targetPlayer, "State") == "dead"){
						targetPlayer = targetPlayer %3 +1;
					}
					if (targetPlayer == 1) {
						targetEnemy = enemy1;
					} else if (targetPlayer == 2) {
						targetEnemy = enemy2;
					} else {
						targetPlayer = 3;
						targetEnemy = enemy3;
					}
					if( VRMWdb.getPlayerInfoString (1, "State") != "action" 
						&& VRMWdb.getPlayerInfoString (2, "State") != "action" 
						&& VRMWdb.getPlayerInfoString (3, "State") != "action")
					transform.FindChild ("Model").GetComponentInChildren<ModelInterface>().attack(targetEnemy.transform.FindChild ("Model"),0,targetPlayer);
				}

			}

			//If Active time circle full, and player are idle or ready, Attack player
			if (Time.time - localStartTime >= VRMWdb.getEnemyInfoFloat("ActiveTime")){
				playAnim="Attack";
			}
		}

		//Update HP Bar
		HPBar.transform.rotation = Quaternion.LookRotation(Camera.current.transform.position - HPBar.transform.position) * Quaternion.Euler(0, 180, 0);
		TextMesh HPBarText = HPBar.GetComponent<TextMesh> ();
		HPBarText.text = "" + VRMWdb.getEnemyInfoInt("HP");

		//Update Active Time Circle
		ProgressRadialBehaviour bar = activeTimeBar.GetComponent<ProgressRadialBehaviour>();
		if (Time.time - localStartTime >= VRMWdb.getEnemyInfoFloat("ActiveTime")) {
			bar.Value = 100;
		} else {
			bar.Value = (Time.time - localStartTime)*100 / VRMWdb.getEnemyInfoFloat("ActiveTime");
		}
	}*/
		
}
