using UnityEngine;
using System.Collections;
using System;

public class DragonBehavior : MonoBehaviour, ModelInterface  {

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
		StartCoroutine(startDefend(user,8f));
	}

	public void defend(int user,float duration){
		StartCoroutine(startDefend(user,duration));
	}

	public void heal(int user){
		StartCoroutine(startHeal(user));
	}

	public bool getDefendState(){
		return defendState;
	}

	public IEnumerator startDefend(int user,float duration){
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
		an.GetComponent<ShieldAnim>().Play(duration);
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

		//Warp to Target
		anim.Play ("Dragon_Attack");
		AudioClip audioClip = Resources.Load("Audio/SE/085-Monster07", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);

		yield return new WaitForSeconds(1f);
		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/FireAnim")) as GameObject;
		an.transform.parent = target;
		an.transform.position = target.position;
		an.GetComponent<AnimationHandler>().Play();


		yield return new WaitForSeconds(0.1f);

		if (user > 0) {
			int atk=VRMWdb.getPlayerMonsterInfoInt(user,"Atk");
			atk= (int)(atk * UnityEngine.Random.Range(80, 120)/100.0);
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
		anim.Play ("Dragon_Attack");
		AudioClip audioClip = Resources.Load("Audio/SE/085-Monster07", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);

		yield return new WaitForSeconds(1f);
		GameObject an = GameObject.Instantiate(Resources.Load("Prefabs/Animations/SuperExplodeAnim")) as GameObject;
		an.transform.parent = target;
		an.transform.position = target.position;
		an.GetComponent<AnimationHandler>().Play();


		yield return new WaitForSeconds(0.1f);


		if (user > 0) {
			int atk=VRMWdb.getPlayerMonsterInfoInt(user,"Skill");
			atk= (int)(atk * UnityEngine.Random.Range(80, 120)/100.0);
			atk+= (int)Math.Min(VRMWdb.getPlayerInfoInt(1,"ActiveTime")*5.0,(VRMWdb.currentTime() - double.Parse(VRMWdb.getPlayerInfoString(1,"StartTime")))/200.0);
			atk+= (int)Math.Min(VRMWdb.getPlayerInfoInt(2,"ActiveTime")*5.0,(VRMWdb.currentTime() - double.Parse(VRMWdb.getPlayerInfoString(2,"StartTime")))/200.0);
			atk+= (int)Math.Min(VRMWdb.getPlayerInfoInt(3,"ActiveTime")*5.0,(VRMWdb.currentTime() - double.Parse(VRMWdb.getPlayerInfoString(3,"StartTime")))/200.0);
			VRMWdb.setPlayerInfo (1, "StartTime", VRMWdb.currentTime ().ToString ());
			VRMWdb.setPlayerInfo (2, "StartTime", VRMWdb.currentTime ().ToString ());
			VRMWdb.setPlayerInfo (3, "StartTime", VRMWdb.currentTime ().ToString ());
			VRMWdb.setEnemyInfo ("Attacked/Player"+user+"/Damage", atk);
		} else {
			int atk=VRMWdb.getEnemyMonsterInfoInt("Skill");
			atk= (int)(atk * UnityEngine.Random.Range(80, 120)/100.0);
			atk = atk + (VRMWdb.getEnemyInfoInt("ActiveTime")*5);
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
}
