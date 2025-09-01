using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;

public class GameManager : MonoBehaviour
{
    [Header("Références")]
    public NoteSpawner noteSpawner;
    public SongManager songManager;

    [Header("Paramètres")]
    public float countdownDuration = 3f;
    
    [Header("État du jeu")]
    [SerializeField] private GameState currentState = GameState.MainMenu;

    [Header("Statistiques de session")]
    [SerializeField] private int goodNotesCount = 0;     // Bonnes notes (jouées au bon moment)
    [SerializeField] private int badNotesCount = 0;      // Mauvaises notes (non attendues)
    [SerializeField] private int missedNotesCount = 0;   // Notes jouées correctement mais trop tard
    
    // Horodatage de session (UTC)
    private System.DateTime sessionStartTimeUtc;
    private System.DateTime sessionEndTimeUtc;
    
    public enum GameState
    {
        MainMenu,    // Menu principal
        Countdown,   // Compte à rebours avant de commencer
        Playing,     // Jeu en cours
        Paused,      // Jeu en pause
        GameOver     // Fin de partie
    }
    
    private Coroutine countdownCoroutine;
    
    void Start()
    {
        // Désactiver le spawner automatique
        if (noteSpawner != null)
        {
            noteSpawner.startDelay = 0f; // Pas de délai automatique
        }
        
        // Configurer le SongManager
        if (songManager != null)
        {
            songManager.OnSongsLoaded += OnSongsLoaded;
            songManager.OnSongSelected += OnSongSelected;
            songManager.OnError += OnSongError;
        }
        

        // Afficher le menu principal
        SetGameState(GameState.MainMenu);
    }
    
    [ContextMenu("Démarrer le jeu")]
    public void StartGame()
    {
        Debug.Log("🎮 Démarrage du jeu...");
        SetGameState(GameState.Countdown);
        // Démarre une nouvelle session
        ResetSessionStats();
        sessionStartTimeUtc = System.DateTime.UtcNow;
        
        // Démarrer le compte à rebours
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }
    
    IEnumerator CountdownCoroutine()
    {
        Debug.Log($"⏰ Compte à rebours de {countdownDuration} secondes...");
        
        for (int i = (int)countdownDuration; i > 0; i--)
        {
            Debug.Log($"Préparez-vous... {i}");
            yield return new WaitForSeconds(1f);
        }
        
        Debug.Log("C'est parti !");
        yield return new WaitForSeconds(0.5f);
        
        // Démarrer la musique
        StartMusic();
    }
    
    void StartMusic()
    {
        Debug.Log("🎵 Démarrage de la musique !");
        SetGameState(GameState.Playing);
        
        if (noteSpawner != null)
        {
            // Démarrer le spawner de notes
            noteSpawner.StartMusic();
        }
    }
    
    [ContextMenu("Mettre en pause")]
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            Debug.Log("⏸️ Jeu mis en pause");
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
            
            if (noteSpawner != null)
            {
                noteSpawner.PauseMusic();
            }
        }
    }
    
    [ContextMenu("Reprendre")]
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            Debug.Log("▶️ Reprise du jeu");
            SetGameState(GameState.Playing);
            Time.timeScale = 1f;
            
            if (noteSpawner != null)
            {
                noteSpawner.ResumeMusic();
            }
        }
    }
    
    [ContextMenu("Recommencer")]
    public void RestartGame()
    {
        Debug.Log("🔄 Redémarrage du jeu");
        Time.timeScale = 1f;
        
        if (noteSpawner != null)
        {
            noteSpawner.StopMusic();
        }
        

        
        SetGameState(GameState.MainMenu);
    }

    // ====== Statistiques de session ======
    public void ResetSessionStats()
    {
        goodNotesCount = 0;
        badNotesCount = 0;
        missedNotesCount = 0;
    }

    public void RegisterGoodNote()
    {
        goodNotesCount++;
        // Debug.Log($"✅ Bonne note. Total: {goodNotesCount}");
    }

    public void RegisterBadNote()
    {
        badNotesCount++;
        // Debug.Log($"❌ Mauvaise note. Total: {badNotesCount}");
    }

    public void RegisterMissedNote()
    {
        missedNotesCount++;
        // Debug.Log($"⌛ Note manquée. Total: {missedNotesCount}");
    }

    public (int good, int bad, int missed) GetSessionStats()
    {
        return (goodNotesCount, badNotesCount, missedNotesCount);
    }
    
    void SetGameState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"🎮 État du jeu changé vers: {newState}");
    }
    
    // Méthode publique pour vérifier l'état actuel
    public GameState GetCurrentState()
    {
        return currentState;
    }
    
    // Méthode pour terminer la partie (appelée quand la musique se termine)
    public void EndGame()
    {
        Debug.Log("🏁 Fin de la partie");
        

        
        SetGameState(GameState.GameOver);
    }
    
    // ====== Upload du score apprentissage ======
    [ContextMenu("Arrêter la session et envoyer le score")]
    public void EndLearningSessionAndUpload()
    {
        // Arrêter toute lecture/état de jeu
        Time.timeScale = 1f;
        if (noteSpawner != null) noteSpawner.StopMusic();

        sessionEndTimeUtc = System.DateTime.UtcNow;

        // Préparer le payload
        string userId = PlayerPrefs.GetString("idUser", "");
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("Utilisateur non connecté ou token manquant - impossible d'envoyer le score");
            return;
        }

        if (songManager == null)
        {
            Debug.LogError("SongManager manquant");
            return;
        }
        SongData currentSong = songManager.GetCurrentSong();
        if (currentSong == null || string.IsNullOrEmpty(currentSong.id))
        {
            Debug.LogError("Aucune chanson sélectionnée ou ID de chanson manquant");
            return;
        }

        int selectedTempo = noteSpawner != null ? noteSpawner.tempo : 0;
        string hands = GetHandsFromPractice();

        var payload = new UnityLearningScorePayload
        {
            userId = userId,
            songId = currentSong.id,
            correctNotes = goodNotesCount,
            missedNotes = missedNotesCount,
            wrongNotes = badNotesCount,
            hands = hands,
            selectedTempo = selectedTempo,
            sessionStartTime = sessionStartTimeUtc.ToString("o"),
            sessionEndTime = sessionEndTimeUtc.ToString("o")
        };

        StartCoroutine(UploadLearningScoreCoroutine(payload));
    }

    private string GetHandsFromPractice()
    {
        if (noteSpawner == null) return null;
        // Mapper la sélection de main du spawner vers la valeur attendue par l'API
        var ph = noteSpawner.practiceHand.ToString().ToLower();
        if (ph == "both" || ph == "left" || ph == "right") return ph;
        return null;
    }

    private IEnumerator UploadLearningScoreCoroutine(UnityLearningScorePayload payload)
    {
        string baseUrl = songManager.apiConfig != null ? songManager.apiConfig.APIUrl : "";
        if (string.IsNullOrEmpty(baseUrl))
        {
            Debug.LogError("API Url non configurée");
            yield break;
        }
        string url = baseUrl.TrimEnd('/') + "/api/unity/recordLearningScore";

        string authToken = PlayerPrefs.GetString("AuthToken", "");
        string apiKey = songManager.apiConfig != null ? songManager.apiConfig.APIKey : "";

        string json = JsonUtility.ToJson(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(authToken)) www.SetRequestHeader("Authorization", $"Bearer {authToken}");
            if (!string.IsNullOrEmpty(apiKey)) www.SetRequestHeader("X-API-Key", apiKey);

            Debug.Log($"📤 Envoi du score d'apprentissage vers {url}: {json}");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Score d'apprentissage envoyé: {www.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ Échec d'envoi du score: {www.responseCode} - {www.error} - {www.downloadHandler.text}");
            }
        }
    }

    [System.Serializable]
    private class UnityLearningScorePayload
    {
        public string userId;
        public string songId;
        public int correctNotes;
        public int missedNotes;
        public int wrongNotes;
        public string hands; // 'right' | 'left' | 'both' | null
        public int selectedTempo;
        public string sessionStartTime; // ISO8601
        public string sessionEndTime;   // ISO8601
    }
    
    // Méthodes pour gérer les événements du SongManager
    void OnSongsLoaded(List<SongData> songs)
    {
        Debug.Log($"🎵 {songs.Count} chansons chargées dans le GameManager");
    }
    
    void OnSongSelected(SongData song)
    {
        Debug.Log($"🎵 Chanson sélectionnée dans le GameManager: {song.title}");
        // Assigner la chanson au NoteSpawner
        if (noteSpawner != null)
        {
            noteSpawner.songData = song;
        }
        

    }
    
    void OnSongError(string error)
    {
        Debug.LogError($"❌ Erreur SongManager: {error}");
    }
    
    // Méthodes publiques pour contrôler les chansons
    [ContextMenu("Charger les chansons")]
    public void LoadSongs()
    {
        if (songManager != null)
        {
            songManager.LoadUserSongs();
        }
    }
    
    [ContextMenu("Sélectionner la première chanson")]
    public void SelectFirstSong()
    {
        if (songManager != null)
        {
            songManager.SelectFirstSong();
        }
    }


}
