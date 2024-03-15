using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComboEnterTriggerBox : MonoBehaviour
{
    [SerializeField] Vector3 fixedCamPos, fixedCamRot;
    [SerializeField] float transitionDuration = 1; //Duration in seconds that the transitions between locked/unlocked states last
    [SerializeField] AnimationCurve transitionCurve;
    [SerializeField] float maxTriggerAngle = 90;
    [SerializeField] Light doorSpotLight;
    [SerializeField] TMP_Text controlsLabel;
    [SerializeField] GameObject doorPromptLabel;
    PlayerCC charControl;
    Camera mainCam;
    private Vector3 origCamPos, destCamPos; 
    private Quaternion origCamRot, destCamRot; 
    bool playerLocked = false; //Are the player's camera and movement controls currently locked by this script?
    bool switchingStates = false; //Is there currently a transition between locked/unlocked states occurring?
    bool finalState = false; //
    float finalTransitionDuration = 1;
    bool canSwitch = false;
    float transitionTimer = 0;
    System.Func<float, float> lightIntensity; //Generic delegate function calculates the intensity of the spot light, with calculations changing depending on the state being transitioned to

    [SerializeField][TextArea(5,10)] string buttonControlText;
    string moveControlText; 

    private void Start()
    {
        charControl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCC>();
        mainCam = Camera.main;
        moveControlText = controlsLabel.text;
    }

    public void ManuallyUnlockPlayer()
    {
        playerLocked = false;
        destCamPos = origCamPos; //If the player were to start in locked mode, origCamPos (and origCamRot) would need to be defined in the Start function first
        origCamPos = transform.TransformPoint(fixedCamPos);
        destCamRot = origCamRot;
        origCamRot = Quaternion.Euler(fixedCamRot);
        Cursor.lockState = CursorLockMode.Locked; //Locks the cursor so it remains centered and hidden
        controlsLabel.text = moveControlText;
        lightIntensity = t => (1 - t) * 20; //The intensity of the light will gradually transition from 20 to zero
        switchingStates = true;
    }

    public void UnlockDoorSequence(float unlockTime)
    {
        playerLocked = false;
        destCamPos = origCamPos; //If the player were to start in locked mode, origCamPos (and origCamRot) would need to be defined in the Start function first
        origCamPos = transform.TransformPoint(fixedCamPos);
        destCamRot = origCamRot;
        origCamRot = Quaternion.Euler(fixedCamRot);
        Cursor.lockState = CursorLockMode.Locked; //Locks the cursor so it remains centered and hidden
        controlsLabel.text = moveControlText;
        lightIntensity = t => (1 - t) * 20; //The intensity of the light will gradually transition from 20 to zero
        //The duration of this transition needs to be shorter than the duration of the door unlocking, as this sequence would not be fully completed otherwise
        finalTransitionDuration = unlockTime * 0.9f;
        finalState = true;
    }

    private void Update()
    {
        if (finalState)
        {
            if (transitionTimer < 1)
            {
                transitionTimer += Time.deltaTime / finalTransitionDuration;
                mainCam.transform.position = Vector3.Lerp(origCamPos, destCamPos, transitionCurve.Evaluate(transitionTimer));
                mainCam.transform.rotation = Quaternion.Slerp(origCamRot, destCamRot, transitionCurve.Evaluate(transitionTimer));
                doorSpotLight.intensity = lightIntensity(transitionTimer);
            }
            else
            {
                //Since this is the last state of the object, re-enable the player's control of the camera and movement, then disable this object.
                charControl.enabled = true;
                charControl.AfterEnable();
                gameObject.SetActive(false);
            }
            return; //Early return to prevent the other conditionals from being checked.
        }

        if (canSwitch && !switchingStates && Input.GetKeyDown(KeyCode.E))
        {   
            float angle = Vector3.Angle(mainCam.transform.forward, Quaternion.Euler(fixedCamRot) * Vector3.forward);

            if (angle < maxTriggerAngle)
            {
                playerLocked = !playerLocked;
                if (playerLocked)
                {
                    origCamPos = mainCam.transform.position;
                    destCamPos = transform.TransformPoint(fixedCamPos);
                    origCamRot = mainCam.transform.rotation;
                    destCamRot = Quaternion.Euler(fixedCamRot);
                    Cursor.lockState = CursorLockMode.None; //Unlocks the cursor so the numbers on the door can be interacted with
                    controlsLabel.text = buttonControlText;
                    lightIntensity = t => t * 20; //The intensity of the light will gradually transition from zero to 20

                    charControl.BeforeDisable();
                    charControl.enabled = false; //The player's control of the camera and their movement needs to be locked immediately to allow the transition to occur
                    doorPromptLabel.SetActive(false);
                }
                else
                {
                    destCamPos = origCamPos; //If the player were to start in locked mode, origCamPos (and origCamRot) would need to be defined in the Start function first
                    origCamPos = transform.TransformPoint(fixedCamPos);
                    destCamRot = origCamRot;
                    origCamRot = Quaternion.Euler(fixedCamRot);
                    Cursor.lockState = CursorLockMode.Locked; //Locks the cursor so it remains centered and hidden
                    controlsLabel.text = moveControlText;
                    lightIntensity = t => (1 - t) * 20; //The intensity of the light will gradually transition from 20 to zero
                }
                switchingStates = true;
            }
        }

        if (switchingStates)
        {
            if (transitionTimer < 1)
            {
                transitionTimer += Time.deltaTime / transitionDuration;
                mainCam.transform.position = Vector3.Lerp(origCamPos, destCamPos, transitionCurve.Evaluate(transitionTimer));
                /*
                    TODO:
                    The interpolation between the two quaternions can be disorientating depending on where the player angled their camera.
                    This may create unintended feelings of motion sickness in more extreme cases. As such, finding a way to mitigate such feelings must be looked into. 
                    One possible solution could be to rotate each axis individually in a sequence. Another solution could be to only allow changing of states if the player is actually facing the door.
                    SOLVED: 
                    Now the changing of states can only occur if the angle between the direction the player's camera is facing and the direction of the target rotation is within an acceptable range
                */
                mainCam.transform.rotation = Quaternion.Slerp(origCamRot, destCamRot, transitionCurve.Evaluate(transitionTimer));
                doorSpotLight.intensity = lightIntensity(transitionTimer);
            }
            else
            {
                mainCam.transform.position = destCamPos;
                mainCam.transform.rotation = destCamRot;
                doorSpotLight.intensity = lightIntensity(1);
                switchingStates = false;
                transitionTimer = 0;

                if (!playerLocked)
                {
                    charControl.enabled = true; //Give back control of the camera and movement to the player if the transition from the locked to unlocked state has been completed
                    charControl.AfterEnable();
                    doorPromptLabel.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        canSwitch = true;
        doorPromptLabel.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        canSwitch = false;
        doorPromptLabel.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 camPos = transform.TransformPoint(fixedCamPos);
        Gizmos.DrawCube(camPos, Vector3.one * 0.1f);
        Gizmos.DrawRay(camPos, Quaternion.Euler(fixedCamRot) * Vector3.forward);
    }
}
