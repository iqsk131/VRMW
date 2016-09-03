using UnityEngine;
using System.Collections;

public class CharacterSelectionBehavior : MonoBehaviour {

	[SerializeField] private GameObject CharacterScanner;
	public int PlayerId = 1;

	void OnEnable () {
		StartCoroutine(SelectCharacter());
	}
	
	IEnumerator SelectCharacter(){
		while(true){
			yield return new WaitForSeconds(0.1f);
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
