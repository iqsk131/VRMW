﻿using UnityEngine;
using System.Collections;
using System;

public class KnightBehavior : MonoBehaviour, ModelInterface  {

	private bool defendState = false;
	private bool isAction = false;

	public void attack(Transform target, int user, int attackTarget){
		StartCoroutine(startAttack(target,user,attackTarget));
	}

	public void skill(Transform target, int user, int attackTarget){
		StartCoroutine(startSkill(target,user,attackTarget));
	}

	public void damaged(int user){
		StartCoroutine(startDamaged(user));
	}

	public void defend(int user){
		StartCoroutine(startDefend(user));
	}

	public void heal(int user){
		StartCoroutine(startHeal(user));
	}

	public bool getDefendState(){
		return defendState;
	}

	public IEnumerator startDefend(int user){
		if(isAction)yield break;
		isAction = true;

		//Change Player State to action
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "action");
			VRMWdb.setPlayerInfo (user, "ActionType", "");
		} else {
			VRMWdb.setEnemyInfo ("State", "action");
			VRMWdb.setEnemyInfo ("ActionType", "");
		}

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
			VRMWdb.setPlayerInfo (user, "StartTime", VRMWdb.currentTime ().ToString ());
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("StartTime", VRMWdb.currentTime ().ToString ());
		}
		isAction=false;
		yield return new WaitForSeconds(duration);
		defendState = false;

	}

	public IEnumerator startHeal(int user){
		if(isAction)yield break;
		isAction = true;

		//Change Player State to action
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "action");
			VRMWdb.setPlayerInfo (user, "ActionType", "");
		} else {
			VRMWdb.setEnemyInfo ("State", "action");
			VRMWdb.setEnemyInfo ("ActionType", "");
		}

		yield return new WaitForSeconds(0.1f);
		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/HealAnim")) as GameObject;
		an.transform.parent = this.transform;
		an.transform.position = this.transform.position;
		an.GetComponent<AnimationHandler>().Play();
		yield return new WaitForSeconds(0.1f);
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "idle");
			VRMWdb.setPlayerInfo (user, "StartTime", VRMWdb.currentTime ().ToString ());
			VRMWdb.setPlayerInfo (user, "Attacked/Heal", 3);
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("StartTime", VRMWdb.currentTime ().ToString ());
			VRMWdb.setEnemyInfo ("Attacked/Heal", 3);
		}
		isAction=false;
	}

	public IEnumerator startDamaged(int user){
		if(isAction)yield break;
		isAction = true;
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
		isAction=false;
	}


	private IEnumerator startAttack(Transform target,int user, int attackTarget){
		if(isAction)yield break;
		isAction = true;

		//Change Player State to action
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "action");
			VRMWdb.setPlayerInfo (user, "ActionType", "");
		} else {
			VRMWdb.setEnemyInfo ("State", "action");
			VRMWdb.setEnemyInfo ("ActionType", "");
		}

		Animation anim = transform.GetComponent<Animation>();

		//Warp to Target
		Vector3 newTarget = new Vector3 (
			3*target.transform.position.x/5 + 2*transform.position.x/5, 
			3*target.transform.position.y/5 + 2*transform.position.y/5,
			3*target.transform.position.z/5 + 2*transform.position.z/5);
		transform.position = newTarget;

		yield return new WaitForSeconds(0.5f);


		//Play Attack animation
		anim.Play("Attack");
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
		anim.Play("Wait");

		//Change Player to Idle after action

		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "idle");
			VRMWdb.setPlayerInfo (user, "StartTime", VRMWdb.currentTime ().ToString ());
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("StartTime", VRMWdb.currentTime ().ToString ());
		}

		isAction=false;
	}


	private IEnumerator startSkill(Transform target,int user, int attackTarget){
		if(isAction)yield break;
		isAction = true;

		//Change Player State to action
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "action");
			VRMWdb.setPlayerInfo (user, "ActionType", "");
		} else {
			VRMWdb.setEnemyInfo ("State", "action");
			VRMWdb.setEnemyInfo ("ActionType", "");
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
		anim.Play("Attack");
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
		anim.Play("Wait");

		///Some Skill Effects

		yield return new WaitForSeconds(0.5f);

		GameObject an2 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/ExplodeAnim")) as GameObject;
		an2.transform.parent = target.transform;
		an2.transform.position = target.transform.position;
		an2.GetComponent<AnimationHandler>().Play();

		if (user > 0) {
			VRMWdb.setEnemyInfo ("StartTime", (VRMWdb.currentTime() + 3000.0).ToString ());
		} else {

			VRMWdb.setPlayerInfo (attackTarget, "StartTime", (VRMWdb.currentTime() + 3000.0).ToString ());
		}

		/////////////////////

		//Change Player to Idle after action

		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "idle");
			VRMWdb.setPlayerInfo (user, "StartTime", VRMWdb.currentTime ().ToString ());
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("StartTime", VRMWdb.currentTime ().ToString ());
		}

		isAction=false;
	}
}
