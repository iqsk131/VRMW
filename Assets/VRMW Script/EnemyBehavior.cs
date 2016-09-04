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
	//private string playAnim="";
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
		//playAnim = "";
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

			HPBar.transform.rotation = Quaternion.LookRotation(Camera.current.transform.position - HPBar.transform.position) * Quaternion.Euler(0, 180, 0);

			ProgressRadialBehaviour bar = activeTimeBar.GetComponent<ProgressRadialBehaviour>();
			if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getEnemyInfoString("StartTime")))/1000.0 >= VRMWdb.getEnemyInfoFloat("ActiveTime")) {
				bar.Value = 100;
			} else {
				bar.Value = (float)(VRMWdb.currentTime() - double.Parse(VRMWdb.getEnemyInfoString("StartTime")))/1000f*100.0f / VRMWdb.getEnemyInfoFloat("ActiveTime");
			}

			//If Active time circle full, and player are idle or ready, Attack player
			if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getEnemyInfoString("StartTime")))/1000.0 >= VRMWdb.getEnemyInfoFloat("ActiveTime")
			    && !stillPlaying){
				//TO-DO enemy behavior

				//Front Player
				bool AnyFront = false;
				for(int i=1;i<=3;i++)
					if(VRMWdb.getPlayerInfoString(i, "Position") != "Back" 
					   && VRMWdb.getPlayerInfoString(i, "State") != "dead")AnyFront = true;
				
				targetPlayer = Random.Range (1, 4);
				if (targetPlayer != 1 && targetPlayer != 2)targetPlayer = 3;
				while(VRMWdb.getPlayerInfoString (targetPlayer, "State") == "dead" 
				      || (AnyFront && VRMWdb.getPlayerInfoString(targetPlayer, "Position") == "Back")){
					targetPlayer = targetPlayer %3 +1;
				}
				
				VRMWdb.setEnemyInfo ("Target", targetPlayer);

				int randomAction = Random.Range (0, 10);
				if(randomAction<5)
					VRMWdb.setEnemyInfo ("ActionType", "Attack");
				else if(randomAction<8)
					VRMWdb.setEnemyInfo ("ActionType", "Defend");
				else
					VRMWdb.setEnemyInfo ("ActionType", "Skill");
			}
			////////////////
		}
		yield return 0;
	}

	private IEnumerator UpdateHP(){
		
		if (!VRMWdb.isInitiated)
			yield break;
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

			
			bool isBlock = false;

			//Heal
			if (VRMWdb.getEnemyInfoInt ("Attacked/Heal") != 0) {
				Damage2.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage2.transform.position) * Quaternion.Euler (0, 180, 0);
				TextMesh DamageText = Damage2.GetComponent<TextMesh> ();
				DamageText.text = "+" + VRMWdb.getEnemyInfoInt ("Attacked/Heal");
				Damage2.SetActive (true);
				latestShowDamage = Time.time;
				
				VRMWdb.setEnemyInfo ("HP",Mathf.Min(VRMWdb.getEnemyInfoInt("MaxHP"), 
				                                    VRMWdb.getEnemyInfoInt ("HP") + VRMWdb.getEnemyInfoInt ("Attacked/Heal")));
				VRMWdb.setEnemyInfo ("Attacked/Heal", 0);
				isBlock=true;
			}
			
			//Check if Enemey got damaged or not
			if (VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage") != 0
			    || VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage") != 0
			    || VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage") != 0) {

				if(transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().getDefendState()){
					if (VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage") != 0) {
						Damage1.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage1.transform.position) * Quaternion.Euler (0, 180, 0);
						TextMesh DamageText = Damage1.GetComponent<TextMesh> ();
						DamageText.text = "Block!";
						Damage1.SetActive (true);
					}
					
					if (VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage") != 0) {
						Damage2.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage2.transform.position) * Quaternion.Euler (0, 180, 0);
						TextMesh DamageText = Damage2.GetComponent<TextMesh> ();
						DamageText.text = "-" + VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage");
						DamageText.text = "Block!";
						Damage2.SetActive (true);
					}
					
					if (VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage") != 0) {
						Damage3.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage3.transform.position) * Quaternion.Euler (0, 180, 0);
						TextMesh DamageText = Damage3.GetComponent<TextMesh> ();
						DamageText.text = "Block!";
						Damage3.SetActive (true);
					}
					
					latestShowDamage = Time.time;
					AudioClip audioClip = Resources.Load("Audio/SE/040-Knock01", typeof(AudioClip)) as AudioClip;
					AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
					VRMWdb.setEnemyInfo ("Attacked/Player1/Damage", 0);
					VRMWdb.setEnemyInfo ("Attacked/Player2/Damage", 0);
					VRMWdb.setEnemyInfo ("Attacked/Player3/Damage", 0);
					isBlock=true;
				}
				else{
					int baseDef=VRMWdb.getEnemyMonsterInfoInt("Def");
					baseDef = (int)(baseDef * Random.Range(80, 120)/100.0);
					//Update DamageText
					if (VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage") != 0) {
						Damage1.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage1.transform.position) * Quaternion.Euler (0, 180, 0);
						TextMesh DamageText = Damage1.GetComponent<TextMesh> ();
						DamageText.text = "-" +  VRMWdb.CalcDamage(VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage"),baseDef);
						Damage1.SetActive (true);
					}
					
					if (VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage") != 0) {
						Damage2.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage2.transform.position) * Quaternion.Euler (0, 180, 0);
						TextMesh DamageText = Damage2.GetComponent<TextMesh> ();
						DamageText.text = "-" +  VRMWdb.CalcDamage(VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage"),baseDef);
						Damage2.SetActive (true);
					}
					
					if (VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage") != 0) {
						Damage3.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage3.transform.position) * Quaternion.Euler (0, 180, 0);
						TextMesh DamageText = Damage3.GetComponent<TextMesh> ();
						DamageText.text = "-" +  VRMWdb.CalcDamage(VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage"),baseDef);
						Damage3.SetActive (true);
					}

					latestShowDamage = Time.time;


					int baseAtk=VRMWdb.getEnemyInfoInt ("Attacked/Player1/Damage")+VRMWdb.getEnemyInfoInt ("Attacked/Player2/Damage")+VRMWdb.getEnemyInfoInt ("Attacked/Player3/Damage");
					int damage= VRMWdb.CalcDamage(baseAtk,baseDef);

					VRMWdb.setEnemyInfo ("HP", Mathf.Max(VRMWdb.getEnemyInfoInt ("HP") - damage,0));
					
					VRMWdb.setEnemyInfo ("ActionType", "Damaged");
					VRMWdb.setEnemyInfo ("Attacked/Player1/Damage", 0);
					VRMWdb.setEnemyInfo ("Attacked/Player2/Damage", 0);
					VRMWdb.setEnemyInfo ("Attacked/Player3/Damage", 0);
				}
			}
			if (Time.time - latestShowDamage > 2) {
				
				int damageNum=0;
				if(Damage1.gameObject.activeSelf && !isBlock)damageNum++;
				if(Damage2.gameObject.activeSelf && !isBlock)damageNum++;
				if(Damage3.gameObject.activeSelf && !isBlock)damageNum++;

				Damage1.SetActive (false);
				Damage2.SetActive (false);
				Damage3.SetActive (false);
				//Combo
				int before = 0;
				int after = 0;
				for(int i = 1;i<=3;i++){
					if(VRMWdb.getCombo(i,true)<VRMWdb.getCombo(i,false)){
						if(VRMWdb.getCombo(i,true)==0)
							before += VRMWdb.getPlayerMonsterInfoInt(i,"Atk");
						else
							before += VRMWdb.getCombo(i,true);
						after += VRMWdb.getCombo(i,false);
					}
				}
				if(damageNum>1 && before<after){
					Damage2.transform.rotation = Quaternion.LookRotation (Camera.current.transform.position - Damage2.transform.position) * Quaternion.Euler (0, 180, 0);
					TextMesh DamageText = Damage2.GetComponent<TextMesh> ();
					DamageText.text = "<color=red><size=128>Combo</size></color>\n       <color=orange><size=128>"+(int)(after*100/before)+"%</size></color>";
					Damage2.SetActive (true);
					AudioClip audioClip = Resources.Load("Audio/SE/056-Right02", typeof(AudioClip)) as AudioClip;
					AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
					latestShowDamage = Time.time;
				}
				for(int i = 1;i<=3;i++){
						VRMWdb.setCombo(i,true,0);
						VRMWdb.setCombo(i,false,0);
				}
			}
			
			//Check if animation end or not
			if (stillPlaying && VRMWdb.getEnemyInfoString ("State") == "idle") {
				stillPlaying = false;
				VRMWdb.setEnemyInfo ("ActionType", "");
			}
			
			//Do some animations only if Enemey is idle
			//Also do when Enemy is stuck at action
			if (VRMWdb.getEnemyInfoString("State")=="idle" || (VRMWdb.getEnemyInfoString("State")=="action" && !stillPlaying)) {
				
				//Play Misc Animation if it has
				if (VRMWdb.getEnemyInfoString("ActionType")!="") {
					
					if (VRMWdb.getEnemyInfoString("ActionType") == "Damaged") {
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().damaged(0);
						VRMWdb.setEnemyInfo ("ActionType", "");
					}

					if (VRMWdb.getEnemyInfoString("ActionType") == "Defend") {
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().defend(0);
						VRMWdb.setEnemyInfo ("ActionType", "");
					}

					if (VRMWdb.getEnemyInfoString("ActionType") == "Heal") {
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().heal(0);
						VRMWdb.setEnemyInfo ("ActionType", "");
					}
					
					if (VRMWdb.getEnemyInfoString("ActionType") == "Attack") {
						stillPlaying = true;

						targetPlayer=VRMWdb.getEnemyInfoInt ("Target");

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
						   && VRMWdb.getPlayerInfoString (3, "State") != "action"){
							transform.FindChild ("Model").GetComponentInChildren<ModelInterface>().attack(targetEnemy.transform.FindChild ("Model"),0,targetPlayer);
							VRMWdb.setEnemyInfo ("ActionType", "");
						}
					}

					if (VRMWdb.getEnemyInfoString("ActionType") == "Skill") {
						stillPlaying = true;
						
						targetPlayer=VRMWdb.getEnemyInfoInt ("Target");
						
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
						   && VRMWdb.getPlayerInfoString (3, "State") != "action"){
							transform.FindChild ("Model").GetComponentInChildren<ModelInterface>().skill(targetEnemy.transform.FindChild ("Model"),0,targetPlayer);
							VRMWdb.setEnemyInfo ("ActionType", "");
						}
					}
				}
			}
			////////////////
		}
		yield return 0;
	}
}
