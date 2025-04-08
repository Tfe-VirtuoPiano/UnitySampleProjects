using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    [SerializeField] private string testEmail = "test@example.com";
    [SerializeField] private string testPassword = "password123";
    [SerializeField] private bool autoLoginOnStart = true;

    private string authToken;
    private const string API_URL = "http://localhost:3000/api/auth/unity";

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
            // Création de l'objet de données
            var loginData = new LoginData(email, password);
            string jsonData = JsonUtility.ToJson(loginData);

            // Log des données envoyées
            Debug.Log($"Données envoyées : {jsonData}");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest www = new UnityWebRequest(API_URL, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                // Log de la requête
                Debug.Log($"URL de la requête : {API_URL}");
                Debug.Log($"Headers : {www.GetRequestHeader("Content-Type")}");

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
                    Debug.LogError($"Erreur de connexion : {www.error}");
                    Debug.LogError($"Code de réponse : {www.responseCode}");
                    Debug.LogError($"Headers de réponse : {www.GetResponseHeaders()}");
                    return false;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception lors de la connexion : {e.Message}");
            return false;
        }
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