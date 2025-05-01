using UnityEngine;
using System.Collections.Generic;

public class SongLoader : MonoBehaviour
{
    public NoteSpawner noteSpawner;

    void Awake()
    {
        SongData mySong = new SongData
        {
            title = "La seule Vraie Chanson",
            composer = "Chicagolil",
            genre = "C majeur",
            tempo = 120,
            duration_ms = 8000,
            songType = "scaleEx",
            sourceType = "library",
            timeSignature = "4/4",
            level = 1,
            notes = new List<NoteData>
    {
        new NoteData { note = "C4", durationInBeats = 1.0f, startBeat = 0.0f, finger = 1, hand = "right" },
        new NoteData { note = "D4", durationInBeats = 1.0f, startBeat = 1.0f, finger = 2, hand = "right" },
        new NoteData { note = "C4", durationInBeats = 1.0f, startBeat = 2.0f, finger = 3, hand = "right" },
        new NoteData { note = "F4", durationInBeats = 1.0f, startBeat = 3.0f, finger = 1, hand = "right" },
        new NoteData { note = "G4", durationInBeats = 1.0f, startBeat = 4.0f, finger = 2, hand = "right" },
        new NoteData { note = "A4", durationInBeats = 1.0f, startBeat = 5.0f, finger = 3, hand = "right" },
        new NoteData { note = "B4", durationInBeats = 1.0f, startBeat = 6.0f, finger = 4, hand = "right" },
        new NoteData { note = "C5", durationInBeats = 1.0f, startBeat = 7.0f, finger = 5, hand = "right" },
    }
        };

        noteSpawner.songData = mySong;
    }
}
