using UnityEngine;
using System.Collections.Generic;

public class SongLoader : MonoBehaviour
{
    public NoteSpawner noteSpawner;

    void Start()
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
        new NoteData { note = "C2", durationInBeats = 1.0f, startBeat = 0.0f, finger = 1, hand = "right" },
        new NoteData { note = "D2", durationInBeats = 1.0f, startBeat = 1.0f, finger = 2, hand = "left" },
        new NoteData { note = "E2", durationInBeats = 1.0f, startBeat = 2.0f, finger = 3, hand = "right" },
        new NoteData { note = "F2", durationInBeats = 1.0f, startBeat = 3.0f, finger = 1, hand = "left" },
        new NoteData { note = "G2", durationInBeats = 1.0f, startBeat = 4.0f, finger = 2, hand = "right" },
        new NoteData { note = "A2", durationInBeats = 1.0f, startBeat = 5.0f, finger = 3, hand = "left" },
        new NoteData { note = "B2", durationInBeats = 1.0f, startBeat = 6.0f, finger = 4, hand = "right" },

        new NoteData { note = "C3", durationInBeats = 1.0f, startBeat = 10.0f, finger = 1, hand = "right" },
        new NoteData { note = "D3", durationInBeats = 1.0f, startBeat = 11.0f, finger = 2, hand = "left" },
        new NoteData { note = "E3", durationInBeats = 1.0f, startBeat = 12.0f, finger = 3, hand = "right" },
        new NoteData { note = "F3", durationInBeats = 1.0f, startBeat = 13.0f, finger = 1, hand = "left" },
        new NoteData { note = "G3", durationInBeats = 1.0f, startBeat = 14.0f, finger = 2, hand = "right" },
        new NoteData { note = "A3", durationInBeats = 1.0f, startBeat = 15.0f, finger = 3, hand = "left" },
        new NoteData { note = "B3", durationInBeats = 1.0f, startBeat = 16.0f, finger = 4, hand = "right" },

        new NoteData { note = "C4", durationInBeats = 1.0f, startBeat = 20.0f, finger = 1, hand = "right" },
        new NoteData { note = "D4", durationInBeats = 1.0f, startBeat = 21.0f, finger = 2, hand = "left" },
        new NoteData { note = "E4", durationInBeats = 1.0f, startBeat = 22.0f, finger = 3, hand = "right" },
        new NoteData { note = "F4", durationInBeats = 1.0f, startBeat = 23.0f, finger = 1, hand = "left" },
        new NoteData { note = "G4", durationInBeats = 1.0f, startBeat = 24.0f, finger = 2, hand = "right" },
        new NoteData { note = "A4", durationInBeats = 1.0f, startBeat = 25.0f, finger = 3, hand = "left" },
        new NoteData { note = "B4", durationInBeats = 1.0f, startBeat = 26.0f, finger = 4, hand = "right" },

        new NoteData { note = "C5", durationInBeats = 1.0f, startBeat = 30.0f, finger = 1, hand = "right" },
        new NoteData { note = "D5", durationInBeats = 1.0f, startBeat = 31.0f, finger = 2, hand = "left" },
        new NoteData { note = "E5", durationInBeats = 1.0f, startBeat = 32.0f, finger = 3, hand = "right" },
        new NoteData { note = "F5", durationInBeats = 1.0f, startBeat = 33.0f, finger = 1, hand = "left" },
        new NoteData { note = "G5", durationInBeats = 1.0f, startBeat = 34.0f, finger = 2, hand = "right" },
        new NoteData { note = "A5", durationInBeats = 1.0f, startBeat = 35.0f, finger = 3, hand = "left" },
        new NoteData { note = "B5", durationInBeats = 1.0f, startBeat = 36.0f, finger = 4, hand = "right" },


        new NoteData { note = "C6", durationInBeats = 1.0f, startBeat = 40.0f, finger = 1, hand = "right" },
        new NoteData { note = "D6", durationInBeats = 1.0f, startBeat = 41.0f, finger = 2, hand = "left" },
        new NoteData { note = "E6", durationInBeats = 1.0f, startBeat = 42.0f, finger = 3, hand = "right" },
        new NoteData { note = "F6", durationInBeats = 1.0f, startBeat = 43.0f, finger = 1, hand = "left" },
        new NoteData { note = "G6", durationInBeats = 1.0f, startBeat = 44.0f, finger = 2, hand = "right" },
        new NoteData { note = "A6", durationInBeats = 1.0f, startBeat = 45.0f, finger = 3, hand = "left" },
        new NoteData { note = "B6", durationInBeats = 1.0f, startBeat = 46.0f, finger = 4, hand = "right" },

        new NoteData { note = "C#2", durationInBeats = 1.0f, startBeat = 50.0f, finger = 1, hand = "right" },
        new NoteData { note = "D#2", durationInBeats = 1.0f, startBeat = 51.0f, finger = 2, hand = "left" },
        new NoteData { note = "F#2", durationInBeats = 1.0f, startBeat = 52.0f, finger = 3, hand = "right" },
        new NoteData { note = "G#2", durationInBeats = 1.0f, startBeat = 53.0f, finger = 1, hand = "left" },
        new NoteData { note = "A#2", durationInBeats = 1.0f, startBeat = 54.0f, finger = 2, hand = "right" },

        new NoteData { note = "C#3", durationInBeats = 1.0f, startBeat = 60.0f, finger = 1, hand = "right" },
        new NoteData { note = "D#3", durationInBeats = 1.0f, startBeat = 61.0f, finger = 2, hand = "left" },
        new NoteData { note = "F#3", durationInBeats = 1.0f, startBeat = 62.0f, finger = 3, hand = "right" },
        new NoteData { note = "G#3", durationInBeats = 1.0f, startBeat = 63.0f, finger = 1, hand = "left" },
        new NoteData { note = "A#3", durationInBeats = 1.0f, startBeat = 64.0f, finger = 2, hand = "right" },

        new NoteData { note = "C#4", durationInBeats = 1.0f, startBeat = 70.0f, finger = 1, hand = "right" },
        new NoteData { note = "D#4", durationInBeats = 1.0f, startBeat = 71.0f, finger = 2, hand = "left" },
        new NoteData { note = "F#4", durationInBeats = 1.0f, startBeat = 72.0f, finger = 3, hand = "right" },
        new NoteData { note = "G#4", durationInBeats = 1.0f, startBeat = 73.0f, finger = 1, hand = "left" },
        new NoteData { note = "A#4", durationInBeats = 1.0f, startBeat = 74.0f, finger = 2, hand = "right" },

        new NoteData { note = "C#5", durationInBeats = 1.0f, startBeat = 80.0f, finger = 1, hand = "right" },
        new NoteData { note = "D#5", durationInBeats = 1.0f, startBeat = 81.0f, finger = 2, hand = "left" },
        new NoteData { note = "F#5", durationInBeats = 1.0f, startBeat = 82.0f, finger = 3, hand = "right" },
        new NoteData { note = "G#5", durationInBeats = 1.0f, startBeat = 83.0f, finger = 1, hand = "left" },
        new NoteData { note = "A#5", durationInBeats = 1.0f, startBeat = 84.0f, finger = 2, hand = "right" },


        new NoteData { note = "C#6", durationInBeats = 1.0f, startBeat = 90.0f, finger = 1, hand = "right" },
        new NoteData { note = "D#6", durationInBeats = 1.0f, startBeat = 91.0f, finger = 2, hand = "left" },
        new NoteData { note = "F#6", durationInBeats = 1.0f, startBeat = 92.0f, finger = 3, hand = "right" },
        new NoteData { note = "G#6", durationInBeats = 1.0f, startBeat = 93.0f, finger = 1, hand = "left" },
        new NoteData { note = "A#6", durationInBeats = 1.0f, startBeat = 94.0f, finger = 2, hand = "right" },

    }
        };

        noteSpawner.songData = mySong;
    }
}
