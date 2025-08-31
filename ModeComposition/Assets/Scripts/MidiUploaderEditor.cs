using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

[CustomEditor(typeof(MidiUploader))]
public class MidiUploaderEditor : Editor
{
    private string[] keyNameOptions;
    private string[] songTypeOptions;
    private int selectedKeyIndex = 0;
    private int selectedSongTypeIndex = 0;
    
    void OnEnable()
    {
        // Préparer les options pour les dropdowns
        keyNameOptions = GetEnumDescriptions(typeof(KeyName));
        songTypeOptions = GetEnumDescriptions(typeof(SongType));
        
        // Trouver les index actuels
        MidiUploader uploader = (MidiUploader)target;
        selectedKeyIndex = Array.IndexOf(keyNameOptions, GetEnumDescription(uploader.songMetadata.keyName));
        selectedSongTypeIndex = Array.IndexOf(songTypeOptions, GetEnumDescription(uploader.songMetadata.songType));
    }
    
    private string[] GetEnumDescriptions(Type enumType)
    {
        var names = Enum.GetNames(enumType);
        var descriptions = new string[names.Length];
        
        for (int i = 0; i < names.Length; i++)
        {
            var field = enumType.GetField(names[i]);
            var attribute = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            descriptions[i] = attribute?.Description ?? names[i];
        }
        
        return descriptions;
    }
    
    private string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
    
    public override void OnInspectorGUI()
    {
        MidiUploader uploader = (MidiUploader)target;
        
        // Configuration API
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Configuration API", EditorStyles.boldLabel);
        
        // Accéder directement au champ public
        uploader.apiConfig = (APIConfig)EditorGUILayout.ObjectField("Configuration API", uploader.apiConfig, typeof(APIConfig), false);
        
        // Afficher les informations de configuration API
        if (uploader.apiConfig != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Informations API", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("URL API:", uploader.apiConfig.APIUrl);
            EditorGUILayout.LabelField("URL Connexion:", uploader.apiConfig.APIConnectionUrl);
            EditorGUILayout.LabelField("API Key:", uploader.apiConfig.APIKey.Substring(0, Math.Min(10, uploader.apiConfig.APIKey.Length)) + "...");
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("⚠️ Veuillez assigner un APIConfig pour pouvoir utiliser l'upload MIDI.", MessageType.Warning);
        }
        
        // Afficher les informations d'authentification
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Authentification", EditorStyles.boldLabel);
        
        string authToken = PlayerPrefs.GetString("AuthToken", "");
        string userId = PlayerPrefs.GetString("idUser", "");
        
        EditorGUILayout.LabelField("Token:", string.IsNullOrEmpty(authToken) ? "❌ Non connecté" : "✅ Connecté");
        EditorGUILayout.LabelField("User ID:", string.IsNullOrEmpty(userId) ? "Non défini" : userId);
        
        if (string.IsNullOrEmpty(authToken))
        {
            EditorGUILayout.HelpBox("Vous devez vous connecter via AuthScript avant de pouvoir uploader des fichiers MIDI.", MessageType.Warning);
        }
        
        // Métadonnées
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Métadonnées de la chanson", EditorStyles.boldLabel);
        
        uploader.songMetadata.title = EditorGUILayout.TextField("Titre", uploader.songMetadata.title);
        uploader.songMetadata.composer = EditorGUILayout.TextField("Compositeur", uploader.songMetadata.composer);
        uploader.songMetadata.difficulty = EditorGUILayout.IntSlider("Difficulté", uploader.songMetadata.difficulty, 1, 10);
        uploader.songMetadata.genre = EditorGUILayout.TextField("Genre", uploader.songMetadata.genre);
        
        // Dropdown pour SongType
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type de chanson", GUILayout.Width(120));
        int newSongTypeIndex = EditorGUILayout.Popup(selectedSongTypeIndex, songTypeOptions);
        if (newSongTypeIndex != selectedSongTypeIndex)
        {
            selectedSongTypeIndex = newSongTypeIndex;
            uploader.songMetadata.songType = (SongType)selectedSongTypeIndex;
        }
        EditorGUILayout.EndHorizontal();
        
        // Dropdown pour KeyName
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Clé", GUILayout.Width(120));
        int newKeyIndex = EditorGUILayout.Popup(selectedKeyIndex, keyNameOptions);
        if (newKeyIndex != selectedKeyIndex)
        {
            selectedKeyIndex = newKeyIndex;
            uploader.songMetadata.keyName = (KeyName)selectedKeyIndex;
        }
        EditorGUILayout.EndHorizontal();
        
        // L'ID utilisateur est maintenant automatiquement récupéré depuis PlayerPrefs
        EditorGUILayout.LabelField("ID Utilisateur (auto)", PlayerPrefs.GetString("idUser", "Non défini"));
        
        // Boutons
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        
        if (string.IsNullOrEmpty(authToken) || uploader.apiConfig == null)
        {
            GUI.enabled = false;
            string reason = string.IsNullOrEmpty(authToken) ? "Connexion requise" : "APIConfig requis";
            GUILayout.Button($"Upload MIDI ({reason})");
            GUI.enabled = true;
        }
        else
        {
            if (GUILayout.Button("Upload MIDI"))
            {
                uploader.UploadMidiFile();
            }
        }
        
        // Informations sur le fichier
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Informations Fichier", EditorStyles.boldLabel);
        
        var barScript = FindFirstObjectByType<BarScript>();
        if (barScript != null)
        {
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, barScript.FileName + ".mid");
            bool fileExists = System.IO.File.Exists(filePath);
            
            EditorGUILayout.LabelField("Fichier MIDI:", fileExists ? "✅ Trouvé" : "❌ Non trouvé");
            EditorGUILayout.LabelField("Chemin:", filePath);
            
            if (fileExists)
            {
                var fileInfo = new System.IO.FileInfo(filePath);
                EditorGUILayout.LabelField("Taille:", $"{fileInfo.Length} bytes");
                EditorGUILayout.LabelField("Modifié:", fileInfo.LastWriteTime.ToString());
            }
        }
        else
        {
            EditorGUILayout.HelpBox("BarScript non trouvé !", MessageType.Error);
        }
    }
}
