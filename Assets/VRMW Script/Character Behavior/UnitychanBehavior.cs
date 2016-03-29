﻿using UnityEngine;
using System.Collections;

public class UnitychanBehavior : MonoBehaviour, ModelInterface  {

	private int user;

	public void attack(Transform target, int user){
		this.user = user;
		StartCoroutine(startAttack(target));
	}

	public void damaged(int user){
		string currentState = "";
		if (user > 0) {
			currentState = VRMWdb.getPlayerInfoString (user, "State");
			VRMWdb.setPlayerInfo (user, "State", "action");
		} else {
			currentState = VRMWdb.getEnemyInfoString ("State");
			VRMWdb.setEnemyInfo ("State", "action");

		}

		Animator anim = transform.GetComponent<Animator>();
		anim.Play ("DamageDown");
		anim.Play ("Headspring");

		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", currentState);
		} else {
			VRMWdb.setEnemyInfo ("State", currentState);
		}

	}


	private IEnumerator startAttack(Transform target){

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
		AudioClip audioClip = Resources.Load("Audio/SE/Blow1", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);


		yield return new WaitForSeconds(0.1f);


		if (user > 0) {
			VRMWdb.setEnemyInfo ("Attacked/Damage", VRMWdb.getEnemyInfoInt ("Attacked/Damage") + 1);
		} else {

			VRMWdb.setPlayerInfo (1, "Attacked/Damage", VRMWdb.getEnemyInfoInt ("Attacked/Damage") + 1);
		}

		yield return new WaitForSeconds(0.5f);


		//Warp back
		transform.position = transform.parent.position;



		//Change Player to Idle after action

		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "idle");
			VRMWdb.setPlayerInfo (user, "StartTime", VRMWdb.currentTime ().ToString ());
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("StartTime", VRMWdb.currentTime ().ToString ());
		}

	}

}
