using UnityEngine;

[RequireComponent(typeof(Light))]
public class LuzFlickerSyntro : MonoBehaviour
{
    public enum ModoFlicker { Irregular, FrioParpadeante }

    [Header("Modo")]
    public ModoFlicker modo = ModoFlicker.Irregular;

    [Header("Intensidad")]
    public float intensidadMin = 0.1f;
    public float intensidadMax = 1.5f;

    [Header("Velocidad (solo Irregular)")]
    [Range(0.03f, 0.4f)]
    public float intervaloMin = 0.05f;
    [Range(0.05f, 0.5f)]
    public float intervaloMax = 0.25f;
    [Range(0f, 0.4f)]
    public float probabilidadApagon = 0.12f;

    [Header("Pulso Frio (solo FrioParpadeante)")]
    public float velocidadPulso = 3.5f;
    [Range(0f, 0.3f)]
    public float probabilidadMicroCorte = 0.08f;

    private Light luz;
    private float timer;

    void Start()
    {
        luz = GetComponent<Light>();
    }

    void Update()
    {
        if (luz == null) return;

        if (modo == ModoFlicker.Irregular)
            ActualizarIrregular();
        else
            ActualizarFrioParpadeante();
    }

    void ActualizarIrregular()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (Random.value < probabilidadApagon)
        {
            luz.intensity = 0f;
            timer = Random.Range(0.08f, 0.2f);
        }
        else
        {
            luz.intensity = Random.Range(intensidadMin, intensidadMax);
            timer = Random.Range(intervaloMin, intervaloMax);
        }
    }

    void ActualizarFrioParpadeante()
    {
        float wave = (Mathf.Sin(Time.time * velocidadPulso) + 1f) * 0.5f;
        float intensidad = Mathf.Lerp(intensidadMin, intensidadMax, wave);

        if (Random.value < probabilidadMicroCorte * Time.deltaTime * 10f)
            intensidad *= Random.Range(0f, 0.1f);

        luz.intensity = intensidad;
    }
}
