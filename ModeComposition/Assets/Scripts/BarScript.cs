using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarScript : MonoBehaviour
{
    const int keysCount = 88;
    [SerializeField] GameObject barManager;
    GameObject[] barsPressed = new GameObject[keysCount];
    [SerializeField] List<GameObject> barsReleased = new List<GameObject>();
   
    bool[] isKeyPressed = new bool[keysCount];

    [SerializeField] float barSpeed = (float)0.01;
    [SerializeField] float upperPositionLimit = (float)100;
    
    [Header("Couleurs des mains")]
    [SerializeField] Color mainGaucheColor = Color.blue;
    [SerializeField] Color mainDroiteColor = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] int noteSeparation = 39; // C4 = note 39 (après offset de 21)
    [SerializeField] float darkenFactor = 0.6f; // Facteur d'assombrissement pour les touches noires
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < keysCount; i++){
            // initialize: keys are not pressed
            isKeyPressed[i] = false;
            barsPressed[i] = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
         for (int i = 0; i < keysCount; i++){
            if (isKeyPressed[i] && barsPressed[i] != null) {
                Vector3 scale = barsPressed[i].transform.localScale;
                scale.y += barSpeed * 2;
                barsPressed[i].transform.localScale = scale;
                Vector3 pos = barsPressed[i].transform.position;
                pos.y += barSpeed;
                barsPressed[i].transform.position = pos;
            }
        }

        for(int i = barsReleased.Count - 1; i >= 0; i--){
            Vector3 pos = barsReleased[i].transform.position;

            // destroy bars when it reached upperPositionLimit
            if (pos.y > upperPositionLimit){
                Destroy(barsReleased[i]);
                barsReleased.RemoveAt(i);
            } else {
                pos.y += barSpeed * 2;
                barsReleased[i].transform.position = pos;
            }
        }
    }

    public void onNoteOn(int noteNumber, float velocity)
    {
        // clearfy that the key is pressed
        isKeyPressed[noteNumber] = true;

        // craete bar object
        GameObject barPrefab;
        barPrefab = (GameObject)Resources.Load("Prefab/Bar");
        barsPressed[noteNumber] = Instantiate(barPrefab);
        barsPressed[noteNumber].transform.position = new Vector3(noteNumber, 0, 0);
        barsPressed[noteNumber].transform.SetParent(barManager.transform, true);
        
        // Appliquer la couleur selon la hauteur de la note
        Color couleurNote = GetCouleurPourNote(noteNumber);
        Renderer renderer = barsPressed[noteNumber].GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = couleurNote;
        }
        
        // Debug temporaire pour vérifier les touches noires
        if (IsToucheNoire(noteNumber))
        {
            Debug.Log($"Touche noire détectée: Note {noteNumber} (position {noteNumber % 12})");
        }
    }

    
    public void onNoteOff(int noteNumber)
    {
        barsReleased.Add(Clone(barsPressed[noteNumber]));
        DestroyImmediate(barsPressed[noteNumber]);

        isKeyPressed[noteNumber] = false;
    }

        GameObject Clone( GameObject obj )
    {
        var clone = GameObject.Instantiate( obj ) as GameObject;
        clone.transform.parent = obj.transform.parent;
        clone.transform.localPosition = obj.transform.localPosition;
        clone.transform.localScale = obj.transform.localScale;
        return clone;
    }
    
    private Color GetCouleurPourNote(int noteNumber)
    {
        Color baseColor;
        
        if (noteNumber >= noteSeparation)
        {
            baseColor = mainDroiteColor; // Main droite (notes hautes)
        }
        else
        {
            baseColor = mainGaucheColor; // Main gauche (notes basses)
        }
        
        // Si c'est une touche noire, assombrir la couleur
        if (IsToucheNoire(noteNumber))
        {
            return DarkenColor(baseColor);
        }
        
        return baseColor;
    }
    
    private bool IsToucheNoire(int noteNumber)
    {
        // A0 (note 21) = position 0, donc le pattern commence sur A
        // Les touches noires sont : A#, C#, D#, F#, G#
        // Pattern : 1, 4, 6, 9, 11 (relatif à A)
        int positionDansOctave = noteNumber % 12;
        return positionDansOctave == 1 || positionDansOctave == 4 || 
               positionDansOctave == 6 || positionDansOctave == 9 || 
               positionDansOctave == 11;
    }
    
    private Color DarkenColor(Color color)
    {
        return new Color(
            color.r * darkenFactor,
            color.g * darkenFactor,
            color.b * darkenFactor,
            color.a
        );
    }


}
