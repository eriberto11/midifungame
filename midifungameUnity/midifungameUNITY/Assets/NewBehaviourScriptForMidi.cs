using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiJack;

// Ursprungligt debug-script. Spellogiken hanteras nu av RhythmController,
// GameManager, PlayerController och EnemyController.
public class NewBehaviourScriptForMidi : MonoBehaviour {

	void Start () {
		// Använd += för att inte ta bort RhythmControllers lyssnare
		MidiMaster.noteOnDelegate += DebugNoteOn;
	}

	void OnDestroy () {
		MidiMaster.noteOnDelegate -= DebugNoteOn;
	}

	void DebugNoteOn (MidiChannel channel, int note, float velocity)
	{
		Debug.Log("MIDI: not=" + note + " kanal=" + channel + " velocity=" + velocity);
	}

	void Update () { }
}
