using UnityEngine;
using System.Collections;
using System;

public class WitchBehavior : MonoBehaviour, ModelInterface  {

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

		Animator anim = transform.GetComponent<Animator>();


		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/CastAnim")) as GameObject;
		an.transform.parent = this.transform;
		an.transform.position = this.transform.position;
		an.GetComponent<AnimationHandler>().Play();

		yield return new WaitForSeconds(0.5f);


		//Play Attack animation
		anim.Play ("Witch_Attack");
		yield return new WaitForSeconds(0.2f);
		GameObject an2 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/ThunderAnim")) as GameObject;
		an2.transform.parent = target;
		an2.transform.position = target.position;
		an2.GetComponent<AnimationHandler>().Play();


		yield return new WaitForSeconds(0.1f);


		if (user > 0) {
			VRMWdb.setEnemyInfo ("Attacked/Player"+user+"/Damage", 1);
		} else {

			VRMWdb.setPlayerInfo (attackTarget, "Attacked/Damage", 1);
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

		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/CastAnim")) as GameObject;
		an.transform.parent = this.transform;
		an.transform.position = this.transform.position;
		an.GetComponent<AnimationHandler>().Play();

		yield return new WaitForSeconds(0.5f);
		
		
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
			yield return new WaitForSeconds(0.2f);
			GameObject an2 = GameObject.Instantiate(Resources.Load("Prefabs/Animations/SpeedUpAnim")) as GameObject;
			an2.transform.parent = this.transform;
			an2.transform.position = this.transform.position;
			an2.GetComponent<AnimationHandler>().Play();
		}
		
		
		yield return new WaitForSeconds(0.1f);

		if (user > 0) {
			VRMWdb.setPlayerInfo (1, "StartTime", (VRMWdb.getPlayerInfoFloat(1,"StartTime") - 3000.0).ToString ());
			VRMWdb.setPlayerInfo (2, "StartTime", (VRMWdb.getPlayerInfoFloat(2,"StartTime") - 3000.0).ToString ());
			VRMWdb.setPlayerInfo (3, "StartTime", (VRMWdb.getPlayerInfoFloat(3,"StartTime") - 3000.0).ToString ());
		} else {
			VRMWdb.setPlayerInfo (1, "StartTime", (VRMWdb.getPlayerInfoFloat(1,"StartTime") + 3000.0).ToString ());
			VRMWdb.setPlayerInfo (2, "StartTime", (VRMWdb.getPlayerInfoFloat(2,"StartTime") + 3000.0).ToString ());
			VRMWdb.setPlayerInfo (3, "StartTime", (VRMWdb.getPlayerInfoFloat(3,"StartTime") + 3000.0).ToString ());
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
