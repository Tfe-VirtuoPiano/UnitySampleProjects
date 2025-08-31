using UnityEngine;
using MidiJack;

public class PianoKey : MonoBehaviour
{

    public int midiNoteNumber;             // Numéro de note MIDI (ex: 60 pour C4)
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

        // Vérification de l'entrée MIDI
        if (MidiMaster.GetKeyDown(midiNoteNumber) )
        {
            PressKey();
        }
        if (MidiMaster.GetKeyUp(midiNoteNumber))
        {
            ReleaseKey();
        }
    }

    private void PressKey()
    {
        meshRenderer.material = highlightMaterial;
    }

    private void ReleaseKey()
    {
        meshRenderer.material = originalMaterial;
    }


}