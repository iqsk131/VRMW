using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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
	}		

	public static string getPlayerInfo(int player,string info){
		try{
			return gameDB.Child ("Player"+player).Child(info).StringValue;
		}
		catch(Exception e){
			return "Error: " + e.ToString();
		}
	}
}
