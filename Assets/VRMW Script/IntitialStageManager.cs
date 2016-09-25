using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;

public class IntitialStageManager : MonoBehaviour {

	[SerializeField] private GameObject GameStartScanner;
	[SerializeField] private TextMesh Guide;
	[SerializeField] private TextMesh HighScore;
	[SerializeField] private VideoPlaybackBehaviour Video;

	void OnEnable () {
		Timing.RunCoroutine(GameStartScan());
	}

	void OnDisable () {
		Video.VideoPlayer.Stop();
	}

	IEnumerator<float> GameStartScan(){
		while(true){
			yield return Timing.WaitForSeconds(0.1f);
			if (!VRMWdb.isInitiated)
				continue;

			if(Video.VideoPlayer.GetStatus() == VideoPlayerHelper.MediaState.READY){
				Debug.Log("PLAY Intro");
				Video.VideoPlayer.Play(false,0.0f);
			}

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
