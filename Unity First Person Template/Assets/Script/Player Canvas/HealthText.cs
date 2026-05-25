using TMPro;
using UnityEngine;

public class HealthText : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public PlayerHealth playerHealth;

    // Update is called once per frame
    void Update()
    {
        healthText.text = "Health: {" + $"{playerHealth.health}" + "}";
    }
}
