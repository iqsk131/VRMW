using UnityEngine;
using System.Collections;
using ProgressBar;
using System.Collections.Generic;
using MovementEffects;

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
	protected Vector3 originalPosition;
	

	protected IEnumerator<float> ActiveTime(){
		while(true){
			yield return Timing.WaitForSeconds(0.1f);
			if (!VRMWdb.isInitiated)
				continue;
			//Behavior Field

			//Rotate HP
			HPBar.transform.rotation = Quaternion.LookRotation(Camera.current.transform.position - HPBar.transform.position) * Quaternion.Euler(0, 180, 0);

			//Update Active Time Circle
			ProgressRadialBehaviour bar = activeTimeBar.GetComponent<ProgressRadialBehaviour>();
			if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getPlayerInfoString(playerNum,"StartTime")))/1000.0 >= VRMWdb.getPlayerInfoFloat(playerNum,"ActiveTime")) {
				bar.Value = 100;
			} else {
				bar.Value = (float)(VRMWdb.currentTime() - double.Parse(VRMWdb.getPlayerInfoString(playerNum,"StartTime")))/1000.0f*100f / VRMWdb.getPlayerInfoFloat(playerNum,"ActiveTime");
			}
			////////////////
		}
		yield return 0f;
	}
	
	protected IEnumerator<float> UpdateHP(){
		
		if (!VRMWdb.isInitiated)
			yield break;
		
		TextMesh HPBarText = HPBar.GetComponent<TextMesh> ();
		HPBarText.text = "" + VRMWdb.getPlayerInfoInt(playerNum,"HP");
		
		yield return 0f;
	}
	
	protected IEnumerator<float> ActionBehavior(){
		while(true){
			yield return Timing.WaitForSeconds(0.1f);
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
						VRMWdb.setStage ("AfterBattle");
						yield break;
					}
				} else {
					this.gameObject.SetActive (false);
					yield break;
				}
			}

			//Check Position
			if(VRMWdb.getPlayerInfoString(playerNum, "Position") == "Back"){
				transform.FindChild ("Model").transform.position = this.transform.position;
			}
			else{
				transform.FindChild ("Model").transform.position = new Vector3 (
					enemy.transform.position.x/2 + this.transform.position.x/2, 
					enemy.transform.position.y/2 + this.transform.position.y/2,
					enemy.transform.position.z/2 + this.transform.position.z/2);
			}

			//Heal
			if (VRMWdb.getPlayerInfoInt (playerNum, "Attacked/Heal") != 0) {
				Damage.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage.transform.position) * Quaternion.Euler (0, 180, 0);
				TextMesh DamageText = Damage.GetComponent<TextMesh> ();
				DamageText.text = "+" + VRMWdb.getPlayerInfoInt (playerNum, "Attacked/Heal");
				Damage.SetActive (true);
				latestShowDamage = Time.time;
				
				VRMWdb.setPlayerInfo (playerNum,"HP",Mathf.Min(VRMWdb.getPlayerInfoInt(playerNum,"MaxHP"), 
				                                               VRMWdb.getPlayerInfoInt (playerNum,"HP") + VRMWdb.getPlayerInfoInt (playerNum,"Attacked/Heal")));
				VRMWdb.setPlayerInfo (playerNum,"Attacked/Heal", 0);
				VRMWdb.addScore("Aid",1);
			}

			//Check if Player got damaged or not
			if (VRMWdb.getPlayerInfoInt (playerNum, "Attacked/Damage") != 0) {

				if(transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().getDefendState()){
					
					Damage.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage.transform.position) * Quaternion.Euler (0, 180, 0);
					TextMesh DamageText = Damage.GetComponent<TextMesh> ();
					DamageText.text = "Block!";
					Damage.SetActive (true);
					latestShowDamage = Time.time;

					AudioClip audioClip = Resources.Load("Audio/SE/040-Knock01", typeof(AudioClip)) as AudioClip;
					VRMWdb.addScore("PerfectGuard",5);
					AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
					VRMWdb.setPlayerInfo (playerNum,"Attacked/Damage", 0);

				}
				else{
					int baseAtk=VRMWdb.getPlayerInfoInt (playerNum,"Attacked/Damage");
					int baseDef=VRMWdb.getPlayerMonsterInfoInt (playerNum,"Def");
					//baseDef = (int)(baseDef * Random.Range(80, 120)/100.0);
					int damage= VRMWdb.CalcDamage(baseAtk,baseDef);

					Damage.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage.transform.position) * Quaternion.Euler (0, 180, 0);
					TextMesh DamageText = Damage.GetComponent<TextMesh> ();
					DamageText.text = "-" + damage;
					Damage.SetActive (true);
					latestShowDamage = Time.time;

					VRMWdb.addScore("DamageReceive",damage);
					VRMWdb.setPlayerInfo (playerNum,"HP",Mathf.Max(0, VRMWdb.getPlayerInfoInt (playerNum,"HP") - damage));
					playAnim = "Damaged";
					VRMWdb.setPlayerInfo (playerNum,"Attacked/Damage", 0);
				}
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
			if ( (VRMWdb.getPlayerInfoString(playerNum,"State")=="idle" || VRMWdb.getPlayerInfoString(playerNum,"State")=="ready") && !stillPlaying) {
				
				//Play Animation if it has
				if (playAnim!="") {
					
					if (playAnim == "Damaged") {
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().damaged(playerNum);
					}
					
					if (playAnim == "Attack") {
						VRMWdb.addScore("ActionUsed",1);
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().attack (enemy.transform.FindChild ("Model"),playerNum,0);
					}

					if (playAnim == "Defend") {
						VRMWdb.addScore("ActionUsed",1);
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().defend(playerNum);
					}

					if (playAnim == "Heal") {
						VRMWdb.addScore("ActionUsed",2);
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().heal(playerNum);
					}

					if (playAnim == "Skill") {
						VRMWdb.addScore("ActionUsed",2);
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().skill (enemy.transform.FindChild ("Model"),playerNum,0);
					}
				}
				
				//If Active time circle full, Player is ready, and Enemy are idle or ready, Attack Ememy
				//Also attack when both Enemy and Player stuck at action state, giving priority to player.
				if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getPlayerInfoString(playerNum,"StartTime")))/1000.0 >= VRMWdb.getPlayerInfoFloat(playerNum,"ActiveTime")
				    && (VRMWdb.getPlayerInfoString(playerNum,"State")=="ready" || (VRMWdb.getPlayerInfoString(playerNum,"State")=="action" && !stillPlaying))
				    && (VRMWdb.getEnemyInfoString ("State") == "idle" 
				    || (VRMWdb.getEnemyInfoString("State")=="action" && !enemy.GetComponent<EnemyBehavior>().stillPlaying)) ) {
					playAnim=VRMWdb.getPlayerInfoString(playerNum,"ActionType");
				}
				
			}
			
			if (VRMWdb.getPlayerInfoString (playerNum, "State") == "ready" 
			    && VRMWdb.getPlayerInfoString (playerNum, "ActionType") == "") {
				VRMWdb.setPlayerInfo(playerNum,"State","idle");
			}

			//Update Ready Symbol
			if (VRMWdb.getPlayerInfoString (playerNum, "State") == "ready") {
				this.transform.FindChild ("Ready").gameObject.SetActive(true);//GetComponent<Renderer> ().enabled = true;
			} else {
				this.transform.FindChild ("Ready").gameObject.SetActive(false);//GetComponent<Renderer> ().enabled = false;
			}
			////////////////
		}
		yield return 0f;
	}
}
