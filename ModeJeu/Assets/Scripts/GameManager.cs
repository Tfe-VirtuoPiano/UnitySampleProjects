using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Références")]
    public NoteSpawner noteSpawner;
    public SongManager songManager;
    public ScoreManager scoreManager;
    public ScoreUploader scoreUploader;
    
    [Header("Paramètres")]
    public float countdownDuration = 3f;
    
    [Header("État du jeu")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    
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
        
        // Configurer le ScoreManager
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += OnScoreChanged;
            scoreManager.OnMultiplierChanged += OnMultiplierChanged;
            scoreManager.OnNoteHit += OnNoteHit;
            scoreManager.OnNoteMissed += OnNoteMissed;
        }
        
        // Configurer le ScoreUploader
        if (scoreUploader != null)
        {
            scoreUploader.OnScoreUploaded += OnScoreUploaded;
        }
        
        // Afficher le menu principal
        SetGameState(GameState.MainMenu);
    }
    
    [ContextMenu("Démarrer le jeu")]
    public void StartGame()
    {
        Debug.Log("🎮 Démarrage du jeu...");
        SetGameState(GameState.Countdown);
        
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
        
        // Réinitialiser le score à chaque redémarrage
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
            Debug.Log("🎯 Score réinitialisé pour le redémarrage");
        }
        
        SetGameState(GameState.MainMenu);
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
        
        // Terminer la session de score
        if (scoreManager != null)
        {
            scoreManager.EndSession();
            
            // Uploader le score si on a les données nécessaires
            UploadScoreIfPossible();
        }
        
        SetGameState(GameState.GameOver);
    }
    
    // Méthode pour uploader le score si possible
    private void UploadScoreIfPossible()
    {
        if (scoreManager == null || scoreUploader == null || songManager == null)
        {
            Debug.LogWarning("⚠️ Impossible d'uploader le score - composants manquants");
            return;
        }
        
        // Récupérer l'ID utilisateur
        string userId = PlayerPrefs.GetString("idUser", "");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("⚠️ Impossible d'uploader le score - ID utilisateur manquant");
            return;
        }
        
        // Récupérer l'ID de la chanson
        SongData currentSong = songManager.GetCurrentSong();
        if (currentSong == null)
        {
            Debug.LogWarning("⚠️ Impossible d'uploader le score - Aucune chanson sélectionnée");
            return;
        }
        
        Debug.Log($"🔍 Chanson actuelle - ID: '{currentSong.id}', Titre: '{currentSong.title}'");
        
        if (string.IsNullOrEmpty(currentSong.id))
        {
            Debug.LogWarning("⚠️ Impossible d'uploader le score - ID chanson manquant ou vide");
            return;
        }
        
        // Récupérer les données de score
        GameScoreData scoreData = scoreManager.GetScoreDataForAPI();
        
        // Uploader le score
        Debug.Log($"📤 Upload du score: {scoreData.totalPoints} points pour la chanson {currentSong.title}");
        scoreUploader.UploadScore(scoreData, userId, currentSong.id);
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
        
        // Réinitialiser le score à chaque changement de chanson
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
            Debug.Log("🎯 Score réinitialisé pour la nouvelle chanson");
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
    
    // Méthodes pour gérer les événements du ScoreManager
    void OnScoreChanged(int newScore, int multiplier)
    {
        Debug.Log($"🎯 Score mis à jour: {newScore} (x{multiplier})");
    }
    
    void OnMultiplierChanged(int newMultiplier)
    {
        Debug.Log($"🔥 Multiplicateur: x{newMultiplier}");
    }
    
    void OnNoteHit(int points, int multiplier)
    {
        Debug.Log($"✅ Note jouée ! +{points} points (x{multiplier})");
    }
    
    void OnNoteMissed()
    {
        Debug.Log($"❌ Note manquée ou mauvais input !");
    }
    
    // Méthode pour gérer l'événement d'upload de score
    void OnScoreUploaded(bool success, string message)
    {
        if (success)
        {
            Debug.Log($"✅ {message}");
        }
        else
        {
            Debug.LogError($"❌ {message}");
        }
    }
    
    // Méthodes publiques pour contrôler le score
    [ContextMenu("Réinitialiser le score")]
    public void ResetScore()
    {
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }
    }
    
    // Méthode publique pour forcer l'upload du score
    [ContextMenu("Uploader le score")]
    public void ForceUploadScore()
    {
        UploadScoreIfPossible();
    }
}
