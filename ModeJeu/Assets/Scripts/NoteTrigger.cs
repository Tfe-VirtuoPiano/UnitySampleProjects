using UnityEngine;
using MidiJack;

public class NoteTrigger : MonoBehaviour
{
    public string expectedNote; // La note à attendre (ex: "C4")
    public ScoreManager scoreManager; // Référence au ScoreManager

    private NoteMover noteInZone;
    private GameObject noteObject; // Référence à l'objet note (enfant du pivot)
    private int expectedMidiNote;

    void Start()
    {
        // Récupérer le numéro MIDI correspondant à la note attendue
        expectedMidiNote = MidiNoteUtils.GetMidiNumber(expectedNote);
        
        Debug.Log($"NoteTrigger pour {expectedNote} initialisé - MIDI: {expectedMidiNote}");
        
        // Vérifier que le trigger a bien un collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"Le NoteTrigger pour {expectedNote} n'a pas de Collider! Ajout automatique...");
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.2f, 0.2f, 0.5f);
            col.isTrigger = true;
        }
        else if (!GetComponent<Collider>().isTrigger)
        {
            Debug.LogWarning($"Le Collider du NoteTrigger pour {expectedNote} n'est pas en mode Trigger!");
            GetComponent<Collider>().isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"⚡ Collision détectée sur {expectedNote} avec {other.gameObject.name}");
        
        // Vérifier si c'est un pivot de note qui entre dans la zone
        NoteMover mover = other.GetComponent<NoteMover>();
        if (mover != null && mover.noteName == expectedNote && !mover.hasBeenHit)
        {
            noteInZone = mover;
            mover.isInHitZone = true;
            
            // Stocker une référence au premier enfant (l'objet note)
            if (other.transform.childCount > 0)
            {
                noteObject = other.transform.GetChild(0).gameObject;
            }
            
            Debug.Log($"🎹 Note {expectedNote} prête à être jouée avec MIDI {expectedMidiNote}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        NoteMover mover = other.GetComponent<NoteMover>();
        if (mover == noteInZone && !mover.hasBeenHit)
        {
            mover.isInHitZone = false;
            noteInZone = null;
            noteObject = null;
            
            // Note manquée - pénaliser le score
            if (scoreManager != null)
            {
                scoreManager.NoteMissed();
            }
            
            Debug.Log($"❌ MISS: {expectedNote} left the hit zone without being played.");
        }
    }

    void Update()
    {
        // Détection des entrées MIDI uniquement
        if (MidiMaster.GetKeyDown(expectedMidiNote))
        {
            Debug.Log($"🎹 Note MIDI {expectedMidiNote} ({expectedNote}) enfoncée!");
            
            // Si une note est dans la zone, la jouer
            if (noteInZone != null)
            {
                PlayNote();
            }
            else
            {
                // Mauvais input - aucune note dans la zone
                if (scoreManager != null)
                {
                    scoreManager.BadInput();
                }
                Debug.Log($"⚠️ Mauvais input sur {expectedNote} - aucune note dans la zone");
            }
        }
    }
    
    // Méthode séparée pour jouer la note
    private void PlayNote()
    {
        if (noteInZone != null)
        {
            // Utiliser la méthode Hit() du NoteMover
            noteInZone.Hit();
            
            // Ajouter des points au score
            if (scoreManager != null)
            {
                scoreManager.NoteHit();
            }
            
            // Réinitialiser les références
            noteInZone = null;
            noteObject = null;
        }
    }
}
