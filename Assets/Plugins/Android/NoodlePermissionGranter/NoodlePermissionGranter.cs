///////////////////////////////////////////////////////////
///////////////// NoodlePermissionGranter /////////////////
/// Implements runtime granting of Android permissions. ///
/// This is necessary for Android M (6.0) and above. //////
///////////////////////////////////////////////////////////   
//////////////////// Noodlecake Studios ///////////////////
///////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System;

public class NoodlePermissionGranter : MonoBehaviour {

	// subscribe to this callback to see if your permission was granted.
	public static Action<bool> PermissionRequestCallback;

	// for now, it only implements the external storage permission
	public enum NoodleAndroidPermission
	{
		WRITE_EXTERNAL_STORAGE,
		READ_EXTERNAL_STORAGE,
		NFC,
		INTERNET,
		CAMERA,
		ACCESS_NETWORK_STATE,
		ACCESS_WIFI_STATE
	}
	public static void GrantPermission(NoodleAndroidPermission permission) 
	{
		if (!initialized)
			initialize ();

		noodlePermissionGranterClass.CallStatic ("grantPermission", activity, (int)permission);
	}







	//////////////////////////////
	/// Initialization Stuff /////
	//////////////////////////////

	// it's a singleton, but no one needs to know about it. hush hush. dont touch me.
	private static NoodlePermissionGranter instance;
	private static bool initialized = false;

	public void Awake()
	{
		// instance is also set in initialize.
		// having it here ensures this thing doesnt break
		// if you added this component to the scene manually
		instance = this;
		DontDestroyOnLoad (this.gameObject);
		// object name must match UnitySendMessage call in NoodlePermissionGranter.java
		if (name != NOODLE_PERMISSION_GRANTER)
			name = NOODLE_PERMISSION_GRANTER;
	}


	private static void initialize()
	{
		// runs once when you call GrantPermission

		// add object to scene
		if (instance == null) {
			GameObject go = new GameObject();
			// instance will also be set in awake, but having it here as well seems extra safe
			instance = go.AddComponent<NoodlePermissionGranter>();
			// object name must match UnitySendMessage call in NoodlePermissionGranter.java
			go.name = NOODLE_PERMISSION_GRANTER; 
		}

		// get the jni stuff. we need the activty class and the NoodlePermissionGranter class.
		noodlePermissionGranterClass = new AndroidJavaClass("com.noodlecake.unityplugins.NoodlePermissionGranter");
		AndroidJavaClass u3d = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		activity = u3d.GetStatic<AndroidJavaObject> ("currentActivity");

		initialized = true;
	}







	///////////////////
	//// JNI Stuff ////
	///////////////////

	static AndroidJavaClass noodlePermissionGranterClass;
	static AndroidJavaObject activity;
	private const string WRITE_EXTERNAL_STORAGE="WRITE_EXTERNAL_STORAGE";
	private const string READ_EXTERNAL_STORAGE="READ_EXTERNAL_STORAGE";
	private const string NFC="NFC";
	private const string INTERNET="INTERNET";
	private const string CAMERA="CAMERA";
	private const string ACCESS_NETWORK_STATE="ACCESS_NETWORK_STATE";
	private const string ACCESS_WIFI_STATE="ACCESS_WIFI_STATE";
	private const string PERMISSION_GRANTED = "PERMISSION_GRANTED"; // must match NoodlePermissionGranter.java
	private const string PERMISSION_DENIED = "PERMISSION_DENIED"; // must match NoodlePermissionGranter.java
	private const string NOODLE_PERMISSION_GRANTER = "NoodlePermissionGranter"; // must match UnitySendMessage call in NoodlePermissionGranter.java

	private void permissionRequestCallbackInternal(string message)
	{
		// were calling this method from the java side.
		// the method name and gameobject must match NoodlePermissionGranter.java's UnitySendMessage
		bool permissionGranted = (message == PERMISSION_GRANTED);
		if (PermissionRequestCallback != null)
			PermissionRequestCallback (permissionGranted);
	}
}