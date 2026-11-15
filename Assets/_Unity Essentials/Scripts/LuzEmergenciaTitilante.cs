using UnityEngine;

public class LuzEmergenciaTitilante : MonoBehaviour
{
    [Header("Rotacion")]
    [SerializeField] private float velocidadRotacion = 90f;
    [SerializeField] private bool rotarEjeX;

    [Header("Pulso de Intensidad")]
    [SerializeField] private float pulsoMinIntensidad = 0.1f;
    [SerializeField] private float pulsoMaxIntensidad = 2.5f;
    [SerializeField] private float pulsoPorSegundo = 1.5f;

    [Header("Flicker Realista")]
    [SerializeField] private bool flickerActivo = true;
    [SerializeField] [Range(0f, 1f)] private float flickerProbabilidad = 0.15f;
    [SerializeField] private float flickerDuracionMin = 0.02f;
    [SerializeField] private float flickerDuracionMax = 0.08f;

    [Header("Color Dinamico")]
    [SerializeField] private bool colorDinamico;
    [SerializeField] private Color colorMinimo = new Color(0.8f, 0.2f, 0f);
    [SerializeField] private Color colorMaximo = new Color(1f, 0.4f, 0f);

    [Header("Zumbido Electrico")]
    [SerializeField] private AudioSource zumbidoSource;
    [SerializeField] private float zumbidoMinPitch = 0.9f;
    [SerializeField] private float zumbidoMaxPitch = 1.1f;

    private Light luz;
    private float faseInicial;
    private float flickerTimer;
    private bool enFlicker;
    private float intensidadPreFlicker;

    void Start()
    {
        luz = GetComponent<Light>();
        faseInicial = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (luz == null) return;

        float pulso = Mathf.Sin((Time.time * pulsoPorSegundo * Mathf.PI * 2f) + faseInicial);
        float t = (pulso + 1f) * 0.5f;
        float intensidadBase = Mathf.Lerp(pulsoMinIntensidad, pulsoMaxIntensidad, t);

        if (flickerActivo)
        {
            intensidadBase = AplicarFlicker(intensidadBase);
        }

        luz.intensity = intensidadBase;

        if (colorDinamico)
        {
            luz.color = Color.Lerp(colorMinimo, colorMaximo, t);
        }

        Vector3 ejeRotacion = rotarEjeX ? Vector3.right : Vector3.up;
        transform.Rotate(ejeRotacion, velocidadRotacion * Time.deltaTime, Space.World);

        ActualizarZumbido(t);
    }

    float AplicarFlicker(float intensidad)
    {
        if (enFlicker)
        {
            flickerTimer -= Time.deltaTime;
            if (flickerTimer <= 0f)
            {
                enFlicker = false;
                return intensidadPreFlicker;
            }
            return intensidad * Random.Range(0f, 0.15f);
        }

        if (Random.value < flickerProbabilidad * Time.deltaTime * 10f)
        {
            enFlicker = true;
            intensidadPreFlicker = intensidad;
            flickerTimer = Random.Range(flickerDuracionMin, flickerDuracionMax);
            return intensidad * 0.05f;
        }

        return intensidad;
    }

    void ActualizarZumbido(float intensidadNormalizada)
    {
        if (zumbidoSource == null) return;
        zumbidoSource.pitch = Mathf.Lerp(zumbidoMinPitch, zumbidoMaxPitch, intensidadNormalizada);
        zumbidoSource.volume = Mathf.Lerp(0.05f, 0.2f, intensidadNormalizada);
    }
}
