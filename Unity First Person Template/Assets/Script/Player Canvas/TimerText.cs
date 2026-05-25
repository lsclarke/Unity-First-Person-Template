using TMPro;
using UnityEngine;

public class TimerText : MonoBehaviour
{

    public TextMeshProUGUI timerText;

    private float elapseTime;

    // Update is called once per frame
    void Update()
    {
        elapseTime += Time.deltaTime;
        int min = Mathf.FloorToInt(elapseTime/60);
        int sec = Mathf.FloorToInt(elapseTime % 60);

        timerText.text = "Time Spent: {" + string.Format("{0:00}:{1:00}",min,sec) + "}";
    }
}
