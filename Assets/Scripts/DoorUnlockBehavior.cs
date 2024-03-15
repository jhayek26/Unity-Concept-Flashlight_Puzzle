using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorUnlockBehavior : MonoBehaviour
{
    [SerializeField] ComboEnterTriggerBox triggerBox;
    [SerializeField] GameObject doorObj;
    [SerializeField] AnimationCurve doorMoveCurve;
    [SerializeField] Vector3 doorMoveDest;
    [SerializeField] float unlockDuration = 1;
    float unlockTimer;
    Vector3 doorMoveOrig;
    bool unlocked = false;

    private void Update()
    {
        if (unlocked)
        {
            if (unlockTimer < 1)
            {
                unlockTimer += Time.deltaTime / unlockDuration;
                doorObj.transform.position = doorMoveOrig + doorMoveDest * doorMoveCurve.Evaluate(unlockTimer);
            }
            else
            {
                doorObj.transform.position = doorMoveOrig + doorMoveDest;
                gameObject.SetActive(false);
            }
        }
    }


    public void UnlockObject()
    {
        triggerBox.UnlockDoorSequence(unlockDuration);
        unlocked = true;
        unlockTimer = 0;
        doorMoveOrig = doorObj.transform.position;
    }
}
