using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Références")]
    public NoteSpawner noteSpawner;
    public Button startButton;
    public Button pauseButton;
    public Button restartButton;
    
    [Header("UI Elements")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public Text statusText;
    
    [Header("Paramètres")]
    public float countdownDuration = 3f;
    
    public enum GameState
    {
        MainMenu,    // Menu principal
        Countdown,   // Compte à rebours avant de commencer
        Playing,     // Jeu en cours
        Paused,      // Jeu en pause
        GameOver     // Fin de partie
    }
    
    private GameState currentState = GameState.MainMenu;
    private Coroutine countdownCoroutine;
    
    void Start()
    {
        // Initialiser l'interface
        SetupUI();
        
        // Désactiver le spawner automatique
        if (noteSpawner != null)
        {
            noteSpawner.startDelay = 0f; // Pas de délai automatique
        }
        
        // Afficher le menu principal
        SetGameState(GameState.MainMenu);
    }
    
    void SetupUI()
    {
        // Configurer les boutons
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
            
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }
    
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
            if (statusText != null)
                statusText.text = $"Préparez-vous... {i}";
            yield return new WaitForSeconds(1f);
        }
        
        if (statusText != null)
            statusText.text = "C'est parti !";
        
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
    
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            PauseGame();
        }
        else if (currentState == GameState.Paused)
        {
            ResumeGame();
        }
    }
    
    void PauseGame()
    {
        Debug.Log("⏸️ Jeu mis en pause");
        SetGameState(GameState.Paused);
        Time.timeScale = 0f;
        
        if (noteSpawner != null)
        {
            noteSpawner.PauseMusic();
        }
    }
    
    void ResumeGame()
    {
        Debug.Log("▶️ Reprise du jeu");
        SetGameState(GameState.Playing);
        Time.timeScale = 1f;
        
        if (noteSpawner != null)
        {
            noteSpawner.ResumeMusic();
        }
    }
    
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
        UpdateUI();
        
        Debug.Log($"🎮 État du jeu changé vers: {newState}");
    }
    
    void UpdateUI()
    {
        switch (currentState)
        {
            case GameState.MainMenu:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                if (gamePanel != null) gamePanel.SetActive(false);
                if (statusText != null) statusText.text = "Prêt à jouer ?";
                break;
                
            case GameState.Countdown:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
                if (gamePanel != null) gamePanel.SetActive(true);
                break;
                
            case GameState.Playing:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
                if (gamePanel != null) gamePanel.SetActive(true);
                if (statusText != null) statusText.text = "Jouez !";
                break;
                
            case GameState.Paused:
                if (statusText != null) statusText.text = "Pause";
                break;
                
            case GameState.GameOver:
                if (statusText != null) statusText.text = "Partie terminée !";
                break;
        }
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
}
