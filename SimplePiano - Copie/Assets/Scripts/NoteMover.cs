using UnityEngine;

public class NoteMover : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float travelTime; // Temps de parcours basé sur le tempo, pas sur la durée de la note
    private float elapsed;

    public string noteName;
    public bool isInHitZone = false;

    public void Init(float targetTravelTime, Vector3 destination, string note)
    {
        travelTime = targetTravelTime; // Temps basé sur le tempo, pas sur la durée de la note
        startPosition = transform.position;
        targetPosition = destination;
        noteName = note;
        elapsed = 0f;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / travelTime);
        transform.position = Vector3.Lerp(startPosition, targetPosition, t);

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
