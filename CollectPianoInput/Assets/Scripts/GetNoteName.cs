public static class MidiNoteUtils
{
    public static string GetNoteName(int midiNote)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return noteNames[noteIndex] + octave;
    }
}