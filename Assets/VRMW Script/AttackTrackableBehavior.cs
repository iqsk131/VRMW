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
	public class AttackTrackableBehavior : MonoBehaviour,
	ITrackableEventHandler
	{
		//public GameObject targetedObject;
		public GameObject actionScanner;

		#region PRIVATE_MEMBER_VARIABLES

		private TrackableBehaviour mTrackableBehaviour;
		private string dialog;
		//private IFirebase firebase;
		private double distanceToReference;
		private bool isTrack;
		private bool isUsed;
		private string p1State;
		#endregion // PRIVATE_MEMBER_VARIABLES



		#region UNTIY_MONOBEHAVIOUR_METHODS

		void Start()
		{
			//CameraDevice.Instance.SetFocusMode (CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

			dialog="Initialize";

			//Initialize Distance from this card to action scanner
			distanceToReference = 9999;

			//Initialize Tracking and Used state
			isTrack = false;
			isUsed = false;

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
		public void calcDistance(){
			Vector3 focusPos = actionScanner.transform.position;
			Vector3 thisPos = this.transform.position;
			distanceToReference=Mathf.Sqrt((focusPos.x-thisPos.x)*(focusPos.x-thisPos.x) + (focusPos.y-thisPos.y)*(focusPos.y-thisPos.y)  + (focusPos.z-thisPos.z)*(focusPos.z-thisPos.z));
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

				//Change Card Used state to false
				isUsed = false;

				//Change Tracking state to false
				isTrack = false;

				dialog = "waiting for trigger...";

				//Hide Tracking Model
				OnTrackingLost ();
			}
		}

		public void OnGUI(){
			//Debug.Log ("Start GUI");
			//GUI.Label (new Rect (200, 100, 100, 100), dialog);
			//GUI.Label (new Rect (200, 200, 100, 100), "Distance: " + distanceToReference);
		}


		void Update(){

			//Get Player State
			p1State = VRMWdb.getPlayerInfoString (1, "State");

			//If action card are tracking..
			if (isTrack == true) {

				//Calculate Distance to action scanner
				calcDistance ();

				//If distance to action scanner less than 700, do trigger
				if (distanceToReference < 700) {
					dialog = "trigger!!"; 

					//If the card is unused and Player is idle,...
					if (!isUsed && p1State == "idle") {
						//Change card state to used
						isUsed = true;
						//Hide Attack Model on the card
						OnTrackingLost ();
						//Change Player state to Ready
						VRMWdb.setPlayerInfo(1,"State","ready");
					}
				} else {
					//If distance is more than 700,

					//Change card state to unused
					isUsed = false;
					//Show Attack Model on the card
					OnTrackingFound();

					dialog = "place on the trigger..";
				}
			} else {
				//If the action card are not track, change distance to 9999
				distanceToReference = 9999;
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
