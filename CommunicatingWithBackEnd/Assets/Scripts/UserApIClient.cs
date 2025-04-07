using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using static UserApIClient;

public class UserApIClient : MonoBehaviour
{

    [SerializeField]
    private APIConfig APIConfig;

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

    IEnumerator GetUsers()
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{APIConfig.APIUrl}/api/users"))
        {
            request.SetRequestHeader("x-api-key", APIConfig.APIKey);
            yield return request.SendWebRequest();
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
                Debug.LogError($"Erreur: {request.error}");
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(GetUsers());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
