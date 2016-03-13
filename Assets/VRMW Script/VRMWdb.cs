using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Vuforia;

public class VRMWdb : MonoBehaviour {
	public static IFirebase firebase;
	public static IDataSnapshot gameDB;
	public static bool isInitiated;

	void Start ()
	{
		isInitiated = false;
		firebase = Firebase.CreateNew ("https://sweltering-heat-6741.firebaseio.com");
		firebase.ValueUpdated += (object sender, ChangedEventArgs e) => {
			gameDB = e.DataSnapshot;
			isInitiated=true;
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
