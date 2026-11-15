using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public enum SentidoVR
{
    Vista,
    Oido,
    Tacto,
    Olfato,
    Gusto
}

[System.Serializable]
public class ConfigSentido
{
    public SentidoVR tipo;
    public bool activo = true;
    [Range(0f, 1f)] public float intensidad = 0.5f;
}

public class ControladorSentidosVR : MonoBehaviour
{
    public static ControladorSentidosVR Instance { get; private set; }

    // =====================
    // VISTA — Parpadeo + Vineta de Confusion
    // =====================
    [Header("=== VISTA: Parpadeo Luces ===")]
    [SerializeField] private Light[] lucesControladas;
    [SerializeField] private float parpadeoMinFreq = 0.03f;
    [SerializeField] private float parpadeoMaxFreq = 0.3f;
    [SerializeField] private Color colorLuzNormal = new Color(1f, 0.95f, 0.8f);
    [SerializeField] private Color colorLuzError = new Color(0.9f, 0.1f, 0.05f);

    [Header("=== VISTA: Vineta Confusion ===")]
    [SerializeField] private Material vinetaConfusionMat;
    [SerializeField] private float vinetaMaxIntensidad = 0.9f;
    [SerializeField] private float vinetaVelocidadCambio = 0.4f;
    [SerializeField] private float distorsionCromaticaMax = 0.04f;

    // =====================
    // OIDO — Pasos y Susurros en Gradas
    // =====================
    [Header("=== OIDO: Pasos Fantasma ===")]
    [SerializeField] private AudioClip[] clipsPasosFantasma;
    [SerializeField] private AudioClip[] clipsSusurrosGradas;
    [SerializeField] private AudioSource audioEspacial3D;
    [SerializeField] private float sonidoMinDelay = 3f;
    [SerializeField] private float sonidoMaxDelay = 9f;
    [SerializeField] private float distanciaSonido = 4f;

    // =====================
    // TACTO — Pulso Cardiaco Haptico Intenso
    // =====================
    [Header("=== TACTO: Pulso Cardiaco ===")]
    [SerializeField] private AudioSource latidoSource;
    [SerializeField] private AudioClip latidoClip;
    [SerializeField] private float pulsoCalmaInterval = 1.1f;
    [SerializeField] private float pulsoPanicoInterval = 0.22f;
    [SerializeField] private float hapticaMinAmplitud = 0.08f;
    [SerializeField] private float hapticaMaxAmplitud = 1f;

    // =====================
    // OLFATO — Humo de Servidor Quemado
    // =====================
    [Header("=== OLFATO: Humo Servidor ===")]
    [SerializeField] private ParticleSystem humoServidor;
    [SerializeField] private float humoEmisionMin = 3f;
    [SerializeField] private float humoEmisionMax = 60f;

    // =====================
    // GUSTO — Tos / Aliento Frio
    // =====================
    [Header("=== GUSTO: Tos y Aliento ===")]
    [SerializeField] private AudioSource respiracionSource;
    [SerializeField] private AudioClip[] clipsTos;
    [SerializeField] private AudioClip clipAlientoFrio;
    [SerializeField] private ParticleSystem particulasAliento;
    [SerializeField] private float tosMinDelay = 6f;
    [SerializeField] private float tosMaxDelay = 18f;

    // =====================
    // CONFIGURACION
    // =====================
    [Header("=== General ===")]
    [SerializeField] private ConfigSentido[] sentidos = new ConfigSentido[5];
    [SerializeField] private string tagPeligro = "Anomalia";
    [SerializeField] private float radioDeteccion = 15f;

    private Transform jugador;
    private float nivelAlerta;
    private float nivelPanico;

    private float timerSonido;
    private float timerLatido;
    private float timerParpadeo;
    private float timerTos;
    private float vinetaActual;
    private float[] intensidadesOriginales;
    private bool lucesEncendidas = true;

    private InputDevice manoIzq;
    private InputDevice manoDer;
    private bool xrListo;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null) jugador = cam.transform;

        GuardarIntensidadesOriginales();
        InicializarTimers();

        if (audioEspacial3D != null)
        {
            audioEspacial3D.spatialBlend = 1f;
            audioEspacial3D.loop = false;
            audioEspacial3D.maxDistance = distanciaSonido * 2f;
        }
        if (respiracionSource != null)
        {
            respiracionSource.spatialBlend = 0f;
            respiracionSource.loop = false;
        }
    }

    void Update()
    {
        if (jugador == null) return;

        ObtenerXR();
        CalcularAlerta();

        if (SentidoActivo(SentidoVR.Vista)) ProcesarVista();
        if (SentidoActivo(SentidoVR.Oido)) ProcesarOido();
        if (SentidoActivo(SentidoVR.Tacto)) ProcesarTacto();
        if (SentidoActivo(SentidoVR.Olfato)) ProcesarOlfato();
        if (SentidoActivo(SentidoVR.Gusto)) ProcesarGusto();
    }

    // ===========================================
    //  DETECCION
    // ===========================================

    void CalcularAlerta()
    {
        float distMin = radioDeteccion;
        GameObject[] peligros = GameObject.FindGameObjectsWithTag(tagPeligro);
        foreach (GameObject p in peligros)
        {
            float d = Vector3.Distance(jugador.position, p.transform.position);
            if (d < distMin) distMin = d;
        }
        float proximidad = 1f - Mathf.Clamp01(distMin / radioDeteccion);
        nivelAlerta = Mathf.Lerp(nivelAlerta, proximidad, Time.deltaTime * 2f);
        nivelPanico = Mathf.Clamp01(nivelAlerta * 1.6f - 0.25f);
    }

    public void ForzarAlerta(float valor)
    {
        nivelAlerta = Mathf.Clamp01(valor);
        nivelPanico = Mathf.Clamp01(valor * 1.6f - 0.25f);
    }

    // ===========================================
    //  VISTA: Parpadeo + Vineta de Confusion
    // ===========================================

    void ProcesarVista()
    {
        float intensidad = GetIntensidad(SentidoVR.Vista);
        ProcesarParpadeo(intensidad);
        ProcesarVineta(intensidad);
    }

    void ProcesarParpadeo(float intensidad)
    {
        if (lucesControladas == null || lucesControladas.Length == 0) return;

        timerParpadeo -= Time.deltaTime;
        if (timerParpadeo > 0f) return;

        float freq = Mathf.Lerp(parpadeoMaxFreq, parpadeoMinFreq, nivelAlerta * intensidad);
        timerParpadeo = Random.Range(freq * 0.4f, freq);
        lucesEncendidas = !lucesEncendidas;

        for (int i = 0; i < lucesControladas.Length; i++)
        {
            if (lucesControladas[i] == null) continue;

            if (nivelAlerta > 0.4f && Random.value < nivelAlerta)
            {
                lucesControladas[i].color = lucesEncendidas ? colorLuzError : colorLuzNormal;
                lucesControladas[i].intensity = intensidadesOriginales[i] * Random.Range(0.05f, 1.1f);
            }
            else
            {
                lucesControladas[i].color = colorLuzNormal;
                lucesControladas[i].intensity = intensidadesOriginales[i];
            }
        }
    }

    void ProcesarVineta(float intensidad)
    {
        if (vinetaConfusionMat == null) return;

        float objetivo = nivelAlerta * vinetaMaxIntensidad * intensidad;
        vinetaActual = Mathf.Lerp(vinetaActual, objetivo, Time.deltaTime * vinetaVelocidadCambio);

        vinetaConfusionMat.SetFloat("_VignetteIntensity", vinetaActual);
        vinetaConfusionMat.SetFloat("_VignetteRadius", Mathf.Lerp(1f, 0.25f, vinetaActual));
        vinetaConfusionMat.SetFloat("_ChromaticAberration", vinetaActual * distorsionCromaticaMax);

        float pulsoConfusion = Mathf.Sin(Time.time * 3f * nivelPanico) * 0.05f * nivelPanico;
        vinetaConfusionMat.SetFloat("_ConfusionPulse", pulsoConfusion);
    }

    // ===========================================
    //  OIDO: Pasos Fantasma + Susurros en Gradas
    // ===========================================

    void ProcesarOido()
    {
        float intensidad = GetIntensidad(SentidoVR.Oido);

        timerSonido -= Time.deltaTime;
        if (timerSonido > 0f) return;

        float delay = Mathf.Lerp(sonidoMaxDelay, sonidoMinDelay, nivelAlerta);
        timerSonido = Random.Range(delay * 0.6f, delay * 1.3f);

        bool usarSusurros = nivelAlerta > 0.5f && clipsSusurrosGradas != null && clipsSusurrosGradas.Length > 0;
        AudioClip[] banco = usarSusurros ? clipsSusurrosGradas : clipsPasosFantasma;
        if (banco == null || banco.Length == 0) return;
        if (audioEspacial3D == null) return;

        Vector3 posDetras = CalcularPosDetras();
        audioEspacial3D.transform.position = posDetras;

        AudioClip clip = banco[Random.Range(0, banco.Length)];
        audioEspacial3D.volume = Mathf.Lerp(0.1f, 0.55f, nivelAlerta) * intensidad;
        audioEspacial3D.pitch = Random.Range(0.8f, 1.2f);
        audioEspacial3D.PlayOneShot(clip);
    }

    Vector3 CalcularPosDetras()
    {
        float angulo = Random.Range(120f, 240f);
        float rad = angulo * Mathf.Deg2Rad;
        Vector3 dirLocal = new Vector3(Mathf.Sin(rad), Random.Range(-0.4f, 0.6f), Mathf.Cos(rad));
        return jugador.position + jugador.TransformDirection(dirLocal) * distanciaSonido;
    }

    public void ReproducirSusurroEn(Vector3 posicion)
    {
        if (audioEspacial3D == null) return;
        if (clipsSusurrosGradas == null || clipsSusurrosGradas.Length == 0) return;

        audioEspacial3D.transform.position = posicion;
        AudioClip clip = clipsSusurrosGradas[Random.Range(0, clipsSusurrosGradas.Length)];
        audioEspacial3D.volume = 0.5f;
        audioEspacial3D.PlayOneShot(clip);
    }

    public void ReproducirPasosEn(Vector3 posicion)
    {
        if (audioEspacial3D == null) return;
        if (clipsPasosFantasma == null || clipsPasosFantasma.Length == 0) return;

        audioEspacial3D.transform.position = posicion;
        AudioClip clip = clipsPasosFantasma[Random.Range(0, clipsPasosFantasma.Length)];
        audioEspacial3D.volume = 0.4f;
        audioEspacial3D.PlayOneShot(clip);
    }

    // ===========================================
    //  TACTO: Pulso Cardiaco Haptico Intenso
    // ===========================================

    void ProcesarTacto()
    {
        if (nivelAlerta < 0.05f) return;
        float intensidad = GetIntensidad(SentidoVR.Tacto);

        timerLatido -= Time.deltaTime;
        if (timerLatido > 0f) return;

        float intervalo = Mathf.Lerp(pulsoCalmaInterval, pulsoPanicoInterval, nivelPanico);
        timerLatido = intervalo;

        float amplitud = Mathf.Lerp(hapticaMinAmplitud, hapticaMaxAmplitud, nivelPanico) * intensidad;

        if (manoIzq.isValid)
            manoIzq.SendHapticImpulse(0, amplitud, 0.15f);
        if (manoDer.isValid)
            manoDer.SendHapticImpulse(0, amplitud * 0.6f, 0.1f);

        if (latidoSource != null && latidoClip != null)
        {
            latidoSource.pitch = Mathf.Lerp(0.85f, 1.5f, nivelPanico);
            latidoSource.volume = Mathf.Lerp(0.15f, 0.8f, nivelPanico) * intensidad;
            latidoSource.PlayOneShot(latidoClip);
        }

        StartCoroutine(SegundoGolpeCardiaco(amplitud * 0.45f, intervalo * 0.28f));
    }

    IEnumerator SegundoGolpeCardiaco(float amplitud, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (manoIzq.isValid)
            manoIzq.SendHapticImpulse(0, amplitud, 0.1f);
        if (manoDer.isValid)
            manoDer.SendHapticImpulse(0, amplitud * 0.6f, 0.07f);
    }

    public void PulsoUnico(float amplitud, float duracion)
    {
        if (manoIzq.isValid) manoIzq.SendHapticImpulse(0, amplitud, duracion);
        if (manoDer.isValid) manoDer.SendHapticImpulse(0, amplitud, duracion);
    }

    // ===========================================
    //  OLFATO: Humo de Servidor Quemado
    // ===========================================

    void ProcesarOlfato()
    {
        if (humoServidor == null) return;
        float intensidad = GetIntensidad(SentidoVR.Olfato);

        var emision = humoServidor.emission;
        float rate = Mathf.Lerp(humoEmisionMin, humoEmisionMax, nivelAlerta * intensidad);
        emision.rateOverTime = rate;

        var main = humoServidor.main;
        main.startColor = Color.Lerp(
            new Color(0.35f, 0.35f, 0.35f, 0.25f),
            new Color(0.12f, 0.08f, 0.04f, 0.75f),
            nivelAlerta
        );

        if (!humoServidor.isPlaying && nivelAlerta > 0.1f)
            humoServidor.Play();
        else if (humoServidor.isPlaying && nivelAlerta < 0.05f)
            humoServidor.Stop();
    }

    public void ActivarHumo(bool activo)
    {
        if (humoServidor == null) return;
        if (activo && !humoServidor.isPlaying) humoServidor.Play();
        else if (!activo && humoServidor.isPlaying) humoServidor.Stop();
    }

    // ===========================================
    //  GUSTO: Tos + Aliento Frio
    // ===========================================

    void ProcesarGusto()
    {
        float intensidad = GetIntensidad(SentidoVR.Gusto);

        if (respiracionSource != null && clipAlientoFrio != null)
        {
            if (nivelAlerta > 0.35f && !respiracionSource.isPlaying)
            {
                respiracionSource.clip = clipAlientoFrio;
                respiracionSource.loop = true;
                respiracionSource.volume = 0f;
                respiracionSource.Play();
            }

            if (respiracionSource.isPlaying)
            {
                float volObj = nivelAlerta > 0.35f
                    ? Mathf.Lerp(0.08f, 0.45f, nivelPanico) * intensidad
                    : 0f;
                respiracionSource.volume = Mathf.Lerp(respiracionSource.volume, volObj, Time.deltaTime * 2f);
                respiracionSource.pitch = Mathf.Lerp(0.9f, 1.35f, nivelPanico);

                if (respiracionSource.volume < 0.01f && nivelAlerta < 0.2f)
                    respiracionSource.Stop();
            }
        }

        if (particulasAliento != null)
        {
            if (nivelAlerta > 0.5f && !particulasAliento.isPlaying)
                particulasAliento.Play();
            else if (nivelAlerta < 0.3f && particulasAliento.isPlaying)
                particulasAliento.Stop();
        }

        if (clipsTos != null && clipsTos.Length > 0 && nivelAlerta > 0.45f)
        {
            timerTos -= Time.deltaTime;
            if (timerTos <= 0f)
            {
                float delay = Mathf.Lerp(tosMaxDelay, tosMinDelay, nivelPanico);
                timerTos = Random.Range(delay * 0.7f, delay * 1.3f);

                if (respiracionSource != null)
                {
                    AudioClip tos = clipsTos[Random.Range(0, clipsTos.Length)];
                    respiracionSource.PlayOneShot(tos, Mathf.Lerp(0.3f, 0.8f, nivelPanico) * intensidad);
                }

                PulsoUnico(0.4f, 0.2f);
            }
        }
    }

    // ===========================================
    //  EMERGENCIA TOTAL (climax narrativo)
    // ===========================================

    public void DisparoDeEmergencia()
    {
        nivelAlerta = 1f;
        nivelPanico = 1f;

        PulsoUnico(1f, 0.6f);

        foreach (Light luz in lucesControladas)
        {
            if (luz != null) luz.color = colorLuzError;
        }

        if (humoServidor != null) humoServidor.Play();
        if (particulasAliento != null) particulasAliento.Play();
    }

    // ===========================================
    //  API PUBLICA
    // ===========================================

    public void SetIntensidadSentido(SentidoVR tipo, float valor)
    {
        foreach (ConfigSentido s in sentidos)
            if (s.tipo == tipo) { s.intensidad = Mathf.Clamp01(valor); return; }
    }

    public void ActivarSentido(SentidoVR tipo, bool activo)
    {
        foreach (ConfigSentido s in sentidos)
            if (s.tipo == tipo) { s.activo = activo; return; }
    }

    public float GetNivelAlerta() => nivelAlerta;
    public float GetNivelPanico() => nivelPanico;

    // ===========================================
    //  UTILIDADES INTERNAS
    // ===========================================

    void ObtenerXR()
    {
        if (xrListo) return;
        manoIzq = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        manoDer = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (manoIzq.isValid && manoDer.isValid) xrListo = true;
    }

    void GuardarIntensidadesOriginales()
    {
        if (lucesControladas == null) return;
        intensidadesOriginales = new float[lucesControladas.Length];
        for (int i = 0; i < lucesControladas.Length; i++)
            if (lucesControladas[i] != null)
                intensidadesOriginales[i] = lucesControladas[i].intensity;
    }

    void InicializarTimers()
    {
        timerSonido = Random.Range(sonidoMinDelay, sonidoMaxDelay);
        timerLatido = pulsoCalmaInterval;
        timerTos = Random.Range(tosMinDelay, tosMaxDelay);
    }

    bool SentidoActivo(SentidoVR tipo)
    {
        foreach (ConfigSentido s in sentidos)
            if (s.tipo == tipo) return s.activo;
        return true;
    }

    float GetIntensidad(SentidoVR tipo)
    {
        foreach (ConfigSentido s in sentidos)
            if (s.tipo == tipo) return s.intensidad;
        return 0.5f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}