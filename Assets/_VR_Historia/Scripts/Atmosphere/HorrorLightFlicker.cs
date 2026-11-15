using UnityEngine;

[RequireComponent(typeof(Light))]
public class HorrorLightFlicker : MonoBehaviour
{
    public enum LightMode { BrokenFluorescent, EmergencyPulse }

    [Header("Configuracion General")]
    public LightMode mode = LightMode.BrokenFluorescent;
    public float minIntensity = 0.05f;
    public float maxIntensity = 1.8f;

    [Header("Ajustes Fluorescente Danado")]
    [Range(0.01f, 0.5f)]
    public float flickerSpeed = 0.08f;
    [Range(0f, 1f)]
    public float blackoutChance = 0.15f;

    [Header("Ajustes Luz de Emergencia")]
    public float pulseSpeed = 2.0f;

    [Header("Audio Opcional (Zumbido / Chispa)")]
    public AudioSource sparkAudio;

    private Light targetLight;
    private float timer;

    void Start()
    {
        targetLight = GetComponent<Light>();
    }

    void Update()
    {
        if (mode == LightMode.BrokenFluorescent)
        {
            HandleFluorescentFlicker();
        }
        else if (mode == LightMode.EmergencyPulse)
        {
            HandleEmergencyPulse();
        }
    }

    private void HandleFluorescentFlicker()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            if (Random.value < blackoutChance)
            {
                targetLight.intensity = 0f;
            }
            else
            {
                targetLight.intensity = Random.Range(minIntensity, maxIntensity);
                if (sparkAudio != null && !sparkAudio.isPlaying && Random.value < 0.2f)
                {
                    sparkAudio.Play();
                }
            }
            timer = Random.Range(flickerSpeed * 0.5f, flickerSpeed * 1.5f);
        }
    }

    private void HandleEmergencyPulse()
    {
        float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, wave);
    }
}
