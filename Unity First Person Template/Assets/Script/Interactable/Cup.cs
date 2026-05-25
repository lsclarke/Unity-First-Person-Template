using System.Collections;
using UnityEngine;

public class Cup : MonoBehaviour, IInteractable
{
    private Transform originalPosition;
    public Transform newPosition;
    public bool isPickedUp = false;
    [SerializeField]
    private PlayerInteract interaction;
    private Collider collider;

    public Transform oldParent;

    //Check for ground
    [Header("Ground Check")]
    public float checkHeight;
    public LayerMask whatIsGround;
    bool grounded;


    private void Start()
    {
        originalPosition = this.transform;
        originalPosition.localPosition = this.transform.localPosition;
        collider = GetComponent<Collider>();
        isPickedUp=false;

    }

    Transform IInteractable.OriginalTransform()
    {
        return originalPosition;
    }

    void IInteractable.Interact()
    {
       isPickedUp = !isPickedUp;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, -transform.up * checkHeight);
    }

    private void Update()
    {
        if (isPickedUp)
        {
            transform.parent = newPosition;
            transform.position = newPosition.position;
            collider.enabled = false;
        }
        else
        {
            transform.parent = oldParent;
            transform.localPosition = originalPosition.localPosition;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            collider.enabled = true;
        }

        if (Input.GetKeyDown("g"))
        {
            isPickedUp = false;
            transform.localPosition = originalPosition.localPosition;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collider.gameObject.CompareTag("ResetItemLocation"))
        {
            isPickedUp = false;
            transform.position = originalPosition.position;
        }
    }

}
