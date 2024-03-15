using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SecretWallMaker : MonoBehaviour
{
    [SerializeField] Material[] secretMats; //Each type of secret wall
    [SerializeField] Material defaultMat;
    [SerializeField] TMP_Text cheatSheetText;

    private static int[] secretWallCounts; //Count of each type of secret wall. Currently, there is only one door in a scene, so this array can be static for convenience.

    void Start()
    {
        secretWallCounts = new int[secretMats.Length];
        CreateWalls(true);
        cheatSheetText.text =   "R: " + secretWallCounts[0] + "\n" +
                                "B: " + secretWallCounts[1] + "\n" +
                                "G: " + secretWallCounts[2] + "\n" +
                                "Y: " + secretWallCounts[3];
        cheatSheetText.enabled = false; //Disable the cheat sheet by default
    }

    private void Update()
    {
        //Toggle the cheat sheet on and off when the F1 key is pressed
        if (Input.GetKeyDown(KeyCode.F1))
            cheatSheetText.enabled = !cheatSheetText.enabled;
    }

    //Allow other objects to get the number of secret walls
    public static int[] GetSuitCounts()
    {
        return secretWallCounts;
    }

    //Compare the secretWallCounts array with another to see if all the values match
    public static bool CheckCombo(int[] combo)
    {
        for (int i = 0; i < secretWallCounts.Length; i++)
        {
            if (secretWallCounts[i] != combo[i] + 1)
                return false;
        }
        return true;
    }

    public void CreateWalls(bool firstIteration = false)
    {
        List<GameObject> walls = new List<GameObject>(GameObject.FindGameObjectsWithTag("SecretWall"));

        if (!firstIteration)
        {
            foreach (GameObject wall in walls)
            {
                wall.GetComponent<Renderer>().material = defaultMat;
            }
        }

        for (int i = 0; i < secretMats.Length; i++)
        {
            secretWallCounts[i] = Random.Range(1, 5); //Generate a number of secret walls between 1 and 4

            for (int j = 0; j < secretWallCounts[i]; j++)
            {
                int idx = Random.Range(0, walls.Count);
                walls[idx].GetComponent<Renderer>().material = secretMats[i];
                walls.RemoveAt(idx); //Remove this wall from the list so it can't be changed again
            }
        }
    }

}
