using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using MovementEffects;

public class ShieldAnim : AnimationHandler {
	
	[SerializeField] protected Sprite[] frames;
	[SerializeField] protected Image image;

	public override void Play(){
		Play (5f);
		//Timing.RunCoroutine(PlayAnim());
	}

	public void Play(float duration){
		Timing.RunCoroutine(PlayAnim(duration));
	}

	private IEnumerator<float> PlayAnim(float duration){
		AudioClip audioClip = Resources.Load("Audio/SE/113-Remedy01", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
		//image.gameObject.SetActive(true);
		foreach(Sprite s in frames){
			image.sprite=s;
			yield return Timing.WaitForSeconds(0.1f);
		}
		yield return Timing.WaitForSeconds(duration);
		image.gameObject.SetActive(false);
		GameObject.Destroy(this.gameObject);
		//image.gameObject.SetActive(false);
	}
}
