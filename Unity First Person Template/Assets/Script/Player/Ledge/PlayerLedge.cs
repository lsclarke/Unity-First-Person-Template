using UnityEngine;

public class PlayerLedge : MonoBehaviour
{
    [Header("Ledge Detection")]

    [SerializeField]
    private PlayerMovement move;
    public Transform ledgeCheckerStart;
    public Transform LeftChecker;
    public Transform RightChecker;
    public Transform RightCornerCheck;
    public float checkDistance;
    public LayerMask whatIsLedge;
    public Transform orientation;
    bool isLedge;

    [Header("Ledge Hang")]

    [SerializeField]
    private Rigidbody rb;
    public bool canHang;
    public bool isHanging;
    private Vector3 moveDirection;

    [HideInInspector]
    public RaycastHit val;
    private Vector3 val2;

    [SerializeField]
    private Collider ledgeCollider;
    public float step;

    public float offsetZ;
    public float offsetX;
    public float offsetY;

    [SerializeField]
    private CapsuleCollider playerObject;

    [SerializeField]
    private Transform model;

    private void Awake()
    {
        isHanging = false;
        canHang = false;
    }
    private void OnDrawGizmos()
    {
        //Main Ray Detector
        Gizmos.DrawRay(ledgeCheckerStart.position, orientation.forward * checkDistance);
        Gizmos.DrawWireSphere(val.point, .02f);
        Gizmos.DrawWireSphere(val2, .02f);

      
        Gizmos.DrawWireSphere(RightCornerCheck.position, .1f);
    }

    private void FixedUpdate()
    {
        LedgeDetector();
    }

    private void LedgeDetector()
    {
        //Check for mesh with Ledge layer
        isLedge = Physics.Raycast(ledgeCheckerStart.position, orientation.forward,out RaycastHit hit, checkDistance, whatIsLedge);
        //Main Ray Detector

        val = hit;
  
        if (isLedge)
        {
            var obj = hit.collider.gameObject;
            ledgeCollider = obj.GetComponent<Collider>();
            canHang = true;
        }
        else
        {
            canHang = false;
            ledgeCollider = null;
        }

        if(canHang) 
            CalculateLedgeHeight(hit);

        if (isHanging)
        {
            MoveOnLedge();
        }
    }

    private float CalculateLedgeHeight(RaycastHit ledgeRay)
    {
        //Represents y distance between raycast hit and top of ledge
        float ledgeHeight = 0f;

        //Get the top height of the mesh
        Vector3 topCollider = ledgeRay.point;
        topCollider.y = ledgeRay.collider.bounds.max.y;

        //Top point Y of the mesh
        var startTopPos = topCollider + Vector3.up * ledgeHeight;
        val2 = startTopPos;
        Debug.DrawRay(startTopPos, Vector3.up * -0.1f, Color.red);

        //Disable wiresphere
        if (!isLedge)
            val2 = Vector3.zero;

        ///////HANG ON LEDGE AT LOCATION BELOW///////

        //Create a forward Raycast
        if (!Physics.Raycast(startTopPos, Vector3.up, out RaycastHit hit, -0.3f))
        {
            Debug.DrawRay(startTopPos, orientation.forward * 0.1f, Color.red);
            isHanging = true;
        }

        var lookdir = (ledgeRay.collider.transform.position - transform.position);

        //Stop the gravity and freeze the player speed and cancel jump animation
        if (isHanging)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Quaternion.LookRotation(lookdir);
            rb.useGravity = false;
            move.jumpButtonPressed = false;
            playerObject.center = new Vector3(0f,0.5f,0f);
            transform.position = new Vector3(transform.position.x, ((ledgeRay.collider.bounds.max.y /2)), transform.position.z);
            move.canMove = false;
        }
        else
        {
            playerObject.center = new Vector3(0f, 0f, 0f);
            move.canMove = true;
        }

            return ledgeHeight;
    }

    public Vector3 MoveDirection()
    {
        return Vector3.zero;
    }

    /// <summary>
    /// Move the player along the X axis of the normal of the ledge game object. 
    /// The speed is less than walking speed while hanging and climbing
    /// </summary>
    public void MoveOnLedge()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        // calculate movement direction
        moveDirection = orientation.right * horizontalInput;

        rb.AddForce(moveDirection * 0.25f, ForceMode.Force);

        if (Mathf.Abs(moveDirection.y) == 0f)
        {
            moveDirection.y = verticalInput;
        }
        else
        {
            moveDirection.y = verticalInput;
        }

        if ((Mathf.Abs(horizontalInput) == 0f))
        {
            rb.linearVelocity = Vector3.zero;
        }
        CheckCorners();
    }

    private void CheckCorners()
    {
        ///////HANG ON LEDGE AT LOCATION BELOW///////

        //Create a forward Raycast
        if (isHanging)
        {
            Debug.DrawRay(LeftChecker.position, move.orientation.forward * 1f, Color.red);
            Debug.DrawRay(RightChecker.position, move.orientation.forward * 1f, Color.red);

            if (!Physics.Raycast(RightChecker.position, move.orientation.forward, out RaycastHit lineHit, 1f))
            {
                if (Physics.SphereCast(RightCornerCheck.position, .1f, move.orientation.forward, out RaycastHit sphereHit, 1.5f, whatIsLedge))
                {
                    Vector3.Slerp(transform.forward, sphereHit.point, 1f);
                }
            }
        }
    }

}
