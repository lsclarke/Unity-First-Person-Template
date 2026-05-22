using UnityEngine;

public class Cup : MonoBehaviour, IInteractable
{
    private Transform originalPosition;
    public Transform newPosition;
    public bool isPickedUp = false;
    [SerializeField]
    private PlayerInteract interaction;
    private Collider collider;

    private Rigidbody rb;

    //Check for ground
    [Header("Ground Check")]
    public float checkHeight;
    public LayerMask whatIsGround;
    bool grounded;


    private void Start()
    {
        originalPosition = this.transform;
        collider = GetComponent<Collider>();
        isPickedUp=false;
        rb=GetComponent<Rigidbody>();
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
            transform.parent = null;
            transform.position = originalPosition.position;
            collider.enabled = true;
        }

        if (Input.GetKeyDown("g"))
        {
            isPickedUp = false;
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
