/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;


namespace Vuforia
{
	/// <summary>
	/// A custom handler that implements the ITrackableEventHandler interface.
	/// </summary>
	public class DefendTrackableBehavior : MonoBehaviour,
	ITrackableEventHandler
	{
		//public GameObject targetedObject;
		public GameObject actionScanner1;
		public GameObject actionScanner2;
		public GameObject actionScanner3;

		#region PRIVATE_MEMBER_VARIABLES

		private TrackableBehaviour mTrackableBehaviour;
		//private IFirebase firebase;
		private bool isTrack;
		private bool isUsed;
		private string p1State,p2State,p3State;
		#endregion // PRIVATE_MEMBER_VARIABLES



		#region UNTIY_MONOBEHAVIOUR_METHODS

		void Start()
		{
			//CameraDevice.Instance.SetFocusMode (CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);


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
		public float calcDistance1(){
			Vector3 focusPos = actionScanner1.transform.position;
			Vector3 thisPos = this.transform.position;
			return Mathf.Sqrt((focusPos.x-thisPos.x)*(focusPos.x-thisPos.x) + (focusPos.y-thisPos.y)*(focusPos.y-thisPos.y)  + (focusPos.z-thisPos.z)*(focusPos.z-thisPos.z));
		}
		public float calcDistance2(){
			Vector3 focusPos = actionScanner2.transform.position;
			Vector3 thisPos = this.transform.position;
			return Mathf.Sqrt((focusPos.x-thisPos.x)*(focusPos.x-thisPos.x) + (focusPos.y-thisPos.y)*(focusPos.y-thisPos.y)  + (focusPos.z-thisPos.z)*(focusPos.z-thisPos.z));
		}
		public float calcDistance3(){
			Vector3 focusPos = actionScanner3.transform.position;
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
				StartCoroutine(StartAttack());

				//Show Attack Model
				OnTrackingFound();
			}
			else
			{

				//Change Card Used state to false
				isUsed = false;

				//Change Tracking state to false
				isTrack = false;

				//Hide Tracking Model
				OnTrackingLost ();
			}
		}


		private IEnumerator StartAttack(){
			while(isTrack){
				yield return new WaitForSeconds(0.1f);
				if (!VRMWdb.isInitiated)
					continue;
				
				//Get Player State
				p1State = VRMWdb.getPlayerInfoString (1, "State");
				p2State = VRMWdb.getPlayerInfoString (2, "State");
				p3State = VRMWdb.getPlayerInfoString (3, "State");
					
				//If distance to action scanner less than 700, do trigger
				if (calcDistance1() < 700) {
					
					//If the card is unused and Player is idle,...
					if (!isUsed && p1State == "idle") {
						//Change card state to used
						isUsed = true;
						//Hide Attack Model on the card
						OnTrackingLost ();
						//Change Player state to Ready
						VRMWdb.setPlayerInfo(1,"State","ready");
						VRMWdb.setPlayerInfo(1,"ActionType","Defend");
					}
				} else if (calcDistance2() < 700) {
					
					//If the card is unused and Player is idle,...
					if (!isUsed && p2State == "idle") {
						//Change card state to used
						isUsed = true;
						//Hide Attack Model on the card
						OnTrackingLost ();
						//Change Player state to Ready
						VRMWdb.setPlayerInfo(2,"State","ready");
						VRMWdb.setPlayerInfo(2,"ActionType","Defend");
					}
				} else if (calcDistance3() < 700) {
					
					//If the card is unused and Player is idle,...
					if (!isUsed && p3State == "idle") {
						//Change card state to used
						isUsed = true;
						//Hide Attack Model on the card
						OnTrackingLost ();
						//Change Player state to Ready
						VRMWdb.setPlayerInfo(3,"State","ready");
						VRMWdb.setPlayerInfo(3,"ActionType","Defend");
					}
				} else {
					//If distance is more than 700,
					
					//Change card state to unused
					isUsed = false;
					//Show Attack Model on the card
					if(isTrack)OnTrackingFound();
					
				}

			}
			yield return 0;
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
				component.enabled = true;
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
