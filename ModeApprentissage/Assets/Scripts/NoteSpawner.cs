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
    
    [Header("Références")]
    public GameManager gameManager;

    [Header("Mode d'Apprentissage")]
    public bool useStopAndWaitMode = true; // Mode où le morceau s'arrête tant que la note n'est pas jouée
    public float waitTimeout = 10f; // Temps maximum d'attente avant de passer à la note suivante (en secondes)
    
    [Header("Boucle par mesures")]
    public bool useLoopByMeasures = false; // Activer la boucle
    public int loopStartMeasure = 1; // Mesure de début (1-indexée)
    public int loopEndMeasure = 2;   // Mesure de fin incluse (1-indexée)

    public enum PracticeHand { Both, Right, Left }
    [Header("Filtre de main à pratiquer")]
    public PracticeHand practiceHand = PracticeHand.Both; // Main ciblée pour la pratique
    public Material disabledNoteMaterial; // Matériau gris pour les notes de la main non sélectionnée

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
    
    // Variables pour le mode stop-and-wait
    private bool isWaitingForNote = false;
    private NoteData currentWaitingNote = null;
    private Coroutine waitTimeoutCoroutine;
    private bool isMusicPaused = false; // Indique si la musique est en pause à cause d'une note en attente
    
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
        
        // Déterminer le nombre de temps par mesure depuis la signature rythmique (ex: "4/4")
        int beatsPerMeasure = ParseBeatsPerMeasure(songData.timeSignature);
        if (beatsPerMeasure <= 0) beatsPerMeasure = 4;

        // Préparer la sélection des notes selon le mode
        List<NoteData> notesToPlay = sortedNotes;
        int loopStartBeat = 0;
        int loopEndBeatExclusive = int.MaxValue;

        if (useLoopByMeasures)
        {
            // Clamp des valeurs de mesures
            int startMeasure = Mathf.Max(1, loopStartMeasure);
            int endMeasure = Mathf.Max(startMeasure, loopEndMeasure);

            loopStartBeat = (startMeasure - 1) * beatsPerMeasure;
            loopEndBeatExclusive = endMeasure * beatsPerMeasure; // fin exclusive

            notesToPlay = sortedNotes.FindAll(n => n.startBeat >= loopStartBeat && n.startBeat < loopEndBeatExclusive);
        }

        // Si aucune note à jouer (segment vide), on sort proprement
        if (notesToPlay.Count == 0)
        {
            Debug.LogWarning("Aucune note dans l'intervalle sélectionné. Lecture annulée.");
            yield break;
        }

        // Fonction locale pour jouer une passe (entière ou segment)
        IEnumerator PlayPass()
        {
            foreach (NoteData note in notesToPlay)
            {
                // Temps d'attente basé sur les beats et le tempo
                float startBeat = useLoopByMeasures ? (note.startBeat - loopStartBeat) : note.startBeat;
                float startTimeInSeconds = startBeat * beatDuration;
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
                        if (note.hand != null && note.hand.ToLower() == "left") baseMaterial = leftHandNoteMaterial; else baseMaterial = rightHandNoteMaterial;

                        if (IsBlackKey(note.note))
                        {
                            Material darkerMaterial = new Material(baseMaterial);
                            Color darkerColor = darkerMaterial.color * 0.6f;
                            darkerColor.a = baseMaterial.color.a;
                            darkerMaterial.color = darkerColor;
                            noteRenderer.material = darkerMaterial;
                        }
                        else
                        {
                            noteRenderer.material = baseMaterial;
                        }
                    }

                    // Ajuster l'échelle pour la longueur de la note
                    float noteWidth = IsBlackKey(note.note) ?  0.08f : 0.14f;
                    newNote.transform.localScale = new Vector3(noteWidth, 0.1f, noteLengthUnits);

                    // Créer le pivot comme enfant du spawner
                    GameObject pivotObject = new GameObject("NotePivot_" + note.note);
                    pivotObject.transform.SetParent(transform);

                    // Position relative au piano - Notes noires plus hautes
                    float yPos = IsBlackKey(note.note) ? 0.055f : 0f;
                    pivotObject.transform.localPosition = new Vector3(xPos, yPos, travelDistance);

                    // Collider et rigidbody pour détection
                    BoxCollider pivotCollider = pivotObject.AddComponent<BoxCollider>();
                    pivotCollider.size = new Vector3(0.15f, 0.2f, 0.5f);
                    pivotCollider.center = Vector3.zero;
                    pivotCollider.isTrigger = true;

                    Rigidbody rb = pivotObject.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;

                    // Reparenter la note
                    newNote.transform.SetParent(pivotObject.transform);
                    newNote.transform.localPosition = new Vector3(0, 0, noteLengthUnits / 2);

                    // Mouvement
                    float travelTime = travelDistance / noteSpeed;
                    NoteMover noteMover = pivotObject.AddComponent<NoteMover>();
                    float targetYPos = IsBlackKey(note.note) ? 0.055f : 0f;
                    noteMover.Init(travelTime, new Vector3(xPos, targetYPos, 0), note.note);

                    // Matériau de hit et main
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

                    // Activer/désactiver selon la main à pratiquer
                    bool isRight = noteMover.handType == "right";
                    bool isLeft = noteMover.handType == "left";
                    bool active = practiceHand == PracticeHand.Both ||
                                  (practiceHand == PracticeHand.Right && isRight) ||
                                  (practiceHand == PracticeHand.Left && isLeft);
                    noteMover.isActiveForPractice = active;

                    // Si non active, assigner un matériau grisé
                    if (!active && disabledNoteMaterial != null)
                    {
                        Renderer nr = newNote.GetComponent<Renderer>();
                        if (nr != null)
                        {
                            nr.material = disabledNoteMaterial;
                        }
                    }
                }
            }

            // Attendre un peu après la dernière note
            yield return new WaitForSeconds(5f);
        }

        if (useLoopByMeasures)
        {
            Debug.Log($"🔁 Lecture en boucle des mesures {loopStartMeasure} à {loopEndMeasure} (beats {loopStartBeat} à {loopEndBeatExclusive - 1})");
            while (isMusicStarted)
            {
                songStartTime = Time.time;
                yield return StartCoroutine(PlayPass());
            }
        }
        else
        {
            yield return StartCoroutine(PlayPass());

            // Appeler EndGame() si le GameManager est disponible
            if (gameManager != null)
            {
                Debug.Log("🏁 Fin de la chanson - Appel de EndGame()");
                gameManager.EndGame();
            }
            else
            {
                Debug.LogWarning("⚠️ GameManager non assigné - Impossible d'appeler EndGame()");
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
        
        // Réinitialiser le système d'attente
        ResetWaitingSystem();
        
        // Nettoyer toutes les notes existantes
        CleanupNotes();
    }
    
    // Méthode pour réinitialiser le système d'attente
    private void ResetWaitingSystem()
    {
        isWaitingForNote = false;
        currentWaitingNote = null;
        
        if (waitTimeoutCoroutine != null)
        {
            StopCoroutine(waitTimeoutCoroutine);
            waitTimeoutCoroutine = null;
        }
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

    private int ParseBeatsPerMeasure(string timeSignature)
    {
        if (string.IsNullOrEmpty(timeSignature)) return 4;
        int slash = timeSignature.IndexOf('/')
;        if (slash <= 0) { int n; if (int.TryParse(timeSignature, out n)) return n; return 4; }
        int numerator;
        if (int.TryParse(timeSignature.Substring(0, slash), out numerator)) return numerator;
        return 4;
    }
    
    // Coroutine pour gérer le timeout d'attente (non affectée par Time.timeScale)
    IEnumerator WaitTimeoutCoroutine()
    {
        yield return new WaitForSecondsRealtime(waitTimeout);
        
        if (isWaitingForNote)
        {
            Debug.LogWarning($"⏰ Timeout atteint pour la note {currentWaitingNote?.note} - Passage à la note suivante");
            OnNotePlayed(); // Forcer le passage à la note suivante
        }
    }
    
    // Méthode publique appelée par NoteTrigger quand une note est jouée
    public void OnNotePlayed()
    {
        if (isWaitingForNote)
        {
            Debug.Log($"✅ Note {currentWaitingNote?.note} jouée - Reprise du morceau");
            isWaitingForNote = false;
            currentWaitingNote = null;
            
            // Reprendre la musique si elle était en pause
            if (isMusicPaused)
            {
                ResumeMusicFromNoteWait();
            }
            
            // Arrêter le timeout
            if (waitTimeoutCoroutine != null)
            {
                StopCoroutine(waitTimeoutCoroutine);
                waitTimeoutCoroutine = null;
            }
        }
    }
    
    // Méthode publique pour forcer le passage à la note suivante (utile pour les tests)
    [ContextMenu("Forcer passage à la note suivante")]
    public void ForceNextNote()
    {
        if (isWaitingForNote)
        {
            Debug.Log($"⏭️ Passage forcé à la note suivante (note {currentWaitingNote?.note} ignorée)");
            OnNotePlayed();
        }
        else
        {
            Debug.Log("Aucune note en attente - impossible de forcer le passage");
        }
    }
    
    // Méthode appelée par NoteTrigger quand une note entre dans la zone de jeu
    // On attend désormais que le pivot atteigne le plan du clavier (z <= 0) avant de mettre en pause
    public void OnNoteEnterHitZone(NoteMover mover)
    {
        if (!useStopAndWaitMode || isWaitingForNote || mover == null)
            return;

        // Ne pas déclencher de pause si la note n'est pas active pour la pratique
        if (!mover.isActiveForPractice)
            return;

        StartCoroutine(PauseWhenAtKeyboard(mover));
    }

    private IEnumerator PauseWhenAtKeyboard(NoteMover mover)
    {
        // Attendre que le pivot atteigne le plan du clavier (z <= 0 en local)
        // ou arrêter si la note a déjà été jouée avant d'atteindre le clavier
        while (mover != null && mover.transform.localPosition.z > 0f && !mover.hasBeenHit)
        {
            // Si entre-temps la note devient non active (changement de main), sortir
            if (mover != null && !mover.isActiveForPractice)
                yield break;
            yield return null;
        }

        // Si la note a été jouée avant d'atteindre le clavier, ne pas mettre en pause
        if (mover == null || !useStopAndWaitMode || isWaitingForNote || mover.hasBeenHit)
            yield break;

        Debug.Log($"⏸️ Note {mover.noteName} au clavier - Pause du morceau");
        isWaitingForNote = true;
        currentWaitingNote = new NoteData { note = mover.noteName, hand = mover.handType };
        isMusicPaused = true;

        // Mettre en pause le temps du jeu
        Time.timeScale = 0f;

        // Démarrer le timeout (en temps réel)
        if (waitTimeoutCoroutine != null)
            StopCoroutine(waitTimeoutCoroutine);
        waitTimeoutCoroutine = StartCoroutine(WaitTimeoutCoroutine());
    }
    
    // Méthode pour reprendre la musique après qu'une note soit jouée
    private void ResumeMusicFromNoteWait()
    {
        Debug.Log("▶️ Reprise du morceau après note jouée");
        isMusicPaused = false;
        Time.timeScale = 1f;
    }
}
