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

    public float startDelay = 1f;
    public int tempo; // modifiable via UI plus tard
    
    // Distance parcourue par les notes (maintenant relative au piano)
    public float travelDistance = 50f; 
    // Distance en unités qui représente 1 beat
    public float unitPerBeat = 1.0f;
    
    private float songStartTime;
    private float beatDuration; // Durée d'un temps en secondes
    private float noteSpeed; // Vitesse constante basée sur le tempo
    
    // Variables pour le contrôle du jeu
    private bool isMusicStarted = false;
    private bool isPaused = false;
    private Coroutine spawnCoroutine;
    
    // Mapping des notes vers leur position X relative
    private Dictionary<string, float> noteToX = new Dictionary<string, float>()
    {
        { "C2", -3.08f },
        { "D2", -2.86f },
        { "E2", -2.64f },
        { "F2", -2.42f },
        { "G2", -2.20f },
        { "A2", -1.98f },
        { "B2", -1.76f },

        
        { "C#2", -2.97f },
        { "D#2", -2.75f },
        { "F#2", -2.31f },
        { "G#2", -2.09f },
        { "A#2", -1.87f },



        { "C3", -1.54f },
        { "D3", -1.32f },
        { "E3", -1.10f },
        { "F3", -0.88f },
        { "G3", -0.66f },
        { "A3", -0.44f },
        { "B3", -0.22f },

        { "C#3", -1.43f },
        { "D#3", -1.21f },
        { "F#3", -0.77f },
        { "G#3", -0.55f },
        { "A#3", -0.33f },


        { "C4", 0.00f },
        { "D4", 0.22f },
        { "E4", 0.44f },
        { "F4", 0.66f },
        { "G4", 0.88f },
        { "A4", 1.10f },
        { "B4", 1.32f },

        { "C#4", 0.11f },
        { "D#4", 0.33f },
        { "F#4", 0.77f },
        { "G#4", 0.99f },
        { "A#4", 1.21f },

        { "C5", 1.54f },
        { "D5", 1.76f },
        { "E5", 1.98f },
        { "F5", 2.20f },
        { "G5", 2.42f },
        { "A5", 2.64f },
        { "B5", 2.86f },

        { "C#5", 1.65f },
        { "D#5", 1.87f },
        { "F#5", 2.31f },
        { "G#5", 2.53f },
        { "A#5", 2.75f },


        { "C6", 3.08f },
        { "D6", 3.30f },
        { "E6", 3.52f },
        { "F6", 3.74f },
        { "G6", 3.96f },
        { "A6", 4.18f },
        { "B6", 4.40f },

        { "C#6", 3.19f },
        { "D#6", 3.41f },
        { "F#6", 3.85f },
        { "G#6", 4.07f },
        { "A#6", 4.29f },


        { "C7", 4.62f },

    };



    // Méthode pour identifier les notes noires
    private bool IsBlackKey(string noteName)
    {
        return noteName.Contains("#") || noteName.Contains("b");
    }

    void Start()
    {
        // Vérifier que le spawner est bien un enfant du piano
        if (transform.parent == null)
        {
            Debug.LogError("Le NoteSpawner doit être un enfant du piano !");
            return;
        }

        // Vérifier que les triggers sont bien configurés
        noteTriggers = FindObjectsByType<NoteTrigger>(FindObjectsSortMode.None);
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
        
        // Ne plus démarrer automatiquement - le GameManager s'en charge
        // StartCoroutine(WaitBeforeStarting());
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
                
                // Créer l'objet note comme enfant du spawner
                GameObject newNote = Instantiate(notePrefab, transform);
                
                // Définir le matériau initial en fonction de la main (gauche/droite) et de la couleur de la touche
                Renderer noteRenderer = newNote.GetComponent<Renderer>();
                if (noteRenderer != null)
                {
                    Material baseMaterial;
                    
                    // Déterminer quel matériau utiliser en fonction de la main
                    if (note.hand != null && note.hand.ToLower() == "left")
                    {
                        baseMaterial = leftHandNoteMaterial;
                    }
                    else
                    {
                        baseMaterial = rightHandNoteMaterial;
                    }
                    
                    // Si c'est une note noire, créer une version plus foncée du matériau
                    if (IsBlackKey(note.note))
                    {
                        Material darkerMaterial = new Material(baseMaterial);
                        Color darkerColor = darkerMaterial.color * 0.6f; // 40% plus foncé
                        darkerColor.a = baseMaterial.color.a; // Garder la même transparence
                        darkerMaterial.color = darkerColor;
                        noteRenderer.material = darkerMaterial;
                    }
                    else
                    {
                        noteRenderer.material = baseMaterial;
                    }
                }
                
                // Ajuster l'échelle pour la longueur de la note
                float noteWidth = IsBlackKey(note.note) ?  0.08f : 0.14f; // Notes noires plus fines
                newNote.transform.localScale = new Vector3(noteWidth, 0.1f, noteLengthUnits);
                
                // Créer le pivot comme enfant du spawner
                GameObject pivotObject = new GameObject("NotePivot_" + note.note);
                pivotObject.transform.SetParent(transform);
                
                // Position relative au piano - Notes noires plus hautes
                float yPos = IsBlackKey(note.note) ? 0.055f : 0f; // Notes noires 0.05 unités plus hautes
                pivotObject.transform.localPosition = new Vector3(xPos, yPos, travelDistance);
                
                // Ajouter un BoxCollider plus grand pour la détection des collisions
                BoxCollider pivotCollider = pivotObject.AddComponent<BoxCollider>();
                pivotCollider.size = new Vector3(0.15f, 0.2f, 0.5f); // Collider ajusté à la nouvelle largeur
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
                // Position cible relative au piano - Notes noires plus hautes
                float targetYPos = IsBlackKey(note.note) ? 0.055f : 0f;
                noteMover.Init(travelTime, new Vector3(xPos, targetYPos, 0), note.note);
                
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
    
    // Méthodes publiques pour le contrôle du jeu
    
    public void StartMusic()
    {
        if (!isMusicStarted && !isPaused)
        {
            Debug.Log("🎵 NoteSpawner: Démarrage de la musique");
            isMusicStarted = true;
            songStartTime = Time.time;
            spawnCoroutine = StartCoroutine(SpawnNotes());
        }
        else if (isPaused)
        {
            Debug.Log("🎵 NoteSpawner: Reprise de la musique");
            isPaused = false;
            Time.timeScale = 1f;
        }
    }
    
    public void PauseMusic()
    {
        if (isMusicStarted && !isPaused)
        {
            Debug.Log("⏸️ NoteSpawner: Pause de la musique");
            isPaused = true;
            Time.timeScale = 0f;
        }
    }
    
    public void ResumeMusic()
    {
        if (isMusicStarted && isPaused)
        {
            Debug.Log("▶️ NoteSpawner: Reprise de la musique");
            isPaused = false;
            Time.timeScale = 1f;
        }
    }
    
    public void StopMusic()
    {
        Debug.Log("🛑 NoteSpawner: Arrêt de la musique");
        isMusicStarted = false;
        isPaused = false;
        Time.timeScale = 1f;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        // Nettoyer toutes les notes existantes
        CleanupNotes();
    }
    
    private void CleanupNotes()
    {
        // Supprimer toutes les notes existantes
        NoteMover[] existingNotes = FindObjectsByType<NoteMover>(FindObjectsSortMode.None);
        foreach (NoteMover note in existingNotes)
        {
            if (note != null)
            {
                DestroyImmediate(note.gameObject);
            }
        }
    }
}
