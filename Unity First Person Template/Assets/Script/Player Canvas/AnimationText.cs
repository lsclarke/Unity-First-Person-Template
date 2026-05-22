using TMPro;
using UnityEngine;


public class AnimationText : MonoBehaviour
{
    public TextMeshProUGUI animationText;
    public PlayerInteract interaction;

    string condition;

    public void setCondition(string value)
    {
        condition = value;
    }
    public string getCondition()
    {
        return condition;
    }

    // Update is called once per frame
    void Update()
    {
        animationText.text = "{" + $"{condition}" + "}";
    }
}
