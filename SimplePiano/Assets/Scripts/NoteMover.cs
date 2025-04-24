using UnityEngine;

public class NoteMover : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float travelTime; // Temps total de parcours basé sur le tempo
    private float elapsed;

    public string noteName;
    public bool isInHitZone = false;
    public bool hasBeenHit = false; // Indique si la note a été frappée par le joueur

    // Lors de l'initialisation, on stocke les informations nécessaires
    public void Init(float targetTravelTime, Vector3 destination, string note)
    {
        travelTime = targetTravelTime;
        startPosition = transform.position;
        targetPosition = destination;
        noteName = note;
        elapsed = 0f;
    }

    void Update()
    {
        // Si la note a été jouée, on la détruit immédiatement
        if (hasBeenHit)
        {
            Debug.Log($"💥 Destruction de la note {noteName} après frappe");
            DestroyNote();
            return;
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / travelTime);
        
        // Déplacement du pivot à vitesse constante
        transform.position = Vector3.Lerp(startPosition, targetPosition, t);

     
        if (elapsed > travelTime )
        {
            DestroyNote();
        }
    }
    
    private void DestroyNote()
    {
        // Assurons-nous de détruire toute la hiérarchie de la note
        Destroy(gameObject);
    }
    
    // Cette méthode peut être appelée directement pour frapper la note
    public void Hit()
    {
        hasBeenHit = true;
        
    }
}
