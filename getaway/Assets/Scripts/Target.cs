using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Target : MonoBehaviour
{
    [Header("Target info")]
    public float maxHealth = 100f;
    public float health = 100f;
    public GameObject other;
    private ExplosiveEnemy explosive;

    [Header("Damage Overlay")]
    public Image damageOverlay;
    public float duration;
    public float fadeSpeed = 5f;
    public CamRecoil camShake;
    private float durationTimer;
    private float maxOverlayAlpha = 0.8f;
    private float damageToAlphaFactor = 0.5f;
    private float currentAlpha;
    public Slider vidaBar;

    [Header("Regen System")]
    public bool regen = false;
    public float regen_time = 10f;    
    public float regenSpeed = 5f;      
    private Coroutine regenRoutine;

    private void Start()
    {
        health = maxHealth;

        if (damageOverlay)
            damageOverlay.color = new Color(
                damageOverlay.color.r,
                damageOverlay.color.g,
                damageOverlay.color.b,
                0
            );
    }

    private void Update()
    {
        
        if (CompareTag("Player"))
            if (vidaBar != null)
            {
                vidaBar.maxValue = maxHealth;
                vidaBar.value = health;
            }

        
        if (health <= 0f)
        {
            if (CompareTag("Player"))
            {
                StartCoroutine(DeathDelay());
                return;
            }

            if (GetComponent<ExplosiveEnemy>() != null)
            {
                explosive = GetComponent<ExplosiveEnemy>();
                explosive.Explode();
                return;
            }

            Die();
            return;
        }

       
        if (CompareTag("Player"))
        {
            if (health <= 15f && health > 0f)
            {
                float pulse = Mathf.Abs(Mathf.Sin(Time.time * 2.5f)) * 0.9f;
                SetOverlayAlpha(pulse);
            }
            else if (damageOverlay.color.a > 0f)
            {
                currentAlpha = Mathf.Lerp(currentAlpha, 0f, Time.deltaTime * fadeSpeed);
                SetOverlayAlpha(currentAlpha);
            }
        }

        if (regen && health > 0f && health < maxHealth)
        {
            health += regenSpeed * Time.deltaTime;
            health = Mathf.Clamp(health, 0f, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0f)
            return;

        if (CompareTag("Player"))
        {
            
            if (regenRoutine != null)
                StopCoroutine(regenRoutine);

            regenRoutine = StartCoroutine(RegenCooldown());

            if (camShake)
                camShake.Fire();

            durationTimer = 0;

            float targetAlpha = Mathf.Clamp(
                damageOverlay.color.a + amount * damageToAlphaFactor,
                0,
                maxOverlayAlpha
            );

            SetOverlayAlpha(targetAlpha);
        }

        health -= amount;
        health = Mathf.Clamp(health, 0f, maxHealth);
    }

    private void SetOverlayAlpha(float alpha)
    {
        currentAlpha = alpha;

        if (damageOverlay)
            damageOverlay.color = new Color(
                damageOverlay.color.r,
                damageOverlay.color.g,
                damageOverlay.color.b,
                currentAlpha
            );
    }

    private IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene("DerrotaMenu");
    }

    private IEnumerator RegenCooldown()
    {
        regen = false;
        yield return new WaitForSeconds(regen_time);
        regen = true;
    }

    void Die()
    {
        if (CompareTag("Turret"))
        {
            GetComponent<TurretBehaviour>().setEnergy(false);
            return;
        }

        if (other != null)
            Destroy(other);

        Destroy(gameObject);
    }
}
