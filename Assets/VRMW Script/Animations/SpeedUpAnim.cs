using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using MovementEffects;

public class SpeedUpAnim : AnimationHandler {
	
	[SerializeField] protected Sprite[] frames;
	[SerializeField] protected Image image;
	
	public override void Play(){
		Timing.RunCoroutine(PlayAnim());
	}
	
	private IEnumerator<float> PlayAnim(){
		AudioClip audioClip = Resources.Load("Audio/SE/105-Heal01", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
		for(int i=0;i<3;i++)
		foreach(Sprite s in frames){
			image.transform.rotation = Quaternion.LookRotation(Camera.current.transform.position - image.transform.position) * Quaternion.Euler(0, 180, 0);
			image.sprite=s;
			yield return Timing.WaitForSeconds(0.1f);
		}
		image.gameObject.SetActive(false);
		GameObject.Destroy(this.gameObject);
	}
}
