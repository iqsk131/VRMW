using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;

public class CharacterSelectionBehavior : MonoBehaviour {

	[SerializeField] private GameObject CharacterScanner;
	public int PlayerId = 1;

	void OnEnable () {
		Timing.RunCoroutine(SelectCharacter());
	}
	
	IEnumerator<float> SelectCharacter(){
		while(true){
			yield return Timing.WaitForSeconds(0.1f);
			if (!VRMWdb.isInitiated)
				continue;
			if(VRMWdb.getPlayerInfoInt(PlayerId,"ID")!=-1){
				if(this.transform.FindChild("Model").childCount == 0){
					GameObject playerChar = GameObject.Instantiate(Resources.Load("Prefabs/Characters/"+VRMWdb.getPlayerMonsterInfoString(PlayerId,"PrefabsName"))) as GameObject;
					playerChar.transform.SetParent(transform.FindChild("Model"),false);
					CharacterScanner.SetActive(false);
				}
			}
			else {
				if(this.transform.FindChild("Model").childCount > 0){
					GameObject.Destroy(this.transform.FindChild("Model").GetChild(0).gameObject);
					CharacterScanner.SetActive(true);
				}
			}
		}
	}
}
