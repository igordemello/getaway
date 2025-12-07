using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;

public class Target : MonoBehaviour
{
    [Header("Target info")]
    public float health = 100f;
    public TextMeshProUGUI health_UI;
    public GameObject other;
    private ExplosiveEnemy explosive;

    [Header("Damage Overlay")]
    public Image damageOverlay;
    public float duration;
    public float fadeSpeed;
    public CamRecoil camShake;
    private float durationTimer;
    private float maxOverlayAlpha = 0.8f;
    private float damageToAlphaFactor = 0.5f;
    private float currentAlpha;
    private void Start()
    {
        if(damageOverlay)
            damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, 0);
    }

    private void Update()
    {

        if (CompareTag("Player"))
            health_UI.text = $"Health:\n{health}";

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
        }

        if (CompareTag("Player"))
        {
            if (health <= 15 && health > 0)
            {
                float pulse = Mathf.Abs(Mathf.Sin(Time.time * 2.5f)) * 0.9f;
                SetOverlayAlpha(pulse);
                return;
            }
            if (damageOverlay.color.a > 0)
            {
                currentAlpha = Mathf.Lerp(currentAlpha, 0, Time.deltaTime * fadeSpeed);
                SetOverlayAlpha(currentAlpha);
            }
        }

    }

    public void TakeDamage(float amount)
    {
        if (CompareTag("Player"))
        {
            camShake.Fire();
            durationTimer = 0;
            float targetAlpha = Mathf.Clamp(damageOverlay.color.a + amount * damageToAlphaFactor, 0, maxOverlayAlpha);

            SetOverlayAlpha(targetAlpha);
        }
            

        health -= amount;
        if (health < 0f) health = 0f;

    }
    private void SetOverlayAlpha(float alpha)
    {
        currentAlpha = alpha;
        damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, currentAlpha);
    }
    IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene("DerrotaMenu");
    }

   

    void Die()
    {
        if (other!=null) 
            Destroy(other);
        Destroy(gameObject);
    }
}
