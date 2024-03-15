using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HoverLabel : MonoBehaviour
{
    TMP_Text label;
    static bool updateText = false;
    static string labelText = "";

    void Start()
    {
        label = GetComponent<TMP_Text>();
        label.text = "";
    }

    void Update()
    {
        if (updateText)
        {
            label.text = labelText;
            updateText = false;
        }
    }

    public static void ChangeText(string newText = "")
    {
        labelText = newText;
        updateText = true;
    }
}
