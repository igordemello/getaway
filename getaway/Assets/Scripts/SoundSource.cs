using UnityEngine;

public class SoundSource : MonoBehaviour
{
    public float volume = 1f;           // Volume base do som
    public bool isActive = false;       // Se o som está "existindo"
    public float activeDuration = 0.25f; // Tempo que o som dura

    private float timer = 0f;

    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;

        if (timer >= activeDuration)
        {
            isActive = false;
            timer = 0f;
        }
    }

    public void PlaySound(float vol, float duration = 0.25f)
    {
        volume = vol;
        activeDuration = duration;

        isActive = true;
        timer = 0f;
    }
}
