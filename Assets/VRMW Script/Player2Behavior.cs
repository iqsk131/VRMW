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



public class Player2Behavior : PlayerBehavior
{
	
	// Use this for initialization
	void OnEnable ()
	{
		playerNum=2;
		
		//Set Original Position
		originalPosition = this.transform.position;
		
		//Hide Ready Symbol
		this.transform.FindChild ("Ready").GetComponent<Renderer>().enabled = false;
		
		//Enable Canvas and Bars
		canvas.enabled = true;
		activeTimeBar.SetActive(true);
		HPBar.SetActive (true);
		
		//Initial current action
		playAnim = "";
		stillPlaying = false;
		latestShowDamage = Time.time - 2;
		
		StartCoroutine(ActiveTime());
		StartCoroutine(ActionBehavior());
		StartCoroutine(UpdateHP());
		VRMWdb.OnPlayer2HPChange += (bool st) => {
			StartCoroutine(UpdateHP());
		};
		
	}
}
