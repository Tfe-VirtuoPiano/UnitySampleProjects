using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SongManager))]
public class SongManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SongManager songManager = (SongManager)target;
        
        // Vérification de sécurité
        if (songManager == null)
        {
            EditorGUILayout.HelpBox("SongManager est null!", MessageType.Error);
            return;
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎵 Gestionnaire de Chansons", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // État du chargement
        try
        {
            EditorGUILayout.LabelField("État:", songManager.IsLoading() ? "Chargement..." : "Prêt");
            EditorGUILayout.LabelField("Chansons disponibles:", songManager.GetSongCount().ToString());
        }
        catch (System.Exception e)
        {
            EditorGUILayout.LabelField("État:", "Erreur");
            EditorGUILayout.LabelField("Chansons disponibles:", "0");
            EditorGUILayout.HelpBox($"Erreur: {e.Message}", MessageType.Error);
        }
        
        // Chanson actuelle
        try
        {
            SongData currentSong = songManager.GetCurrentSong();
            if (currentSong != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Chanson actuelle:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Titre:", currentSong.title ?? "Sans titre");
                EditorGUILayout.LabelField("Compositeur:", currentSong.composer ?? "Inconnu");
                EditorGUILayout.LabelField("Genre:", currentSong.genre ?? "Inconnu");
                EditorGUILayout.LabelField("Tempo:", currentSong.tempo.ToString() + " BPM");
                EditorGUILayout.LabelField("Notes:", currentSong.notes?.Count.ToString() ?? "0");
            }
        }
        catch (System.Exception e)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Chanson actuelle:", "Erreur");
            EditorGUILayout.HelpBox($"Erreur lors de la récupération de la chanson: {e.Message}", MessageType.Error);
        }
        
        EditorGUILayout.Space(10);
        
        // Boutons de contrôle
        EditorGUILayout.BeginHorizontal();
        
        // Bouton Charger
        GUI.backgroundColor = Color.blue;
        if (GUILayout.Button("📥 Charger", GUILayout.Height(30)))
        {
            try
            {
                songManager.LoadUserSongs();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur lors du chargement: {e.Message}");
            }
        }
        
        // Bouton Première chanson
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🎵 Première", GUILayout.Height(30)))
        {
            try
            {
                songManager.SelectFirstSong();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur lors de la sélection: {e.Message}");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Liste des chansons disponibles
        try
        {
            List<SongData> songs = songManager.GetAvailableSongs();
            if (songs != null && songs.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("📋 Chansons Disponibles", EditorStyles.boldLabel);
                
                SongData currentSong = songManager.GetCurrentSong();
                
                for (int i = 0; i < songs.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // Bouton de sélection
                    GUI.backgroundColor = songs[i] == currentSong ? Color.yellow : Color.white;
                    if (GUILayout.Button($"🎵 {i + 1}", GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        try
                        {
                            songManager.SelectSongByIndex(i);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Erreur lors de la sélection de la chanson {i}: {e.Message}");
                        }
                    }
                    
                    // Informations de la chanson
                    string title = songs[i].title ?? "Sans titre";
                    string composer = songs[i].composer ?? "Inconnu";
                    string tempo = songs[i].tempo.ToString();
                    EditorGUILayout.LabelField($"{title} - {composer} ({tempo} BPM)");
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        catch (System.Exception e)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox($"Erreur lors de l'affichage des chansons: {e.Message}", MessageType.Error);
        }
        
        // Réinitialiser la couleur
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        // Informations
        EditorGUILayout.LabelField("ℹ️ Informations", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• 'Charger' récupère les chansons depuis l'API\n" +
            "• 'Première' sélectionne la première chanson disponible\n" +
            "• Cliquez sur un numéro pour sélectionner une chanson spécifique\n" +
            "• La chanson sélectionnée sera automatiquement assignée au NoteSpawner",
            MessageType.Info
        );
    }
}
