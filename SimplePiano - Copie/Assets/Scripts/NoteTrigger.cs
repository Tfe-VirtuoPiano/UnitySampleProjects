using UnityEngine;

public class NoteTrigger : MonoBehaviour
{
    public string expectedNote; // La note à attendre (ex: "C4")

    private NoteMover noteInZone;

    void OnTriggerEnter(Collider other)
    {
    

        NoteMover mover = other.GetComponent<NoteMover>();
        if (mover != null && mover.noteName == expectedNote)
        {
            noteInZone = mover;
            mover.isInHitZone = true;
           
        }
    }

    void OnTriggerExit(Collider other)
    {
        NoteMover mover = other.GetComponent<NoteMover>();
        if (mover == noteInZone)
        {
            mover.isInHitZone = false;
            noteInZone = null;
            Debug.Log($"❌ MISS: {expectedNote} left the hit zone without being played.");
        }
    }

    void Update()
    {
        if (noteInZone != null && Input.GetKeyDown(KeyMapper.GetKeyForNote(expectedNote)))
        {
            Debug.Log($"✅ HIT! Played note: {expectedNote}");
            Destroy(noteInZone.gameObject);
            noteInZone = null;
        }
    }
}
