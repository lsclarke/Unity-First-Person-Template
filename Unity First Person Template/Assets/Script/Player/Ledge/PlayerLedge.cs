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
    private bool endHang;

    private void Start()
    {
        Switch = true;
        startClimb = false;
        startClimbOnTop = false;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        PlayerLedgeMovement();
    }

    private void Update()
    {
        //Master bool switch for checking ledges
        if (Switch)
        {

            if (!isHanging)
            {
                CheckForLedge();
            }
            else
            {
                CheckForTopSurface();
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

    private void FaceForward()
    {
        if (Physics.Raycast(ledgeChecker.position, ledgeChecker.forward, out climbabableRay, rayLength + 0.5f, ledgeMask))
        {
            //Spawn debug ray in scene.
            Debug.Log($"Face :: {-climbabableRay.normal}");
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

    /// <summary>
    /// CheckForLedge function creates a raycast that looks for gameObjects with a specific layer. 
    /// Once a object has been found then canClimb will be set to true and when that happens the next block of code will create a ray from the ledgeChecker to the point of contact on the ledge object.
    /// The function will also determine the hanging position of the character based on the startTopPos local variable
    /// </summary>
    private void CheckForLedge()
    {
        //Check for a gameObject with the layer ledge, if hit is true then set canClimb to true.
        canClimb = Physics.Raycast(ledgeChecker.position, ledgeChecker.forward, out climbabableRay, rayLength, ledgeMask);
        if (canClimb)
        {
            //Spawn debug ray in scene.
            Debug.Log("Can Climb Ledge");
            Debug.DrawRay(ledgeChecker.position, ledgeChecker.forward * rayLength, Color.red);

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

    }

    /// <summary>
    /// CheckForLedge function creates a raycast that looks for gameObjects with a specific layer. 
    /// Once a object has been found then canClimb will be set to true and when that happens the next block of code will create a ray from the ledgeChecker to the point of contact on the ledge object.
    /// The function will also determine the hanging position of the character based on the startTopPos local variable
    /// </summary>
    private void CheckForTopSurface()
    {
        Debug.DrawRay(ledgeChecker.position + Vector3.up * 1.5f, ledgeChecker.forward * rayLength, Color.cyan);
        if (!Physics.Raycast(ledgeChecker.position + Vector3.up * 1.5f, ledgeChecker.forward, rayLength))
        {
            Debug.Log("Can Climb On Top");
            canClimbOnTop = true;
        }
    }

    /// <summary>
    /// PlayerLedgeMovement is responsible for moving along the surface of the ledge
    /// </summary>
    private void PlayerLedgeMovement()
    {
        if (isHanging)
        {
            var horizontalInput = UnityEngine.Input.GetAxis("Horizontal");
            var verticalInput = UnityEngine.Input.GetAxis("Vertical");
        }
    }

    private void OnKeyPressed()
    {

        if (canClimb)
        {
            //Start Ledge hang
            if (UnityEngine.Input.GetKeyDown("space") && canClimb && !isHanging)
            {
                Debug.Log("Grab ledge");
                StartCoroutine("StartLedgeGrab");

            }
        }

        if (canClimbOnTop && !endHang)
        {
            //Climb on top of platform
            if (UnityEngine.Input.GetKeyDown("space"))
            {
                Debug.Log("Climbing On Top");
                StartCoroutine("ClimbOnTopLedge");
            }
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
    /// StartLedgeGrab is responsible for setting the player position to the new hanging position and state.
    /// when the coroutine starts the gravity, player movement, and camera movement are disabled to prevent movement errors.
    /// startClimb is set to true which activates the idle to hang animation. When the coroutine ends the player location will be set to the hangPosition, and the hang animation weight layer will be set to 1.
    /// </summary>
    /// <returns></returns>

    IEnumerator StartLedgeGrab()
    {
        playerMovement.enabled = false;
        playerCamera.ledgeCamera = true;
        playerCamera.enabled = false;  
        rb.useGravity = false;
        startClimb = true;
        rb.AddForce(Vector3.up * 20f);
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
        rb.AddForce(Vector3.up * 35f);
        rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(2.5f);
        rb.linearVelocity = Vector3.zero;
        Debug.Log("Move forward ledge");

        yield return new WaitForSeconds(0.2f);

        rb.AddForce(transform.forward * 30f);

        yield return new WaitForSeconds(1.3f);


        Debug.Log("Move forward ledge");



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

