using TMPro;
using UnityEngine;

public class ItemText : MonoBehaviour
{
    public TextMeshProUGUI itemText;
    public PlayerInteract interaction;

    // Update is called once per frame
    void Update()
    {
        itemText.text = "Item In Hand {" + $"{interaction.getName()}" + "}";
    }
}
