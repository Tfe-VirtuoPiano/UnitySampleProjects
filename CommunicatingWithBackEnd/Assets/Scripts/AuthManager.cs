using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

[System.Serializable]
public class ErrorResponse
{
    public string message;
}

[System.Serializable]
public class User
{
    public string userName;
    public string email;
    public int level;
    public string createdAt;
}

public class UsersList
{
    public User[] Users;
}

public class AuthManager : MonoBehaviour
{
    [SerializeField] private string testEmail = "test@example.com";
    [SerializeField] private string testPassword = "password123";
    [SerializeField] private bool autoLoginOnStart = true;
    [SerializeField] private APIConfig APIConfig;

    // Événement pour notifier les erreurs
    public delegate void ErrorHandler(string message);
    public event ErrorHandler OnError;

    private string authToken;
    private const string API_URL = "http://localhost:3000/api/auth/unity";

    private void Start()
    {
        if (autoLoginOnStart)
        {
            TestLogin();
        }
    }

    public async void TestLogin()
    {
        Debug.Log($"Tentative de connexion avec email: {testEmail}");
        bool success = await Login(testEmail, testPassword);
        Debug.Log(success ? "Connexion réussie !" : "Échec de la connexion");
    }

    private async Task<bool> Login(string email, string password)
    {
        try
        {
            var loginData = new LoginData(email, password);
            string jsonData = JsonUtility.ToJson(loginData);

            Debug.Log($"Données envoyées : {jsonData}");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest www = new UnityWebRequest(API_URL, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                var operation = www.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Réponse du serveur : {www.downloadHandler.text}");
                    authToken = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text).token;

                    // Logs de débogage pour le token
                    Debug.Log($"🔍 Token complet reçu: {authToken}");
                    Debug.Log($"🔍 Longueur du token: {authToken?.Length ?? 0}");
                    Debug.Log($"🔍 Token non vide: {!string.IsNullOrEmpty(authToken)}");

                    PlayerPrefs.SetString("AuthToken", authToken);
                    PlayerPrefs.Save();

                    // Appeler l'API utilisateur après une authentification réussie
                    StartCoroutine(GetUsers());

                    return true;
                }
                else
                {
                    // Tenter de parser le message d'erreur
                    string errorMessage = "Erreur de connexion";
                    if (!string.IsNullOrEmpty(www.downloadHandler.text))
                    {
                        try
                        {
                            var errorResponse = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
                            errorMessage = errorResponse.message;
                        }
                        catch
                        {
                            errorMessage = www.error;
                        }
                    }

                    Debug.LogError($"Erreur de connexion : {errorMessage}");
                    OnError?.Invoke(errorMessage); // Déclencher l'événement d'erreur
                    return false;
                }
            }
        }
        catch (System.Exception e)
        {
            string errorMessage = $"Exception lors de la connexion : {e.Message}";
            Debug.LogError(errorMessage);
            OnError?.Invoke(errorMessage); // Déclencher l'événement d'erreur
            return false;
        }
    }

    IEnumerator GetUsers()
    {
        Debug.Log($"🔍 Début de GetUsers()");
        Debug.Log($"🔍 Token stocké: {authToken?.Substring(0, Mathf.Min(50, authToken?.Length ?? 0))}...");
        Debug.Log($"🔍 API URL: {APIConfig.APIUrl}/api/users");
        Debug.Log($"🔍 API Key: {APIConfig.APIKey?.Substring(0, Mathf.Min(10, APIConfig.APIKey?.Length ?? 0))}...");

        using (UnityWebRequest request = UnityWebRequest.Get($"{APIConfig.APIUrl}/api/users"))
        {
            request.SetRequestHeader("x-api-key", APIConfig.APIKey);

            // IMPORTANT: Ajouter le token d'authentification
            if (!string.IsNullOrEmpty(authToken))
            {
                string authHeader = $"Bearer {authToken}";
                request.SetRequestHeader("Authorization", authHeader);
                Debug.Log($"🔍 Header Authorization ajouté: {authHeader.Substring(0, Mathf.Min(60, authHeader.Length))}...");
            }
            else
            {
                Debug.LogError("🔍 Aucun token d'authentification disponible !");
            }

            yield return request.SendWebRequest();

            Debug.Log($"🔍 Résultat de la requête: {request.result}");
            Debug.Log($"🔍 Code de statut: {request.responseCode}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"Users reçus : {jsonResponse}");

                string wrappedJson = "{\"Users\":" + jsonResponse + "}";
                UsersList AllMyUsers = JsonUtility.FromJson<UsersList>(wrappedJson);
                foreach (User user in AllMyUsers.Users)
                {
                    Debug.Log($"Utilisateur: {user.userName}, Niveau: {user.level}");
                }
            }
            else
            {
                Debug.LogError($"Erreur lors de la récupération des utilisateurs: {request.error}");
                Debug.LogError($"Code de statut: {request.responseCode}");
                Debug.LogError($"Réponse du serveur: {request.downloadHandler.text}");
            }
        }
    }
}

[System.Serializable]
public class LoginData
{
    public string email;
    public string password;

    public LoginData(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}

[System.Serializable]
public class AuthResponse
{
    public string token;
    public UserData user;
}

[System.Serializable]
public class UserData
{
    public string id;
    public string email;
    public string userName;
    public int level;
}
