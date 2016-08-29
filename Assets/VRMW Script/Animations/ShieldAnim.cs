using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShieldAnim : AnimationHandler {
	
	[SerializeField] protected Sprite[] frames;
	[SerializeField] protected Image image;

	public override void Play(){
		Play (5f);
		//StartCoroutine(PlayAnim());
	}

	public void Play(float duration){
		StartCoroutine(PlayAnim(duration));
	}

	private IEnumerator PlayAnim(float duration){
		AudioClip audioClip = Resources.Load("Audio/SE/113-Remedy01", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
		//image.gameObject.SetActive(true);
		foreach(Sprite s in frames){
			image.sprite=s;
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(duration);
		image.gameObject.SetActive(false);
		GameObject.Destroy(this.gameObject);
		//image.gameObject.SetActive(false);
	}
}
