using MidiJack;
using UnityEngine;

public class Piano : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        for (int note = 21; note <= 108; note++) // Plage d'un piano complet (A0 à C8)
        {
            if (MidiMaster.GetKeyDown(note))
            {
                string noteName = MidiNoteUtils.GetNoteName(note);
                Debug.Log($" Note ON : {noteName} (MIDI {note})");
                // Appelle ici ton effet visuel, audio, etc.
            }

            if (MidiMaster.GetKeyUp(note))
            {
                string noteName = MidiNoteUtils.GetNoteName(note);
                Debug.Log($" Note OFF : {noteName} (MIDI {note})");
            }
        }
    }
}
