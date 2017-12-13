using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiJack;
using SharpMod;

public class NewBehaviourScriptForMidi : MonoBehaviour {

	// Use this for initialization
	void Start () {
		MidiMaster.noteOnDelegate=setUpNoteOn;
//		noteOnDelegate nd;
//		nd(setUpNoteOn);
	}
	public void	 goodFunc () {

	}
	
	//public void setUpNoteOn(channel, noteNumber, velocity){
	public void setUpNoteOn (MidiChannel channel, int note, float velocity)
	{
		Debug.Log("MidiMess ="+note+" channel = "+channel+" velocity = "+velocity);
	
//		Console.WriteLine("Notification received for: {0}", name);
	}
	

	// Update is called once per frame
	void Update () {
		
	}
}
