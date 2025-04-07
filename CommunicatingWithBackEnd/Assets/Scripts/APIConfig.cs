using UnityEngine;

[CreateAssetMenu(fileName = "APIConfig", menuName = "Config/APIConfig")]
public class APIConfig : ScriptableObject
{
    [SerializeField]
    private string apiKey; 

    [SerializeField]
    private string apiUrl;

    public string APIKey => apiKey;
    public string APIUrl => apiUrl;
}
