using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class AuthScript : MonoBehaviour
{

    [SerializeField] private string testEmail = "test@example.com";
    [SerializeField] private string testPassword = "password123";
    [SerializeField] private bool autoLoginOnStart = true;
    [SerializeField] private APIConfig APIConfig;

    // Événement pour notifier les erreurs
    public delegate void ErrorHandler(string message);
    public event ErrorHandler OnError;

    private string authToken;
    private string API_CONNECTION_URL;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (APIConfig != null)
        {
            API_CONNECTION_URL = APIConfig.APIConnectionUrl;
        }
        if (autoLoginOnStart)
        {
            LoginWithTestCredentials();
        }
    }

    private void LoginWithTestCredentials()
    {
        Debug.Log($"Tentative de login avec :{testEmail} ");
        StartCoroutine(LoginWithTestCredentialsCoroutine());
    }

    private IEnumerator LoginWithTestCredentialsCoroutine(){
        var LoginData = new LoginData(testEmail, testPassword);
        string jsonLoginData = JsonUtility.ToJson(LoginData);


        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonLoginData);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(API_CONNECTION_URL, "POST")){
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            var operation = www.SendWebRequest();
            while (!operation.isDone)
            {
                yield return null;
            }
            
            try
            {
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Réponse du serveur : {www.downloadHandler.text}");
                    authToken = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text).token;
                    string idUser = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text).user.id;
                    PlayerPrefs.SetString("AuthToken", authToken);
                    PlayerPrefs.SetString("idUser", idUser);
                    PlayerPrefs.Save();

                    Debug.Log("Login réussi");
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
                }
            }
            catch (System.Exception e)
            {
                string errorMessage = $"Exception lors de la connexion : {e.Message}";
                Debug.LogError(errorMessage);
                OnError?.Invoke(errorMessage); // Déclencher l'événement d'erreur
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

[System.Serializable]
public class ErrorResponse
{
    public string message;
    public string error;
}


