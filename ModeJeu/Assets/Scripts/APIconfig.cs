using UnityEngine;

[CreateAssetMenu(fileName = "APIConfig", menuName = "Config/APIConfig")]
public class APIConfig : ScriptableObject
{
    [SerializeField]
    private string apiKey;

    [SerializeField]
    private string apiUrl;

    [SerializeField]
    private string apiConnectionUrl;

    public string APIKey => apiKey;
    public string APIUrl => apiUrl;
    public string APIConnectionUrl => apiConnectionUrl;
}
