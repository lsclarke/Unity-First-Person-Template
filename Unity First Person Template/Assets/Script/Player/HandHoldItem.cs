using UnityEngine;

public class HandHoldItem : MonoBehaviour
{
    public bool isFull = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other != null)
        {
            if (other.gameObject.TryGetComponent<IInteractable>(out IInteractable obj))
            {

            }
        }
    }
}
