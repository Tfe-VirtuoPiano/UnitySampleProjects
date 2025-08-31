using UnityEngine;
using UnityEngine.UI;

public class UISetup : MonoBehaviour
{
    [Header("Références")]
    public GameManager gameManager;
    public Canvas mainCanvas;
    
    [Header("Préférences")]
    public bool createUIOnStart = true;
    
    void Start()
    {
        if (createUIOnStart)
        {
            CreateBasicUI();
        }
    }
    
    [ContextMenu("Créer l'interface UI")]
    public void CreateBasicUI()
    {
        if (mainCanvas == null)
        {
            // Créer un Canvas s'il n'existe pas
            GameObject canvasGO = new GameObject("MainCanvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Créer le panel du menu principal
        GameObject mainMenuPanel = CreatePanel("MainMenuPanel", new Vector2(0, 0), new Vector2(1, 1));
        mainMenuPanel.transform.SetParent(mainCanvas.transform, false);
        
        // Créer le panel du jeu
        GameObject gamePanel = CreatePanel("GamePanel", new Vector2(0, 0), new Vector2(1, 1));
        gamePanel.transform.SetParent(mainCanvas.transform, false);
        gamePanel.SetActive(false);
        
        // Créer les boutons
        Button startButton = CreateButton("StartButton", "Commencer", new Vector2(0.5f, 0.6f), new Vector2(200, 60));
        startButton.transform.SetParent(mainMenuPanel.transform, false);
        
        Button pauseButton = CreateButton("PauseButton", "Pause", new Vector2(0.1f, 0.9f), new Vector2(100, 40));
        pauseButton.transform.SetParent(gamePanel.transform, false);
        
        Button restartButton = CreateButton("RestartButton", "Recommencer", new Vector2(0.9f, 0.9f), new Vector2(120, 40));
        restartButton.transform.SetParent(gamePanel.transform, false);
        
        // Créer le texte de statut
        Text statusText = CreateText("StatusText", "Prêt à jouer ?", new Vector2(0.5f, 0.8f), new Vector2(400, 50));
        statusText.transform.SetParent(gamePanel.transform, false);
        
        // Assigner les références au GameManager
        if (gameManager != null)
        {
            gameManager.mainMenuPanel = mainMenuPanel;
            gameManager.gamePanel = gamePanel;
            gameManager.startButton = startButton;
            gameManager.pauseButton = pauseButton;
            gameManager.restartButton = restartButton;
            gameManager.statusText = statusText;
        }
        
        Debug.Log("✅ Interface UI créée avec succès !");
    }
    
    GameObject CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = new GameObject(name);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        return panel;
    }
    
    Button CreateButton(string name, string text, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonGO = new GameObject(name);
        Button button = buttonGO.AddComponent<Button>();
        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        // Créer le texte du bouton
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        Text textComponent = textGO.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textComponent.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Configurer le RectTransform du bouton
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        
        return button;
    }
    
    Text CreateText(string name, string text, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textGO = new GameObject(name);
        Text textComponent = textGO.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 28;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        RectTransform rect = textComponent.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        
        return textComponent;
    }
}
