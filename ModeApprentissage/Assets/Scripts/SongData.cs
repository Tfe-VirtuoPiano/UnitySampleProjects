using System.Collections.Generic;

[System.Serializable]
public class SongData
{
    public string title;
    public string composer;
    public string genre;
    public int tempo;
    public int duration_ms;
    public string songType;
    public string sourceType;
    public string timeSignature;
    public int level;

    public List<NoteData> notes;
}
