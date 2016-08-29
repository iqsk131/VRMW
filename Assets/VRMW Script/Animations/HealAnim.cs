using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealAnim : AnimationHandler {
	
	[SerializeField] protected Sprite[] frames;
	[SerializeField] protected Image image;
	
	public override void Play(){
		StartCoroutine(PlayAnim());
	}
	
	private IEnumerator PlayAnim(){
		AudioClip audioClip = Resources.Load("Audio/SE/108-Heal04", typeof(AudioClip)) as AudioClip;
		AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
		foreach(Sprite s in frames){
			image.transform.rotation = Quaternion.LookRotation(Camera.current.transform.position - image.transform.position) * Quaternion.Euler(0, 180, 0);
			image.sprite=s;
			yield return new WaitForSeconds(0.1f);
		}
		image.gameObject.SetActive(false);
		GameObject.Destroy(this.gameObject);
	}
}
