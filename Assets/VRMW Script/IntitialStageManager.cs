using UnityEngine;
using System.Collections;

public class IntitialStageManager : MonoBehaviour {

	[SerializeField] private GameObject GameStartScanner;
	[SerializeField] private TextMesh Guide;
	[SerializeField] private TextMesh HighScore;

	void OnEnable () {
		StartCoroutine(GameStartScan());
	}

	IEnumerator GameStartScan(){
		while(true){
			yield return new WaitForSeconds(0.1f);
			if (!VRMWdb.isInitiated)
				continue;
			HighScore.text = "Current High Score: " + VRMWdb.getScore("HighScore");
			bool isAnyChar = false;
			for(int i = 1 ; i<= 3 ; i++){
				if(VRMWdb.getPlayerInfoInt(i,"ID")!=-1)isAnyChar=true;
			}
			if(isAnyChar){
				GameStartScanner.SetActive(true);
				Guide.text = "Confirm to Start\n-------------->";
			}
			else {
				GameStartScanner.SetActive(false);
				Guide.text = "";
			}
		}
	}
}
