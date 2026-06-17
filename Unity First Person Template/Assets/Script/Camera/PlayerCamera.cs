using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    /// <summary>
    /// Input Action Vars
    /// </summary>


    [Header("Camera Controls")]

    public float mouseSenX;
    public float mouseSenY;

    public Transform orientation;
    public Transform playerObj;

    private float xRotation;
    private float yRotation;

    private Transform originalTransform;
    private Transform originalPlayerObj;
    private Transform originalOrientation;


    [SerializeField]
    private PlayerLedge ledge;
    public bool ledgeCamera  = false;

    private void Awake()
    {
        originalTransform = transform;
        originalPlayerObj = playerObj;
        originalOrientation = orientation;
    }
    private void Start()
    {
        //Disable Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update()
    {
        if (ledgeCamera)
        {
            LedgeFPSCamera();

        }
        else
        {
            StandardFPSCamera();
        }

    }

    private void StandardFPSCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSenX;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSenY;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);



        //rotate camera and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        playerObj.forward = orientation.forward;
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);

    }

    private void LedgeFPSCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSenX/2f;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSenY/2f;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        yRotation = Mathf.Clamp(yRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        //orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}