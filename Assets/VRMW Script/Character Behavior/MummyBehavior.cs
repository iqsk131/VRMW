using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MovementEffects;

public class MummyBehavior : MonoBehaviour, ModelInterface  {

	private bool defendState = false;
	private bool isAction = false;

	public void attack(Transform target, int user, int attackTarget){
		Timing.RunCoroutine(startAttack(target,user,attackTarget));
	}

	public void skill(Transform target, int user, int attackTarget){
		Timing.RunCoroutine(startSkill(target,user,attackTarget));
	}

	public void damaged(int user){
		Timing.RunCoroutine(startDamaged(user));
	}

	public void defend(int user){
		Timing.RunCoroutine(startDefend(user,5f));
	}

	public void defend(int user,float duration){
		Timing.RunCoroutine(startDefend(user,duration));
	}

	public void heal(int user){
		Timing.RunCoroutine(startHeal(user));
	}

	public bool getDefendState(){
		return defendState;
	}

	public IEnumerator<float> startDefend(int user,float duration){
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
		yield return Timing.WaitForSeconds(0.2f);
		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/ShieldAnim")) as GameObject;
		an.transform.parent = this.transform;
		an.transform.position = this.transform.position;
		an.transform.rotation = this.transform.rotation;
		an.GetComponent<ShieldAnim>().Play(duration);
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "idle");
			VRMWdb.setPlayerInfo (user, "StartTime", VRMWdb.currentTime ().ToString ());
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("StartTime", VRMWdb.currentTime ().ToString ());
		}
		isAction=false;
		yield return Timing.WaitForSeconds(duration);
		defendState = false;

	}

	public IEnumerator<float> startHeal(int user){
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

		yield return Timing.WaitForSeconds(0.1f);
		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/HealAnim")) as GameObject;
		an.transform.parent = this.transform;
		an.transform.position = this.transform.position;
		an.GetComponent<AnimationHandler>().Play();
		yield return Timing.WaitForSeconds(0.1f);
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "idle");
			VRMWdb.setPlayerInfo (user, "StartTime", VRMWdb.currentTime ().ToString ());
			int healPoint = VRMWdb.getPlayerInfoInt(user,"MaxHP");
			healPoint = (int)(healPoint * UnityEngine.Random.Range(10, 20)/100.0);
			VRMWdb.setPlayerInfo (user, "Attacked/Heal", healPoint);
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("StartTime", VRMWdb.currentTime ().ToString ());
			int healPoint = VRMWdb.getEnemyInfoInt("MaxHP");
			healPoint = (int)(healPoint * UnityEngine.Random.Range(10, 20)/100.0);
			VRMWdb.setEnemyInfo ("Attacked/Heal", 3);
		}
		isAction=false;
	}

	public IEnumerator<float> startDamaged(int user){
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
			yield return Timing.WaitForSeconds(0.1f);
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
				VRMWdb.setEnemyInfo ("State", "idle");
				VRMWdb.setEnemyInfo ("ActionType", "");
			}
		}
		yield return 0f;
		isAction=false;
	}


	private IEnumerator<float> startAttack(Transform target,int user, int attackTarget){
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

		yield return Timing.WaitForSeconds(0.5f);


		//Play Attack animation
		anim.Play ("lowKick");
		yield return Timing.WaitForSeconds(0.5f);
		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/BluntAnim")) as GameObject;
		an.transform.parent = target;
		an.transform.position = target.position;
		an.GetComponent<AnimationHandler>().Play();


		yield return Timing.WaitForSeconds(0.1f);


		if (user > 0) {
			int atk=VRMWdb.getPlayerMonsterInfoInt(user,"Atk");
			atk= (int)(atk * UnityEngine.Random.Range(80, 120)/100.0);

			//Combo
			bool isAnyAction=false;
			for(int i=1;i<=3;i++){
				if(i!=user && VRMWdb.getPlayerInfoString(i,"State")=="action")isAnyAction=true;
			}
			if(isAnyAction){
				VRMWdb.setCombo(user,true,atk);
				//Play Combo animation
				anim.Play("Attack");
				yield return Timing.WaitForSeconds(0.2f);
				GameObject an2 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/BluntAnim")) as GameObject;
				an2.transform.parent = target;
				an2.transform.position = target.position;
				an2.GetComponent<AnimationHandler>().Play();
				yield return Timing.WaitForSeconds(0.1f);
				an2 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/BluntAnim")) as GameObject;
				an2.transform.parent = target;
				an2.transform.position = target.position;
				an2.GetComponent<AnimationHandler>().Play();
				yield return Timing.WaitForSeconds(0.1f);
				an2 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/BluntAnim")) as GameObject;
				an2.transform.parent = target;
				an2.transform.position = target.position;
				an2.GetComponent<AnimationHandler>().Play();
				yield return Timing.WaitForSeconds(0.1f);
				//Extra Atk
				atk= (int)(atk * UnityEngine.Random.Range(150, 200)/100.0);
				VRMWdb.setCombo(user,false,atk);
			}

			VRMWdb.setEnemyInfo ("Attacked/Player"+user+"/Damage", atk);
		} else {
			int atk=VRMWdb.getEnemyMonsterInfoInt("Atk");
			atk= (int)(atk * UnityEngine.Random.Range(80, 120)/100.0);
			VRMWdb.setPlayerInfo (attackTarget, "Attacked/Damage", atk);
		}

		yield return Timing.WaitForSeconds(0.5f);


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

		isAction=false;
	}


	private IEnumerator<float> startSkill(Transform target,int user, int attackTarget){
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
		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/CastAnim")) as GameObject;
		an.transform.parent = this.transform;
		an.transform.position = this.transform.position;
		an.GetComponent<AnimationHandler>().Play();
		
		yield return Timing.WaitForSeconds(0.5f);

		isAction=false;
		if(user>0){
			float defendTime=VRMWdb.getPlayerMonsterInfoInt(user,"Skill");
			defendTime= defendTime * UnityEngine.Random.Range(80, 120)/100f;
			defendTime = defendTime/20f;
			for(int i=1;i<=3;i++){
				if(VRMWdb.getPlayerInfoString(i,"State")=="dead")continue;
				GameObject.Find("Player"+i).GetComponentInChildren<ModelInterface>().defend(i,defendTime);
			}

			VRMWdb.addScore("PerfectGuard",1);
		}
		else{
			float defendTime=VRMWdb.getEnemyMonsterInfoInt("Skill");
			defendTime= defendTime * UnityEngine.Random.Range(80, 120)/100f;
			defendTime = defendTime/20f;
			defend(user,defendTime);
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
