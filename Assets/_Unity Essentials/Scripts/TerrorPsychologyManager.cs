using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

public class TerrorPsychologyManager : MonoBehaviour
{
    public static TerrorPsychologyManager Instance { get; private set; }

    [Header("--- LATIDOS Y HAPTICA ---")]
    [SerializeField] private AudioSource latidoSource;
    [SerializeField] private AudioClip latidoClip;
    [SerializeField] private AudioClip latidoFuerteClip;
    [SerializeField] private float latidoMinInterval = 1.2f;
    [SerializeField] private float latidoMaxInterval = 0.3f;
    [SerializeField] private float hapticaMinAmplitud = 0.05f;
    [SerializeField] private float hapticaMaxAmplitud = 0.8f;
    [SerializeField] private float hapticaDuracion = 0.15f;
    [SerializeField] private float radioDeteccion = 15f;

    [Header("--- SUSURROS ESPACIALES ---")]
    [SerializeField] private AudioClip[] susurrosClips;
    [SerializeField] private AudioClip[] lamentosClips;
    [SerializeField] private float susurroMinDelay = 4f;
    [SerializeField] private float susurroMaxDelay = 12f;
    [SerializeField] private float susurroDistancia = 3f;
    [SerializeField] private float susurroVolumen = 0.35f;
    [SerializeField] private AudioSource susurroSource;

    [Header("--- CORDURA VISUAL (URP Volume) ---")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Light[] lucesControladas;
    [SerializeField] private float parpadeoMinInterval = 0.05f;
    [SerializeField] private float parpadeoMaxInterval = 0.4f;
    [SerializeField] [Range(0f, 1f)] private float cordura = 1f;
    [SerializeField] private float corduraPerdidaPorSegundo = 0.01f;
    [SerializeField] private float corduraRecuperacionPorSegundo = 0.02f;

    [Header("--- EFECTOS DE CORDURA (Rangos) ---")]
    [SerializeField] private float vignetteMaxIntensity = 0.6f;
    [SerializeField] private float chromaticMaxIntensity = 1f;
    [SerializeField] private float filmGrainMaxIntensity = 0.8f;
    [SerializeField] private float desaturacionMax = -80f;
    [SerializeField] private float lensDistortionMax = -0.3f;

    [Header("--- ANOMALIAS ---")]
    [SerializeField] private string tagAnomalia = "Anomalia";

    [Header("--- SONIDOS IMPACTO ---")]
    [SerializeField] private AudioClip[] sonidosImpacto;
    [SerializeField] private AudioSource impactoSource;

    private Transform jugador;
    private float proximidadNormalizada;
    private float timerLatido;
    private float timerSusurro;
    private float timerParpadeo;
    private float timerPulsoVisual;
    private bool lucesEncendidas = true;
    private bool enZonaTerror;
    private float[] intensidadesOriginales;
    private Color[] coloresOriginales;

    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;
    private ColorAdjustments colorAdjustments;
    private LensDistortion lensDistortion;

    private InputDevice manoIzquierda;
    private InputDevice manoDerecha;
    private bool dispositivosObtenidos;

    private float pulsoVisualActual;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Camera camaraJugador = Camera.main;
        if (camaraJugador != null)
            jugador = camaraJugador.transform;

        GuardarEstadoOriginalLuces();
        ObtenerComponentesVolume();
        timerSusurro = Random.Range(susurroMinDelay, susurroMaxDelay);

        if (latidoSource != null)
        {
            latidoSource.spatialBlend = 0f;
            latidoSource.loop = false;
        }

        if (susurroSource != null)
        {
            susurroSource.spatialBlend = 1f;
            susurroSource.loop = false;
            susurroSource.volume = susurroVolumen;
            susurroSource.maxDistance = susurroDistancia * 2f;
        }
    }

    void ObtenerComponentesVolume()
    {
        if (postProcessVolume == null) return;

        VolumeProfile profile = postProcessVolume.profile;
        profile.TryGet(out vignette);
        profile.TryGet(out chromaticAberration);
        profile.TryGet(out filmGrain);
        profile.TryGet(out colorAdjustments);
        profile.TryGet(out lensDistortion);
    }

    void Update()
    {
        if (jugador == null) return;

        ObtenerDispositivos();
        CalcularProximidad();
        ActualizarLatidos();
        ActualizarSusurros();
        ActualizarCordura();
        ActualizarParpadeoLuces();
        ActualizarPostProcesadoURP();
        ActualizarIntegracionAudio();
    }

    // ============================
    // DISPOSITIVOS XR
    // ============================

    void ObtenerDispositivos()
    {
        if (dispositivosObtenidos) return;

        manoIzquierda = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        manoDerecha = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (manoIzquierda.isValid && manoDerecha.isValid)
            dispositivosObtenidos = true;
    }

    // ============================
    // PROXIMIDAD Y ANOMALIAS
    // ============================

    void CalcularProximidad()
    {
        float distanciaMinima = radioDeteccion;
        GameObject[] anomalias = GameObject.FindGameObjectsWithTag(tagAnomalia);

        foreach (GameObject a in anomalias)
        {
            float dist = Vector3.Distance(jugador.position, a.transform.position);
            if (dist < distanciaMinima)
                distanciaMinima = dist;
        }

        float targetProximidad = 1f - Mathf.Clamp01(distanciaMinima / radioDeteccion);
        proximidadNormalizada = Mathf.MoveTowards(proximidadNormalizada, targetProximidad, Time.deltaTime * 2f);
        enZonaTerror = proximidadNormalizada > 0.01f;
    }

    // ============================
    // SISTEMA 1: LATIDOS + HAPTICA
    // ============================

    void ActualizarLatidos()
    {
        if (!enZonaTerror) return;

        timerLatido -= Time.deltaTime;
        if (timerLatido > 0f) return;

        float intervalo = Mathf.Lerp(latidoMinInterval, latidoMaxInterval, proximidadNormalizada);
        timerLatido = intervalo;

        bool usarFuerte = proximidadNormalizada > 0.6f && latidoFuerteClip != null;
        AudioClip clipActual = usarFuerte ? latidoFuerteClip : latidoClip;

        if (latidoSource != null && clipActual != null)
        {
            latidoSource.pitch = Mathf.Lerp(0.8f, 1.3f, proximidadNormalizada);
            latidoSource.volume = Mathf.Lerp(0.2f, 1f, proximidadNormalizada);
            latidoSource.PlayOneShot(clipActual);
        }

        float amplitud = Mathf.Lerp(hapticaMinAmplitud, hapticaMaxAmplitud, proximidadNormalizada);
        StartCoroutine(HapticaDoble(amplitud));
    }

    IEnumerator HapticaDoble(float amplitud)
    {
        EnviarHaptica(amplitud);
        yield return new WaitForSeconds(0.12f);
        EnviarHaptica(amplitud * 0.6f);
    }

    void EnviarHaptica(float amplitud)
    {
        if (manoIzquierda.isValid)
            manoIzquierda.SendHapticImpulse(0, amplitud, hapticaDuracion);
        if (manoDerecha.isValid)
            manoDerecha.SendHapticImpulse(0, amplitud, hapticaDuracion);
    }

    // ====================================
    // SISTEMA 2: SUSURROS FUERA DE VISION
    // ====================================

    void ActualizarSusurros()
    {
        if (susurroSource == null) return;

        bool tieneSusurros = susurrosClips != null && susurrosClips.Length > 0;
        bool tieneLamentos = lamentosClips != null && lamentosClips.Length > 0;
        if (!tieneSusurros && !tieneLamentos) return;

        timerSusurro -= Time.deltaTime;
        if (timerSusurro > 0f) return;

        float delayBase = Mathf.Lerp(susurroMaxDelay, susurroMinDelay, proximidadNormalizada);
        timerSusurro = Random.Range(delayBase * 0.7f, delayBase * 1.3f);

        Vector3 posicionSusurro = CalcularPosicionFueraDeFOV();
        susurroSource.transform.position = posicionSusurro;

        bool usarLamento = tieneLamentos && cordura < 0.5f && Random.value < 0.4f;
        AudioClip[] banco = usarLamento ? lamentosClips : susurrosClips;
        if (banco == null || banco.Length == 0) return;

        AudioClip clip = banco[Random.Range(0, banco.Length)];
        susurroSource.volume = Mathf.Lerp(susurroVolumen * 0.5f, susurroVolumen, proximidadNormalizada + 0.3f);
        susurroSource.pitch = Random.Range(0.7f, 1.1f);
        susurroSource.PlayOneShot(clip);
    }

    Vector3 CalcularPosicionFueraDeFOV()
    {
        float angulo = Random.Range(110f, 250f);
        float anguloRad = angulo * Mathf.Deg2Rad;

        Vector3 direccionLocal = new Vector3(Mathf.Sin(anguloRad), 0f, Mathf.Cos(anguloRad));
        Vector3 direccionMundo = jugador.TransformDirection(direccionLocal);

        float alturaOffset = Random.Range(-0.5f, 0.8f);
        Vector3 posicion = jugador.position + direccionMundo * susurroDistancia;
        posicion.y += alturaOffset;

        return posicion;
    }

    // =======================================
    // SISTEMA 3: POST-PROCESADO URP
    // =======================================

    void ActualizarCordura()
    {
        if (enZonaTerror)
            cordura -= corduraPerdidaPorSegundo * proximidadNormalizada * Time.deltaTime;
        else
            cordura += corduraRecuperacionPorSegundo * Time.deltaTime;

        cordura = Mathf.Clamp01(cordura);
    }

    void ActualizarPostProcesadoURP()
    {
        float efecto = Mathf.Max(proximidadNormalizada, 1f - cordura);

        timerPulsoVisual += Time.deltaTime;
        float pulso = Mathf.Sin(timerPulsoVisual * 3f) * 0.1f * efecto;
        float efectoConPulso = Mathf.Clamp01(efecto + pulso);

        if (vignette != null)
        {
            vignette.active = efectoConPulso > 0.02f;
            vignette.intensity.value = Mathf.Lerp(0.25f, vignetteMaxIntensity, efectoConPulso);
            vignette.color.value = Color.Lerp(Color.black, new Color(0.15f, 0f, 0f), efectoConPulso);
            vignette.smoothness.value = Mathf.Lerp(0.2f, 0.6f, efectoConPulso);
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.active = efectoConPulso > 0.15f;
            chromaticAberration.intensity.value = Mathf.Lerp(0f, chromaticMaxIntensity, Mathf.Pow(efectoConPulso, 1.5f));
        }

        if (filmGrain != null)
        {
            filmGrain.active = efectoConPulso > 0.1f;
            filmGrain.intensity.value = Mathf.Lerp(0f, filmGrainMaxIntensity, efectoConPulso);
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.active = efectoConPulso > 0.05f;
            colorAdjustments.saturation.value = Mathf.Lerp(0f, desaturacionMax, efectoConPulso);
            colorAdjustments.contrast.value = Mathf.Lerp(0f, 30f, efectoConPulso);
            colorAdjustments.postExposure.value = Mathf.Lerp(0f, -0.5f, efectoConPulso);
        }

        if (lensDistortion != null)
        {
            lensDistortion.active = efectoConPulso > 0.3f;
            float distPulso = Mathf.Sin(timerPulsoVisual * 5f) * 0.05f * efectoConPulso;
            lensDistortion.intensity.value = Mathf.Lerp(0f, lensDistortionMax, efectoConPulso) + distPulso;
        }
    }

    // =======================================
    // SISTEMA 4: PARPADEO REALISTA DE LUCES
    // =======================================

    void ActualizarParpadeoLuces()
    {
        if (lucesControladas == null || lucesControladas.Length == 0) return;

        float intensidadTerror = Mathf.Max(proximidadNormalizada, 1f - cordura);

        timerParpadeo -= Time.deltaTime;
        if (timerParpadeo > 0f) return;

        float intervalo = Mathf.Lerp(parpadeoMaxInterval, parpadeoMinInterval, intensidadTerror);
        timerParpadeo = Random.Range(intervalo * 0.5f, intervalo);

        if (intensidadTerror < 0.1f)
        {
            RestaurarLuces();
            return;
        }

        for (int i = 0; i < lucesControladas.Length; i++)
        {
            if (lucesControladas[i] == null) continue;

            float patron = ElegirPatronFlicker(intensidadTerror);
            lucesControladas[i].intensity = intensidadesOriginales[i] * patron;

            float colorShift = intensidadTerror * 0.15f;
            lucesControladas[i].color = Color.Lerp(coloresOriginales[i],
                new Color(coloresOriginales[i].r + colorShift, coloresOriginales[i].g - colorShift * 0.5f, coloresOriginales[i].b - colorShift),
                intensidadTerror);
        }
    }

    float ElegirPatronFlicker(float intensidad)
    {
        float r = Random.value;

        if (intensidad > 0.8f)
        {
            // Apagon casi total con destellos
            if (r < 0.3f) return Random.Range(0f, 0.05f);
            if (r < 0.5f) return Random.Range(0.8f, 1.2f);
            return Random.Range(0.02f, 0.15f);
        }
        else if (intensidad > 0.5f)
        {
            // Parpadeo agresivo
            if (r < 0.2f) return Random.Range(0f, 0.1f);
            if (r < 0.6f) return Random.Range(0.3f, 0.7f);
            return Random.Range(0.7f, 1f);
        }
        else if (intensidad > 0.2f)
        {
            // Fluctuacion sutil
            return Random.Range(0.6f, 1.1f);
        }

        return Random.Range(0.85f, 1.05f);
    }

    // =======================================
    // INTEGRACION CON AUDIO AMBIENTAL
    // =======================================

    void ActualizarIntegracionAudio()
    {
        if (AmbientSoundSystem.Instance != null)
        {
            AmbientSoundSystem.Instance.EstablecerTension(Mathf.Max(proximidadNormalizada, 1f - cordura));
        }
    }

    // ============================
    // UTILIDADES
    // ============================

    void GuardarEstadoOriginalLuces()
    {
        if (lucesControladas == null) return;
        intensidadesOriginales = new float[lucesControladas.Length];
        coloresOriginales = new Color[lucesControladas.Length];
        for (int i = 0; i < lucesControladas.Length; i++)
        {
            if (lucesControladas[i] != null)
            {
                intensidadesOriginales[i] = lucesControladas[i].intensity;
                coloresOriginales[i] = lucesControladas[i].color;
            }
        }
    }

    void RestaurarLuces()
    {
        if (lucesControladas == null || intensidadesOriginales == null) return;
        for (int i = 0; i < lucesControladas.Length; i++)
        {
            if (lucesControladas[i] != null)
            {
                lucesControladas[i].intensity = Mathf.MoveTowards(
                    lucesControladas[i].intensity, intensidadesOriginales[i], Time.deltaTime * 3f);
                lucesControladas[i].color = Color.Lerp(lucesControladas[i].color, coloresOriginales[i], Time.deltaTime * 2f);
            }
        }
        lucesEncendidas = true;
    }

    void ResetearPostProcesado()
    {
        if (vignette != null) { vignette.intensity.value = 0.25f; vignette.active = false; }
        if (chromaticAberration != null) { chromaticAberration.intensity.value = 0f; chromaticAberration.active = false; }
        if (filmGrain != null) { filmGrain.intensity.value = 0f; filmGrain.active = false; }
        if (colorAdjustments != null) { colorAdjustments.saturation.value = 0f; colorAdjustments.active = false; }
        if (lensDistortion != null) { lensDistortion.intensity.value = 0f; lensDistortion.active = false; }
    }

    // === API Publica ===

    public float GetProximidad() => proximidadNormalizada;
    public float GetCordura() => cordura;
    public bool EstaEnZonaTerror() => enZonaTerror;

    public void ForzarPerdidaCordura(float cantidad)
    {
        cordura = Mathf.Clamp01(cordura - cantidad);
    }

    public void RestaurarCorduraCompleta()
    {
        cordura = 1f;
        RestaurarLuces();
    }

    public void PulsoTerror(float intensidad = 0.3f, float duracion = 0.5f)
    {
        StartCoroutine(PulsoTerrorCoroutine(intensidad, duracion));
    }

    IEnumerator PulsoTerrorCoroutine(float intensidad, float duracion)
    {
        float corduraPrevia = cordura;
        cordura = Mathf.Clamp01(cordura - intensidad);

        if (impactoSource != null && sonidosImpacto != null && sonidosImpacto.Length > 0)
        {
            AudioClip clip = sonidosImpacto[Random.Range(0, sonidosImpacto.Length)];
            impactoSource.PlayOneShot(clip, intensidad);
        }

        float ampHaptica = Mathf.Lerp(0.3f, 1f, intensidad);
        EnviarHaptica(ampHaptica);

        yield return new WaitForSeconds(duracion);
        cordura = Mathf.Clamp01(cordura + intensidad * 0.5f);
    }

    void OnDestroy()
    {
        ResetearPostProcesado();
        RestaurarLuces();
        if (Instance == this) Instance = null;
    }
}
