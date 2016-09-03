﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CastAnim : AnimationHandler {
	
	[SerializeField] protected Sprite[] frames;
	[SerializeField] protected Image image;
	
	public override void Play(){
		StartCoroutine(PlayAnim());
	}
	
	private IEnumerator PlayAnim(){
		AudioClip audioClip = Resources.Load("Audio/SE/086-Action01", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
		foreach(Sprite s in frames){
			image.transform.rotation = Quaternion.LookRotation(Camera.current.transform.position - image.transform.position) * Quaternion.Euler(0, 180, 0);
			image.sprite=s;
			yield return new WaitForSeconds(0.2f);
		}
		image.gameObject.SetActive(false);
		GameObject.Destroy(this.gameObject);
	}
}
