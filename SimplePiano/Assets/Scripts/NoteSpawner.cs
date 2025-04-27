using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public Transform spawnParent;
    public SongData songData;
    
    // Matériaux pour les notes
    public Material rightHandNoteMaterial; // Matériau pour les notes de la main droite (par défaut)
    public Material leftHandNoteMaterial;  // Matériau pour les notes de la main gauche
    
    // Matériaux pour les notes jouées
    public Material rightHandHitMaterial;  // Matériau quand une note de main droite est jouée
    public Material leftHandHitMaterial;   // Matériau quand une note de main gauche est jouée
    
    // Collection des triggers de notes pour vérification
    public NoteTrigger[] noteTriggers;

    public float startDelay = 3f;
    public int tempo; // modifiable via UI plus tard
    
    // Distance parcourue par les notes
    public float travelDistance = 5f;
    // Distance en unités qui représente 1 beat
    public float unitPerBeat = 1.0f;
    
    private float songStartTime;
    private float beatDuration; // Durée d'un temps en secondes
    private float noteSpeed; // Vitesse constante basée sur le tempo
    
    // Mapping des notes vers leur position X
    private Dictionary<string, float> noteToX = new Dictionary<string, float>()
    {
        { "C4", 0.00f },
        { "D4", 0.22f },
        { "E4", 0.44f },
        { "F4", 0.66f },
        { "G4", 0.88f },
        { "A4", 1.10f },
        { "B4", 1.32f },
        { "C5", 1.54f }
    };

    void Start()
    {
        // Vérifier que les triggers sont bien configurés
        noteTriggers = FindObjectsOfType<NoteTrigger>();
        if (noteTriggers.Length == 0)
        {
            Debug.LogError("Aucun NoteTrigger trouvé dans la scène! La détection des touches ne fonctionnera pas.");
        }
        else
        {
            Debug.Log($"NoteSpawner a trouvé {noteTriggers.Length} NoteTriggers dans la scène");
        }
        
        // Vérifier que les matériaux sont assignés
        if (rightHandHitMaterial == null || leftHandHitMaterial == null)
        {
            Debug.LogWarning("Un ou plusieurs matériaux pour les notes jouées ne sont pas assignés!");
        }
        
        tempo = songData.tempo;
        
        // Calcul de la durée d'un temps et de la vitesse de déplacement
        beatDuration = 60f / tempo;
        noteSpeed = unitPerBeat / beatDuration; // Unités par seconde
        
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
        // Tri des notes par temps de début pour assurer l'ordre correct
        List<NoteData> sortedNotes = new List<NoteData>(songData.notes);
        sortedNotes.Sort((a, b) => a.startBeat.CompareTo(b.startBeat));
        
        foreach (NoteData note in sortedNotes)
        {
            // Temps d'attente basé directement sur les beats et tempo
            float startTimeInSeconds = note.startBeat * beatDuration;
            float waitTime = startTimeInSeconds - (Time.time - songStartTime);
            
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            if (noteToX.TryGetValue(note.note, out float xPos))
            {
                // Pour positionner correctement les notes, on calcule leur longueur en unités
                float noteLengthUnits = note.durationInBeats * unitPerBeat;
                
                // Créer l'objet note à la position initiale
                GameObject newNote = Instantiate(notePrefab, spawnParent);
                
                // Définir le matériau initial en fonction de la main (gauche/droite)
                Renderer noteRenderer = newNote.GetComponent<Renderer>();
                if (noteRenderer != null)
                {
                    // Déterminer quel matériau utiliser en fonction de la main
                    if (note.hand != null && note.hand.ToLower() == "left")
                    {
                        noteRenderer.material = leftHandNoteMaterial;
                    }
                    else
                    {
                        noteRenderer.material = rightHandNoteMaterial;
                    }
                }
                
                // Ajuster l'échelle pour la longueur de la note
                newNote.transform.localScale = new Vector3(0.18f, 0.1f, noteLengthUnits);
                
                // Déplacer le pivot au bord avant (côté piano) de la note
                // Par défaut, le pivot est au centre de l'objet
                // Nous devons créer un objet parent pour gérer correctement le décalage du pivot
                GameObject pivotObject = new GameObject("NotePivot_" + note.note);
                pivotObject.transform.SetParent(spawnParent);
                
                // Le bord avant de la note doit être à Z = travelDistance au départ
                // et se déplacer vers Z = 0 (position du clavier)
                pivotObject.transform.position = new Vector3(xPos, 0.1f, travelDistance);
                
                // Ajouter un BoxCollider plus grand pour la détection des collisions
                BoxCollider pivotCollider = pivotObject.AddComponent<BoxCollider>();
                pivotCollider.size = new Vector3(0.18f, 0.2f, 0.5f); // Collider plus gros pour une meilleure détection
                pivotCollider.center = Vector3.zero; // Centré sur le pivot
                pivotCollider.isTrigger = true; // En mode trigger pour la détection
                
                // Ajouter un Rigidbody pour que les collisions fonctionnent correctement
                Rigidbody rb = pivotObject.AddComponent<Rigidbody>();
                rb.isKinematic = true; // La note ne sera pas affectée par la physique
                rb.useGravity = false; // Pas de gravité
                
                // Reparenter la note sous le pivot
                newNote.transform.SetParent(pivotObject.transform);
                
                // Positionner la note avec le bord avant aligné au pivot
                // Comme le pivot est au centre, on déplace la note de la moitié de sa longueur
                newNote.transform.localPosition = new Vector3(0, 0, noteLengthUnits / 2);
                
                // Déplacer le pivot (point avant de la note) à vitesse constante
                float travelTime = travelDistance / noteSpeed;
                NoteMover noteMover = pivotObject.AddComponent<NoteMover>();
                noteMover.Init(travelTime, new Vector3(xPos, 0.1f, 0), note.note);
                
                // Assigner le matériau de surbrillance approprié en fonction de la main
                if (note.hand != null && note.hand.ToLower() == "left")
                {
                    noteMover.hitMaterial = leftHandHitMaterial;
                    noteMover.handType = "left";
                }
                else
                {
                    noteMover.hitMaterial = rightHandHitMaterial;
                    noteMover.handType = "right";
                }
            }
        }
    }
}
