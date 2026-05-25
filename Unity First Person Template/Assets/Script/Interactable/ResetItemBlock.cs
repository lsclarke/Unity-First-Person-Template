using UnityEngine;

public class ResetItemBlock : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.TryGetComponent(out IInteractable obj))
        {
            other.transform.position = obj.OriginalTransform().position;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.TryGetComponent(out IInteractable obj))
        {
            other.transform.position = obj.OriginalTransform().position;
        }
    }
}
