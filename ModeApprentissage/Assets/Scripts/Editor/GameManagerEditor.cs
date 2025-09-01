using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GameManager gameManager = (GameManager)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎮 Contrôles du Jeu", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Afficher l'état actuel
        GameManager.GameState currentState = gameManager.GetCurrentState();
        EditorGUILayout.LabelField("État actuel:", currentState.ToString());
        
        // Afficher les informations de chanson
        if (gameManager.songManager != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("🎵 Gestion des Chansons", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Chansons disponibles:", gameManager.songManager.GetSongCount().ToString());
            
            SongData currentSong = gameManager.songManager.GetCurrentSong();
            if (currentSong != null)
            {
                EditorGUILayout.LabelField("Chanson actuelle:", currentSong.title);
                EditorGUILayout.LabelField("Compositeur:", currentSong.composer);
                EditorGUILayout.LabelField("Tempo:", currentSong.tempo.ToString() + " BPM");
            }
            else
            {
                EditorGUILayout.LabelField("Chanson actuelle:", "Aucune");
            }
        }
        
        // Afficher les paramètres du NoteSpawner
        if (gameManager.noteSpawner != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("🎹 Mode d'Apprentissage", EditorStyles.boldLabel);
            
            NoteSpawner spawner = gameManager.noteSpawner;
            EditorGUILayout.LabelField("Mode Stop-and-Wait:", spawner.useStopAndWaitMode ? "✅ Activé" : "❌ Désactivé");
            
            if (spawner.useStopAndWaitMode)
            {
                EditorGUILayout.LabelField("Timeout d'attente:", spawner.waitTimeout.ToString() + " secondes");
                EditorGUILayout.HelpBox(
                    "En mode Stop-and-Wait, le morceau se met en pause quand une note arrive dans la zone de jeu et attend d'être jouée.",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("🔁 Boucle par mesures", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            bool loopEnabled = EditorGUILayout.Toggle("Activer la boucle", spawner.useLoopByMeasures);
            int startMeasure = Mathf.Max(1, EditorGUILayout.IntField("Mesure de début", spawner.loopStartMeasure));
            int endMeasure = Mathf.Max(startMeasure, EditorGUILayout.IntField("Mesure de fin (incluse)", spawner.loopEndMeasure));
            if (EditorGUI.EndChangeCheck())
            {
                spawner.useLoopByMeasures = loopEnabled;
                spawner.loopStartMeasure = startMeasure;
                spawner.loopEndMeasure = endMeasure;
                EditorUtility.SetDirty(spawner);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("✋ Sélection de la main", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            var newPracticeHand = (NoteSpawner.PracticeHand)EditorGUILayout.EnumPopup("Main à pratiquer", spawner.practiceHand);
            Material disabledMat = (Material)EditorGUILayout.ObjectField("Matériau désactivé", spawner.disabledNoteMaterial, typeof(Material), false);
            if (EditorGUI.EndChangeCheck())
            {
                spawner.practiceHand = newPracticeHand;
                spawner.disabledNoteMaterial = disabledMat;
                EditorUtility.SetDirty(spawner);
            }
        }
        

        
        EditorGUILayout.Space(10);
        
        // Boutons de contrôle
        EditorGUILayout.BeginHorizontal();
        
        // Bouton Démarrer
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("▶️ Démarrer", GUILayout.Height(30)))
        {
            gameManager.StartGame();
        }
        
        // Bouton Pause/Reprendre
        if (currentState == GameManager.GameState.Playing)
        {
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("⏸️ Pause", GUILayout.Height(30)))
            {
                gameManager.PauseGame();
            }
        }
        else if (currentState == GameManager.GameState.Paused)
        {
            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("▶️ Reprendre", GUILayout.Height(30)))
            {
                gameManager.ResumeGame();
            }
        }
        else
        {
            GUI.backgroundColor = Color.gray;
            GUI.enabled = false;
            GUILayout.Button("⏸️ Pause", GUILayout.Height(30));
            GUI.enabled = true;
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Bouton Recommencer
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("🔄 Recommencer", GUILayout.Height(30)))
        {
            gameManager.RestartGame();
        }
        
        // Boutons pour les chansons
        if (gameManager.songManager != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            
            // Bouton Charger les chansons
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("📥 Charger Chansons", GUILayout.Height(25)))
            {
                gameManager.LoadSongs();
            }
            
            // Bouton Sélectionner première chanson
            GUI.backgroundColor = Color.magenta;
            if (GUILayout.Button("🎵 Première Chanson", GUILayout.Height(25)))
            {
                gameManager.SelectFirstSong();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        // Boutons pour contrôler le mode d'apprentissage
        if (gameManager.noteSpawner != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            
            NoteSpawner spawner = gameManager.noteSpawner;
            
            // Bouton pour activer/désactiver le mode stop-and-wait
            if (spawner.useStopAndWaitMode)
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("⏸️ Désactiver Stop-and-Wait", GUILayout.Height(25)))
                {
                    spawner.useStopAndWaitMode = false;
                    EditorUtility.SetDirty(spawner);
                }
            }
            else
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("▶️ Activer Stop-and-Wait", GUILayout.Height(25)))
                {
                    spawner.useStopAndWaitMode = true;
                    EditorUtility.SetDirty(spawner);
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Bouton pour forcer le passage à la note suivante (si en mode stop-and-wait)
            if (spawner.useStopAndWaitMode)
            {
                GUI.backgroundColor = new Color(1f, 0.5f, 0f); // Orange personnalisé
                if (GUILayout.Button("⏭️ Forcer Note Suivante", GUILayout.Height(25)))
                {
                    spawner.ForceNextNote();
                }
            }
        }
        
        // Réinitialiser la couleur
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        // Informations supplémentaires
        EditorGUILayout.LabelField("ℹ️ Informations", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• Cliquez sur 'Démarrer' pour commencer le jeu\n" +
            "• Utilisez 'Pause' pour mettre en pause\n" +
            "• 'Recommencer' arrête tout et retourne au menu\n" +
            "• Mode Stop-and-Wait: Le morceau se met en pause quand une note arrive dans la zone\n" +
            "• Les messages de debug s'affichent dans la console",
            MessageType.Info
        );
        
        // Afficher des statistiques si le jeu est en cours
        if (currentState == GameManager.GameState.Playing || currentState == GameManager.GameState.Paused)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("📊 Statistiques", EditorStyles.boldLabel);
            
            // Compter les notes actives
            NoteMover[] activeNotes = FindObjectsByType<NoteMover>(FindObjectsSortMode.None);
            EditorGUILayout.LabelField("Notes actives:", activeNotes.Length.ToString());
            
            // Compter les notes jouées
            int playedNotes = 0;
            foreach (NoteMover note in activeNotes)
            {
                if (note.hasBeenHit)
                    playedNotes++;
            }
            EditorGUILayout.LabelField("Notes jouées:", playedNotes.ToString());
        }
    }
}
