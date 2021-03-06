﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;
//
// ↑↓キーでループアニメーションを切り替えるスクリプト（ランダム切り替え付き）Ver.3
// 2014/04/03 N.Kobayashi
//

// Require these components when using this script
using ProgressBar;
using System;


[RequireComponent(typeof(Animator))]
public class EnemyBehavior : MonoBehaviour
{

	[SerializeField] GameObject[] targetPoint;

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
		targetPlayer = 0;

		Timing.RunCoroutine(ActiveTime());
		Timing.RunCoroutine(ActionBehavior());
		Timing.RunCoroutine(UpdateHP());
		VRMWdb.OnEnemyHPChange += (bool st) => {
			Timing.RunCoroutine(UpdateHP());
		};

	}


	private IEnumerator<float> ActiveTime(){
		while(true){
			yield return Timing.WaitForSeconds(0.1f);
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

			//Random Target and Action
			if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getEnemyInfoString("StartTime")))/1000.0 >= VRMWdb.getEnemyInfoFloat("ActiveTime") - 5f
				&& !stillPlaying
				&& VRMWdb.getEnemyInfoString("ActionType") == ""){
				//TO-DO enemy behavior

				//Front Player
				bool AnyFront = false;
				for(int i=1;i<=3;i++)
					if(VRMWdb.getPlayerInfoString(i, "Position") != "Back" 
						&& VRMWdb.getPlayerInfoString(i, "State") != "dead")AnyFront = true;

				targetPlayer = UnityEngine.Random.Range (1, 4);
				if (targetPlayer != 1 && targetPlayer != 2)targetPlayer = 3;
				while(VRMWdb.getPlayerInfoString (targetPlayer, "State") == "dead" 
					|| (AnyFront && VRMWdb.getPlayerInfoString(targetPlayer, "Position") == "Back")){
					targetPlayer = targetPlayer %3 +1;
				}

				VRMWdb.setEnemyInfo ("Target", targetPlayer);

				yield return Timing.WaitForSeconds(0.1f);

				targetPlayer = VRMWdb.getEnemyInfoInt("Target");

				for(int i = 1 ; i <= 3 ; i++ ) targetPoint[i].SetActive(false);
				if(targetPlayer>=1 && targetPlayer<=3)targetPoint[targetPlayer].SetActive(true);

				int bid = VRMWdb.getEnemyInfoInt ("BID");

				//For slime, use skill when high chance to use skill when hp low
				if(bid==3 && UnityEngine.Random.Range (0, 100)>(VRMWdb.getEnemyInfoInt ("HP")*100/VRMWdb.getEnemyInfoInt ("MaxHP")) && VRMWdb.getEnemyInfoString ("ActionType")==""){
					if(AnyFront)
						VRMWdb.addScore("Hero",1);
					VRMWdb.setEnemyInfo ("ActionType", "Skill");
				}

				//For dragon, use skill when target has low hp
				if(bid==4 && VRMWdb.getPlayerInfoInt(targetPlayer,"HP")<=200 && VRMWdb.getEnemyInfoString ("ActionType")==""){
					if(AnyFront)
						VRMWdb.addScore("Hero",1);
					VRMWdb.setEnemyInfo ("ActionType", "Skill");
				}

				//There are players that will attack boss, use defend
				int numPlayerAttack=0;
				for(int i=1;i<=3;i++){
					if(VRMWdb.getPlayerInfoString(i,"ActionType") == "Attack" || VRMWdb.getPlayerInfoString(i,"ActionType") == "Skill"){
						numPlayerAttack++;
					}
				}
				int randomAction = UnityEngine.Random.Range (0, 100);
				if(randomAction<numPlayerAttack*60 && VRMWdb.getEnemyInfoString ("ActionType")==""){
					if(bid==5)
						VRMWdb.setEnemyInfo ("ActionType", "Skill");
					else
						VRMWdb.setEnemyInfo ("ActionType", "Defend");
				}

				//If target not use defend, Attack
				if(VRMWdb.getPlayerInfoString(targetPlayer, "ActionType")!="Defend" && VRMWdb.getEnemyInfoString ("ActionType")==""){
					if(AnyFront)
						VRMWdb.addScore("Hero",1);
					VRMWdb.setEnemyInfo ("ActionType", "Attack");
					if(UnityEngine.Random.Range (0, 100)>50 && (bid==3 || bid==4) ){
						VRMWdb.setEnemyInfo ("ActionType", "Skill");
					}
				}

				if(VRMWdb.getEnemyInfoString ("ActionType")==""){
					randomAction = UnityEngine.Random.Range (0, 10);
					if(randomAction<5){
						if(AnyFront)
							VRMWdb.addScore("Hero",1);
						VRMWdb.setEnemyInfo ("ActionType", "Attack");
					}
					else if(randomAction<8)
						VRMWdb.setEnemyInfo ("ActionType", "Defend");
					else{
						VRMWdb.setEnemyInfo ("ActionType", "Skill");
						if(AnyFront)
							VRMWdb.addScore("Hero",1);
					}
				}
			}
			////////////////
		}
		yield return 0f;
	}

	private IEnumerator<float> UpdateHP(){
		
		if (!VRMWdb.isInitiated)
			yield break;
		TextMesh HPBarText = HPBar.GetComponent<TextMesh> ();
		HPBarText.text = "" + VRMWdb.getEnemyInfoInt("HP");

		yield return 0f;
	}

	private IEnumerator<float> ActionBehavior(){
		while(true){
			yield return Timing.WaitForSeconds(0.1f);
			if (!VRMWdb.isInitiated)
				continue;
			//Behavior Field


			//If Player Dead
			if (VRMWdb.getPlayerInfoString (1, "State") == "dead"
			    && VRMWdb.getPlayerInfoString (2, "State") == "dead"
			    && VRMWdb.getPlayerInfoString (3, "State") == "dead") {
				VRMWdb.setStage ("AfterBattle");
				yield break;
			}

			//Change State
			if (VRMWdb.getEnemyInfoString ("State") == "dead" || VRMWdb.getEnemyInfoInt ("HP") <= 0) {
				if (VRMWdb.getEnemyInfoString ( "State") != "dead")
					VRMWdb.setEnemyInfo ("State", "dead");  
				if (VRMWdb.getPlayerInfoString (1, "State") != "action"
				    && VRMWdb.getPlayerInfoString (2, "State") != "action"
				    && VRMWdb.getPlayerInfoString (3, "State") != "action") {
					VRMWdb.setStage ("AfterBattle");
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
					//baseDef = (int)(baseDef * Random.Range(80, 120)/100.0);
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

					if(damage>80)VRMWdb.addScore("HighDamage",damage-80);
					VRMWdb.addScore("Damage",damage);

					VRMWdb.setEnemyInfo ("HP", Mathf.Max(VRMWdb.getEnemyInfoInt ("HP") - damage,0));
					
					VRMWdb.setEnemyInfo ("ActionType", "Damaged");
					VRMWdb.setEnemyInfo ("Attacked/Player1/Damage", 0);
					VRMWdb.setEnemyInfo ("Attacked/Player2/Damage", 0);
					VRMWdb.setEnemyInfo ("Attacked/Player3/Damage", 0);
				}
			}
			if (Time.time - latestShowDamage > 2) {
				
				int damageNum=0;
				if(Damage1.gameObject.activeSelf)damageNum++;
				if(Damage2.gameObject.activeSelf)damageNum++;
				if(Damage3.gameObject.activeSelf)damageNum++;

				Damage1.SetActive (false);
				Damage2.SetActive (false);
				Damage3.SetActive (false);

				//Combo
				if(!isBlock){
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
						VRMWdb.addScore("Combo",damageNum);
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
			}
			
			//Check if animation end or not
			if (stillPlaying && VRMWdb.getEnemyInfoString ("State") == "idle") {
				stillPlaying = false;
				VRMWdb.setEnemyInfo ("ActionType", "");
				playAnim="";
				for(int i = 1 ; i <= 3 ; i++ ) targetPoint[i].SetActive(false);

				//Random to Appear Question
				if(UnityEngine.Random.Range (0, 100)<VRMWdb.GetQuestionRate() && VRMWdb.GetQuestionID()==0){
					int quizNum = UnityEngine.Random.Range (1, VRMWdb.GetQuestionNum()+1);
					VRMWdb.SetQuestionID(quizNum);
				}
			}
			
			//Do some animations only if Enemey is idle
			//Also do when Enemy is stuck at action
			if (VRMWdb.getEnemyInfoString("State")=="idle" && !stillPlaying) {
				
				//Play Misc Animation if it has
				if (playAnim!="") {
					
					if (playAnim == "Damaged") {
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().damaged(0);
					}

					if (playAnim == "Defend") {
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().defend(0);
					}

					if (playAnim == "Heal") {
						stillPlaying = true;
						transform.FindChild ("Model").GetComponentInChildren<ModelInterface> ().heal(0);
					}
					
					if (playAnim == "Attack") {
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
						}
					}

					if (playAnim == "Skill") {
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
						}
					}
				}

				//If Active time circle full, and player are idle or ready, Attack player
				if ((VRMWdb.currentTime() - double.Parse(VRMWdb.getEnemyInfoString("StartTime")))/1000.0 >= VRMWdb.getEnemyInfoFloat("ActiveTime")
					&& !stillPlaying){
					playAnim=VRMWdb.getEnemyInfoString("ActionType");
				}
			}
			////////////////
		}
		yield return 0f;
	}
}
