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

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (CompareTag("Player"))
            health_UI.text = $"Health:\n{health}";

        if (health <= 0f)
        {
            if (CompareTag("Player"))
            {
                StartCoroutine(DeathDelay());
                return;
            }
            Die();
        }
    }
    IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("DerrotaMenu");
    }

    void Die()
    {
        if (other!=null) 
            Destroy(other);
        Destroy(gameObject);
    }
}
