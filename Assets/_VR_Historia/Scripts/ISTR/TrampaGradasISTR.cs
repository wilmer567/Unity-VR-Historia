using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ConfigVueltaGradas
{
    public string textoPisoCartel;
    public bool activarSilueta;
    public bool parpadeoLuz;
    public float duracionParpadeo = 1.5f;
    [Range(0f, 1f)] public float intensidadTerror;
}

public class TrampaGradasISTR : MonoBehaviour
{
    public static TrampaGradasISTR Instance { get; private set; }

    [Header("=== Triggers de Descanso ===")]
    [SerializeField] private BoxCollider triggerDescansoInferior;
    [SerializeField] private BoxCollider triggerDescansoMedio;
    [SerializeField] private BoxCollider triggerDescansoSuperior;

    [Header("=== Punto de Teletransporte ===")]
    [SerializeField] private Transform puntoRetornoInferior;
    [SerializeField] private Transform puntoLlegadaPiso3;

    [Header("=== Cartel de Piso ===")]
    [SerializeField] private TMPro.TextMeshPro textoCartelPiso;

    [Header("=== Silueta Terrorifica ===")]
    [SerializeField] private GameObject siluetaFantasma;
    [SerializeField] private Transform[] posicionesSilueta;
    [SerializeField] private float tiempoSiluetaVisible = 0.4f;

    [Header("=== Luces Gradas ===")]
    [SerializeField] private Light[] lucesGradas;
    [SerializeField] private AudioClip sonidoFlickerLuz;
    [SerializeField] private AudioClip sonidoSusurroCercano;
    [SerializeField] private AudioSource audioGradas;

    [Header("=== Teletransporte Suave ===")]
    [SerializeField] private float fadeOutDuracion = 0.6f;
    [SerializeField] private float fadeInDuracion = 0.8f;
    [SerializeField] private CanvasGroup panelFadeNegro;

    [Header("=== Configuracion de Vueltas ===")]
    [SerializeField] private ConfigVueltaGradas vuelta1 = new ConfigVueltaGradas
    {
        textoPisoCartel = "PISO 2",
        activarSilueta = false,
        parpadeoLuz = true,
        duracionParpadeo = 1f,
        intensidadTerror = 0.3f
    };
    [SerializeField] private ConfigVueltaGradas vuelta2 = new ConfigVueltaGradas
    {
        textoPisoCartel = "PISO 2",
        activarSilueta = true,
        parpadeoLuz = true,
        duracionParpadeo = 2f,
        intensidadTerror = 0.7f
    };
    [SerializeField] private ConfigVueltaGradas vuelta3 = new ConfigVueltaGradas
    {
        textoPisoCartel = "PISO 3",
        activarSilueta = true,
        parpadeoLuz = true,
        duracionParpadeo = 0.5f,
        intensidadTerror = 1f
    };

    [Header("=== Eventos ===")]
    public UnityEvent OnBucleIniciado;
    public UnityEvent OnVueltaCompletada;
    public UnityEvent OnBucleTerminado;

    private int vueltaActual;
    private bool bucleActivo;
    private bool enTransicion;
    private bool bucleCompletado;
    private float[] intensidadesLucesOriginales;

    public int VueltaActual => vueltaActual;
    public bool BucleCompletado => bucleCompletado;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (siluetaFantasma != null) siluetaFantasma.SetActive(false);

        GuardarIntensidadesLuces();
        DesactivarTriggers();
    }

    // ===========================================
    //  ACTIVACION DEL BUCLE
    // ===========================================

    public void IniciarBucleGradas()
    {
        if (bucleActivo || bucleCompletado) return;

        bucleActivo = true;
        vueltaActual = 0;
        ActivarTriggers();
        OnBucleIniciado?.Invoke();

        if (textoCartelPiso != null)
            textoCartelPiso.text = "PISO 1";

        Debug.Log("[TrampaGradas] Bucle de gradas iniciado. El jugador debe subir 3 veces.");
    }

    // ===========================================
    //  DETECCION DE TRIGGERS
    // ===========================================

    void OnTriggerEnter(Collider other)
    {
        if (!bucleActivo || enTransicion) return;
        if (!other.CompareTag("Player")) return;

        if (other == triggerDescansoSuperior)
            StartCoroutine(ProcesarLlegadaDescansoSuperior());
    }

    public void JugadorLlegoAlDescanso()
    {
        if (!bucleActivo || enTransicion) return;
        StartCoroutine(ProcesarLlegadaDescansoSuperior());
    }

    IEnumerator ProcesarLlegadaDescansoSuperior()
    {
        enTransicion = true;
        vueltaActual++;

        ConfigVueltaGradas config = GetConfigVuelta(vueltaActual);
        Debug.Log($"[TrampaGradas] Vuelta {vueltaActual}/3 — Cartel: {config.textoPisoCartel}");

        if (ControladorSentidosVR.Instance != null)
            ControladorSentidosVR.Instance.ForzarAlerta(config.intensidadTerror);

        if (config.parpadeoLuz)
            yield return StartCoroutine(SecuenciaParpadeoLuces(config.duracionParpadeo));

        if (textoCartelPiso != null)
            textoCartelPiso.text = config.textoPisoCartel;

        if (config.activarSilueta)
            yield return StartCoroutine(MostrarSiluetaFugaz());

        OnVueltaCompletada?.Invoke();

        if (vueltaActual >= 3)
        {
            yield return StartCoroutine(CompletarBucle());
        }
        else
        {
            yield return StartCoroutine(TeletransporteSuave(puntoRetornoInferior));

            if (audioGradas != null && sonidoSusurroCercano != null)
                audioGradas.PlayOneShot(sonidoSusurroCercano, 0.5f);
        }

        enTransicion = false;
    }

    // ===========================================
    //  PARPADEO DE LUCES EN DESCANSO
    // ===========================================

    IEnumerator SecuenciaParpadeoLuces(float duracion)
    {
        if (lucesGradas == null || lucesGradas.Length == 0) yield break;

        if (audioGradas != null && sonidoFlickerLuz != null)
            audioGradas.PlayOneShot(sonidoFlickerLuz);

        float timer = 0f;
        while (timer < duracion)
        {
            float freq = Mathf.Lerp(0.15f, 0.03f, timer / duracion);
            yield return new WaitForSeconds(freq);
            timer += freq;

            foreach (Light luz in lucesGradas)
            {
                if (luz == null) continue;
                luz.enabled = !luz.enabled;
                if (luz.enabled)
                    luz.intensity = Random.Range(0.1f, 1.5f);
            }
        }

        RestaurarLuces();
    }

    // ===========================================
    //  SILUETA TERRORIFICA FUGAZ
    // ===========================================

    IEnumerator MostrarSiluetaFugaz()
    {
        if (siluetaFantasma == null) yield break;

        Transform posSilueta = null;
        if (posicionesSilueta != null && posicionesSilueta.Length > 0)
            posSilueta = posicionesSilueta[Random.Range(0, posicionesSilueta.Length)];

        if (posSilueta != null)
        {
            siluetaFantasma.transform.position = posSilueta.position;
            siluetaFantasma.transform.rotation = posSilueta.rotation;
        }

        foreach (Light luz in lucesGradas)
            if (luz != null) luz.enabled = false;

        yield return new WaitForSeconds(0.1f);

        siluetaFantasma.SetActive(true);

        foreach (Light luz in lucesGradas)
        {
            if (luz == null) continue;
            luz.enabled = true;
            luz.intensity = 0.3f;
        }

        if (ControladorSentidosVR.Instance != null)
            ControladorSentidosVR.Instance.PulsoUnico(0.9f, 0.3f);

        yield return new WaitForSeconds(tiempoSiluetaVisible);

        foreach (Light luz in lucesGradas)
            if (luz != null) luz.enabled = false;

        yield return new WaitForSeconds(0.15f);

        siluetaFantasma.SetActive(false);
        RestaurarLuces();
    }

    // ===========================================
    //  TELETRANSPORTE SUAVE (Fade negro)
    // ===========================================

    IEnumerator TeletransporteSuave(Transform destino)
    {
        if (destino == null) yield break;

        yield return StartCoroutine(Fade(0f, 1f, fadeOutDuracion));

        Transform jugador = Camera.main != null ? Camera.main.transform.parent : null;
        if (jugador == null) jugador = Camera.main != null ? Camera.main.transform : null;

        if (jugador != null)
        {
            CharacterController cc = jugador.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 offset = jugador.position - Camera.main.transform.position;
            offset.y = 0;
            jugador.position = destino.position + offset;

            if (cc != null) cc.enabled = true;
        }

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(Fade(1f, 0f, fadeInDuracion));
    }

    IEnumerator Fade(float desde, float hasta, float duracion)
    {
        if (panelFadeNegro == null) yield break;

        panelFadeNegro.gameObject.SetActive(true);
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            panelFadeNegro.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        panelFadeNegro.alpha = hasta;

        if (hasta <= 0.01f)
            panelFadeNegro.gameObject.SetActive(false);
    }

    // ===========================================
    //  COMPLETAR BUCLE → PISO 3
    // ===========================================

    IEnumerator CompletarBucle()
    {
        bucleActivo = false;
        bucleCompletado = true;

        if (textoCartelPiso != null)
            textoCartelPiso.text = "PISO 3";

        yield return StartCoroutine(TeletransporteSuave(puntoLlegadaPiso3));
        DesactivarTriggers();
        OnBucleTerminado?.Invoke();

        Debug.Log("[TrampaGradas] Bucle completado. Jugador llega al Piso 3.");
    }

    // ===========================================
    //  UTILIDADES
    // ===========================================

    ConfigVueltaGradas GetConfigVuelta(int vuelta)
    {
        switch (vuelta)
        {
            case 1: return vuelta1;
            case 2: return vuelta2;
            default: return vuelta3;
        }
    }

    void ActivarTriggers()
    {
        if (triggerDescansoInferior != null) triggerDescansoInferior.enabled = true;
        if (triggerDescansoMedio != null) triggerDescansoMedio.enabled = true;
        if (triggerDescansoSuperior != null) triggerDescansoSuperior.enabled = true;
    }

    void DesactivarTriggers()
    {
        if (triggerDescansoInferior != null) triggerDescansoInferior.enabled = false;
        if (triggerDescansoMedio != null) triggerDescansoMedio.enabled = false;
        if (triggerDescansoSuperior != null) triggerDescansoSuperior.enabled = false;
    }

    void GuardarIntensidadesLuces()
    {
        if (lucesGradas == null) return;
        intensidadesLucesOriginales = new float[lucesGradas.Length];
        for (int i = 0; i < lucesGradas.Length; i++)
            if (lucesGradas[i] != null)
                intensidadesLucesOriginales[i] = lucesGradas[i].intensity;
    }

    void RestaurarLuces()
    {
        if (lucesGradas == null || intensidadesLucesOriginales == null) return;
        for (int i = 0; i < lucesGradas.Length; i++)
        {
            if (lucesGradas[i] == null) continue;
            lucesGradas[i].enabled = true;
            lucesGradas[i].intensity = intensidadesLucesOriginales[i];
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}