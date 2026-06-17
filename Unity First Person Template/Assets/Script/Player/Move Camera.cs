using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;
    public Transform climbcameraPosition;

    [SerializeField]
    private PlayerLedge ledge;
    // Update is called once per frame
    void Update()
    {
        if (ledge.isHanging)
        {
            transform.position = climbcameraPosition.position;
        }
        else
        {
            transform.position = cameraPosition.position;

        }

    }
}
