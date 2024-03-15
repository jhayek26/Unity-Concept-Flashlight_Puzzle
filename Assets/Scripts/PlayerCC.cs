using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCC : MonoBehaviour
{
    Camera cam;
    CharacterController controller;

    bool jumping = false;
    bool sprinting = false;

    public bool MouseInvertX = false;
    public bool MouseInvertY = false;
    float mouseX, mouseY;
    public float mouseSensitivity = 10f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float sprintMult = 2f;
    [SerializeField] float CameraHeight = 0.5f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float lowJumpMult = 2f;
    [SerializeField] float minJumpTime = 0.5f;
    [SerializeField] float coyoteMax = .125f; //The maximum amount of time the player will stay in the air before dropping
    [SerializeField] float fallMult = 2.5f; //A multiple of the internal gravity.y value

    float jumpTime = 0;
    float velocity = 0;
    bool jumpRel = true; //Has the jump button been released? Set to true on instantiation to allow the first jump to successfully register

    float coyoteTime = 0; //The amount of time the player has been suspended in the air
    bool ignoreCoyote = false; //We don't want to use CoyoteTime if the player is jumping! Therefore, 

    Vector3 inputs = Vector3.zero;
    Vector2 inRaw = Vector2.zero;
    float inMag = 0;

    bool freeCursor = false;

    bool grounded;
    float gravity;
    float vforce; //Vertical force on the player, whether positive (up) or negative (down)

    [SerializeField] bool cameraBobbing = false;
    [SerializeField] AnimationCurve cameraBobCurve;
    [SerializeField] float cameraBobIntensity = 0.1f;
    float bobTime = 0;
    float bobOffset = 0;

    [SerializeField] LayerMask playerMask;

    [SerializeField] Light flashlightBeam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        //hand = GameObject.FindGameObjectWithTag("Hand");

        controller = GetComponent<CharacterController>();

        gravity = Physics.gravity.y;

        cameraBobCurve.postWrapMode = WrapMode.PingPong;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape) && !Application.isEditor) //Does not appear to be necessary, as Application.Quit() calls are ignored when in the editor anyways
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        grounded = CoyoteCheck(controller.isGrounded);

        inRaw = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); //The raw input signal of the movement keys. Used to detect what keys are being held and ignoring smoothed values

        //When the player holds down A and D or W and S at the same time, the movements should cancel each other out
        if (inRaw.x == 0)
            inputs.x = 0;
        if (inRaw.y == 0)
            inputs.z = 0;


        if (!jumpRel && Input.GetButtonUp("Jump"))
            jumpRel = true;
        if (grounded && Input.GetButton("Jump") && jumpRel)
        {
            grounded = false;
            jumpTime = 0;
            velocity = 0;
            jumping = true;
            jumpRel = false;
            ignoreCoyote = true;
        }

        if (grounded && Input.GetKeyDown(KeyCode.LeftShift))
        {
            sprinting = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            sprinting = false;
        }

        vforce = -controller.stepOffset; //Add a tiny bit of downward force by default to keep the .isGrounded Character Controller checks more stable

        if (!grounded)
        {
            if (jumping)
            {
                //If the jump button has been released while jumping AND the minimum jump time has been exceeded, OR if the player hit the ceiling while jumping, initiate falling
                if ((controller.collisionFlags & CollisionFlags.Above) != 0)
                {
                    vforce = -1;
                }
                else if (jumpRel && jumpTime > minJumpTime)
                {
                    velocity += gravity * fallMult * Time.deltaTime;
                    vforce = jumpForce / 2f + velocity;
                }
                else
                {
                    velocity += gravity * lowJumpMult * Time.deltaTime;
                    vforce = jumpForce + velocity;
                    jumpTime += Time.deltaTime;
                }
                //Switch to falling if the vertical force after the last calculation is less than zero
                if (vforce < 0)
                {
                    jumping = false;
                    velocity = gravity * fallMult * Time.deltaTime;
                    vforce = velocity;
                }
            }
            else
            {
                velocity += gravity * fallMult * Time.deltaTime;
                vforce = velocity;
            }
        }
        else if (jumping) //Used for edges case where the player grounds to the surface while they were jumping (like if they were in midair and a platform comes up from underneath them)
        {
            jumping = false;
        }

        //With all the necessary values set, finally have the script move the camera and player accordingly!
        MoveCamera();
        MovePlayer();
    }

    bool CoyoteCheck(bool g)
    {
        if (g)
        {
            coyoteTime = 0;
            ignoreCoyote = false;
        }
        else
        {
            //Return false if the player is not grounded and was told to ignore CoyoteTime
            if (ignoreCoyote)
                return false;

            coyoteTime += Time.deltaTime;
        }

        return coyoteTime <= coyoteMax;
    }

    void MoveCamera()
    {

        if (!freeCursor)
        {
            if (MouseInvertX)
                mouseX -= Input.GetAxis("Mouse X") * mouseSensitivity;
            else
                mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;

            if (MouseInvertY)
                mouseY += Input.GetAxis("Mouse Y") * mouseSensitivity;
            else
                mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            mouseY = Mathf.Clamp(mouseY, -70f, 70f);
        }

        cam.transform.localRotation = Quaternion.Euler(mouseY, mouseX, 0);
    }

    void MovePlayer()
    {
        if (inRaw != Vector2.zero)
        {
            Vector3 newInputs = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            //If the new input direction is not the same as the old one
            if (inputs != newInputs)
            {
                //Assign the current input direction as the new one
                inputs = newInputs;

                inMag = inputs.magnitude;
                if (inMag > 1) //If the length of the Vector3 is greater than 1 (Two directions are being held at the same time)
                    inputs /= inMag; //Divide the Vector3 by its length to make it equal to 1
            }

            inputs = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * inputs * (sprinting ? moveSpeed * sprintMult : moveSpeed);
            
            //Camera bobbing
            if (cameraBobbing && grounded)
            {
                bobOffset = cameraBobCurve.Evaluate(bobTime);
                bobTime += moveSpeed * (sprinting ? sprintMult : 1f) * Time.deltaTime;
            }
        }
        else
        {
            inputs = Vector3.zero;

            if (cameraBobbing && bobOffset != 0)
            {
                bobTime = 0;
                float t = Mathf.Pow(0.5f, 5f * Time.deltaTime);
                bobOffset = Mathf.Lerp(0, bobOffset, t);

                if (bobOffset <= 0.01f)
                {
                    bobOffset = 0;
                }
            }
        }

        controller.Move((inputs + (Vector3.up * vforce)) * Time.deltaTime);
        cam.transform.position = transform.position + new Vector3(0, CameraHeight - (bobOffset * cameraBobIntensity), 0);
    }

    public void BeforeDisable()
    {
        sprinting = false;
        jumping = false;
        flashlightBeam.enabled = false;
    }

    public void AfterEnable()
    {
        flashlightBeam.enabled = true;
    }
}
