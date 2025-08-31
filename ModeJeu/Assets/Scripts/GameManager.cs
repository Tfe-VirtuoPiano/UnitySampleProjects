using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Références")]
    public NoteSpawner noteSpawner;
    public SongManager songManager;
    
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
        SetGameState(GameState.GameOver);
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
