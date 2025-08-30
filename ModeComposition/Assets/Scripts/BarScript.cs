using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Common;

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
    
    [Header("Paramètres MIDI")]
    [SerializeField] int tempoBPM = 120;
    [SerializeField] int timeSignatureNumerator = 4;
    [SerializeField] int timeSignatureDenominator = 4;
    [SerializeField] int ticksPerBeat = 960;
    
    [Header("Contrôles d'enregistrement")]
    [SerializeField] bool showRecordingControls = true;
    
    // Structure pour stocker les événements MIDI
    [System.Serializable]
    public class RecordedMidiEvent
    {
        public float time; // Temps en secondes depuis le début
        public int noteNumber; // Numéro de note (0-87 après offset)
        public float velocity; // Vélocité (0-1)
        public bool isNoteOn; // true = Note On, false = Note Off
        public int originalMidiNote; // Numéro MIDI original (avec offset)
    }
    
    private List<RecordedMidiEvent> recordedEvents = new List<RecordedMidiEvent>();
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
        RecordedMidiEvent midiEvent = new RecordedMidiEvent
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
        
        try
        {
            GenerateMidiFile();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur lors de l'export MIDI : {e.Message}");
        }
    }
    
    private void GenerateMidiFile()
    {
        // Créer un nouveau fichier MIDI
        var midiFile = new MidiFile();
        
        // Définir la division temporelle
        midiFile.TimeDivision = new TicksPerQuarterNoteTimeDivision(ticksPerBeat);
        
        // Créer une piste
        var trackChunk = new TrackChunk();
        midiFile.Chunks.Add(trackChunk);
        
        // Ajouter un événement de tempo au début
        // Convertir BPM en microsecondes par beat
        long microsecondsPerBeat = (long)(60000000.0 / tempoBPM);
        var tempoEvent = new SetTempoEvent(microsecondsPerBeat);
        tempoEvent.DeltaTime = 0; // Tempo au tout début
        trackChunk.Events.Add(tempoEvent);
        
        // Ajouter un événement de signature rythmique
        var timeSignatureEvent = new TimeSignatureEvent(
            (FourBitNumber)timeSignatureNumerator,
            (FourBitNumber)timeSignatureDenominator
        );
        timeSignatureEvent.DeltaTime = 0; // Signature au tout début
        trackChunk.Events.Add(timeSignatureEvent);
        
        // Trier les événements par temps
        var sortedEvents = recordedEvents.OrderBy(e => e.time).ToList();
        
        // Convertir les événements en MIDI
        long previousTicks = 0;
        
        foreach (var evt in sortedEvents)
        {
            // Convertir le temps en ticks MIDI
            // Calculer les ticks par seconde selon le tempo
            // BPM = beats par minute, donc BPM/60 = beats par seconde
            // ticks par seconde = (BPM/60) * ticksPerBeat
            double beatsPerSecond = tempoBPM / 60.0;
            double ticksPerSecond = beatsPerSecond * ticksPerBeat;
            long currentTicks = (long)(evt.time * ticksPerSecond);
            long deltaTicks = currentTicks - previousTicks;
            
            // S'assurer que le delta time n'est pas négatif
            if (deltaTicks < 0) deltaTicks = 0;
            
            // Créer l'événement MIDI
            Melanchall.DryWetMidi.Core.MidiEvent midiEvent;
            
            if (evt.isNoteOn)
            {
                // Note On
                var noteOnEvent = new NoteOnEvent(
                    (SevenBitNumber)evt.originalMidiNote,
                    (SevenBitNumber)(evt.velocity * 127)
                );
                midiEvent = noteOnEvent;
            }
            else
            {
                // Note Off
                var noteOffEvent = new NoteOffEvent(
                    (SevenBitNumber)evt.originalMidiNote,
                    (SevenBitNumber)0
                );
                midiEvent = noteOffEvent;
            }
            
            // Définir le delta time pour cet événement
            midiEvent.DeltaTime = deltaTicks;
            
            // Ajouter l'événement à la piste
            trackChunk.Events.Add(midiEvent);
            
            previousTicks = currentTicks;
        }
        
        // Sauvegarder le fichier MIDI
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName + ".mid");
        midiFile.Write(filePath, true);
        
        Debug.Log($"✅ Fichier MIDI exporté : {filePath}");
        Debug.Log($"📊 Événements enregistrés : {recordedEvents.Count}");
        Debug.Log($"📁 Chemin du fichier : {filePath}");
        
        // Debug: afficher quelques événements
        if (recordedEvents.Count > 0)
        {
            Debug.Log($"🎵 Premier événement : {recordedEvents[0].time}s - Note {recordedEvents[0].originalMidiNote} - {(recordedEvents[0].isNoteOn ? "ON" : "OFF")}");
            Debug.Log($"🎵 Dernier événement : {recordedEvents[recordedEvents.Count-1].time}s - Note {recordedEvents[recordedEvents.Count-1].originalMidiNote} - {(recordedEvents[recordedEvents.Count-1].isNoteOn ? "ON" : "OFF")}");
            Debug.Log($"⏱️ Durée totale enregistrée : {recordedEvents[recordedEvents.Count-1].time - recordedEvents[0].time:F2} secondes");
        }
        
        // Debug: afficher les delta times calculés
        Debug.Log("🔍 Vérification des delta times :");
        Debug.Log($"🎵 Tempo: {tempoBPM} BPM, Signature: {timeSignatureNumerator}/{timeSignatureDenominator}, Ticks/Beat: {ticksPerBeat}");
        long totalTicks = 0;
        foreach (var evt in sortedEvents.Take(5)) // Afficher les 5 premiers
        {
            double beatsPerSecond = tempoBPM / 60.0;
            double ticksPerSecond = beatsPerSecond * ticksPerBeat;
            long currentTicks = (long)(evt.time * ticksPerSecond);
            long deltaTicks = currentTicks - totalTicks;
            Debug.Log($"  {evt.time:F3}s → {currentTicks} ticks (delta: {deltaTicks})");
            totalTicks = currentTicks;
        }
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
