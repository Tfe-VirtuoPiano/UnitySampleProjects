using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarScript : MonoBehaviour
{
    const int keysCount = 88;
    [SerializeField] GameObject barManager;
    GameObject[] barsPressed = new GameObject[keysCount];
    [SerializeField] List<GameObject> barsReleased = new List<GameObject>();
   
    bool[] isKeyPressed = new bool[keysCount];

    [SerializeField] float barSpeed = (float)0.01;
    [SerializeField] float upperPositionLimit = (float)100;
    
    [Header("Couleurs des mains")]
    [SerializeField] Color mainGaucheColor = Color.blue;
    [SerializeField] Color mainDroiteColor = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] int noteSeparation = 39; // C4 = note 39 (après offset de 21)
    [SerializeField] float darkenFactor = 0.75f; // Facteur d'assombrissement pour les touches noires
    
    [Header("Enregistrement MIDI")]
    [SerializeField] bool isRecording = false;
    [SerializeField] string fileName = "Enregistrement";
    
    [Header("Contrôles d'enregistrement")]
    [SerializeField] bool showRecordingControls = true;
    
    // Structure pour stocker les événements MIDI
    [System.Serializable]
    public class MidiEvent
    {
        public float time; // Temps en secondes depuis le début
        public int noteNumber; // Numéro de note (0-87 après offset)
        public float velocity; // Vélocité (0-1)
        public bool isNoteOn; // true = Note On, false = Note Off
        public int originalMidiNote; // Numéro MIDI original (avec offset)
    }
    
    private List<MidiEvent> recordedEvents = new List<MidiEvent>();
    private float recordingStartTime;
    private bool recordingStarted = false;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < keysCount; i++){
            // initialize: keys are not pressed
            isKeyPressed[i] = false;
            barsPressed[i] = null;
        }
        
        // Initialiser l'enregistrement si activé
        if (isRecording)
        {
            StartRecording();
        }
    }

    // Update is called once per frame
    void Update()
    {
         for (int i = 0; i < keysCount; i++){
            if (isKeyPressed[i] && barsPressed[i] != null) {
                Vector3 scale = barsPressed[i].transform.localScale;
                scale.y += barSpeed * 2;
                barsPressed[i].transform.localScale = scale;
                Vector3 pos = barsPressed[i].transform.position;
                pos.y += barSpeed;
                barsPressed[i].transform.position = pos;
            }
        }

        for(int i = barsReleased.Count - 1; i >= 0; i--){
            Vector3 pos = barsReleased[i].transform.position;

            // destroy bars when it reached upperPositionLimit
            if (pos.y > upperPositionLimit){
                Destroy(barsReleased[i]);
                barsReleased.RemoveAt(i);
            } else {
                pos.y += barSpeed * 2;
                barsReleased[i].transform.position = pos;
            }
        }
    }

    public void onNoteOn(int noteNumber, float velocity)
    {
        // clearfy that the key is pressed
        isKeyPressed[noteNumber] = true;

        // craete bar object
        GameObject barPrefab;
        barPrefab = (GameObject)Resources.Load("Prefab/Bar");
        barsPressed[noteNumber] = Instantiate(barPrefab);
        barsPressed[noteNumber].transform.position = new Vector3(noteNumber, 0, 0);
        barsPressed[noteNumber].transform.SetParent(barManager.transform, true);
        
        // Appliquer la couleur selon la hauteur de la note
        Color couleurNote = GetCouleurPourNote(noteNumber);
        Renderer renderer = barsPressed[noteNumber].GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = couleurNote;
        }
        
        // Enregistrer l'événement Note On
        if (isRecording && recordingStarted)
        {
            RecordMidiEvent(noteNumber, velocity, true);
        }
        
        // Debug temporaire pour vérifier les touches noires
        if (IsToucheNoire(noteNumber))
        {
            Debug.Log($"Touche noire détectée: Note {noteNumber} (position {noteNumber % 12})");
        }
    }

    
    public void onNoteOff(int noteNumber)
    {
        barsReleased.Add(Clone(barsPressed[noteNumber]));
        DestroyImmediate(barsPressed[noteNumber]);

        isKeyPressed[noteNumber] = false;
        
        // Enregistrer l'événement Note Off
        if (isRecording && recordingStarted)
        {
            RecordMidiEvent(noteNumber, 0, false);
        }
    }

        GameObject Clone( GameObject obj )
    {
        var clone = GameObject.Instantiate( obj ) as GameObject;
        clone.transform.parent = obj.transform.parent;
        clone.transform.localPosition = obj.transform.localPosition;
        clone.transform.localScale = obj.transform.localScale;
        return clone;
    }
    
    private Color GetCouleurPourNote(int noteNumber)
    {
        Color baseColor;
        
        if (noteNumber >= noteSeparation)
        {
            baseColor = mainDroiteColor; // Main droite (notes hautes)
        }
        else
        {
            baseColor = mainGaucheColor; // Main gauche (notes basses)
        }
        
        // Si c'est une touche noire, assombrir la couleur
        if (IsToucheNoire(noteNumber))
        {
            return DarkenColor(baseColor);
        }
        
        return baseColor;
    }
    
    private bool IsToucheNoire(int noteNumber)
    {
        // A0 (note 21) = position 0, donc le pattern commence sur A
        // Les touches noires sont : A#, C#, D#, F#, G#
        // Pattern : 1, 4, 6, 9, 11 (relatif à A)
        int positionDansOctave = noteNumber % 12;
        return positionDansOctave == 1 || positionDansOctave == 4 || 
               positionDansOctave == 6 || positionDansOctave == 9 || 
               positionDansOctave == 11;
    }
    
    private Color DarkenColor(Color color)
    {
        return new Color(
            color.r * darkenFactor,
            color.g * darkenFactor,
            color.b * darkenFactor,
            color.a
        );
    }
    
    // Méthodes d'enregistrement MIDI
    public void StartRecording()
    {
        recordedEvents.Clear();
        recordingStartTime = Time.time;
        recordingStarted = true;
        isRecording = true;
        Debug.Log("🎵 Enregistrement MIDI démarré !");
    }
    
    public void StopRecording()
    {
        recordingStarted = false;
        isRecording = false;
        Debug.Log($"🎵 Enregistrement MIDI terminé ! {recordedEvents.Count} événements enregistrés.");
    }
    
    private void RecordMidiEvent(int noteNumber, float velocity, bool isNoteOn)
    {
        MidiEvent midiEvent = new MidiEvent
        {
            time = Time.time - recordingStartTime,
            noteNumber = noteNumber,
            velocity = velocity,
            isNoteOn = isNoteOn,
            originalMidiNote = noteNumber + 21 // Restaurer l'offset
        };
        
        recordedEvents.Add(midiEvent);
    }
    
    public void ExportToMidi()
    {
        if (recordedEvents.Count == 0)
        {
            Debug.LogWarning("❌ Aucun événement à exporter !");
            return;
        }
        
        byte[] midiBytes = GenerateMidiBytes();
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName + ".mid");
        
        try
        {
            System.IO.File.WriteAllBytes(filePath, midiBytes);
            Debug.Log($"✅ Fichier MIDI exporté : {filePath}");
            Debug.Log($"📊 Taille du fichier : {midiBytes.Length} bytes");
            Debug.Log($"🎵 Événements enregistrés : {recordedEvents.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur lors de l'export : {e.Message}");
        }
    }
    
    private byte[] GenerateMidiBytes()
    {
        // Créer un vrai fichier MIDI binaire
        List<byte> midiBytes = new List<byte>();
        
        // Header MIDI (MThd)
        midiBytes.AddRange(System.Text.Encoding.ASCII.GetBytes("MThd"));
        
        // Longueur du header (6 bytes)
        midiBytes.AddRange(IntToBytes(6, 4));
        
        // Format 0 (1 track)
        midiBytes.AddRange(IntToBytes(0, 2));
        
        // Nombre de tracks (1)
        midiBytes.AddRange(IntToBytes(1, 2));
        
        // Division (ticks par beat) - 480 ticks par beat
        midiBytes.AddRange(IntToBytes(480, 2));
        
        // Track header (MTrk)
        midiBytes.AddRange(System.Text.Encoding.ASCII.GetBytes("MTrk"));
        
        // Placeholder pour la longueur du track
        int trackLengthPosition = midiBytes.Count;
        midiBytes.AddRange(IntToBytes(0, 4));
        
        // Tempo (120 BPM)
        midiBytes.AddRange(new byte[] { 0x00, 0xFF, 0x51, 0x03, 0x07, 0xA1, 0x20 });
        
        // Instrument (Piano)
        midiBytes.AddRange(new byte[] { 0x00, 0xC0, 0x00 });
        
        // Convertir les événements en MIDI
        float previousTime = 0;
        foreach (var evt in recordedEvents)
        {
            // Calculer le delta time entre cet événement et le précédent
            float deltaTime = evt.time - previousTime;
            
            // À 120 BPM : 2 beats/sec, 480 ticks/beat = 960 ticks/sec
            // Donc 1 seconde = 960 ticks
            int ticks = (int)(deltaTime * 960);
            
            // Delta time (VLQ)
            midiBytes.AddRange(IntToVLQ(ticks));
            
            // Note On/Off
            byte status = evt.isNoteOn ? (byte)0x90 : (byte)0x80; // Note On = 0x90, Note Off = 0x80
            midiBytes.Add(status);
            
            // Note number
            midiBytes.Add((byte)evt.originalMidiNote);
            
            // Velocity
            byte velocity = (byte)(evt.velocity * 127);
            midiBytes.Add(velocity);
            
            previousTime = evt.time;
        }
        
        // End of track
        midiBytes.AddRange(new byte[] { 0x00, 0xFF, 0x2F, 0x00 });
        
        // Calculer et insérer la longueur du track
        int trackLength = midiBytes.Count - trackLengthPosition - 4;
        byte[] trackLengthBytes = IntToBytes(trackLength, 4);
        for (int i = 0; i < 4; i++)
        {
            midiBytes[trackLengthPosition + i] = trackLengthBytes[i];
        }
        
        // Retourner les bytes binaires
        return midiBytes.ToArray();
    }
    
    private byte[] IntToBytes(int value, int length)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[length - 1 - i] = (byte)((value >> (i * 8)) & 0xFF);
        }
        return bytes;
    }
    
    private byte[] IntToVLQ(int value)
    {
        List<byte> bytes = new List<byte>();
        
        if (value == 0)
        {
            bytes.Add(0);
            return bytes.ToArray();
        }
        
        while (value > 0)
        {
            byte b = (byte)(value & 0x7F);
            value >>= 7;
            
            if (value > 0)
                b |= 0x80;
                
            bytes.Add(b);
        }
        
        return bytes.ToArray();
    }
    
    // Méthodes publiques pour contrôler l'enregistrement
    [ContextMenu("Démarrer Enregistrement")]
    public void StartRecordingFromMenu()
    {
        StartRecording();
    }
    
    [ContextMenu("Arrêter Enregistrement")]
    public void StopRecordingFromMenu()
    {
        StopRecording();
    }
    
    [ContextMenu("Exporter MIDI")]
    public void ExportMidiFromMenu()
    {
        ExportToMidi();
    }
    
    // Méthodes pour l'Inspector (plus faciles à utiliser)
    public void StartRecordingButton()
    {
        StartRecording();
    }
    
    public void StopRecordingButton()
    {
        StopRecording();
    }
    
    public void ExportMidiButton()
    {
        ExportToMidi();
    }
    
    // Méthode pour afficher le statut dans l'Inspector
    public string GetRecordingStatus()
    {
        if (!isRecording)
            return "Enregistrement arrêté";
        
        if (recordingStarted)
            return $"Enregistrement en cours... ({recordedEvents.Count} événements)";
        else
            return "Enregistrement prêt";
    }
}
