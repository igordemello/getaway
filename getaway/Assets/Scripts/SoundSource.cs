using UnityEngine;

public class SoundSource : MonoBehaviour
{
    public float volume = 1f;
    public float soundCd = 1f;
    private float soundTimer = 0f;
    public bool isActive = false;
    public float activeDuration = 0.2f;

    void Update()
    {
        if (isActive)
        {
            soundTimer += Time.deltaTime;
            if (soundTimer >= activeDuration)
            {
                isActive = false;
                soundTimer = 0f;
            }
        }
    }

    public void PlaySound(float vol)
    {
        volume = vol;
        isActive = true;
        soundTimer = 0f;
    }
}
