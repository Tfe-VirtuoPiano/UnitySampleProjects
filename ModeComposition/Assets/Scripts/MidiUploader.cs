using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

[System.Serializable]
public class SongMetadata
{
    public string title = "Enregistrement Unity";
    public string composer = "";
    [Range(1, 10)]
    public int difficulty = 5;
    public string genre = "";
    public SongType songType = SongType.song;
    public KeyName keyName = KeyName.C;
    public string userId = "";
}

public enum SongType
{
    [System.ComponentModel.Description("Chanson")]
    song,
    [System.ComponentModel.Description("Exercice de gamme")]
    scaleEx,
    [System.ComponentModel.Description("Exercice d'accord")]
    chordEx,
    [System.ComponentModel.Description("Exercice de rythme")]
    rythmEx,
    [System.ComponentModel.Description("Exercice d'arpège")]
    arpeggioEx
}

public enum KeyName
{
    [System.ComponentModel.Description("C")]
    C,
    [System.ComponentModel.Description("C#")]
    CSharp,
    [System.ComponentModel.Description("D")]
    D,
    [System.ComponentModel.Description("D#")]
    DSharp,
    [System.ComponentModel.Description("E")]
    E,
    [System.ComponentModel.Description("F")]
    F,
    [System.ComponentModel.Description("F#")]
    FSharp,
    [System.ComponentModel.Description("G")]
    G,
    [System.ComponentModel.Description("G#")]
    GSharp,
    [System.ComponentModel.Description("A")]
    A,
    [System.ComponentModel.Description("A#")]
    ASharp,
    [System.ComponentModel.Description("B")]
    B
}

[System.Serializable]
public class ApiResponse
{
    public bool success;
    public string songId;
    public string message;
    public SongData song;
}

[System.Serializable]
public class SongData
{
    public string id;
    public string title;
    public string composer;
    public string genre;
    public int tempo;
    public long duration_ms;
    public string timeSignature;
    public int level;
    public string songType;
    public KeyData key;
    public string imageUrl;
    public string createdAt;
}

[System.Serializable]
public class KeyData
{
    public string id;
    public string name;
}

[System.Serializable]
public class ApiMetadataResponse
{
    public bool success;
    public MetadataData data;
}

[System.Serializable]
public class MetadataData
{
    public KeyInfo[] keys;
    public SongTypeInfo[] songTypes;
    public int maxDifficulty;
    public int minDifficulty;
}

[System.Serializable]
public class KeyInfo
{
    public string id;
    public string name;
}

[System.Serializable]
public class SongTypeInfo
{
    public string value;
    public string label;
}

[System.Serializable]
public class UploadData
{
    public string title;
    public string composer;
    public int difficulty;
    public string genre;
    public string songType;
    public string keyName;
    public string midiFile;
    public string userId;
}

public class MidiUploader : MonoBehaviour
{
    [Header("Configuration API")]
    [SerializeField] public APIConfig apiConfig;
    
    [Header("Métadonnées de la chanson")]
    [SerializeField] public SongMetadata songMetadata = new SongMetadata();
    
    // Méthode pour récupérer la description d'un enum
    private string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
        return attribute.Length > 0 ? ((System.ComponentModel.DescriptionAttribute)attribute[0]).Description : value.ToString();
    }
    
    // Méthode pour convertir les enums en strings pour l'API
    private string GetKeyNameString(KeyName key)
    {
        return GetEnumDescription(key);
    }
    
    [Header("Interface")]
    // Interface contrôlée par MidiUploaderEditor
    
    private BarScript barScript;
    
    void Start()
    {
        barScript = FindFirstObjectByType<BarScript>();
        if (barScript == null)
        {
            Debug.LogError("❌ BarScript non trouvé !");
        }
        
        // Vérifier la configuration API
        if (apiConfig == null)
        {
            Debug.LogError("❌ APIConfig non configuré ! Veuillez assigner un APIConfig dans l'Inspector.");
        }
    }
    
    public void UploadMidiFile()
    {
        if (barScript == null)
        {
            Debug.LogError("❌ BarScript non trouvé !");
            return;
        }
        
        StartCoroutine(UploadMidiCoroutine());
    }
    
    private IEnumerator UploadMidiCoroutine()
    {
        // Vérifier la configuration API
        if (apiConfig == null)
        {
            Debug.LogError("❌ APIConfig non configuré !");
            yield break;
        }
        
        // Récupérer le token d'authentification
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        string userId = PlayerPrefs.GetString("idUser", "");
        
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("❌ Token d'authentification non trouvé ! Veuillez vous connecter d'abord.");
            yield break;
        }
        
        // Vérifier que le fichier MIDI existe
        string midiFilePath = Path.Combine(Application.persistentDataPath, barScript.FileName + ".mid");
        
        if (!File.Exists(midiFilePath))
        {
            Debug.LogError($"❌ Fichier MIDI non trouvé : {midiFilePath}");
            yield break;
        }
        
        Debug.Log($"📁 Fichier MIDI trouvé : {midiFilePath}");
        
        // Lire le fichier MIDI
        byte[] midiBytes = File.ReadAllBytes(midiFilePath);
        string midiBase64 = Convert.ToBase64String(midiBytes);
        
        // Préparer les données pour l'API
        var uploadData = new UploadData
        {
            title = songMetadata.title,
            composer = songMetadata.composer,
            difficulty = songMetadata.difficulty,
            genre = songMetadata.genre,
            songType = songMetadata.songType.ToString(),
            keyName = GetKeyNameString(songMetadata.keyName),
            midiFile = midiBase64,
            userId = userId // Utiliser l'ID utilisateur récupéré lors de la connexion
        };
        
        string jsonData = JsonUtility.ToJson(uploadData);
        
        Debug.Log($" Envoi des données à l'API...");
        Debug.Log($" Taille du fichier MIDI : {midiBytes.Length} bytes");
        Debug.Log($" Titre : {songMetadata.title}");
        Debug.Log($" Clé : {GetKeyNameString(songMetadata.keyName)}");
        Debug.Log($" Type : {songMetadata.songType}");
        Debug.Log($" Utilisateur ID : {userId}");
        Debug.Log($" JSON envoyé : {jsonData}");
        
        // Construire l'URL complète
        string fullApiUrl = apiConfig.APIUrl + "/api/unity/recordSong";
        
        // Créer la requête POST
        using (UnityWebRequest request = new UnityWebRequest(fullApiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("X-API-Key", apiConfig.APIKey);
            
            // Envoyer la requête
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Upload réussi !");
                Debug.Log($" Réponse : {request.downloadHandler.text}");
                
                // Parser la réponse
                try
                {
                    ApiResponse response = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);
                    if (response.success)
                    {
                        Debug.Log($" Chanson enregistrée avec succès ! ID: {response.songId}");
                        Debug.Log($" Titre: {response.song.title}");
                        Debug.Log($" Clé: {response.song.key.name}");
                        Debug.Log($" Durée: {response.song.duration_ms}ms");
                    }
                    else
                    {
                        Debug.LogError($"❌ Erreur API : {response.message}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ Erreur parsing réponse : {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"❌ Erreur upload : {request.error}");
                Debug.LogError($" Réponse : {request.downloadHandler.text}");
            }
        }
    }
    

    
    // Méthodes pour l'interface
    public void SetTitle(string title)
    {
        songMetadata.title = title;
    }
    
    public void SetComposer(string composer)
    {
        songMetadata.composer = composer;
    }
    
    public void SetDifficulty(int difficulty)
    {
        songMetadata.difficulty = Mathf.Clamp(difficulty, 1, 10);
    }
    
    public void SetGenre(string genre)
    {
        songMetadata.genre = genre;
    }
    
    public void SetSongType(SongType songType)
    {
        songMetadata.songType = songType;
    }
    
    public void SetKeyName(KeyName keyName)
    {
        songMetadata.keyName = keyName;
    }
    
    public void SetUserId(string userId)
    {
        songMetadata.userId = userId;
    }
    
    // Méthode pour vérifier l'état de l'authentification
    public bool IsAuthenticated()
    {
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        return !string.IsNullOrEmpty(authToken);
    }
    
    // Méthode pour obtenir l'ID utilisateur
    public string GetUserId()
    {
        return PlayerPrefs.GetString("idUser", "");
    }
}
