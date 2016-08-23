using UnityEngine;
using System.Collections;
using ProgressBar;

public abstract class PlayerBehavior : MonoBehaviour {

	protected int playerNum;

	public GameObject activeTimeBar;
	public GameObject HPBar;
	public GameObject Damage;
	public Canvas canvas;
	public GameObject enemy;
	public bool stillPlaying=false;
	protected float latestShowDamage;
	protected string playAnim = "";
	

	protected IEnumerator ActiveTime(){
		while(true){
			yield return new WaitForSeconds(0.1f);
			if (!VRMWdb.isInitiated)
				continue;
			//Behavior Field

			//Update Active Time Circle
			ProgressRadialBehaviour bar = activeTimeBar.GetComponent<ProgressRadialBehaviour>();
			if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getPlayerInfoString(playerNum,"StartTime")))/1000.0 >= VRMWdb.getPlayerInfoFloat(playerNum,"ActiveTime")) {
				bar.Value = 100;
			} else {
				bar.Value = (float)(VRMWdb.currentTime() - double.Parse(VRMWdb.getPlayerInfoString(playerNum,"StartTime")))/1000.0f*100f / VRMWdb.getPlayerInfoFloat(playerNum,"ActiveTime");
			}
			////////////////
		}
		yield return 0;
	}
	
	protected IEnumerator UpdateHP(){
		
		if (!VRMWdb.isInitiated)
			yield break;
		
		HPBar.transform.rotation = Quaternion.LookRotation(Camera.current.transform.position - HPBar.transform.position) * Quaternion.Euler(0, 180, 0);
		TextMesh HPBarText = HPBar.GetComponent<TextMesh> ();
		HPBarText.text = "" + VRMWdb.getPlayerInfoInt(playerNum,"HP");
		
		yield return 0;
	}
	
	protected IEnumerator ActionBehavior(){
		while(true){
			yield return new WaitForSeconds(0.1f);
			if (!VRMWdb.isInitiated)
				continue;
			//Behavior Field
			if (VRMWdb.getPlayerInfoString (playerNum,"State") == "dead" || VRMWdb.getPlayerInfoInt (playerNum,"HP") <= 0) {
				if (VRMWdb.getPlayerInfoString (playerNum, "State") != "dead")
					VRMWdb.setPlayerInfo (playerNum, "State", "dead");  
				if (VRMWdb.getPlayerInfoString (1, "State") == "dead"
				    && VRMWdb.getPlayerInfoString (2, "State") == "dead"
				    && VRMWdb.getPlayerInfoString (3, "State") == "dead"){
					if (VRMWdb.getEnemyInfoString ("State") != "action") {
						VRMWdb.setStage ("Initial");
						yield break;
					}
				} else {
					this.gameObject.SetActive (false);
					yield break;
				}
			}
			
			//Check if Player got damaged or not
			if (VRMWdb.getPlayerInfoInt (playerNum, "Attacked/Damage") != 0) {
				
				Damage.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage.transform.position) * Quaternion.Euler (0, 180, 0);
				TextMesh DamageText = Damage.GetComponent<TextMesh> ();
				DamageText.text = "-" + VRMWdb.getPlayerInfoInt (playerNum, "Attacked/Damage");
				Damage.SetActive (true);
				latestShowDamage = Time.time;
				
				VRMWdb.setPlayerInfo (playerNum,"HP",Mathf.Max(0, VRMWdb.getPlayerInfoInt (playerNum,"HP") - VRMWdb.getPlayerInfoInt (playerNum,"Attacked/Damage")));
				playAnim = "Damaged";
				VRMWdb.setPlayerInfo (playerNum,"Attacked/Damage", 0);
				
			}
			if (Time.time - latestShowDamage > 2) {
				Damage.SetActive (false);
			}
			
			
			
			//Check if animation end or not
			if (stillPlaying && (VRMWdb.getPlayerInfoString (playerNum, "State") == "idle"||VRMWdb.getPlayerInfoString (playerNum, "State") == "ready")) {
				stillPlaying = false;
				playAnim = "";
			}
			
			//Do some animations only if Player is idle or ready
			//Also do when Player is stuck at action
			if (VRMWdb.getPlayerInfoString(playerNum,"State")=="idle" 
			    || VRMWdb.getPlayerInfoString(playerNum,"State")=="ready"  
			    || (VRMWdb.getPlayerInfoString(playerNum,"State")=="action" && !stillPlaying)) {
				
				//Play Animation if it has
				if (playAnim!="") {
					
					if (playAnim == "Damaged") {
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().damaged(playerNum);
					}
					
					if (playAnim == "Attack") {
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().attack (enemy.transform.FindChild ("Model"),playerNum,0);
					}
				}
				
				//If Active time circle full, Player is ready, and Enemy are idle or ready, Attack Ememy
				//Also attack when both Enemy and Player stuck at action state, giving priority to player.
				if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getPlayerInfoString(playerNum,"StartTime")))/1000.0 >= VRMWdb.getPlayerInfoFloat(playerNum,"ActiveTime")
				    && (VRMWdb.getPlayerInfoString(playerNum,"State")=="ready" || (VRMWdb.getPlayerInfoString(playerNum,"State")=="action" && !stillPlaying))
				    && (VRMWdb.getEnemyInfoString ("State") == "idle" 
				    || (VRMWdb.getEnemyInfoString("State")=="action" && !enemy.GetComponent<EnemyBehavior>().stillPlaying)) ) {
					playAnim="Attack";
				}
				
			}
			
			//Update Ready Symbol
			if (VRMWdb.getPlayerInfoString (playerNum, "State") == "ready") {
				this.transform.FindChild ("Ready").GetComponent<Renderer> ().enabled = true;
			} else {
				this.transform.FindChild ("Ready").GetComponent<Renderer> ().enabled = false;
			}
			////////////////
		}
		yield return 0;
	}
}
