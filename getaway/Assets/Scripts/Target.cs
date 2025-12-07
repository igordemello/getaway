using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Target : MonoBehaviour
{
    [Header("Target info")]
    public float health = 100f;
    public TextMeshProUGUI health_UI;
    public GameObject other;
    private ExplosiveEnemy explosive;

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

    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health < 0f) health = 0f;

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
