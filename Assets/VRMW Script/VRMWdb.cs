﻿using UnityEngine;
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

	void Start ()
	{
		isInitiated = false;
		firebase = Firebase.CreateNew ("https://sweltering-heat-6741.firebaseio.com");
		firebase.ValueUpdated += (object sender, ChangedEventArgs e) => {

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

			gameDB = e.DataSnapshot;
			isInitiated=true;

			if(isStageChange){
				OnStageChange(true);
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
		firebase.Child ("Initialize").SetValue ("Trigger");


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
			Debug.Log("setStage Error: " + e.ToString());
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
			return (int)(gameDB.Child ("Player"+player).Child(field).FloatValue);
		}
		catch{
			return 0;
		}
	}

	public static float getPlayerInfoFloat(int player,string field){
		try{
			return gameDB.Child ("Player"+player).Child(field).FloatValue;
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
			Debug.Log("setPlayerInfo Error: " + e.ToString());
		}
	}
	public static void setPlayerInfo(int player,string field, float value){
		try{
			firebase.Child ("Player"+player).Child(field).SetValue(value);
		}
		catch(Exception e){
			Debug.Log("setPlayerInfo Error: " + e.ToString());
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
			return (int)(gameDB.Child ("Enemy").Child(field).FloatValue);
		}
		catch{
			return 0;
		}
	}

	public static float getEnemyInfoFloat(string field){
		try{
			return gameDB.Child ("Enemy").Child(field).FloatValue;
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
			Debug.Log("setPlayerInfo Error: " + e.ToString());
		}
	}
	public static void setEnemyInfo(string field, float value){
		try{
			firebase.Child ("Enemy").Child(field).SetValue(value);
		}
		catch(Exception e){
			Debug.Log("setPlayerInfo Error: " + e.ToString());
		}
	}
}