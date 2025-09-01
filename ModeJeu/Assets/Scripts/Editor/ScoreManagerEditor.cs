using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScoreManager))]
public class ScoreManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ScoreManager scoreManager = (ScoreManager)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎯 Gestionnaire de Score", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Affichage du score actuel
        EditorGUILayout.LabelField("Score actuel:", scoreManager.GetScoreText());
        EditorGUILayout.LabelField("Multiplicateur:", scoreManager.GetMultiplierText());
        EditorGUILayout.LabelField("Combo:", scoreManager.GetConsecutiveHits().ToString());
        EditorGUILayout.LabelField("Précision:", scoreManager.GetAccuracyText());
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Statistiques:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Notes jouées:", scoreManager.GetTotalNotesHit().ToString());
        EditorGUILayout.LabelField("Notes manquées:", scoreManager.GetTotalNotesMissed().ToString());
        
        EditorGUILayout.Space(10);
        
        // Boutons de contrôle
        EditorGUILayout.BeginHorizontal();
        
        // Bouton Réinitialiser
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("🔄 Réinitialiser", GUILayout.Height(30)))
        {
            scoreManager.ResetScore();
        }
        
        // Bouton Test Note Hit
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✅ Test Note Hit", GUILayout.Height(30)))
        {
            scoreManager.NoteHit();
        }
        
        // Bouton Test Note Missed
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("❌ Test Note Missed", GUILayout.Height(30)))
        {
            scoreManager.NoteMissed();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Réinitialiser la couleur
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        // Informations
        EditorGUILayout.LabelField("ℹ️ Système de Score", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• Chaque note correcte donne des points\n" +
            "• Les bonnes notes consécutives augmentent le multiplicateur\n" +
            "• Un mauvais input ou une note manquée remet le multiplicateur à 1\n" +
            "• Le multiplicateur maximum est configurable",
            MessageType.Info
        );
    }
}
