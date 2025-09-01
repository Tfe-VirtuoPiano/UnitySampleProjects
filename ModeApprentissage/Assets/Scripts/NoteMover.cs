using UnityEngine;

public class NoteMover : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float travelTime; // Temps total de parcours du pivot (bord avant) basé sur le tempo
    private float elapsed;
    private Transform noteTransform; // Référence au transform de la note enfant
    private float totalTravelTime; // Temps total incluant le passage de l'arrière de la note

    public string noteName;
    public string handType = "right"; // "left" ou "right" - par défaut "right"
    public bool isInHitZone = false;
    public bool hasBeenHit = false; // Indique si la note a été frappée par le joueur
    public Material hitMaterial; // Matériau à appliquer quand la note est jouée
    public bool isActiveForPractice = true; // Si faux, la note est ignorée pour la pratique (main non sélectionnée)
    
    private MeshRenderer noteRenderer; // Référence au renderer de la note
    private Vector3 endPosition; // Position finale du pivot après avoir traversé le clavier
    private float noteLength; // Longueur réelle de la note en unités mondiales
    private float extraTailTime; // Temps supplémentaire nécessaire pour que l'arrière traverse le clavier
    
    // Lors de l'initialisation, on stocke les informations nécessaires
    public void Init(float targetTravelTime, Vector3 destination, string note)
    {
        travelTime = targetTravelTime;
        startPosition = transform.localPosition; // Utiliser localPosition au lieu de position
        targetPosition = destination;
        noteName = note;
        elapsed = 0f;
        
        // Obtenir la référence à la note (enfant)
        if (transform.childCount > 0)
        {
            noteTransform = transform.GetChild(0);
            noteRenderer = noteTransform.GetComponent<MeshRenderer>();
            
            if (noteRenderer == null)
            {
                Debug.LogWarning($"Pas de MeshRenderer trouvé pour la note {noteName}");
            }
            
            // Calculer la longueur réelle de la note dans l'espace mondial
            // La longueur est la dimension Z de la note, en tenant compte de sa position locale
            // La position locale Z est le centre de la note, il faut ajouter la moitié de sa longueur
            noteLength = noteTransform.localScale.z;
            float backEdgeOffset = noteTransform.localPosition.z + (noteLength / 2);
            
            // Calculer la vitesse de déplacement
            float moveSpeed = Vector3.Distance(startPosition, targetPosition) / travelTime;
            
            // Temps supplémentaire nécessaire pour que l'arrière de la note traverse le clavier
            extraTailTime = backEdgeOffset / moveSpeed;
            
            // Temps total incluant le passage de l'arrière de la note
            totalTravelTime = travelTime + extraTailTime;
            
            // Calculer la position finale avec le déplacement supplémentaire
            Vector3 moveDirection = (targetPosition - startPosition).normalized;
            float extraDistance = moveSpeed * extraTailTime;
            endPosition = targetPosition + (moveDirection * extraDistance);
            
            Debug.Log($"Note {noteName} ({handType}) - Longueur: {noteLength}, Position arrière: {backEdgeOffset}, " +
                     $"Temps standard: {travelTime}s, Temps supplémentaire: {extraTailTime}s, " +
                     $"Total: {totalTravelTime}s");
        }
        else
        {
            Debug.LogWarning($"Pas d'enfant trouvé pour le pivot de la note {noteName}");
            totalTravelTime = travelTime;
            endPosition = targetPosition;
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        
        if (elapsed <= totalTravelTime)
        {
            // Si on n'a pas encore atteint le clavier (première partie du mouvement)
            if (elapsed <= travelTime)
            {
                // Déplacement linéaire standard jusqu'au clavier
                float t = elapsed / travelTime;
                transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t); // Utiliser localPosition
            }
            else
            {
                // Une fois le clavier atteint, on continue au-delà pour que l'arrière passe
                // Calcul de la progression dans cette seconde phase
                float t2 = (elapsed - travelTime) / extraTailTime;
                transform.localPosition = Vector3.Lerp(targetPosition, endPosition, t2); // Utiliser localPosition
            }
        }
        else
        {
            // Note entièrement traversée, détruire
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
        if (hasBeenHit)
            return; // Éviter de traiter plusieurs fois
            
        hasBeenHit = true;
        
        // Changer la couleur de la note si on a un renderer et un matériau
        if (isActiveForPractice && noteRenderer != null && hitMaterial != null)
        {
            Debug.Log($"🎨 Changement de couleur pour la note {noteName} ({handType})");
            noteRenderer.material = hitMaterial;
        }
        else
        {
            // Ne rien faire visuellement si non active, ou si pas de matériel
        }
    }
}
