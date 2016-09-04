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
	public class GameStartTrackableBehavior : MonoBehaviour,
	ITrackableEventHandler
	{
		public GameObject actionScanner;

		#region PRIVATE_MEMBER_VARIABLES

		private TrackableBehaviour mTrackableBehaviour;
		private bool isTrack;
		private bool isStartCoroutine=false;
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
				isTrack=true;
				StartCoroutine(StartBattle());

				//Show Attack Model
				OnTrackingFound();
			}
			else
			{

				//Change Tracking state to false
				isTrack=false;

				//Hide Tracking Model
				OnTrackingLost ();
			}
		}
	
		#endregion // PUBLIC_METHODS



		#region PRIVATE_METHODS


		private IEnumerator StartBattle(){
			while(isTrack){
				yield return new WaitForSeconds(0.1f);

				//Don't Start update until VRMWdb is initiated
				if (!VRMWdb.isInitiated)
					continue ;

				//If distance to action scanner less than 700, do trigger
				if (calcDistance() < 700) {

					//Hide Attack Model on the card
					OnTrackingLost ();
					
					//Initial Battle Stage
					for(int i=1;i<=3;i++){
						if(VRMWdb.getPlayerInfoInt(i,"ID")==-1){
							VRMWdb.setPlayerInfo(i,"State","dead");
						}
						else{
							VRMWdb.setPlayerInfo(i,"MaxHP",VRMWdb.getPlayerMonsterInfoInt(i,"HP"));
							VRMWdb.setPlayerInfo(i,"ActiveTime",VRMWdb.getPlayerMonsterInfoInt(i,"ActiveTime"));
							VRMWdb.setPlayerInfo(i,"HP",VRMWdb.getPlayerInfoInt(i,"MaxHP"));
							VRMWdb.setPlayerInfo(i,"Attacked/Damage",0);
							VRMWdb.setPlayerInfo(i,"StartTime",VRMWdb.currentTime ().ToString ());
							VRMWdb.setPlayerInfo(i,"State","idle");
							VRMWdb.setPlayerInfo(i,"Position","Back");
						}
					}

					/*if(VRMWdb.getEnemyInfoInt("BID")==-1){
						int bossID = UnityEngine.Random.Range(3,6);
						VRMWdb.setEnemyInfo("BID",bossID);
					}*/
					yield return new WaitForSeconds(1);
					VRMWdb.setEnemyInfo("MaxHP",VRMWdb.getEnemyMonsterInfoInt("HP"));
					VRMWdb.setEnemyInfo("ActiveTime",VRMWdb.getEnemyMonsterInfoInt("ActiveTime"));
					VRMWdb.setEnemyInfo("HP",VRMWdb.getEnemyInfoInt("MaxHP"));
					VRMWdb.setEnemyInfo("Attacked/Player1/Damage",0);
					VRMWdb.setEnemyInfo("Attacked/Player2/Damage",0);
					VRMWdb.setEnemyInfo("Attacked/Player3/Damage",0);
					VRMWdb.setEnemyInfo("StartTime",VRMWdb.currentTime ().ToString ());
					VRMWdb.setEnemyInfo("State","idle");
					yield return new WaitForSeconds(1);
					//Change Stage to Battle
					VRMWdb.setStage("Battle");
				} else {
					//If distance is more than 700,
					
					//Show Attack Model on the card
					if(isTrack)OnTrackingFound();
					
				}

			}
			yield return 0;
		}

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
		}

		#endregion // PRIVATE_METHODS
	}
}
