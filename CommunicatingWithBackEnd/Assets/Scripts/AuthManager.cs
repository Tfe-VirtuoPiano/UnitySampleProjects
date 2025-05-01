using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class ErrorResponse
{
    public string message;
}

public class AuthManager : MonoBehaviour
{
    [SerializeField] private string testEmail = "test@example.com";
    [SerializeField] private string testPassword = "password123";
    [SerializeField] private bool autoLoginOnStart = true;

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
                    PlayerPrefs.SetString("AuthToken", authToken);
                    PlayerPrefs.Save();
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