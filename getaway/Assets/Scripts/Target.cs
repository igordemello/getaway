using TMPro;
using UnityEngine;

public class Target : MonoBehaviour
{
    //[Header("vida")];
    public float health = 100f;
    public TextMeshProUGUI health_UI;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (CompareTag("player"))
            health_UI.text = $"Health:\n{health}";

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
