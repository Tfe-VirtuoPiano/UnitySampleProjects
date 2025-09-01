using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class SongManager : MonoBehaviour
{
    [Header("Références")]
    public AuthScript authScript;
    public APIConfig apiConfig;
    
    [Header("Paramètres")]
    public int songsPerPage = 20;
    public bool loadSongsOnStart = true;
    
    [Header("Filtres")]
    public string songTypeFilter = "";
    public string genreFilter = "";
    
    [Header("État")]
    [SerializeField] private bool isLoading = false;
    [SerializeField] private List<SongData> availableSongs = new List<SongData>();
    [SerializeField] private SongData currentSong = null;
    
    // Événements
    public delegate void SongsLoadedHandler(List<SongData> songs);
    public event SongsLoadedHandler OnSongsLoaded;
    
    public delegate void SongSelectedHandler(SongData song);
    public event SongSelectedHandler OnSongSelected;
    
    public delegate void ErrorHandler(string message);
    public event ErrorHandler OnError;
    
    void Start()
    {
        if (loadSongsOnStart)
        {
            LoadUserSongs();
        }
    }
    
    [ContextMenu("Charger les chansons")]
    public void LoadUserSongs()
    {
        if (isLoading)
        {
            Debug.LogWarning("Chargement déjà en cours...");
            return;
        }
        
        StartCoroutine(LoadUserSongsCoroutine());
    }
    
    IEnumerator LoadUserSongsCoroutine()
    {
        isLoading = true;
        
        // Récupérer l'ID utilisateur depuis PlayerPrefs
        string userId = PlayerPrefs.GetString("idUser", "");
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(authToken))
        {
            Debug.LogError("Utilisateur non connecté ou token manquant");
            OnError?.Invoke("Utilisateur non connecté");
            isLoading = false;
            yield break;
        }

        // Construire l'URL avec les paramètres
        string baseUrl = apiConfig.APIUrl + "/api/unity/userSongs";
        string url = $"{baseUrl}?userId={userId}&limit={songsPerPage}&offset=0";
        
        if (!string.IsNullOrEmpty(songTypeFilter))
            url += $"&songType={songTypeFilter}";
        if (!string.IsNullOrEmpty(genreFilter))
            url += $"&genre={genreFilter}";
        
        Debug.Log($"🎵 Chargement des chansons depuis: {url}");
        
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Authorization", $"Bearer {authToken}");
            www.SetRequestHeader("X-API-Key", apiConfig.APIKey);
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
                    string responseText = www.downloadHandler.text;
                    Debug.Log($"Réponse API: {responseText}");
                    
                    var apiResponse = JsonUtility.FromJson<SongApiResponse>(responseText);
                    
                    if (apiResponse.success)
                    {
                        availableSongs.Clear();
                        
                        foreach (var songApiData in apiResponse.data)
                        {
                            SongData songData = ConvertApiSongToSongData(songApiData);
                            availableSongs.Add(songData);
                        }
                        
                        Debug.Log($"✅ {availableSongs.Count} chansons chargées avec succès");
                        OnSongsLoaded?.Invoke(availableSongs);
                    }
                    else
                    {
                        Debug.LogError($"Erreur API: {apiResponse.error}");
                        OnError?.Invoke(apiResponse.error);
                    }
                }
                else
                {
                    Debug.LogError($"Erreur HTTP: {www.error}");
                    OnError?.Invoke($"Erreur de connexion: {www.error}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception lors du chargement: {e.Message}");
                OnError?.Invoke($"Erreur: {e.Message}");
            }
        }
        
        isLoading = false;
    }
    
    SongData ConvertApiSongToSongData(SongApiData apiSong)
    {
        SongData songData = new SongData
        {
            id = apiSong.id,        // Assigner l'ID de l'API
            title = apiSong.title,
            composer = apiSong.composer,
            genre = apiSong.genre,
            tempo = apiSong.tempo,
            duration_ms = apiSong.duration_ms,
            songType = apiSong.songType,
            sourceType = apiSong.sourceType,
            timeSignature = apiSong.timeSignature,
            level = apiSong.level,
            notes = new List<NoteData>()
        };
        
        // Convertir les notes de l'API vers NoteData
        if (apiSong.notes != null)
        {
            foreach (var noteApi in apiSong.notes)
            {
                NoteData noteData = new NoteData
                {
                    note = noteApi.note,
                    durationInBeats = noteApi.durationInBeats,
                    startBeat = noteApi.startBeat,
                    finger = noteApi.finger,
                    hand = noteApi.hand
                };
                songData.notes.Add(noteData);
            }
        }
        
        return songData;
    }
    
    [ContextMenu("Sélectionner la première chanson")]
    public void SelectFirstSong()
    {
        if (availableSongs.Count > 0)
        {
            SelectSong(availableSongs[0]);
        }
        else
        {
            Debug.LogWarning("Aucune chanson disponible");
        }
    }
    
    public void SelectSong(SongData song)
    {
        currentSong = song;
        Debug.Log($"🎵 Chanson sélectionnée: {song.title}");
        OnSongSelected?.Invoke(song);
    }
    
    public void SelectSongByIndex(int index)
    {
        if (index >= 0 && index < availableSongs.Count)
        {
            SelectSong(availableSongs[index]);
        }
        else
        {
            Debug.LogError($"Index de chanson invalide: {index}");
        }
    }
    
    // Getters publics
    public List<SongData> GetAvailableSongs() => availableSongs;
    public SongData GetCurrentSong() => currentSong;
    public bool IsLoading() => isLoading;
    public int GetSongCount() => availableSongs.Count;
}

// Classes pour la sérialisation JSON de l'API
[System.Serializable]
public class SongApiResponse
{
    public bool success;
    public SongApiData[] data;
    public string error;
    public PaginationData pagination;
}

[System.Serializable]
public class SongApiData
{
    public string id;
    public string title;
    public string composer;
    public string genre;
    public int tempo;
    public int duration_ms;
    public string songType;
    public string sourceType;
    public string timeSignature;
    public int level;
    public NoteApiData[] notes;
    public KeyData key;
    public string imageUrl;
    public string createdAt;
    public string releaseDate;
}

[System.Serializable]
public class NoteApiData
{
    public string note;
    public float durationInBeats;
    public float startBeat;
    public int finger;
    public string hand;
}

[System.Serializable]
public class KeyData
{
    public string id;
    public string name;
    public string notes;
}

[System.Serializable]
public class PaginationData
{
    public int currentPage;
    public int totalPages;
    public int totalSongs;
    public bool hasNextPage;
    public bool hasPreviousPage;
    public int limit;
    public int offset;
}
