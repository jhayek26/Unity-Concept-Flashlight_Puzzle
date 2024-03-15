using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GemSelectInfo : MonoBehaviour
{
    [SerializeField] TMP_Text selectionLabel;
    [SerializeField] GameObject GameEndPanel;
    [SerializeField] string selectText = "Lorem Ipsum";
    PlayerCC charControl;

    bool selected = false;

    private void Start()
    {
        charControl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCC>();
    }

    void Update()
    {
        if (selected && Input.GetMouseButtonDown(0))
        {
            GameEndPanel.SetActive(true);
            charControl.enabled = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnMouseEnter()
    {
        selected = true;
        selectionLabel.text = selectText;
    }

    private void OnMouseExit()
    {
        selected = false;
        selectionLabel.text = "";
    }
}
