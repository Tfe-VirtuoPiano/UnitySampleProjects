using UnityEngine;

public class PianoKey : MonoBehaviour
{
    public KeyCode inputKey;               // Touche clavier qui déclenche cette note
    private Material originalMaterial;
    public Material highlightMaterial;     // Matériau de surbrillance (ex: jaune)

    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;
    }

    void Update()
    {
        if (Input.GetKeyDown(inputKey))
        {
            meshRenderer.material = highlightMaterial;
        }
        if (Input.GetKeyUp(inputKey))
        {
            meshRenderer.material = originalMaterial;
        }
    }
}