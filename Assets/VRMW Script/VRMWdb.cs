using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Vuforia;

public class VRMWdb : MonoBehaviour {
	public static IFirebase firebase;
	public static IDataSnapshot gameDB;
	public static bool isInitiated;

	
	public static event Action<bool> OnStageChange = _ => {};
	public static event Action<bool> OnEnemyHPChange = _ => {};
	public static event Action<bool> OnPlayer1HPChange = _ => {};
	public static event Action<bool> OnPlayer2HPChange = _ => {};
	public static event Action<bool> OnPlayer3HPChange = _ => {};
	public static event Action<bool> OnQuizStart = _ => {};

	void Start ()
	{
		isInitiated = false;
		firebase = Firebase.CreateNew ("https://sweltering-heat-6741.firebaseio.com");
		firebase.ValueUpdated += (object sender, FirebaseChangedEventArgs e) => {

			bool isStageChange = false;
			if(gameDB!=null && gameDB.Child ("Stage").StringValue != e.DataSnapshot.Child ("Stage").StringValue){
				isStageChange=true;
			}
			bool isEnemyHPChange = false;
			if(gameDB!=null && gameDB.Child ("Enemy/HP").StringValue != e.DataSnapshot.Child ("Enemy/HP").StringValue){
				isEnemyHPChange=true;
			}
			bool isPlayer1HPChange = false;
			if(gameDB!=null && gameDB.Child ("Player1/HP").StringValue != e.DataSnapshot.Child ("Player1/HP").StringValue){
				isPlayer1HPChange=true;
			}
			bool isPlayer2HPChange = false;
			if(gameDB!=null && gameDB.Child ("Player2/HP").StringValue != e.DataSnapshot.Child ("Player2/HP").StringValue){
				isPlayer2HPChange=true;
			}
			bool isPlayer3HPChange = false;
			if(gameDB!=null && gameDB.Child ("Player3/HP").StringValue != e.DataSnapshot.Child ("Player3/HP").StringValue){
				isPlayer3HPChange=true;
			}

			bool isQuizStart = false;
			if(gameDB!=null && gameDB.Child ("Quiz/CurrentQuiz").StringValue != e.DataSnapshot.Child ("Quiz/CurrentQuiz").StringValue){
				isQuizStart=true;
			}

			gameDB = e.DataSnapshot;
			isInitiated=true;

			if(isStageChange){
				OnStageChange(true);
			}

			if(isQuizStart){
				OnQuizStart(true);
			}

			if(isEnemyHPChange){
				OnEnemyHPChange(true);
			}

			if(isPlayer1HPChange){
				OnPlayer1HPChange(true);
			}
			if(isPlayer2HPChange){
				OnPlayer2HPChange(true);
			}
			if(isPlayer3HPChange){
				OnPlayer3HPChange(true);
			}

		};
		firebase.Child ("Initialize").SetValue (""+currentTime());


		VuforiaBehaviour.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
		VuforiaBehaviour.Instance.RegisterOnPauseCallback(OnPaused);
	}		


	private void OnVuforiaStarted()
	{
		CameraDevice.Instance.SetFocusMode(
			CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
	}

	private void OnPaused(bool paused)
	{
		if (!paused) // resumed
		{
			// Set again autofocus mode when app is resumed
			CameraDevice.Instance.SetFocusMode(
				CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
		}
	}

	public static double currentTime(){
		return (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
	}

	public static int CalcDamage(int attack, int defend){
		return attack*(60-(defend+200)/8)/100;
	}


	/////////////////////////////
	///                       ///
	///   Global Variables    ///
	///                       ///
	/////////////////////////////
	public static string getStage(){
		try{
			return gameDB.Child ("Stage").StringValue;
		}
		catch(Exception e){
			return "Error: " + e.ToString();
		}
	}
	public static void setStage(string value){
		try{
			firebase.Child ("Stage").SetValue(value);
		}
		catch(Exception e){
		}
	}

	public static int getCombo(int user,bool isBefore){
		try{
			if(isBefore)
				return (int)float.Parse(gameDB.Child ("Combo/Player"+user+"/before").StringValue);
			else
				return (int)float.Parse(gameDB.Child ("Combo/Player"+user+"/after").StringValue);
		}
		catch(Exception e){
			return 0;
		}
	}

	public static void setCombo(int user,bool isBefore, int value){
		try{
			if(isBefore)
				firebase.Child ("Combo/Player"+user+"/before").SetValue(value);
			else
				firebase.Child ("Combo/Player"+user+"/after").SetValue(value);
		}
		catch(Exception e){
		}
	}

	public static int getScore(string field){
		try{
			return (int)float.Parse(gameDB.Child ("Score/"+field).StringValue);
		}
		catch(Exception e){
			return 0;
		}
	}

	public static void setScore(string field, int value){
		try{
			firebase.Child ("Score/"+field).SetValue(value);
		}
		catch(Exception e){
		}
	}

	public static void addScore(string field, int value){
		try{
			firebase.Child ("Score/"+field).SetValue(getScore(field)+value);
		}
		catch(Exception e){
		}
	}
	////////////////////////////////////////////////////////
	public static int GetQuestionID(){
		try{
			return (int)float.Parse(gameDB.Child ("Quiz/CurrentQuiz").StringValue);
		}
		catch(Exception e){
			return 0;
		}
	}

	public static void SetQuestionID(int id){
		try{
			firebase.Child ("Quiz/CurrentQuiz").SetValue(id);
		}
		catch(Exception e){
		}
	}

	public static int GetQuestionNum(){
		try{
			return (int)float.Parse(gameDB.Child ("Quiz/QuizNum").StringValue);
		}
		catch(Exception e){
			return 0;
		}
	}

	public static int GetQuestionRate(){
		try{
			return (int)float.Parse(gameDB.Child ("Quiz/Rate").StringValue);
		}
		catch(Exception e){
			return 0;
		}
	}


	public static double GetQuestionStartTime(){
		try{
			return double.Parse(gameDB.Child ("Quiz/StartTime").StringValue);
		}
		catch(Exception e){
			return 0.0;
		}
	}

	public static void SetQuestionStartTime(string time){
		try{
			firebase.Child("Quiz/StartTime").SetValue(time);
		}
		catch(Exception e){
		}
	}

	public static string GetQuestionName(){
		try{
			string SID="";
			if(GetQuestionID()<10)SID+="0";
			SID += ""+GetQuestionID();
			if(GetQuestionID()==0) return "";
			return gameDB.Child ("Quiz/Q"+SID+"/Question").StringValue;
		}
		catch(Exception e){
			return "";
		}
	}

	public static float GetQuestionDuration(){
		try{
			string SID="";
			if(GetQuestionID()<10)SID+="0";
			SID += ""+GetQuestionID();
			if(GetQuestionID()==0) return 0f;
			return float.Parse(gameDB.Child ("Quiz/Q"+SID+"/Duration").StringValue);
		}
		catch(Exception e){
			return 0f;
		}
	}

	public static string GetQuestionAnswer(){
		try{
			string SID="";
			if(GetQuestionID()<10)SID+="0";
			SID += ""+GetQuestionID();
			if(GetQuestionID()==0) return "";
			return gameDB.Child ("Quiz/Q"+SID+"/Answer").StringValue;
		}
		catch(Exception e){
			return "";
		}
	}

	public static int GetQuestionReward(){
		try{
			string SID="";
			if(GetQuestionID()<10)SID+="0";
			SID += ""+GetQuestionID();
			if(GetQuestionID()==0) return 0;
			return (int)float.Parse(gameDB.Child ("Quiz/Q"+SID+"/Reward").StringValue);
		}
		catch(Exception e){
			return 0;
		}
	}

	public static string GetQuestionUserAnswer(){
		try{
			return gameDB.Child ("Quiz/Answer").StringValue;
		}
		catch(Exception e){
			return "";
		}
	}

	public static void SetQuestionUserAnswer(string ans){
		try{
			firebase.Child ("Quiz/Answer").SetValue(ans);
		}
		catch(Exception e){
		}
	}

	/////////////////////////////
	///                       ///
	/// Player Get-Set Method ///
	///                       ///
	/////////////////////////////
	public static string getPlayerInfoString(int player,string field){
		try{
			return gameDB.Child ("Player"+player).Child(field).StringValue;
		}
		catch(Exception e){
			return "Error: " + e.ToString();
		}
	}

	public static int getPlayerInfoInt(int player,string field){
		try{
			return (int)float.Parse(gameDB.Child ("Player"+player).Child(field).StringValue);
		}
		catch{
			return 0;
		}
	}

	public static float getPlayerInfoFloat(int player,string field){
		try{
			return float.Parse(gameDB.Child ("Player"+player).Child(field).StringValue);
		}
		catch{
			return 0;
		}
	}

	public static void setPlayerInfo(int player,string field, string value){
		try{
			firebase.Child ("Player"+player).Child(field).SetValue(value);
		}
		catch(Exception e){
		}
	}
	public static void setPlayerInfo(int player,string field, float value){
		try{
			firebase.Child ("Player"+player).Child(field).SetValue(value);
		}
		catch(Exception e){
		}
	}
	public static string getPlayerMonsterInfoString(int player,string field){
		try{
			return getMonsterInfoString((int)float.Parse(gameDB.Child ("Player"+player).Child("ID").StringValue),field);
		}
		catch(Exception e){
			return "Error: " + e.ToString();
		}
	}
	
	public static int getPlayerMonsterInfoInt(int player,string field){
		try{
			return getMonsterInfoInt((int)float.Parse(gameDB.Child ("Player"+player).Child("ID").StringValue),field);
		}
		catch{
			return 0;
		}
	}
	
	public static float getPlayerMonsterInfoFloat(int player,string field){
		try{
			return getMonsterInfoFloat((int)float.Parse(gameDB.Child ("Player"+player).Child("ID").StringValue),field);
		}
		catch{
			return 0;
		}
	}


	////////////////////////////
	///                      ///
	/// Enemy Get-Set Method ///
	///                      ///
	////////////////////////////
	public static string getEnemyInfoString(string field){
		try{
			return gameDB.Child ("Enemy").Child(field).StringValue;
		}
		catch(Exception e){
			return "Error: " + e.ToString();
		}
	}

	public static int getEnemyInfoInt(string field){
		try{
			return (int)float.Parse(gameDB.Child ("Enemy").Child(field).StringValue);
		}
		catch{
			return 0;
		}
	}

	public static float getEnemyInfoFloat(string field){
		try{
			return float.Parse(gameDB.Child ("Enemy").Child(field).StringValue);
		}
		catch{
			return 0;
		}
	}

	public static void setEnemyInfo(string field, string value){
		try{
			firebase.Child ("Enemy").Child(field).SetValue(value);
		}
		catch(Exception e){
		}
	}
	public static void setEnemyInfo(string field, float value){
		try{
			firebase.Child ("Enemy").Child(field).SetValue(value);
		}
		catch(Exception e){
		}
	}

	public static string getEnemyMonsterInfoString(string field){
		try{
			return getMonsterInfoString((int)float.Parse(gameDB.Child ("Enemy").Child("BID").StringValue),field,true);
		}
		catch(Exception e){
			return "Error: " + e.ToString();
		}
	}
	
	public static int getEnemyMonsterInfoInt(string field){
		try{
			return getMonsterInfoInt((int)float.Parse(gameDB.Child ("Enemy").Child("BID").StringValue),field,true);
		}
		catch{
			return 0;
		}
	}
	
	public static float getEnemyMonsterInfoFloat(string field){
		try{
			return getMonsterInfoFloat((int)float.Parse(gameDB.Child ("Enemy").Child("BID").StringValue),field,true);
		}
		catch{
			return 0;
		}
	}

	//////////////////////////////
	///                        ///
	/// MonsterInfo Get-Method ///
	///                        ///
	//////////////////////////////
	public static string getMonsterInfoString(int id, string field, bool isBoss=false){
		string mid = "id0" + id;
		if(isBoss) mid = "b"+mid;
		try{
			return gameDB.Child ("MonsterInfo").Child(mid).Child(field).StringValue;
		}
		catch(Exception e){
			return "Error: " + e.ToString();
		}
	}
	public static int getMonsterInfoInt(int id, string field, bool isBoss=false){
		string mid = "id0" + id;
		if(isBoss) mid = "b"+mid;
		try{
			return (int)float.Parse( gameDB.Child ("MonsterInfo").Child(mid).Child(field).StringValue);
		}
		catch(Exception e){
			return 0;
		}
	}
	public static float getMonsterInfoFloat(int id, string field, bool isBoss=false){
		string mid = "id0" + id;
		if(isBoss) mid = "b"+mid;
		try{
				return float.Parse(gameDB.Child ("MonsterInfo").Child(mid).Child(field).StringValue);
		}
		catch(Exception e){
			return 0f;
		}
	}
}
