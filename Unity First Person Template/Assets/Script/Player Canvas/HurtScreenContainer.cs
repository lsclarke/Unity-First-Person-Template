using UnityEngine;

public class HurtScreenContainer : MonoBehaviour
{
    public GameObject container;
    [SerializeField]
    private Animator animator;
    public void PlayAnimation()
    {
        animator.SetTrigger("Show");
    }
    public void TurnOff()
    {
        container.SetActive(false);
    }
}
