using UnityEngine;
using System.Collections;
using System;

public class AfterBattleStageManager : MonoBehaviour {

	[SerializeField] private TextMesh Score;
	[SerializeField] private TextMesh ResertText;
	[SerializeField] private GameObject[] PlayersModel;
	[SerializeField] private GameObject EnemyModel;


	void OnEnable () {
		ResertText.gameObject.SetActive(false);
		Score.text="";
		StartCoroutine(ScoreCalculation());
	}

	IEnumerator ScoreCalculation(){
		Score.text = "Wait for Result..";

		yield return new WaitForSeconds(5f);

		while (!VRMWdb.isInitiated)yield return new WaitForSeconds(0.001f);

		string tmpText="";
		int score=0;
		int totalScore=0;
		AudioClip audioClip = Resources.Load("Audio/SE/001-System01", typeof(AudioClip)) as AudioClip;
		for(int i=0;i<3;i++){
			if(PlayersModel[i].transform.childCount>0)
				GameObject.Destroy(PlayersModel[i].transform.GetChild(0).gameObject);
		}
		if(EnemyModel.transform.childCount>0)
			GameObject.Destroy(EnemyModel.transform.GetChild(0).gameObject);

		// Show Win - Lose
		if(VRMWdb.getEnemyInfoString("State")=="dead"){
			Score.text = "<color=yellow>You Win!</color>";
			for(int i=0;i<3;i++){
				if(VRMWdb.getPlayerInfoInt(i+1,"ID")==-1)continue;
				GameObject playerChar = GameObject.Instantiate(Resources.Load("Prefabs/Characters/"+VRMWdb.getPlayerMonsterInfoString(i+1,"PrefabsName"))) as GameObject;
				playerChar.transform.SetParent(PlayersModel[i].transform,false);
			}

			AudioClip winAudio = Resources.Load("Audio/SE/002-Victory02", typeof(AudioClip)) as AudioClip;
			AudioSource.PlayClipAtPoint (winAudio, Vector3.zero);
		}
		else{
			Score.text = "<color=red>You Lose!</color>";
			GameObject bossChar = GameObject.Instantiate(Resources.Load("Prefabs/Characters/"+VRMWdb.getEnemyMonsterInfoString("PrefabsName"))) as GameObject;
			bossChar.transform.SetParent(EnemyModel.transform,false);

			AudioClip loseAudio = Resources.Load("Audio/SE/016-Shock01", typeof(AudioClip)) as AudioClip;
			AudioSource.PlayClipAtPoint (loseAudio, Vector3.zero);
		}
		yield return new WaitForSeconds(5f);

		// Show Score
		Score.text = "Here is your score.";
		yield return new WaitForSeconds(1f);

		Score.text += "\nHP\t";
		tmpText = Score.text;
		score=0;
		for(int i=1;i<=3;i++){
			if(VRMWdb.getPlayerInfoString(i,"State")!="dead" && VRMWdb.getPlayerInfoInt(i,"ID")!=-1)
				score+=VRMWdb.getPlayerInfoInt(i,"HP");
		}
		score *= 10;
		totalScore+=score;
		for(int i=0;i<=score;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=yellow>" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=yellow>" + score + "</color>";
		yield return new WaitForSeconds(1f);

		Score.text += "\nDamage\t";
		tmpText = Score.text;
		score=VRMWdb.getScore("Damage")*10;
		totalScore+=score;
		for(int i=0;i<=score;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=yellow>" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=yellow>" + score + "</color>";
		yield return new WaitForSeconds(1f);

		Score.text += "\nCombo\t";
		tmpText = Score.text;
		score=VRMWdb.getScore("Combo")*500;
		totalScore+=score;
		for(int i=0;i<=score;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=yellow>" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=yellow>" + score + "</color>";
		yield return new WaitForSeconds(1f);

		Score.text += "\nHero\t";
		tmpText = Score.text;
		score=VRMWdb.getScore("Hero")*5;
		totalScore+=score;
		for(int i=0;i<=score;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=yellow>" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=yellow>" + score + "</color>";
		yield return new WaitForSeconds(1f);

		Score.text += "\nPerfect Guard\t";
		tmpText = Score.text;
		score=VRMWdb.getScore("PerfectGuard")*500;
		totalScore+=score;
		for(int i=0;i<=score;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=yellow>" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=yellow>" + score + "</color>";
		yield return new WaitForSeconds(1f);

		Score.text += "\nHigh Damage\t";
		tmpText = Score.text;
		score=VRMWdb.getScore("HighDamage")*100;
		totalScore+=score;
		for(int i=0;i<=score;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=yellow>" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=yellow>" + score + "</color>";
		yield return new WaitForSeconds(1f);

		Score.text += "\nAid\t";
		tmpText = Score.text;
		score=VRMWdb.getScore("Aid")*200;
		totalScore+=score;
		for(int i=0;i<=score;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=yellow>" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=yellow>" + score + "</color>";
		yield return new WaitForSeconds(1f);

		//Zone Panelty

		Score.text += "\nAction Used\t";
		tmpText = Score.text;
		score=VRMWdb.getScore("ActionUsed")*50;
		totalScore-=score;
		for(int i=0;i<=score;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=red>-" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=red>-" + score + "</color>";
		yield return new WaitForSeconds(1f);

		Score.text += "\nDamage Receive\t";
		tmpText = Score.text;
		score=VRMWdb.getScore("DamageReceive")*5;
		totalScore-=score;
		for(int i=0;i<=score;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=red>-" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=red>-" + score + "</color>";
		yield return new WaitForSeconds(1f);


		//////////////////////////////////
		Score.text += "\n--------------------";
		yield return new WaitForSeconds(2f);
		//////////////////////////////////
		Score.text += "\nTotal Score\t";
		yield return new WaitForSeconds(1f);
		tmpText = Score.text;
		totalScore = Math.Max(0,totalScore);
		for(int i=0;i<=totalScore;i+=100){
			if(i%500==0)AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			Score.text = tmpText + "<color=yellow>" + i + "</color>";
			yield return new WaitForSeconds(0.001f);
		}
		Score.text = tmpText + "<color=yellow>" + totalScore + "</color>";
		yield return new WaitForSeconds(3f);

		if(totalScore>VRMWdb.getScore("HighScore")){
			AudioClip clapAudio = Resources.Load("Audio/SE/060-Cheer01", typeof(AudioClip)) as AudioClip;
			AudioSource.PlayClipAtPoint (clapAudio, Vector3.zero);
			Score.text += "\n<color=yellow>You get NEW HIGHSCORE!!</color>";
			VRMWdb.setScore("HighScore",totalScore);
			yield return new WaitForSeconds(3f);
		}

		ResertText.gameObject.SetActive(true);
	}
}
