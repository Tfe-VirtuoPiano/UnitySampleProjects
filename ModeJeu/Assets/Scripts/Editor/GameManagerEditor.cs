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
        
        // Réinitialiser la couleur
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        // Informations supplémentaires
        EditorGUILayout.LabelField("ℹ️ Informations", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• Cliquez sur 'Démarrer' pour commencer le jeu\n" +
            "• Utilisez 'Pause' pour mettre en pause\n" +
            "• 'Recommencer' arrête tout et retourne au menu\n" +
            "• Les messages de debug s'affichent dans la console",
            MessageType.Info
        );
        
        // Afficher des statistiques si le jeu est en cours
        if (currentState == GameManager.GameState.Playing || currentState == GameManager.GameState.Paused)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("📊 Statistiques", EditorStyles.boldLabel);
            
            // Compter les notes actives
            NoteMover[] activeNotes = FindObjectsOfType<NoteMover>();
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
