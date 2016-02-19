/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
==============================================================================*/

using UnityEngine;
using System.Collections.Generic;

namespace Vuforia
{
	/// <summary>
	/// A custom handler that implements the ITrackableEventHandler interface.
	/// </summary>
	public class TriggerTrackableEventHandler : MonoBehaviour,
	ITrackableEventHandler
	{
		//public GameObject targetedObject;
		public GameObject focusObject;

		#region PRIVATE_MEMBER_VARIABLES

		private TrackableBehaviour mTrackableBehaviour;
		private string dialog;
		//private IFirebase firebase;
		private double distanceToReference;
		private bool isTrack;
		private bool isUpdate;
		private string p1State;
		#endregion // PRIVATE_MEMBER_VARIABLES



		#region UNTIY_MONOBEHAVIOUR_METHODS

		void Start()
		{
			CameraDevice.Instance.SetFocusMode (CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

			dialog="Initial...";
			distanceToReference = 9999;
			isTrack = false;
			isUpdate = false;

			//firebase = Firebase.CreateNew ("https://sweltering-heat-6741.firebaseio.com");
			//dialog="Connection Complete!!";

			/*firebase.AuthWithPassword ("iq.at.sk131@gmail.com", "iqatsk131", (AuthData auth) => {
				dialog = "Auth success!!" + auth.Uid;
			}, (FirebaseError e) => {
				dialog = "Auth failure!!";
			});*/
			//focusObject.GetComponent<Renderer> ().enabled = false;
			VRMWdb.firebase.Child("Player1").Child("State").ValueUpdated += (object sender, ChangedEventArgs e) => {
				p1State = e.DataSnapshot.StringValue;
				dialog ="Value Change!";
			};
			mTrackableBehaviour = GetComponent<TrackableBehaviour>();
			if (mTrackableBehaviour)
			{
				mTrackableBehaviour.RegisterTrackableEventHandler(this);
			}
		}
		#endregion // UNTIY_MONOBEHAVIOUR_METHODS

		public void calcDistance(){
			Vector3 focusPos = focusObject.transform.position;
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
				isTrack = true;
				OnTrackingFound();
			}
			else
			{
				isUpdate = false;
				isTrack = false;
				dialog = "waiting for trigger...";
				OnTrackingLost ();
			}
		}

		public void OnGUI(){
			Debug.Log ("Start GUI");
			GUI.Label (new Rect (200, 100, 100, 100), dialog);
			GUI.Label (new Rect (200, 200, 100, 100), "Distance: " + distanceToReference);
		}


		void Update(){
			if (isTrack == true) {
				calcDistance ();
				if (distanceToReference < 700) {
					dialog = "trigger!!"; 
					if (!isUpdate && p1State == "idle") {
						isUpdate = true;
						OnTrackingLost ();
						VRMWdb.firebase.Child ("Player1").Child ("State").SetValue ("ready");
					}
				} else {
					isUpdate = false;
					OnTrackingFound();
					dialog = "place on the trigger..";
				}
			} else {
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
