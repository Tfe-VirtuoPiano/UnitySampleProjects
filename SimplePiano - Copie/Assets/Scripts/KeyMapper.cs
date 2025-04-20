using UnityEngine;

public static class KeyMapper
{
    public static KeyCode GetKeyForNote(string note)
    {
        switch (note)
        {
            case "C4": return KeyCode.A;
            case "D4": return KeyCode.S;
            case "E4": return KeyCode.D;
            case "F4": return KeyCode.F;
            case "G4": return KeyCode.G;
            case "A4": return KeyCode.H;
            case "B4": return KeyCode.J;
            case "C5": return KeyCode.K;
            default: return KeyCode.None;
        }
    }
}
