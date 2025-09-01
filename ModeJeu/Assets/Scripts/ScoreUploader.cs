using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class ScoreUploader : MonoBehaviour
{
    [Header("Configuration API")]
    public APIConfig apiConfig;
    
    [Header("État")]
    [SerializeField] private bool isUploading = false;
    
    // Événements
    public delegate void ScoreUploadedHandler(bool success, string message);
    public event ScoreUploadedHandler OnScoreUploaded;
    
    void Start()
    {
        // Trouver la configuration API si elle n'est pas assignée
        if (apiConfig == null)
        {
            apiConfig = FindFirstObjectByType<APIConfig>();
            if (apiConfig == null)
            {
                Debug.LogError("❌ ScoreUploader: APIConfig non trouvé !");
            }
        }
    }
    
    [ContextMenu("Tester l'upload de score")]
    public void TestUploadScore()
    {
        // Créer des données de test
        GameScoreData testData = new GameScoreData
        {
            totalPoints = 1500,
            maxMultiplier = 3,
            maxCombo = 5,
            sessionStartTime = System.DateTime.UtcNow.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            sessionEndTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
        
        // Utiliser des IDs de test
        string testUserId = "b981ac4c-4669-4904-8018-021d5eb6dad7";
        string testSongId = "f66ea6a5-d940-45c9-91f3-ccd71cafae60";
        
        StartCoroutine(UploadScoreCoroutine(testData, testUserId, testSongId));
    }
    
    public void UploadScore(GameScoreData scoreData, string userId, string songId)
    {
        if (isUploading)
        {
            Debug.LogWarning("⚠️ Upload déjà en cours...");
            return;
        }
        
        if (apiConfig == null)
        {
            Debug.LogError("❌ APIConfig non configuré !");
            OnScoreUploaded?.Invoke(false, "Configuration API manquante");
            return;
        }
        
        StartCoroutine(UploadScoreCoroutine(scoreData, userId, songId));
    }
    
    IEnumerator UploadScoreCoroutine(GameScoreData scoreData, string userId, string songId)
    {
        isUploading = true;
        
        // Récupérer le token d'authentification
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        Debug.Log("🔍 Token d'authentification: " + authToken);
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("❌ Token d'authentification manquant !");
            OnScoreUploaded?.Invoke(false, "Token d'authentification manquant");
            isUploading = false;
            yield break;
        }
        
        // Construire l'URL de l'API
        string apiUrl = apiConfig.APIUrl + "/api/unity/recordGameScore";
        
        // Créer l'objet de données à envoyer
        GameScoreUploadData uploadData = new GameScoreUploadData
        {
            userId = userId,
            songId = songId,
            totalPoints = scoreData.totalPoints,
            maxMultiplier = scoreData.maxMultiplier,
            maxCombo = scoreData.maxCombo,
            sessionStartTime = scoreData.sessionStartTime,
            sessionEndTime = scoreData.sessionEndTime
        };
        
        // Vérifier que les données sont bien remplies
        Debug.Log($"🔍 Données avant envoi - userId: '{uploadData.userId}', songId: '{uploadData.songId}', totalPoints: {uploadData.totalPoints}");
        
        // Convertir en JSON
        string jsonData = JsonUtility.ToJson(uploadData);
        Debug.Log($"📤 JSON à envoyer: {jsonData}");
        
        // Créer la requête
        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            
            // Ajouter les headers
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {authToken}");
            www.SetRequestHeader("X-API-Key", apiConfig.APIKey);
            
            // Envoyer la requête
            yield return www.SendWebRequest();
            
            // Traiter la réponse
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Score uploadé avec succès ! Réponse: {www.downloadHandler.text}");
                
                // Parser la réponse pour extraire les informations
                try
                {
                    var response = JsonUtility.FromJson<ScoreUploadResponse>(www.downloadHandler.text);
                    if (response.success)
                    {
                        Debug.Log($"🎯 Score enregistré - ID: {response.scoreId}");
                        OnScoreUploaded?.Invoke(true, $"Score enregistré avec succès ! ID: {response.scoreId}");
                    }
                    else
                    {
                        Debug.LogError($"❌ Erreur API: {response.error}");
                        OnScoreUploaded?.Invoke(false, $"Erreur API: {response.error}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ Erreur de parsing de la réponse: {e.Message}");
                    OnScoreUploaded?.Invoke(false, "Erreur de parsing de la réponse");
                }
            }
            else
            {
                Debug.LogError($"❌ Erreur d'upload: {www.error}");
                OnScoreUploaded?.Invoke(false, $"Erreur d'upload: {www.error}");
            }
        }
        
        isUploading = false;
    }
    
    public bool IsUploading()
    {
        return isUploading;
    }
}

// Classe pour parser la réponse de l'API
[System.Serializable]
public class ScoreUploadResponse
{
    public bool success;
    public string scoreId;
    public string message;
    public string error;
}

// Classe pour les données d'upload de score
[System.Serializable]
public class GameScoreUploadData
{
    public string userId;
    public string songId;
    public int totalPoints;
    public int maxMultiplier;
    public int maxCombo;
    public string sessionStartTime;
    public string sessionEndTime;
}
