using System.Collections;
using UnityEngine;

public class PlayerLedge : MonoBehaviour
{
    [Header("Ledge Detection Settings")]
    [Space(10f)]

    [SerializeField]
    private PlayerMovement playerMovement;
    [SerializeField]
    private PlayerAnimationController playerAnimationController;

    [SerializeField]
    private PlayerCamera playerCamera;

    //Player Capsule Collider
    [SerializeField]
    private CapsuleCollider capsuleCollider;

    private Rigidbody rb;
    [Space(10f)]

    ///Ledge Detection Variables///
    [SerializeField]
    private Transform playerObjectModel;
    public Transform ledgeChecker;
    public float rayLength;
    public LayerMask ledgeMask;
    private RaycastHit climbabableRay;
    private RaycastHit cornerLedgeRay;

    //The gameObject of the collider hit by the RaycastHit variable "climbableRay".
    [SerializeField]
    private GameObject ledgeObject;
    [Space(10f)]

    [Header("Hanging Position Settings")]
    [Space(10f)]

    ///Hanging Position Variables
    public float ledgeHeight;
    private Vector3 hangingPosition;
    private Vector3 movinghangPostion;
    public float offsetZ;
    public float offsetY;
    public float offsetX;
    private Vector3 hangPosition;

    Vector3 moveDirection;
    public Transform orientation;

    [SerializeField]
    private float offset = -0.01f;
    public float offsetYMoveOnLedge;
    public float offsetXMoveOnLedge;
    [Space(10f)]

    ///All bool checks for ledge detection and climbing
    [SerializeField]
    private bool Switch;

    [Space(10f)]
    private bool canClimb;
    private bool startClimb;
    private bool canClimbOnTop = false;
    private bool startClimbOnTop = false;
    public bool isHanging;
    private bool canMove;
    private bool canMoveLeft;
    private bool canMoveRight;
    [SerializeField]
    private bool canTurnCorner;
    private bool isTurningCorner;
    private bool endHang;
    private bool dropFromLedge;

    [Space(10f)]

    ///All keybinds
    [SerializeField]
    private KeyCode climbButton;
    [SerializeField]
    private KeyCode moveRightButton;
    [SerializeField]
    private KeyCode moveLeftButton;

    private float horizontalInput;
    private float verticalInput;
    [SerializeField]
    private float ledgeMoveSpeed;

    private void Start()
    {
        Switch = true;
        startClimb = false;
        startClimbOnTop = false;
        rb = GetComponent<Rigidbody>();
        canMoveLeft = false;
        canMoveRight = false;
        canMove = true;
    }

    private void FixedUpdate()
    {
        PlayerLedgeMovement();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(hangPosition, .25f);
    }

    private void Update()
    {
        //Master bool switch for checking ledges
        if (Switch)
        {
            if (!isTurningCorner) {
                if (!isHanging)
                {
                    CheckForLedge();
                }
                else
                {
                    CheckForTopSurface();
                }
            }
            else
            {
                transform.LookAt(ledgeObject.transform);
                playerObjectModel.LookAt(ledgeObject.transform);
            }
                FaceForward();

            OnKeyPressed();

            //Shrink the collider when animation is playing
            if (startClimb)
            {
                capsuleCollider.height = 0.01f;
                capsuleCollider.radius = 0f;
            }
        }
    }

    /// <summary>
    /// Allows public access to private bool checks for other scripts
    /// </summary>

    public bool GetCanClimb()
    {
        return canClimb;
    }
    public bool GetStartClimb()
    {
        return startClimb;
    }
    public bool GetStartClimbOnTop()
    {
        return startClimbOnTop;
    }

    //This method is responsible for making sure the player faces the surface ledge 
    private void FaceForward()
    {
        if (Physics.Raycast(ledgeChecker.position, ledgeChecker.forward, out climbabableRay, rayLength + 0.5f, ledgeMask))
        {
            //Spawn debug ray in scene.
            Debug.DrawRay(ledgeChecker.position, ledgeChecker.forward * rayLength, Color.green);

            //Set player controller to face ledge
            transform.forward = -climbabableRay.normal;

            //Set player mesh/model to face ledge
            playerObjectModel.forward = -climbabableRay.normal;

        }
        else
        {
            transform.forward = this.transform.forward;
        }
    }

    //This method is responsible for making sure the player faces the surface ledge 
    private void MoveOnLedgeChecker()
    {
        if (canMove)
        {
            // calculate movement direction
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            Debug.DrawRay(ledgeChecker.position + Vector3.up * offsetYMoveOnLedge + Vector3.right * offsetXMoveOnLedge, ledgeChecker.forward * rayLength, Color.red);
            Debug.DrawRay(ledgeChecker.position + Vector3.up * offsetYMoveOnLedge + Vector3.right * -offsetXMoveOnLedge, ledgeChecker.forward * rayLength, Color.red);

            //Right Ledge Movement Checker
            if (Physics.Raycast(ledgeChecker.position + Vector3.up * offsetYMoveOnLedge + Vector3.right * offsetXMoveOnLedge, ledgeChecker.forward, out climbabableRay, rayLength + 0.5f, ledgeMask))
            {
                canMoveRight = true;
            }
            else
            {
                canMoveRight = false;
            }


            //Left Ledge Movement Checker
            if (Physics.Raycast(ledgeChecker.position + Vector3.up * offsetYMoveOnLedge + Vector3.right * -offsetXMoveOnLedge, ledgeChecker.forward, out climbabableRay, rayLength + 0.5f, ledgeMask))
            {
                canMoveLeft = true;
            }
            else
            {
                canMoveLeft = false;
            }

            ///Checking corners of ledges to see if we can turn or not
            var lineOffset = .3f;
            var lineForwardOffset = .5f;

            if (!canMoveRight)
            {
                for (int i = 0; i < 5; i++)
                {
                    Debug.DrawRay(ledgeChecker.position + ledgeChecker.forward * lineForwardOffset + ledgeChecker.right * lineOffset + Vector3.up * .025f * i, -ledgeChecker.right * .5f, Color.cyan);
                    Debug.Log($"Can Turn Right :: {canTurnCorner}");
                    canTurnCorner = Physics.Raycast(ledgeChecker.position + ledgeChecker.forward * lineForwardOffset + ledgeChecker.right * lineOffset + Vector3.up * .025f * i, -ledgeChecker.right, out cornerLedgeRay, .5f, ledgeMask);

                    if (canTurnCorner)
                    {
                        //Spawn debug ray in scene.
                        Debug.DrawRay(ledgeChecker.position + Vector3.up * offset * i, ledgeChecker.forward * rayLength, Color.red);

                        //Get the gameObject that is hit and then find the top point of the collider to create a hanging position for the player.

                        if (ledgeObject == null)
                            ledgeObject = cornerLedgeRay.collider.gameObject;
                        //Represents y distance between raycast hit and top of ledge
                        float ledgeHeight = 0f;

                        //Get the top height of the mesh
                        Vector3 topCollider = cornerLedgeRay.point;
                        topCollider.y = cornerLedgeRay.collider.bounds.max.y;

                        //Top point Y of the mesh
                        var startTopPos = topCollider + Vector3.up * ledgeHeight;
                        Debug.DrawRay(startTopPos, Vector3.up * -0.1f, Color.red);

                        //New hang position
                        Vector3 newhangLocation = new Vector3(startTopPos.x, startTopPos.y + offsetY, startTopPos.z + offsetZ);
                        hangPosition = newhangLocation;

                        Debug.Log($"Next Hang Position :: {newhangLocation}");
                    }
                }

            }
        }
    }


    /// <summary>
    /// CheckForLedge function creates a raycast that looks for gameObjects with a specific layer. 
    /// Once a object has been found then canClimb will be set to true and when that happens the next block of code will create a ray from the ledgeChecker to the point of contact on the ledge object.
    /// The function will also determine the hanging position of the character based on the startTopPos local variable
    /// </summary>
    private void CheckForLedge()
    {

        //Check for a gameObject with the layer ledge, if hit is true then set canClimb to true.

        for (int i = 0; i < 5; i++)
        {
            
            canClimb = Physics.Raycast(ledgeChecker.position + Vector3.up * offset * i, ledgeChecker.forward, out climbabableRay, rayLength, ledgeMask);
            if (canClimb)
            {
                //Spawn debug ray in scene.
                Debug.DrawRay(ledgeChecker.position + Vector3.up * offset * i, ledgeChecker.forward * rayLength, Color.red);

                //Get the gameObject that is hit and then find the top point of the collider to create a hanging position for the player.
                ledgeObject = climbabableRay.collider.gameObject;
                //Represents y distance between raycast hit and top of ledge
                float ledgeHeight = 0f;

                //Get the top height of the mesh
                Vector3 topCollider = climbabableRay.point;
                topCollider.y = climbabableRay.collider.bounds.max.y;

                //Top point Y of the mesh
                var startTopPos = topCollider + Vector3.up * ledgeHeight;
                Debug.DrawRay(startTopPos, Vector3.up * -0.1f, Color.red);

                //New hang position
                Vector3 hangLocation = new Vector3(startTopPos.x, startTopPos.y + offsetY, startTopPos.z + offsetZ);
                hangPosition = hangLocation;

            }
            else
            {
                Debug.DrawRay(ledgeChecker.position + Vector3.up * offset * i, ledgeChecker.forward * rayLength, Color.blue);
            }
        }

       
    }

    /// <summary>
    /// CheckForLedge function creates a raycast that looks for gameObjects with a specific layer. 
    /// Once a object has been found then canClimb will be set to true and when that happens the next block of code will create a ray from the ledgeChecker to the point of contact on the ledge object.
    /// The function will also determine the hanging position of the character based on the startTopPos local variable
    /// </summary>
    private void CheckForTopSurface()
    {
        Debug.DrawRay(ledgeChecker.position + Vector3.up * 1.5f, ledgeChecker.forward * (rayLength + 1f), Color.cyan);
        if (!Physics.Raycast(ledgeChecker.position + Vector3.up * 1.5f, ledgeChecker.forward, rayLength + 1f))
        {
            canClimbOnTop = true;
        }

    }

    /// <summary>
    /// PlayerLedgeMovement is responsible for moving along the surface of the ledge
    /// </summary>
    private void PlayerLedgeMovement()
    {
        if (isHanging && !startClimbOnTop)
        {
            StartCoroutine("ActivateLedgeMovement");

            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > ledgeMoveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * ledgeMoveSpeed;

                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void OnKeyPressed()
    {
        horizontalInput = UnityEngine.Input.GetAxis("Horizontal");
        verticalInput = UnityEngine.Input.GetAxis("Vertical");


        if (canClimb)
        {
            //Start Ledge hang from on ground
            if (UnityEngine.Input.GetKeyDown(climbButton) && canClimb && !isHanging)
            {
                StartCoroutine("StartLedgeGrab");
            }

        }

        //Drop from ledge
        if (UnityEngine.Input.GetKeyDown(KeyCode.Backspace) && isHanging)
        {
            StartCoroutine("DropFromLedge");
        }

        if (isHanging && !startClimbOnTop)
        {
            //Move player right while on ledge
            if (UnityEngine.Input.GetKey(moveRightButton) && canMoveRight)
            {

                rb.AddForce(moveDirection.normalized * ledgeMoveSpeed * 10f, ForceMode.Force);
            } else
            if (UnityEngine.Input.GetKey(moveLeftButton) && canMoveLeft)//Move player left while on ledge
            {

                rb.AddForce(moveDirection.normalized * ledgeMoveSpeed * 10f, ForceMode.Force);
            }
            else
            {
                if (!isTurningCorner)
                {
                    rb.linearVelocity = Vector3.zero;
                }
            }
        }

        if (canClimbOnTop && !endHang)
        {
            //Climb on top of platform
            if (UnityEngine.Input.GetKeyDown("space"))
            {
                StartCoroutine("ClimbOnTopLedge");
            }
        }


        ///Turn corner of ledge 

        if (UnityEngine.Input.GetKeyDown(moveRightButton) && canTurnCorner)
        {
            Debug.Log("Turn Right on ledge corner");
            StartCoroutine("TurnCorner");
        }
    }


    private void ResetAllBools()
    {
        canClimb = false;
        startClimb = false;
        canClimbOnTop = false;
        startClimbOnTop = false;
        playerCamera.ledgeCamera = false;
        isHanging = false;
    }

    /// <summary>
    /// Activates player movement on ledge
    /// </summary>
    /// <returns></returns>

    IEnumerator ActivateLedgeMovement()
    {
        yield return new WaitForSeconds(.5f);
        MoveOnLedgeChecker();
    }


    /// <summary>
    /// StartLedgeGrab is responsible for setting the player position to the new hanging position and state.
    /// when the coroutine starts the gravity, player movement, and camera movement are disabled to prevent movement errors.
    /// startClimb is set to true which activates the idle to hang animation. When the coroutine ends the player location will be set to the hangPosition, and the hang animation weight layer will be set to 1.
    /// </summary>
    /// <returns></returns>

    IEnumerator StartLedgeGrab()
    {
        playerCamera.ledgeCamera = true;
        playerMovement.enabled = false;
        playerCamera.enabled = false;
        rb.useGravity = false;
        startClimb = true;

        if (playerMovement.grounded)
        {
            rb.AddForce(Vector3.up * 20f);
        }
        //Set player controller to face ledge
        transform.forward = -climbabableRay.normal;

        //Set player mesh/model to face ledge
        playerObjectModel.forward = -climbabableRay.normal;
        yield return new WaitForSeconds(.75f);

        playerAnimationController.HangAnimationStart();
        rb.linearVelocity = Vector3.zero;
        transform.position = hangPosition;
        playerCamera.enabled = true;

        startClimb = false;
        isHanging = true;
        
    }

    /// <summary>
    /// Activates player movement on ledge
    /// </summary>
    /// <returns></returns>

    IEnumerator DropFromLedge()
    {
        playerAnimationController.HangAnimationEnd();
        Switch = false;
        playerMovement.enabled = false;
        dropFromLedge = true;
        yield return new WaitForSeconds(2.5f);
        Switch = true;
        playerMovement.enabled = false;
        dropFromLedge = false;
        isHanging = false;
    }



    /// <summary>
    /// TurnCorner is responsible for changing the player location to the other side of the turningon point of the ledge.
    /// The function moves the player to the far end of the corner, then it moves the player forward horizontally along the surface to the new hanging point
    /// </summary>
    /// <returns></returns>

    IEnumerator TurnCorner()
    {
        isTurningCorner = true;
  
        rb.AddForce(Vector3.right * 160f);
        rb.AddForce(transform.forward * 260f);
        yield return new WaitForSeconds(1f);
        rb.AddForce(transform.right * 260f);
        isTurningCorner = false;
    }

    /// <summary>
    /// ClimbOnTopLedge is responsible for moving the player on top of surfaces. The function sets startClimbTop true in order to trigger the climb up animation in the player animation script. 
    /// Then the hang animation layer weight is set to 0 and force is slowly applied to the player as the animation is playing to simulate the look and feel that the player is moving along the ledge to climb upward.
    /// The collider radius and size is reset and all bools are reset back to nomal once the coroutine ends
    /// </summary>
    /// <returns></returns>
    IEnumerator ClimbOnTopLedge()
    {
        endHang = true;
        startClimbOnTop = true;
        playerAnimationController.HangAnimationEnd();
        rb.AddForce(Vector3.up * 35f + transform.forward * 10f);
        rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(2.5f);
        rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(0.2f);

        rb.AddForce(transform.forward * 30f);

        yield return new WaitForSeconds(1.3f);


        rb.useGravity = true;
        capsuleCollider.height = 2f;
        capsuleCollider.radius = 0.5f;
        ResetAllBools();
        yield return new WaitForSeconds(1f);

        playerMovement.enabled = true;
        endHang = false;
        rb.linearVelocity = Vector3.zero;
    }
}

