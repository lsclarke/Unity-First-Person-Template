using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PlayerHealth : MonoBehaviour
{

    private float timeScale = 0.001f;
    private float pauseTime = 0.0001f;

    [SerializeField]
    private CameraShake cameraShake;

    public float health;
    public bool isHurt = false;
    [SerializeField]
    const float maxHealth = 100f;

    //[Header("References")]
    [SerializeField] public HurtScreenContainer hurtScreen;

    private void Start()
    {
        health = maxHealth;
        isHurt = false;
        hurtScreen.gameObject.SetActive(isHurt);
    }

    IEnumerator resetCondition()
    {
        hurtScreen.gameObject.SetActive(isHurt);
        isHurt = true;
        yield return new WaitForSeconds(1f);
        isHurt = false;
    }

    public void OnTakeDamage(float value)
    {
        health -= value;

        isHurt = true;
        StartCoroutine(resetCondition());

        if (health <= 0)
        {
            health = maxHealth;
        }

    }

    //private void OnCollisionEnter2D(Collision2D other)
    //{
    //    if (other.gameObject.CompareTag("Enemy") && !playerCombatSystem.PlayerIsAttacking)
    //    {
    //        PlayKnockBack(other.gameObject);
    //        OnTakeDamage(15);

    //        StartCoroutine(HitStop(pauseTime));
    //        StartCoroutine(cameraShake.Shake(cameraShake.shakeTime, cameraShake.shakeSpeed));
    //    }

    //    if (other.gameObject.CompareTag("Hazard"))
    //    {
    //        PlayKnockBack(other.gameObject);
    //        OnTakeDamage(5);
    //        StartCoroutine(HitStop(pauseTime));
    //        StartCoroutine(cameraShake.Shake(cameraShake.shakeTime, cameraShake.shakeSpeed));
    //    }
    //}

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.gameObject.CompareTag("Hazard"))
    //    {
    //        if (!playerManager.isHurt)
    //        {
    //            PlayKnockBack(other.gameObject);
    //            OnTakeDamage(5);
    //            StartCoroutine(HitStop(pauseTime));
    //            StartCoroutine(cameraShake.Shake(cameraShake.shakeTime, cameraShake.shakeSpeed));
    //        }
    //    }
    //}

    public IEnumerator HitStop(float duration)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSeconds(duration);
        Time.timeScale = 1f;
        isHurt = false;

    }

    private void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            isHurt =true;
            hurtScreen.PlayAnimation();
            OnTakeDamage(50f);
            StartCoroutine(HitStop(pauseTime));
            StartCoroutine(cameraShake.Shake(cameraShake.ShakeValues().x, cameraShake.ShakeValues().y));
            StartCoroutine(resetCondition());
        }
    }

}
