/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System;


namespace Vuforia
{
	/// <summary>
	/// A custom handler that implements the ITrackableEventHandler interface.
	/// </summary>
	public class GameStartTrackableBehavior : MonoBehaviour,
	ITrackableEventHandler
	{
		public GameObject actionScanner;

		#region PRIVATE_MEMBER_VARIABLES

		private TrackableBehaviour mTrackableBehaviour;
		private bool isTrack;
		#endregion // PRIVATE_MEMBER_VARIABLES



		#region UNTIY_MONOBEHAVIOUR_METHODS

		void Start()
		{
			//CameraDevice.Instance.SetFocusMode (CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);


			//Initialize Tracking and Used state
			isTrack = false;

			//Get track component
			mTrackableBehaviour = GetComponent<TrackableBehaviour>();
			if (mTrackableBehaviour)
			{
				mTrackableBehaviour.RegisterTrackableEventHandler(this);
			}
		}
		#endregion // UNTIY_MONOBEHAVIOUR_METHODS


		/// <summary>
		/// Calcualte Distance from this card to Action Scanner
		/// </summary>
		public float calcDistance(){
			Vector3 focusPos = actionScanner.transform.position;
			Vector3 thisPos = this.transform.position;
			return Mathf.Sqrt((focusPos.x-thisPos.x)*(focusPos.x-thisPos.x) + (focusPos.y-thisPos.y)*(focusPos.y-thisPos.y)  + (focusPos.z-thisPos.z)*(focusPos.z-thisPos.z));
		}

		#region PUBLIC_METHODS

		/// <summary>
		/// Implementation of the ITrackableEventHandler function called when the
		/// tracking state changes.
		/// </summary>
		public void OnTrackableStateChanged(
			TrackableBehaviour.Status previousStatus,
			TrackableBehaviour.Status newStatus)
		{
			if (newStatus == TrackableBehaviour.Status.DETECTED ||
				newStatus == TrackableBehaviour.Status.TRACKED ||
				newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
			{
				//Change Tracking state to true
				isTrack = true;

				//Show Attack Model
				OnTrackingFound();
			}
			else
			{

				//Change Tracking state to false
				isTrack = false;

				//Hide Tracking Model
				OnTrackingLost ();
			}
		}


		void Update(){
			//Don't Start update until VRMWdb is initiated
			if (!VRMWdb.isInitiated)
				return;

			//If action card are tracking..
			if (isTrack == true) {

				//If distance to action scanner less than 700, do trigger
				if (calcDistance() < 700) {
					//Hide Attack Model on the card
					OnTrackingLost ();

					//Initial Battle Stage
					VRMWdb.setPlayerInfo(1,"HP",VRMWdb.getPlayerInfoInt(1,"MaxHP"));
					VRMWdb.setPlayerInfo(1,"Attacked/Damage",0);
					VRMWdb.setPlayerInfo(1,"StartTime",VRMWdb.currentTime ().ToString ());
					VRMWdb.setPlayerInfo(1,"State","idle");
					VRMWdb.setPlayerInfo(2,"HP",VRMWdb.getPlayerInfoInt(2,"MaxHP"));
					VRMWdb.setPlayerInfo(2,"Attacked/Damage",0);
					VRMWdb.setPlayerInfo(2,"StartTime",VRMWdb.currentTime ().ToString ());
					VRMWdb.setPlayerInfo(2,"State","idle");
					VRMWdb.setPlayerInfo(3,"HP",VRMWdb.getPlayerInfoInt(3,"MaxHP"));
					VRMWdb.setPlayerInfo(3,"Attacked/Damage",0);
					VRMWdb.setPlayerInfo(3,"StartTime",VRMWdb.currentTime ().ToString ());
					VRMWdb.setPlayerInfo(3,"State","idle");
					VRMWdb.setEnemyInfo("HP",VRMWdb.getEnemyInfoInt("MaxHP"));
					VRMWdb.setEnemyInfo("Attacked/Player1/Damage",0);
					VRMWdb.setEnemyInfo("Attacked/Player2/Damage",0);
					VRMWdb.setEnemyInfo("Attacked/Player3/Damage",0);
					VRMWdb.setEnemyInfo("StartTime",VRMWdb.currentTime ().ToString ());
					VRMWdb.setEnemyInfo("State","idle");

					//Change Stage to Battle
					VRMWdb.setStage("Battle");
				} else {
					//If distance is more than 700,

					//Show Attack Model on the card
					OnTrackingFound();

				}
			}

		}
		#endregion // PUBLIC_METHODS



		#region PRIVATE_METHODS


		private void OnTrackingFound()
		{
			Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
			Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

			// Enable rendering:
			foreach (Renderer component in rendererComponents)
			{
				if (component.name == "ImageTarget Inside") {

				} else {
					component.enabled = true;
				}
			}

			// Enable colliders:
			foreach (Collider component in colliderComponents)
			{
				component.enabled = true;
			}

			Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
		}


		private void OnTrackingLost()
		{
			Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
			Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

			// Disable rendering:
			foreach (Renderer component in rendererComponents)
			{
				component.enabled = false;
			}

			// Disable colliders:
			foreach (Collider component in colliderComponents)
			{
				component.enabled = false;
			}

			Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
		}

		#endregion // PRIVATE_METHODS
	}
}
