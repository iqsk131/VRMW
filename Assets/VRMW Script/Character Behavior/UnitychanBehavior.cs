﻿using UnityEngine;
using System.Collections;
using System;

public class UnitychanBehavior : MonoBehaviour, ModelInterface  {

	private bool defendState = false;

	public void attack(Transform target, int user, int attackTarget){
		StartCoroutine(startAttack(target,user,attackTarget));
	}

	public void damaged(int user){
		StartCoroutine(startDamaged(user));
	}

	public void defend(int user){
		StartCoroutine(startDefend(user));
	}

	public bool getDefendState(){
		return defendState;
	}

	public IEnumerator startDefend(int user){
		defendState = true;
		yield return new WaitForSeconds(0.2f);
		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/ShieldAnim")) as GameObject;
		an.transform.parent = this.transform;
		an.transform.position = this.transform.position;
		an.transform.rotation = this.transform.rotation;
		float duration=5f;
		an.GetComponent<ShieldAnim>().Play(5f);
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "idle");
			VRMWdb.setPlayerInfo (user, "ActionType", "");
			VRMWdb.setPlayerInfo (user, "StartTime", VRMWdb.currentTime ().ToString ());
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("ActionType", "");
			VRMWdb.setEnemyInfo ("StartTime", VRMWdb.currentTime ().ToString ());
		}
		yield return new WaitForSeconds(duration);
		defendState = false;
		Debug.Log ("End Defend");

	}

	public IEnumerator startDamaged(int user){
		string currentState = "";
		string currentAction = "";
		if (user > 0) {
			currentState = VRMWdb.getPlayerInfoString (user, "State");
			currentAction = VRMWdb.getPlayerInfoString (user, "ActionType");
			VRMWdb.setPlayerInfo (user, "State", "action");
		} else {
			currentState = VRMWdb.getEnemyInfoString ("State");
			currentAction = VRMWdb.getEnemyInfoString ("ActionType");
			VRMWdb.setEnemyInfo ("State", "action");

		}

		Animator anim = transform.GetComponent<Animator>();
		////Blink
		Renderer[] rendererComponents = GetComponentsInChildren<Renderer>();
		for(int i=0;i<5;i++){
			//toggle renderer
			foreach (Renderer component in rendererComponents)
			{
				component.enabled = !component.enabled;
			}
			//wait for a bit
			yield return new WaitForSeconds(0.1f);
		}
		
		//make sure renderer is enabled when we exit
		foreach (Renderer component in rendererComponents)
		{
			component.enabled = true;
		}
		////////
		//anim.Play ("DamageDown");
		//anim.Play ("Headspring");

		if (user > 0) {
			if(VRMWdb.getPlayerInfoInt(user,"HP")<=0)
				VRMWdb.setPlayerInfo (user, "State", "dead");
			else{
				VRMWdb.setPlayerInfo (user, "ActionType", currentAction);
				VRMWdb.setPlayerInfo (user, "State", currentState);
			}
		} else {
			if(VRMWdb.getEnemyInfoInt("HP")<=0)
				VRMWdb.setEnemyInfo ("State", "dead");
			else{
				VRMWdb.setEnemyInfo ("State", currentState);
				VRMWdb.setEnemyInfo ("ActionType", currentAction);
			}
		}
		yield return 0;
	}


	private IEnumerator startAttack(Transform target,int user, int attackTarget){

		//Change Player State to action
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "action");
		} else {
			VRMWdb.setEnemyInfo ("State", "action");
		}

		Animator anim = transform.GetComponent<Animator>();

		//Warp to Target
		Vector3 newTarget = new Vector3 (
			3*target.transform.position.x/5 + 2*transform.position.x/5, 
			3*target.transform.position.y/5 + 2*transform.position.y/5,
			3*target.transform.position.z/5 + 2*transform.position.z/5);
		transform.position = newTarget;

		yield return new WaitForSeconds(0.5f);


		//Play Attack animation
		anim.Play ("Hikick");
		yield return new WaitForSeconds(0.2f);
		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/BluntAnim")) as GameObject;
		an.transform.parent = target;
		an.transform.position = target.position;
		an.GetComponent<AnimationHandler>().Play();


		yield return new WaitForSeconds(0.1f);


		if (user > 0) {
			VRMWdb.setEnemyInfo ("Attacked/Player"+user+"/Damage", 1);
		} else {

			VRMWdb.setPlayerInfo (attackTarget, "Attacked/Damage", 1);
		}

		yield return new WaitForSeconds(0.5f);


		//Warp back
		transform.position = transform.parent.position;



		//Change Player to Idle after action

		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "idle");
			VRMWdb.setPlayerInfo (user, "ActionType", "");
			VRMWdb.setPlayerInfo (user, "StartTime", VRMWdb.currentTime ().ToString ());
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("ActionType", "");
			VRMWdb.setEnemyInfo ("StartTime", VRMWdb.currentTime ().ToString ());
		}

	}

}
