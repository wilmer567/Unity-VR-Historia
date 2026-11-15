using System.Collections;
using UnityEngine;

public class DiarioDisparador : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private string flagAlInteractuar = "diario_recogido";
    [SerializeField] private AudioClip sonidoInteraccion;
    [SerializeField] private AudioClip sonidoPaginasPasando;

    [Header("Efecto Visual Flotacion")]
    [SerializeField] private float velocidadFlotacion = 1.5f;
    [SerializeField] private float alturaFlotacion = 0.05f;
    [SerializeField] private float velocidadRotacionSuave = 10f;

    [Header("Brillo Atractivo")]
    [SerializeField] private float emisionPulsoMin = 0.1f;
    [SerializeField] private float emisionPulsoMax = 0.5f;
    [SerializeField] private float emisionVelocidad = 2f;
    [SerializeField] private Color colorEmision = new Color(0.4f, 0.6f, 1f);

    [Header("Particulas")]
    [SerializeField] private ParticleSystem particulasBrillo;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private AudioSource audioSource;
    private Renderer objectRenderer;
    private bool activado;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        audioSource = GetComponent<AudioSource>();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
            objectRenderer.material.EnableKeyword("_EMISSION");

        if (particulasBrillo != null)
            particulasBrillo.Play();
    }

    void Update()
    {
        if (activado) return;

        float time = Time.time;

        float offsetY = Mathf.Sin(time * velocidadFlotacion) * alturaFlotacion;
        float offsetX = Mathf.Sin(time * velocidadFlotacion * 0.7f) * alturaFlotacion * 0.3f;
        transform.position = posicionInicial + new Vector3(offsetX, offsetY, 0f);

        transform.Rotate(Vector3.up, velocidadRotacionSuave * Time.deltaTime, Space.World);

        if (objectRenderer != null)
        {
            float pulso = (Mathf.Sin(time * emisionVelocidad) + 1f) * 0.5f;
            float intensidad = Mathf.Lerp(emisionPulsoMin, emisionPulsoMax, pulso);
            objectRenderer.material.SetColor(EmissionColor, colorEmision * intensidad);
        }
    }

    public void Interactuar()
    {
        if (activado) return;
        activado = true;

        if (particulasBrillo != null)
            particulasBrillo.Stop();

        if (audioSource != null && sonidoInteraccion != null)
        {
            audioSource.PlayOneShot(sonidoInteraccion);
        }

        if (GameManagerVR.Instance != null)
        {
            GameManagerVR.Instance.ActivarFlag(flagAlInteractuar);
            GameManagerVR.Instance.AvanzarCapitulo();
        }

        if (TerrorPsychologyManager.Instance != null)
            TerrorPsychologyManager.Instance.PulsoTerror(0.1f, 0.5f);

        StartCoroutine(AnimacionRecogida());
    }

    IEnumerator AnimacionRecogida()
    {
        Vector3 escalaInicio = transform.localScale;
        Vector3 posInicio = transform.position;
        Camera cam = Camera.main;
        Vector3 posTarget = cam != null
            ? cam.transform.position + cam.transform.forward * 0.4f
            : posInicio + Vector3.up * 0.5f;

        float duracion = 0.6f;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            float progreso = t / duracion;
            float curva = 1f - Mathf.Pow(1f - progreso, 3f);

            transform.position = Vector3.Lerp(posInicio, posTarget, curva);
            transform.localScale = Vector3.Lerp(escalaInicio, escalaInicio * 0.1f, curva);
            yield return null;
        }

        if (sonidoPaginasPasando != null && audioSource != null)
            audioSource.PlayOneShot(sonidoPaginasPasando);

        yield return new WaitForSeconds(0.3f);
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Interactuar();
        }
    }
}
