using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DoorNumberChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Suits thisSuit;
    [SerializeField] Sprite[] numberSprites;
    [SerializeField] string labelName = "Button";
    DoorUnlockBehavior doorUnlock;
    Image img;
    bool selected = false;
    static int[] suitCounts;

    void Start()
    {
        img = GetComponent<Image>();
        suitCounts = new int[numberSprites.Length];
        doorUnlock = GameObject.FindGameObjectWithTag("PuzzleDoor").GetComponent<DoorUnlockBehavior>();
    }

    void Update()
    {
        if (selected)
        {
            if (Input.GetMouseButtonDown(0))    
            {
                int count = suitCounts[(int)thisSuit];
                count = (count + 1 == numberSprites.Length) ? 0 : count + 1;
                suitCounts[(int)thisSuit] = count;
                img.sprite = numberSprites[count];
                if (SecretWallMaker.CheckCombo(suitCounts))
                {
                    doorUnlock.UnlockObject();
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                int count = suitCounts[(int)thisSuit];
                count = (count - 1 < 0) ? numberSprites.Length - 1 : count - 1;
                suitCounts[(int)thisSuit] = count;
                img.sprite = numberSprites[count];
                if (SecretWallMaker.CheckCombo(suitCounts))
                {
                    doorUnlock.UnlockObject();
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        selected = true;
        HoverLabel.ChangeText(labelName);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        selected = false;
        HoverLabel.ChangeText();
    }
}


public enum Suits
{
    RedHeart,       //0
    BlueDiamond,    //1
    GreenClub,      //2
    YellowSpade     //3
}