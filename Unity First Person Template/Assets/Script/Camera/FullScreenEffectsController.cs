using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullScreenEffectsController : MonoBehaviour
{
    [Header("Time Stats")]
    [SerializeField] private float _topSpeedWindDisplayTime = 1.5f;
    [SerializeField] private float _topSpeedWindFadeOutTime = 0.5f;
    [SerializeField] private float _chaosEffectDisplayTime = 1.5f;
    [SerializeField] private float _chaosEffectFadeOutTime = 0.5f;

    [Header("References")]
    [SerializeField] private ScriptableRendererFeature _topSpeedWindScreen;
    [SerializeField] private Material _topSpeedWindMaterial;

    [SerializeField] private ScriptableRendererFeature _chaosEffectScreen;
    [SerializeField] private Material _chaosEffectMaterial;

    [Header("Intensity Stats")]
    [SerializeField] private float _voroniIntensityStat = 1.5f;
    [SerializeField] private float _vignetteIntensityStat = 1.25f;

    private int _voroniIntensity = Shader.PropertyToID("");
    private int _vignetteIntensity = Shader.PropertyToID("");
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _topSpeedWindScreen.SetActive(false);
        _chaosEffectScreen.SetActive(false);
    }

    private IEnumerator ChaosEffectScreen()
    {
        _chaosEffectScreen.SetActive(true);

        yield return new WaitForSeconds(_chaosEffectDisplayTime);
    }

    private IEnumerator TopSpeed()
    {
        _topSpeedWindScreen.SetActive(true);

        yield return new WaitForSeconds(_topSpeedWindDisplayTime);
    }
}
