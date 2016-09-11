using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;

public class QuizManager : MonoBehaviour {

	[SerializeField] private TextMesh Question;
	[SerializeField] private GameObject ConfirmCard;


	// Use this for initialization
	void Start () {
		Timing.RunCoroutine(StartQuiz());
		VRMWdb.OnQuizStart += (bool st) => {
			Timing.RunCoroutine(StartQuiz());
		};
	}
	
	private IEnumerator<float> StartQuiz(){

		while (!VRMWdb.isInitiated)
			yield return Timing.WaitForSeconds(1f);

		//If no Question, Hide it
		if(VRMWdb.GetQuestionID()==0){
			VRMWdb.SetQuestionStartTime("0.0");
			VRMWdb.SetQuestionUserAnswer("");
			VRMWdb.SetQuestionID(0);
			ConfirmCard.SetActive(false);
			Question.gameObject.SetActive(false);
			yield break;
		}

		//Show Question
		Question.gameObject.SetActive(true);
		Question.text = VRMWdb.GetQuestionName();

		//If Time is not initialize, initialize it
		if(VRMWdb.GetQuestionStartTime() == 0f){
			VRMWdb.SetQuestionUserAnswer("");
			VRMWdb.SetQuestionStartTime(VRMWdb.currentTime().ToString());
		}

		//Wait Until Time Updated
		yield return Timing.WaitForSeconds(1f);

		//Enabled Confirm Card
		ConfirmCard.SetActive(true);

		//Show Question until timeup or answer
		while((VRMWdb.currentTime() - VRMWdb.GetQuestionStartTime())/1000.0 < VRMWdb.GetQuestionDuration()){
			if(VRMWdb.GetQuestionUserAnswer()!="")break;
			Question.text = VRMWdb.GetQuestionName() + " \n<color=yellow>[" + (int)(VRMWdb.GetQuestionDuration() - (VRMWdb.currentTime() - VRMWdb.GetQuestionStartTime())/1000.0) + "]</color>";
			yield return Timing.WaitForSeconds(1f);
		}

		//Disabled Confirm Card
		ConfirmCard.SetActive(false);

		//Give Reward
		if(VRMWdb.GetQuestionUserAnswer()==VRMWdb.GetQuestionAnswer()){
			AudioClip audioClip = Resources.Load("Audio/SE/055-Right01", typeof(AudioClip)) as AudioClip;
			AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			VRMWdb.setPlayerInfo(1,"Attacked/Heal",VRMWdb.GetQuestionReward());
			VRMWdb.setPlayerInfo(2,"Attacked/Heal",VRMWdb.GetQuestionReward());
			VRMWdb.setPlayerInfo(3,"Attacked/Heal",VRMWdb.GetQuestionReward());
		}
		else{
			AudioClip audioClip = Resources.Load("Audio/SE/057-Wrong01", typeof(AudioClip)) as AudioClip;
			AudioSource.PlayClipAtPoint (audioClip, Vector3.zero);
			VRMWdb.setEnemyInfo("Attacked/Heal",VRMWdb.GetQuestionReward());
		}

		//Reset
		VRMWdb.SetQuestionStartTime("0.0");
		VRMWdb.SetQuestionUserAnswer("");
		VRMWdb.SetQuestionID(0);
		ConfirmCard.SetActive(false);
		Question.gameObject.SetActive(false);
		yield return 0f;
	}
}
