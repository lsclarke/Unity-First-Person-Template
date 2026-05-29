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
    public Vector3 ledgeForwardPosition;
    public Transform ledgeChecker;
    public Transform playerObj;

    public float rayRadius;
    public LayerMask ledgeMask;
    private RaycastHit rayLedgeHit;

    [SerializeField]
    private GameObject ledgeObject;
    [Space(10f)]

    public float ledgeHeight;
    private Vector3 hangingPosition;
    private Vector3 movinghangPostion;
    public float offsetZ;
    public float offsetY;
    public float offsetX;

    [Space(10f)]
    private bool Switch;
    private bool canClimb;
    public bool isClimbing;
    private bool canHang;
    public bool isHanging;
    public bool canMoveOnLedge;
    public bool isMovingOnLedge;
    public bool isCorner;
    public bool noSpaceRight;
    public bool noSpaceLeft;

    [Space(10f)]

    [SerializeField]
    private CapsuleCollider capsuleCollider;
    private void Start()
    {
        isClimbing = false;
        isHanging = false;
        canClimb = false;
        canHang = false;
        canMoveOnLedge = false;
        isMovingOnLedge = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(ledgeChecker.position, rayRadius);
    }

    private void Update()
    {
        TurnOnLedgeDetector();
    }

    public bool CanPlayerClimb()
    {
        return canClimb;
    }

    public void TurnOnLedgeDetector()
    {

        //Raycast to check for a gameObject with the layer "Ledge"
        if (Physics.SphereCast(ledgeChecker.position, rayRadius, playerObj.forward, out rayLedgeHit, 0.01f, ledgeMask, QueryTriggerInteraction.Ignore))
        {
            canClimb = true;
            ledgeObject = rayLedgeHit.collider.transform.gameObject;


            //if (UnityEngine.Input.GetKeyDown("space"))
            //{
            //    if (canClimb && playerMovement.OnGround())
            //    {
            //    }
            //}
        }

    }

    private IEnumerator GrabLedge()
    {
        yield return new WaitForSeconds(.5f);
        var rb = playerMovement.getRigidbody();
        isHanging = true;
        transform.position = hangingPosition;
        capsuleCollider.center = new Vector3(0f, 5f, 0f);
        playerObj.forward = -rayLedgeHit.normal;
        rb.useGravity = false;
    }
}
