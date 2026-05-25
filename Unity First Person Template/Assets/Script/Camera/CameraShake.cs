using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    const float shakeTime = 0.3f;
    const float shakeSpeed = 0.025f;
    private Vector3 originalPos;
    private bool endedShake = false;


    public Vector3 ShakeValues()
    {
        Vector3 value = new Vector2(shakeTime, shakeSpeed);
        return value;
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        originalPos = new Vector3(0f, 0f, -10f); ;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            endedShake = false;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (elapsed >= duration)
        {
            elapsed = 0.0f;
            transform.localPosition = originalPos;
            endedShake = true;
        }
    }

    public void ShakeCamera()
    {
        StartCoroutine("Shake");
        Debug.Log("CameraShake");
    }

    private void Update()
    {

    }
}
