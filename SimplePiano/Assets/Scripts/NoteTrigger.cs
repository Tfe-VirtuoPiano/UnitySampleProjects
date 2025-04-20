using UnityEngine;

public class NoteTrigger : MonoBehaviour
{
    public string expectedNote; // La note à attendre (ex: "C4")

    private NoteMover noteInZone;
    private GameObject noteObject; // Référence à l'objet note (enfant du pivot)
    private KeyCode expectedKey;
    private bool debugKeyPressed = false;

    void Start()
    {
        // Récupérer la touche correspondante à la note attendue
        expectedKey = KeyMapper.GetKeyForNote(expectedNote);
        Debug.Log($"NoteTrigger pour {expectedNote} initialisé - Touche attendue: {expectedKey}");
        
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
            
            Debug.Log($"🎹 Note {expectedNote} prête à être jouée avec la touche {expectedKey}");
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
            Debug.Log($"❌ MISS: {expectedNote} left the hit zone without being played.");
        }
    }

    void Update()
    {
        // Test spécifique pour cette note - appuyez sur la touche Tab pour frapper n'importe quelle note
        if (Input.GetKeyDown(KeyCode.Tab) && noteInZone != null)
        {
            Debug.Log($"🔨 Forçage de frappe avec Tab pour {expectedNote}");
            PlayNote();
            return;
        }
    
        // Détection normale de la touche associée à cette note
        if (Input.GetKeyDown(expectedKey))
        {
            Debug.Log($"⌨️ Touche {expectedKey} pour note {expectedNote} enfoncée!");
            debugKeyPressed = true;
            
            // Si une note est dans la zone, la jouer
            if (noteInZone != null)
            {
                PlayNote();
            }
        }
    }
    
    // Méthode séparée pour jouer la note
    private void PlayNote()
    {
        if (noteInZone != null)
        {
            Debug.Log($"✅ HIT! Played note: {expectedNote} with key {expectedKey}");
            
            // Utiliser la méthode Hit() du NoteMover
            noteInZone.Hit();
            
            // Réinitialiser les références
            noteInZone = null;
            noteObject = null;
        }
    }
}
