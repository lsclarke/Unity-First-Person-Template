using System.Collections;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// These variables are responsible for all the player ground movement and move state condiditons
    /// </summary>
    [Header("Movement")]
    Vector3 moveDirection;
    float horizontalInput;
    float verticalInput;
    public float stompTimer = 0;
    public bool canMove;
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public bool isWalking = false;
    public bool isRunning = false;
    public float groundDrag;
    Rigidbody rb;

    [Space(5f)]
    public MovementState state;
    [Space(5f)]

    [Header("Stamina")]
    private float stamina = 0f;
    const float MAX_STAMINA = 100f;
    [Space(5f)]


    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool jumpButtonPressed = false;
    bool readyToJump;
    bool playerLanded = false;
    [Space(5f)]




    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    //Player Input Keys
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode bounceKey = KeyCode.Q;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;
    public Transform orientation;
    public PhysicsMaterial[] physicsMaterialsArray;
    [SerializeField]
    private CapsuleCollider playerObjectCollider;
    public GameObject PlayerObj;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Ledge Handling")]

    [SerializeField]
    private PlayerLedge ledge;


    
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        isWalking = false;
        canMove = true;
        readyToJump = true;
        jumpButtonPressed = false;
        playerLanded = false;
        startYScale = transform.localScale.y;
        stamina = MAX_STAMINA;
    }

    //Player Stamina
    public float GetPlayerStamina
    {
        get{
            return stamina;
        }

        set
        {
            stamina = value;
        }
    }

    /// <summary>
    /// The Update method is the main method that checks every frame.
    /// </summary>
    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight, whatIsGround);
        Debug.DrawLine(this.transform.position, new Vector3(transform.position.x, transform.position.y - (playerHeight * 0.5f + 0.2f), transform.position.z), Color.yellow);


        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
        {
            rb.linearDamping = groundDrag;
            Invoke(nameof(ResetPlayerObj), 0.1f);

            //Reset movement back to walk speed
            if (!isRunning)
            {
                moveSpeed = walkSpeed;
            }
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    /// <summary>
    /// FixedUpdate method is menat for anything physics related
    /// </summary>
    private void FixedUpdate()
    {
        if (canMove)
        {
            MovePlayer();
        }
    }

    //Custom Friciton method that switches physics material depending on the movement
    public void Friction()
    {
        if (grounded || ledge.isHanging)
        {
            playerObjectCollider.material = physicsMaterialsArray[0];
        }
        else
        {
            playerObjectCollider.material = physicsMaterialsArray[1];
        }
    }

    /// <summary>
    /// MyInput Method handles all the player input and keybinds
    /// </summary>
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded && !ledge.GetCanClimb())
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    /// <summary>
    /// StateHandler handles all the state logic. The run logic is within the StateHandler, and is only capable when the stamina is greater than 0.
    /// There is also a reset for the stamina when the player is no longer in run mode
    /// </summary>
    private void StateHandler()
    {
        if (Input.GetKeyUp(sprintKey) && stamina < MAX_STAMINA) 
        {
            Invoke("resetStamina", 0.1f);
        }

        // Mode - Sprinting
        if (grounded && Input.GetKey(sprintKey) && stamina > 0f)
        {
            moveSpeed = sprintSpeed;

            if(Mathf.Abs(rb.linearVelocity.magnitude) > .1f)
            {
                isRunning = true;
                isWalking = false;

                stamina -= Time.deltaTime * 10f;

                // Set Stamina to 0
                if (stamina <= 0f)
                {
                    stamina = 0f;
                }
            }
            else
            {
                isRunning = false;
            }
        }
        // Mode - Walking
        else if (grounded)
        {
            moveSpeed = walkSpeed;
            isRunning = false;

            if (!isRunning && Mathf.Abs(rb.linearVelocity.magnitude) > .1f)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }
        }
    }

    IEnumerator resetStamina()
    {
        yield return new WaitForSeconds(5f);
        stamina = MAX_STAMINA;
    }

    /// <summary>
    /// Move Player handles all the basic player movement logic. Ground, slope and air movement physics is applied in this script
    /// </summary>
    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        Friction();

        if (Mathf.Abs(rb.linearVelocity.magnitude) > .1f && !isRunning)
        {
            isWalking = true;
        }

        if (Mathf.Abs(rb.linearVelocity.magnitude) == 0f)
        {
            isWalking = false;
        }

        if (stamina <= 0f)
        {
            StartCoroutine("resetStamina");
        }

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    /// <summary>
    /// Controls the speed of the player controller on different surfaces and slopes. Limited velocity is also created to ensure the player doesn't blitz of the screen.
    /// </summary>

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void ResetPlayerObj()
    {
        PlayerObj.SetActive(true);
    }
    

    /// <summary>
    /// Jump method is the force applied when the junp button is pressed
    /// </summary>
    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        jumpButtonPressed = true;
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    /// <summary>
    /// Raycast detector for findng slope surfaces
    /// </summary>
    /// <returns></returns>
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    public Rigidbody getRigidbody()
    {
        return rb;
    }
    public void setMoveDirection(Vector3 value)
    {
        moveDirection = value;
    }

    public void setMoveDirectionY(float value)
    {
         moveDirection.y = value;
    }

    public Vector3 getMoveDirection()
    {
        return moveDirection;
    }
    public float getMoveSpeed()
    {
        return moveSpeed;
    }

    public bool OnGround()
    {
        return grounded;
    }

}



