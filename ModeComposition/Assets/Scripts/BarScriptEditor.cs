using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BarScript))]
public class BarScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Dessiner l'Inspector par défaut
        DrawDefaultInspector();
        
        BarScript barScript = (BarScript)target;
        
        // Ajouter un espace
        EditorGUILayout.Space();
        
        // Section d'enregistrement
        EditorGUILayout.LabelField("🎵 Contrôles d'enregistrement", EditorStyles.boldLabel);
        
        // Statut de l'enregistrement
        string status = barScript.GetRecordingStatus();
        EditorGUILayout.LabelField("Statut:", status);
        
        EditorGUILayout.Space();
        
        // Boutons d'enregistrement
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("▶️ Démarrer", GUILayout.Height(30)))
        {
            barScript.StartRecordingButton();
        }
        
        if (GUILayout.Button("⏹️ Arrêter", GUILayout.Height(30)))
        {
            barScript.StopRecordingButton();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("💾 Exporter MIDI", GUILayout.Height(30)))
        {
            barScript.ExportMidiButton();
        }
        
        // Instructions
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Instructions:\n" +
            "1. Cliquez sur 'Démarrer' pour commencer l'enregistrement\n" +
            "2. Jouez votre morceau au piano\n" +
            "3. Cliquez sur 'Arrêter' quand vous avez fini\n" +
            "4. Cliquez sur 'Exporter MIDI' pour sauvegarder",
            MessageType.Info
        );
    }
}
