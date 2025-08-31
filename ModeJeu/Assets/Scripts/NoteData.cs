[System.Serializable]
public class NoteData
{
    public string note;     // ex: "C4"
    public float durationInBeats;    // en beats (temps musicaux)
    public float startBeat;       // position de départ en beats
    public int finger;
    public string hand;
}