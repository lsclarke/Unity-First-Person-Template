using UnityEngine;
using UnityEngine.UIElements;

public class ReticleContainerController : MonoBehaviour
{

    public GameObject interactText;

    [SerializeField]
    private PlayerInteract interaction;
    private void ChangeIcon()
    {
        if (interaction.canInteract)
        {
            interactText.SetActive(true);
        }
        else
        {
            interactText.SetActive(false);
        }
    }

    private void Update()
    {
        ChangeIcon();
    }
}
