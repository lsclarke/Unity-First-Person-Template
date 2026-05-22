using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    //This variables is for checking if we can interact with an object
    public bool canInteract;

    public bool isInteracting;
    //This variable is for keeping track of what object we are looking at !
    private Collider InteractableObject;

    //This variable checks for the layer the player is detecting
    public LayerMask DetectThisLayer;

    //This variable controls the contact radius
    public float contactDistance;

    //public GameObject Icon;
    Ray ray;

    bool pickUp;

    IInteractable interactable;
    //Name of object detected
    string objectName = "";

    [SerializeField]
    private HandHoldItem hand;

    private void Update()
    {
        //Calls the Interactor method
        Interactor();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.forward * contactDistance);
    }

    //A custom method created for detecting Interacatable objects
    public void Interactor()
    {
        //Camera FPS Raycast based detection
        
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, contactDistance, DetectThisLayer))
        {
            Debug.Log("Object found");
            canInteract = true;
            InteractableObject = hit.collider;

            if(objectName == "")
                objectName = InteractableObject.gameObject.name;

            if (Input.GetKey("f") && InteractableObject.TryGetComponent<IInteractable>(out IInteractable obj))
            {
                obj.Interact();
                interactable = obj;
                isInteracting = true;
                objectName = InteractableObject.gameObject.name;
            }
        }
        else
        {
            canInteract = false;

            if (isInteracting)
            {
                objectName = InteractableObject.gameObject.name;
                pickUp = true;
            }
            else
            {
                canInteract = false;
                InteractableObject = null;
                interactable = null;
                objectName = "";
            }
        }
    }
    public string getName()
    {
        return objectName;
    }
}
