using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScoreUploader))]
public class ScoreUploaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ScoreUploader scoreUploader = (ScoreUploader)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("📤 Upload de Score", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Afficher l'état
        EditorGUILayout.LabelField("État:", scoreUploader.IsUploading() ? "Upload en cours..." : "Prêt");
        
        EditorGUILayout.Space(10);
        
        // Boutons de contrôle
        EditorGUILayout.BeginHorizontal();
        
        // Bouton Test Upload
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🧪 Test Upload", GUILayout.Height(30)))
        {
            scoreUploader.TestUploadScore();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Réinitialiser la couleur
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        // Informations
        EditorGUILayout.LabelField("ℹ️ Upload de Score", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• L'upload se fait automatiquement à la fin de la chanson\n" +
            "• Utilisez 'Test Upload' pour tester avec des données fictives\n" +
            "• Vérifiez que l'APIConfig est correctement configuré\n" +
            "• L'utilisateur doit être connecté (authToken requis)",
            MessageType.Info
        );
        
        // Afficher les informations de configuration
        if (scoreUploader.apiConfig != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("🔧 Configuration API", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("URL:", scoreUploader.apiConfig.APIUrl);
            EditorGUILayout.LabelField("API Key:", scoreUploader.apiConfig.APIKey.Length > 0 ? "***" + scoreUploader.apiConfig.APIKey.Substring(scoreUploader.apiConfig.APIKey.Length - 4) : "Non configurée");
        }
        else
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("⚠️ Configuration API", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("APIConfig non assigné ! L'upload ne fonctionnera pas.", MessageType.Warning);
        }
    }
}
