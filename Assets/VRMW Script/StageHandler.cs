/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
==============================================================================*/

using UnityEngine;

namespace Vuforia
{
	/// <summary>
	/// A custom handler that implements the ITrackableEventHandler interface.
	/// </summary>
	public class StageHandler : MonoBehaviour,
	ITrackableEventHandler
	{
		#region PRIVATE_MEMBER_VARIABLES

		private TrackableBehaviour mTrackableBehaviour;
		private string currentStage="";
		private bool isTrack=false;
		#endregion // PRIVATE_MEMBER_VARIABLES



		#region UNTIY_MONOBEHAVIOUR_METHODS

		void Start()
		{

			//CameraDevice.Instance.SetFocusMode (CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
			mTrackableBehaviour = GetComponent<TrackableBehaviour>();
			if (mTrackableBehaviour)
			{
				mTrackableBehaviour.RegisterTrackableEventHandler(this);
			}
		}

		void Update(){
			//Don't Start update until VRMWdb is initiated
			if (!VRMWdb.isInitiated)
				return;

			//If the currentStage is not initiate...
			if (currentStage == "") {

				//Deactivate All stage (currently, battle and initial)
				transform.FindChild ("BattleStage").gameObject.SetActive (false);
				transform.FindChild ("InitialStage").gameObject.SetActive (false);

				//Initialize currentStage with Stage in VRMWdb
				currentStage = VRMWdb.getStage ()+"Stage";
				//Activate the currentStage
				transform.FindChild (currentStage).gameObject.SetActive (true);

				//If it is still Tracking, Enable all components in currentStage
				if (isTrack)
					OnTrackingFound ();
			}

			//If the stage change...
			if (currentStage != VRMWdb.getStage () + "Stage") {
				
				//If it is still Tracking, Disable all components in currentStage
				if (isTrack)
					OnTrackingLost ();

				//Deactivate the currentStage
				transform.FindChild (currentStage).gameObject.SetActive (false);


				//Update the currentStage
				currentStage = VRMWdb.getStage ()+"Stage";

				//Enable all components in new currentStage
				transform.FindChild (currentStage).gameObject.SetActive (true);

				//If the new stage is BattleStage, reactivate them
				if (currentStage == "BattleStage") {
					transform.FindChild ("BattleStage").FindChild ("Enemy").GetComponent<EnemyBehavior> ().reActivate();
					transform.FindChild ("BattleStage").FindChild ("Player1").GetComponent<Player1Behavior> ().reActivate ();
				}

				//If it is still Tracking, Enable all components in currentStage
				if (isTrack)
					OnTrackingFound ();
			}
		}


		#endregion // UNTIY_MONOBEHAVIOUR_METHODS



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
				isTrack = false;
				OnTrackingLost();
			}
		}

		#endregion // PUBLIC_METHODS




		#region PRIVATE_METHODS


		private void OnTrackingFound()
		{
			//If the current stage does not exist, return
			if (transform.FindChild (currentStage) == null)
				return;

			//Get the components in current stage
			Renderer[] rendererComponents = transform.FindChild (currentStage).gameObject.GetComponentsInChildren<Renderer>(true);
			Collider[] colliderComponents = transform.FindChild (currentStage).gameObject.GetComponentsInChildren<Collider>(true);

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
			//If the current stage does not exist, return
			if (transform.FindChild (currentStage) == null)
				return;

			//Get the components in current stage
			Renderer[] rendererComponents = transform.FindChild (currentStage).gameObject.GetComponentsInChildren<Renderer>(true);
			Collider[] colliderComponents = transform.FindChild (currentStage).gameObject.GetComponentsInChildren<Collider>(true);

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
