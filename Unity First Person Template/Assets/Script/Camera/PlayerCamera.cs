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

    [SerializeField]
    private PlayerLedge Onledge;
    private void Start()
    {
        //Disable Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }




    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSenX;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSenY;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);



        //rotate camera and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if (!Onledge.isHanging)
        {
            playerObj.forward = orientation.forward;
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
}