using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float stompTimer = 0;
    [SerializeField]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public bool isWalking = false;
    public bool isRunning = false;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool jumpButtonPressed = false;
    bool readyToJump;
    bool playerLanded = false;  

    public GameObject PlayerObj;


    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode bounceKey = KeyCode.Q;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
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
        readyToJump = true;
        jumpButtonPressed = false;
        playerLanded = false;
        startYScale = transform.localScale.y;

    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight /** 0.5f + 0.2f*/, whatIsGround);

        Debug.DrawLine(this.transform.position, new Vector3(transform.position.x, transform.position.y - (playerHeight * 0.5f + 0.2f), transform.position.z), Color.yellow);
        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
        {
            rb.linearDamping = groundDrag;
            Invoke(nameof(ResetPlayerObj), 0.1f);

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

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
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


    private void StateHandler()
    {

        // Mode - Sprinting
        if (grounded && Input.GetKey(sprintKey))
        {
            moveSpeed = sprintSpeed;

            if(Mathf.Abs(rb.linearVelocity.magnitude) > .1f)
            {
                isRunning = true;
                isWalking = false;
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

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(new Vector3(0f,0f,1f * 10f), ForceMode.Force);

        //if (Mathf.Abs(rb.linearVelocity.magnitude) > .1f && !isRunning)
        //{
        //    isWalking = true;
        //}

        //if (Mathf.Abs(rb.linearVelocity.magnitude) == 0f)
        //{
        //    isWalking = false;
        //}

        // on slope
        //if (OnSlope() && !exitingSlope)
        //{
        //    rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

        //    if (rb.linearVelocity.y > 0)
        //        rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        //}

        // on ground
        ///*else */if (grounded)
        //    //rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        //// in air
        //else if (!grounded)
        //    //rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        //// turn gravity off while on slope
        //rb.useGravity = !OnSlope();
    }

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



