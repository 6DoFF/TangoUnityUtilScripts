// Attach to a Unity GUI Text object. Provides a simple static function to display Unity script debug messages on and Android device without using ADB.
// In any other script simply write: Log.d("Hello World");

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Log : MonoBehaviour {

	public static string text = "";

	private Text textDisplay;

	// Use this for initialization
	void Start () {
		textDisplay = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		textDisplay.text = text;
	}

	public static void d (string message){
		text = Time.time.ToString("F4") + " ::: " + message;
	}
}
