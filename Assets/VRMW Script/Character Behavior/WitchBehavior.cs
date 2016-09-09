using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MovementEffects;

public class WitchBehavior : MonoBehaviour, ModelInterface  {

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
		Timing.RunCoroutine(startDefend(user,3f));
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


		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/CastAnim")) as GameObject;
		an.transform.parent = this.transform;
		an.transform.position = this.transform.position;
		an.GetComponent<AnimationHandler>().Play();

		yield return Timing.WaitForSeconds(0.5f);


		//Play Attack animation
		anim.Play ("Witch_Attack");
		yield return Timing.WaitForSeconds(0.2f);
		GameObject an2 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/ThunderAnim")) as GameObject;
		an2.transform.parent = target;
		an2.transform.position = target.position;
		an2.GetComponent<AnimationHandler>().Play();


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
				GameObject an3 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/ThunderAnim")) as GameObject;
				an3.transform.parent = target;
				an3.transform.position = target.position;
				an3.GetComponent<AnimationHandler>().Play();
				yield return Timing.WaitForSeconds(0.1f);
				an3 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/ThunderAnim")) as GameObject;
				an3.transform.parent = target;
				an3.transform.position = target.position;
				an3.GetComponent<AnimationHandler>().Play();
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

		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/CastAnim")) as GameObject;
		an.transform.parent = this.transform;
		an.transform.position = this.transform.position;
		an.GetComponent<AnimationHandler>().Play();

		yield return Timing.WaitForSeconds(0.5f);
		
		
		//Play Attack animation
		if(user>0){
			for(int i=1;i<=3;i++){
				if(VRMWdb.getPlayerInfoString(i,"State")=="dead")continue;
				Transform pTarget = GameObject.Find("Player"+i).transform.FindChild("Model").transform.GetChild(0).transform;
				GameObject anp = GameObject.Instantiate(Resources.Load("Prefabs/Animations/SpeedUpAnim")) as GameObject;
				anp.transform.parent = pTarget;
				anp.transform.position = pTarget.position;
				anp.GetComponent<AnimationHandler>().Play();
			}
		}
		else{
			anim.Play ("Witch_Attack");
			yield return Timing.WaitForSeconds(0.2f);
			GameObject an2 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/SpeedUpAnim")) as GameObject;
			an2.transform.parent = this.transform;
			an2.transform.position = this.transform.position;
			an2.GetComponent<AnimationHandler>().Play();
		}
		
		
		yield return Timing.WaitForSeconds(0.1f);

		if (user > 0) {
			double speedUpTime=VRMWdb.getPlayerMonsterInfoInt(user,"Skill");
			speedUpTime= speedUpTime * UnityEngine.Random.Range(80, 120)/100.0;
			speedUpTime = speedUpTime*20 + 1000;
			VRMWdb.setPlayerInfo (1, "StartTime", (Double.Parse(VRMWdb.getPlayerInfoString(1,"StartTime")) - speedUpTime).ToString ());
			VRMWdb.setPlayerInfo (2, "StartTime", (Double.Parse(VRMWdb.getPlayerInfoString(2,"StartTime")) - speedUpTime).ToString ());
			VRMWdb.setPlayerInfo (3, "StartTime", (Double.Parse(VRMWdb.getPlayerInfoString(3,"StartTime")) - speedUpTime).ToString ());
		} else {
			double speedUpTime=VRMWdb.getPlayerMonsterInfoInt(user,"Skill");
			speedUpTime= speedUpTime * UnityEngine.Random.Range(80, 120)/100.0;
			speedUpTime = speedUpTime*20 + 1000;
			VRMWdb.setPlayerInfo (1, "StartTime", (Double.Parse(VRMWdb.getPlayerInfoString(1,"StartTime")) + speedUpTime).ToString ());
			VRMWdb.setPlayerInfo (2, "StartTime", (Double.Parse(VRMWdb.getPlayerInfoString(2,"StartTime")) + speedUpTime).ToString ());
			VRMWdb.setPlayerInfo (3, "StartTime", (Double.Parse(VRMWdb.getPlayerInfoString(3,"StartTime")) + speedUpTime).ToString ());
		}

		/////////////////////

		//Change Player to Idle after action
		
		if (user > 0) {
			VRMWdb.setPlayerInfo (user, "State", "idle");
			VRMWdb.setPlayerInfo (user, "StartTime", (VRMWdb.currentTime () - 3000.0).ToString ());
		} else {
			VRMWdb.setEnemyInfo ("State", "idle");
			VRMWdb.setEnemyInfo ("StartTime", (VRMWdb.currentTime () - 3000.0).ToString ());
		}
		
		isAction=false;
	}
}
