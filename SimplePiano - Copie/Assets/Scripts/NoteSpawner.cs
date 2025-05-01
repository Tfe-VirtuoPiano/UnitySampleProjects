using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public Transform spawnParent;
    public SongData songData;

    public float startDelay = 3f;
    public int tempo; // modifiable via UI plus tard
    
    // Distance parcourue par les notes
    public float travelDistance = 5f;
    
    // Facteur de zoom - plus la valeur est petite, plus on "dézoome"
    public float zoomFactor = 2f; 
    
    private float songStartTime;
    private float beatDuration; // Durée d'un temps en secondes
    private float noteSpeed; // Vitesse constante basée sur le tempo
    private float unitsPerBeat; // Unités de distance par beat
    
    // Mapping des notes vers leur position X
    private Dictionary<string, float> noteToX = new Dictionary<string, float>()
    {
        { "C4", 0.00f },
        { "D4", 0.21f },
        { "E4", 0.42f },
        { "F4", 0.63f },
        { "G4", 0.84f },
        { "A4", 1.05f },
        { "B4", 1.26f },
        { "C5", 1.47f }
    };

    void Start()
    {
        tempo = songData.tempo;
        
        // Ajuste la vitesse en fonction du facteur de zoom
        beatDuration = 60f / tempo;
        noteSpeed = (travelDistance / beatDuration) * zoomFactor; 
        unitsPerBeat = noteSpeed * beatDuration; // Distance parcourue pendant un beat
        
        StartCoroutine(WaitBeforeStarting());
    }

    IEnumerator WaitBeforeStarting()
    {
        Debug.Log($"⌛ Starting in {startDelay} seconds...");
        yield return new WaitForSeconds(startDelay);

        songStartTime = Time.time;
        StartCoroutine(SpawnNotes());
    }

    IEnumerator SpawnNotes()
    {
        foreach (NoteData note in songData.notes)
        {
            // Temps d'attente basé directement sur les beats et tempo
            float startTimeInSeconds = note.startBeat * beatDuration;
            float waitTime = startTimeInSeconds - (Time.time - songStartTime);
            
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            if (noteToX.TryGetValue(note.note, out float xPos))
            {
                Vector3 spawnPos = new Vector3(xPos, 0.1f, travelDistance);
                Vector3 targetPos = new Vector3(xPos, 0.1f, 0f);

                GameObject newNote = Instantiate(notePrefab, spawnPos, Quaternion.identity, spawnParent);
                
                // La durée en beats détermine la longueur visuelle en unités de jeu
                // La note doit avoir une longueur proportionnelle à sa durée en beats
                float noteLength = note.durationInBeats * unitsPerBeat;
                newNote.transform.localScale = new Vector3(0.18f, 0.1f, noteLength);

                // On ajuste le temps de parcours en fonction du facteur de zoom
                float travelTime = travelDistance / noteSpeed;
                newNote.AddComponent<NoteMover>().Init(travelTime, targetPos, note.note);
            }
        }
    }
}
