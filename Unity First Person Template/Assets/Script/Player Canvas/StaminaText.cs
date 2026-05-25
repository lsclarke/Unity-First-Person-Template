using TMPro;
using UnityEngine;

public class StaminaText : MonoBehaviour
{
    public TextMeshProUGUI staminaText;


    public PlayerMovement movement;

    // Update is called once per frame
    void Update()
    {
        staminaText.text = "Stamina: {" + $"{movement.GetPlayerStamina}" + "}";
    }
}
