﻿/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
==============================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;

namespace Vuforia
{
	/// <summary>
	/// A custom handler that implements the ITrackableEventHandler interface.
	/// </summary>
	public class StageHandler : MonoBehaviour,
	ITrackableEventHandler
	{
		public GameObject targetCard;

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
			VRMWdb.OnStageChange += (bool st) => {
				Timing.RunCoroutine(updateStage());
			};
		}

		private IEnumerator<float> updateStage(){

			while(!VRMWdb.isInitiated)yield return Timing.WaitForSeconds(1f);

			//If the currentStage is not initiate...
			if (currentStage == "") {
				
				//Deactivate All stage (currently, battle and initial)
				if(transform.FindChild ("BattleStage")!=null)
					transform.FindChild ("BattleStage").gameObject.SetActive (false);
				if(transform.FindChild ("InitialStage")!=null)
					transform.FindChild ("InitialStage").gameObject.SetActive (false);
				if(transform.FindChild ("AfterBattleStage")!=null)
					transform.FindChild ("AfterBattleStage").gameObject.SetActive (false);
				
				//Initialize currentStage with Stage in VRMWdb
				currentStage = VRMWdb.getStage()+"Stage";
				//Activate the currentStage
				transform.FindChild (currentStage).gameObject.SetActive (true);
				if (currentStage == "BattleStage") {

					//Instantiate Player
					for(int i=1;i<=3;i++){
						int playerId = VRMWdb.getPlayerInfoInt(i,"ID");
						if(playerId>0){
							GameObject playerChar = GameObject.Instantiate(Resources.Load("Prefabs/Characters/"+VRMWdb.getMonsterInfoString(playerId,"PrefabsName"))) as GameObject;
							playerChar.transform.SetParent(transform.FindChild ("BattleStage").FindChild ("Player"+i).FindChild("Model"),false);
						}
						else{
							transform.FindChild ("BattleStage").FindChild ("Player"+i).gameObject.SetActive(false);
						}
					}

					//Instantiate Boss
					int bossId = VRMWdb.getEnemyInfoInt("BID");
					GameObject bassChar = GameObject.Instantiate(Resources.Load("Prefabs/Characters/"+VRMWdb.getMonsterInfoString(bossId,"PrefabsName",true))) as GameObject;
					bassChar.transform.SetParent(transform.FindChild ("BattleStage").FindChild ("Enemy").FindChild("Model"),false);

					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ActionCard"), true);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("CharacterCard"), false);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ConfirmCard"), false);
				} else if (currentStage == "InitialStage") {
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ActionCard"), false);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("CharacterCard"), true);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ConfirmCard"), true);
				} else if (currentStage == "AfterBattleStage") {
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ActionCard"), false);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("CharacterCard"), false);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ConfirmCard"), false);
				}
				
				//If it is still Tracking, Enable all components in currentStage
				if (isTrack)
					OnTrackingFound ();
			}
			
			//If the stage change...
			if (currentStage != VRMWdb.getStage() + "Stage") {
				
				//If it is still Tracking, Disable all components in currentStage
				if (isTrack)
					OnTrackingLost ();
				
				//Deactivate the currentStage
				transform.FindChild (currentStage).gameObject.SetActive (false);
				
				
				//Update the currentStage
				currentStage = VRMWdb.getStage()+"Stage";
				
				//Enable all components in new currentStage
				transform.FindChild (currentStage).gameObject.SetActive (true);
				
				//If the new stage is BattleStage, reactivate them
				if (currentStage == "BattleStage") {
					AnimationHandler[] anims = this.transform.FindChild ("BattleStage").GetComponentsInChildren<AnimationHandler>(true);
					foreach(AnimationHandler a in anims){
						GameObject.Destroy(a.gameObject);
					}
					//Instantiate Player
					for(int i=1;i<=3;i++){
						//Remove Old Moldel
						if(transform.FindChild ("BattleStage").FindChild ("Player"+i).FindChild("Model").childCount>0){
							GameObject.Destroy(transform.FindChild ("BattleStage").FindChild ("Player"+i).FindChild("Model").GetChild(0).gameObject);
						}
						int playerId = VRMWdb.getPlayerInfoInt(i,"ID");
						if(playerId>0){
							GameObject playerChar = GameObject.Instantiate(Resources.Load("Prefabs/Characters/"+VRMWdb.getMonsterInfoString(playerId,"PrefabsName"))) as GameObject;
							playerChar.transform.SetParent(transform.FindChild ("BattleStage").FindChild ("Player"+i).FindChild("Model"),false);
						}
						else{
							transform.FindChild ("BattleStage").FindChild ("Player"+i).gameObject.SetActive(false);
						}
					}

					//Instantiate Boss
					//Remove Old Moldel
					if(transform.FindChild ("BattleStage").FindChild ("Enemy").FindChild("Model").childCount>0){
						GameObject.Destroy(transform.FindChild ("BattleStage").FindChild ("Enemy").FindChild("Model").GetChild(0).gameObject);
					}
					int bossId = VRMWdb.getEnemyInfoInt("BID");
					GameObject bassChar = GameObject.Instantiate(Resources.Load("Prefabs/Characters/"+VRMWdb.getMonsterInfoString(bossId,"PrefabsName",true))) as GameObject;
					bassChar.transform.SetParent(transform.FindChild ("BattleStage").FindChild ("Enemy").FindChild("Model"),false);
					
					transform.FindChild ("BattleStage").FindChild ("Enemy").gameObject.SetActive (true);
					transform.FindChild ("BattleStage").FindChild ("Player1").gameObject.SetActive (true);
					transform.FindChild ("BattleStage").FindChild ("Player2").gameObject.SetActive (true);
					transform.FindChild ("BattleStage").FindChild ("Player3").gameObject.SetActive (true);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ActionCard"), true);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("CharacterCard"), false);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ConfirmCard"), false);

				} else if (currentStage == "InitialStage") {
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ActionCard"), false);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("CharacterCard"), true);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ConfirmCard"), true);
				} else if (currentStage == "AfterBattleStage") {
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ActionCard"), false);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("CharacterCard"), true);
					setChildTrackableBehaviorState (targetCard.transform.FindChild ("ConfirmCard"), true);
				}
				
				//If it is still Tracking, Enable all components in currentStage
				if (isTrack)
					OnTrackingFound ();
			}
			yield return 0f;
		}
	
		#endregion // UNTIY_MONOBEHAVIOUR_METHODS

		private void setChildTrackableBehaviorState(Transform card,bool state){
			card.gameObject.SetActive (state);
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
			
			Timing.RunCoroutine(updateStage());
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
