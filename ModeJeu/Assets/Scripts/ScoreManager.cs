using UnityEngine;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    [Header("Paramètres de Score")]
    public int basePointsPerNote = 100;
    public int multiplierIncrement = 1;
    public int maxMultiplier = 8;
    
    [Header("État du Score")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int currentMultiplier = 1;
    [SerializeField] private int consecutiveHits = 0;
    [SerializeField] private int totalNotesHit = 0;
    [SerializeField] private int totalNotesMissed = 0;
    
    [Header("Statistiques de Session")]
    [SerializeField] private int maxMultiplierReached = 1;
    [SerializeField] private int maxComboReached = 0;
    [SerializeField] private System.DateTime sessionStartTime;
    [SerializeField] private System.DateTime sessionEndTime;
    
    // Événements
    public delegate void ScoreChangedHandler(int newScore, int multiplier);
    public event ScoreChangedHandler OnScoreChanged;
    
    public delegate void MultiplierChangedHandler(int newMultiplier);
    public event MultiplierChangedHandler OnMultiplierChanged;
    
    public delegate void NoteHitHandler(int points, int multiplier);
    public event NoteHitHandler OnNoteHit;
    
    public delegate void NoteMissedHandler();
    public event NoteMissedHandler OnNoteMissed;
    
    void Start()
    {
        ResetScore();
    }
    
    [ContextMenu("Réinitialiser le score")]
    public void ResetScore()
    {
        currentScore = 0;
        currentMultiplier = 1;
        consecutiveHits = 0;
        totalNotesHit = 0;
        totalNotesMissed = 0;
        maxMultiplierReached = 1;
        maxComboReached = 0;
        
        // Démarrer une nouvelle session
        sessionStartTime = System.DateTime.UtcNow;
        
        OnScoreChanged?.Invoke(currentScore, currentMultiplier);
        OnMultiplierChanged?.Invoke(currentMultiplier);
        
        Debug.Log("🎯 Score réinitialisé");
    }
    
    public void NoteHit()
    {
        // Calculer les points avec le multiplicateur
        int pointsEarned = basePointsPerNote * currentMultiplier;
        currentScore += pointsEarned;
        totalNotesHit++;
        consecutiveHits++;
        
        // Mettre à jour les statistiques de session
        if (currentMultiplier > maxMultiplierReached)
            maxMultiplierReached = currentMultiplier;
        if (consecutiveHits > maxComboReached)
            maxComboReached = consecutiveHits;
        
        // Augmenter le multiplicateur
        IncreaseMultiplier();
        
        // Déclencher les événements
        OnScoreChanged?.Invoke(currentScore, currentMultiplier);
        OnNoteHit?.Invoke(pointsEarned, currentMultiplier);
        
        Debug.Log($"🎯 Note jouée ! +{pointsEarned} points (x{currentMultiplier}) - Score: {currentScore}");
    }
    
    public void NoteMissed()
    {
        totalNotesMissed++;
        ResetMultiplier();
        
        OnScoreChanged?.Invoke(currentScore, currentMultiplier);
        OnNoteMissed?.Invoke();
        
        Debug.Log($"❌ Note manquée ! Multiplicateur remis à {currentMultiplier} - Score: {currentScore}");
    }
    
    public void BadInput()
    {
        ResetMultiplier();
        
        OnScoreChanged?.Invoke(currentScore, currentMultiplier);
        OnNoteMissed?.Invoke();
        
        Debug.Log($"⚠️ Mauvais input ! Multiplicateur remis à {currentMultiplier} - Score: {currentScore}");
    }
    
    private void IncreaseMultiplier()
    {
        if (currentMultiplier < maxMultiplier)
        {
            currentMultiplier += multiplierIncrement;
            OnMultiplierChanged?.Invoke(currentMultiplier);
        }
    }
    
    private void ResetMultiplier()
    {
        currentMultiplier = 1;
        consecutiveHits = 0;
        OnMultiplierChanged?.Invoke(currentMultiplier);
    }
    
    // Getters publics
    public int GetCurrentScore() => currentScore;
    public int GetCurrentMultiplier() => currentMultiplier;
    public int GetConsecutiveHits() => consecutiveHits;
    public int GetTotalNotesHit() => totalNotesHit;
    public int GetTotalNotesMissed() => totalNotesMissed;
    public float GetAccuracy() => totalNotesHit + totalNotesMissed > 0 ? (float)totalNotesHit / (totalNotesHit + totalNotesMissed) * 100f : 0f;
    
    // Getters pour les statistiques de session
    public int GetMaxMultiplierReached() => maxMultiplierReached;
    public int GetMaxComboReached() => maxComboReached;
    public System.DateTime GetSessionStartTime() => sessionStartTime;
    public System.DateTime GetSessionEndTime() => sessionEndTime;
    
    // Méthodes pour affichage
    public string GetScoreText()
    {
        return $"Score: {currentScore:N0}";
    }
    
    public string GetMultiplierText()
    {
        return currentMultiplier > 1 ? $"x{currentMultiplier}" : "";
    }
    
    public string GetAccuracyText()
    {
        return $"Précision: {GetAccuracy():F1}%";
    }
    
    public string GetStatsText()
    {
        return $"Notes jouées: {totalNotesHit} | Manquées: {totalNotesMissed} | Combo: {consecutiveHits}";
    }
    
    // Méthode pour terminer la session et préparer les données pour l'API
    public void EndSession()
    {
        sessionEndTime = System.DateTime.UtcNow;
        Debug.Log($"🏁 Session terminée - Durée: {(sessionEndTime - sessionStartTime).TotalSeconds:F1}s");
    }
    
    // Méthode pour obtenir les données de score pour l'API
    public GameScoreData GetScoreDataForAPI()
    {
        return new GameScoreData
        {
            totalPoints = currentScore,
            maxMultiplier = maxMultiplierReached,
            maxCombo = maxComboReached,
            sessionStartTime = sessionStartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            sessionEndTime = sessionEndTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
    }
}

// Classe pour les données de score à envoyer à l'API
[System.Serializable]
public class GameScoreData
{
    public int totalPoints;
    public int maxMultiplier;
    public int maxCombo;
    public string sessionStartTime;
    public string sessionEndTime;
}
