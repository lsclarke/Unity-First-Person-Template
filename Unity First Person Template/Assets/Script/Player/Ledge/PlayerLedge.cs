using UnityEngine;

public class PlayerLedge : MonoBehaviour
{
    [Header("Ledge Detection")]
    public Transform ledgeCheckerStart;
    public float checkDistance;
    public LayerMask whatIsLedge;
    public Transform orientation;
    bool isLedge;

    [Header("Ledge Hang")]

    [SerializeField]
    private Rigidbody rb;
    public bool canHang;
    public bool isHanging;

    private RaycastHit val;
    private Vector3 val2;

    [SerializeField]
    private Collider ledgeCollider;
    public float step;

    public float offsetZ;
    public float offsetX;
    public float offsetY;

    private CapsuleCollider playerObject;
    private void OnDrawGizmos()
    {
        //Main Ray Detector
        Gizmos.DrawRay(ledgeCheckerStart.position, orientation.forward * checkDistance);
        Gizmos.DrawWireSphere(val.point, .02f);
        Gizmos.DrawWireSphere(val2, .02f);
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

        CalculateLedgeHeight(hit);
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
       
        Vector3 hangLocation = new Vector3(startTopPos.x + offsetX, startTopPos.y + offsetY, startTopPos.z + offsetZ);

        if (isHanging)
        {
            transform.position = hangLocation;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            playerObject.enabled = false;
        }

            return ledgeHeight;
    }

}
