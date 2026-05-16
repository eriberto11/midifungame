using System.Collections.Generic;
using UnityEngine;
using MidiJack;

// Hanterar rytmspellogik: schemalägger noter baserat på BPM och avgör träff/miss
// via MIDI-input inom ett konfigurerbart tidsfönster.
public class RhythmController : MonoBehaviour
{
    [Header("Rytminställningar")]
    public float bpm = 120f;
    public float hitWindowSeconds = 0.18f;
    public bool requireExactNote = false;

    [Header("Notsekvens (MIDI-notnummer, 60 = C4)")]
    public int[] noteSequence = { 60, 62, 64, 65, 67, 69, 71, 72 };

    // Antal slag att schemalägga i förväg
    [Header("Schemaläggning")]
    public int scheduleAheadBeats = 8;

    public bool IsPlaying { get; private set; }
    public float BeatInterval { get { return 60f / bpm; } }

    // Informerar lyssnare om resultatet (notnummer, är träff)
    public event System.Action<int, float, bool> OnNoteResult;
    // Informerar om en ny not är redo att visas på skärmen
    public event System.Action<int, float> OnNoteScheduled;

    struct ScheduledNote
    {
        public int noteNumber;
        public float targetTime;
    }

    Queue<ScheduledNote> upcoming = new Queue<ScheduledNote>();
    float gameStartTime;
    int nextBeatIndex;

    void Start()
    {
        MidiMaster.noteOnDelegate += OnMidiNoteOn;
    }

    void OnDestroy()
    {
        MidiMaster.noteOnDelegate -= OnMidiNoteOn;
    }

    public void StartGame()
    {
        upcoming.Clear();
        nextBeatIndex = 0;
        gameStartTime = Time.time;
        IsPlaying = true;
        ScheduleAhead();
    }

    public void StopGame()
    {
        IsPlaying = false;
    }

    void Update()
    {
        if (!IsPlaying) return;

        float elapsed = Time.time - gameStartTime;

        // Markera förbi-fönstret noter som miss
        while (upcoming.Count > 0)
        {
            ScheduledNote next = upcoming.Peek();
            if (elapsed > next.targetTime + hitWindowSeconds)
            {
                upcoming.Dequeue();
                if (GameManager.Instance != null) GameManager.Instance.RegisterMiss();
                if (OnNoteResult != null) OnNoteResult(next.noteNumber, 0f, false);
            }
            else break;
        }

        ScheduleAhead();
    }

    void ScheduleAhead()
    {
        float lookAhead = nextBeatIndex * BeatInterval;
        float currentElapsed = Time.time - gameStartTime;

        while (lookAhead < currentElapsed + BeatInterval * scheduleAheadBeats)
        {
            int noteNumber = noteSequence[nextBeatIndex % noteSequence.Length];
            float targetTime = nextBeatIndex * BeatInterval;

            upcoming.Enqueue(new ScheduledNote { noteNumber = noteNumber, targetTime = targetTime });
            if (OnNoteScheduled != null) OnNoteScheduled(noteNumber, targetTime);

            nextBeatIndex++;
            lookAhead = nextBeatIndex * BeatInterval;
        }
    }

    void OnMidiNoteOn(MidiChannel channel, int note, float velocity)
    {
        if (!IsPlaying) return;

        float elapsed = Time.time - gameStartTime;

        if (upcoming.Count > 0)
        {
            ScheduledNote next = upcoming.Peek();
            bool inWindow = Mathf.Abs(elapsed - next.targetTime) <= hitWindowSeconds;
            bool correctNote = !requireExactNote || note == next.noteNumber;

            if (inWindow && correctNote)
            {
                upcoming.Dequeue();
                if (GameManager.Instance != null) GameManager.Instance.RegisterHit();
                if (OnNoteResult != null) OnNoteResult(note, velocity, true);
                return;
            }
        }

        // Not utanför fönster eller fel not
        if (OnNoteResult != null) OnNoteResult(note, velocity, false);
    }
}
