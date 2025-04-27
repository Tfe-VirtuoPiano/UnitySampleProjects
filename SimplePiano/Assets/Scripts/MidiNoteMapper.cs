using UnityEngine;
using System.Collections.Generic;

public static class MidiNoteUtils
{
    // Tableau des noms de notes
    private static readonly string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    
    // Obtenir le numéro MIDI à partir du nom de note
    public static int GetMidiNumber(string note)
    {
        // Format de note attendu: "C4", "D#3", etc.
        if (note.Length < 2)
        {
            Debug.LogWarning($"Format de note invalide: {note}, utilisation de la valeur par défaut 60 (C4)");
            return 60; // C4 par défaut
        }
        
        // Extraire le nom de la note (sans l'octave)
        string noteName = note.Substring(0, note.Length - 1);
        
        // Extraire l'octave
        if (!int.TryParse(note.Substring(note.Length - 1), out int octave))
        {
            Debug.LogWarning($"Octave invalide dans la note: {note}, utilisation de la valeur par défaut 60 (C4)");
            return 60; // C4 par défaut
        }
        
        // Trouver l'index de la note dans le tableau
        int noteIndex = -1;
        for (int i = 0; i < noteNames.Length; i++)
        {
            if (noteNames[i] == noteName)
            {
                noteIndex = i;
                break;
            }
        }
        
        if (noteIndex == -1)
        {
            Debug.LogWarning($"Nom de note inconnu: {noteName}, utilisation de la valeur par défaut 60 (C4)");
            return 60; // C4 par défaut
        }
        
        // Calcul du numéro MIDI: (octave + 1) * 12 + noteIndex
        return (octave + 1) * 12 + noteIndex;
    }
    
    // Obtenir le nom de note à partir du numéro MIDI
    public static string GetNoteName(int midiNumber)
    {
        int noteIndex = midiNumber % 12;
        int octave = (midiNumber / 12) - 1;
        return noteNames[noteIndex] + octave;
    }
} 