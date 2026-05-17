using UnityEngine;

public class MoveArms : MonoBehaviour
{
    public Transform handPosition;

    // Update is called once per frame
    void Update()
    {
        transform.position = handPosition.position;
    }
}
