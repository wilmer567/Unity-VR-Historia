using System.Collections;
using UnityEngine;

public class AmbientSoundSystem : MonoBehaviour
{
    public static AmbientSoundSystem Instance { get; private set; }

    [Header("=== Capas de Audio Ambiental ===")]
    [SerializeField] private AudioSource capaBase;
    [SerializeField] private AudioSource capaTension;
    [SerializeField] private AudioSource capaDetalles;
    [SerializeField] private AudioSource capaRespiracion;

    [Header("Clips Base (loops)")]
    [SerializeField] private AudioClip zumbidoElectrico;
    [SerializeField] private AudioClip vientoLejano;
    [SerializeField] private AudioClip tuberiasAgua;

    [Header("Clips Tension (loops)")]
    [SerializeField] private AudioClip droneOscuro;
    [SerializeField] private AudioClip interferencia;

    [Header("Clips Detalles (one-shot aleatorios)")]
    [SerializeField] private AudioClip[] crujidosMadera;
    [SerializeField] private AudioClip[] gotasAgua;
    [SerializeField] private AudioClip[] metalDistante;
    [SerializeField] private AudioClip[] pasosLejanos;
    [SerializeField] private AudioClip[] puertasChirrido;

    [Header("Respiracion")]
    [SerializeField] private AudioClip respiracionNormal;
    [SerializeField] private AudioClip respiracionAgitada;
    [SerializeField] private AudioClip respiracionPanico;

    [Header("Configuracion")]
    [SerializeField] private float volumenBase = 0.15f;
    [SerializeField] private float volumenTensionMax = 0.4f;
    [SerializeField] private float volumenDetalles = 0.25f;
    [SerializeField] private float volumenRespiracion = 0.2f;
    [SerializeField] private float detalleMinDelay = 3f;
    [SerializeField] private float detalleMaxDelay = 12f;
    [SerializeField] private float crossfadeDuration = 2f;

    private float nivelTension;
    private float nivelTensionTarget;
    private float timerDetalle;
    private AudioClip[][] todosDetalles;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        todosDetalles = new[] { crujidosMadera, gotasAgua, metalDistante, pasosLejanos, puertasChirrido };

        ConfigurarAudioSource(capaBase, true, 0f);
        ConfigurarAudioSource(capaTension, true, 0f);
        ConfigurarAudioSource(capaDetalles, false, 1f);
        ConfigurarAudioSource(capaRespiracion, true, 0f);

        IniciarCapaBase();
        IniciarRespiracion();
        timerDetalle = Random.Range(detalleMinDelay, detalleMaxDelay);
    }

    void Update()
    {
        nivelTension = Mathf.MoveTowards(nivelTension, nivelTensionTarget, Time.deltaTime / crossfadeDuration);

        ActualizarVolumenTension();
        ActualizarRespiracion();
        ActualizarDetalles();
    }

    void ConfigurarAudioSource(AudioSource src, bool loop, float spatialBlend)
    {
        if (src == null) return;
        src.loop = loop;
        src.spatialBlend = spatialBlend;
        src.playOnAwake = false;
        src.volume = 0f;
    }

    void IniciarCapaBase()
    {
        if (capaBase == null) return;

        AudioClip clip = zumbidoElectrico != null ? zumbidoElectrico
                       : vientoLejano != null ? vientoLejano
                       : tuberiasAgua;
        if (clip == null) return;

        capaBase.clip = clip;
        capaBase.volume = volumenBase;
        capaBase.Play();
    }

    void IniciarRespiracion()
    {
        if (capaRespiracion == null || respiracionNormal == null) return;
        capaRespiracion.clip = respiracionNormal;
        capaRespiracion.volume = volumenRespiracion * 0.3f;
        capaRespiracion.Play();
    }

    void ActualizarVolumenTension()
    {
        if (capaTension == null) return;
        capaTension.volume = Mathf.Lerp(0f, volumenTensionMax, nivelTension);

        if (nivelTension > 0.05f && !capaTension.isPlaying)
        {
            AudioClip clip = droneOscuro != null ? droneOscuro : interferencia;
            if (clip != null)
            {
                capaTension.clip = clip;
                capaTension.Play();
            }
        }
        else if (nivelTension <= 0.05f && capaTension.isPlaying)
        {
            capaTension.Stop();
        }
    }

    void ActualizarRespiracion()
    {
        if (capaRespiracion == null) return;

        float volTarget = Mathf.Lerp(volumenRespiracion * 0.3f, volumenRespiracion, nivelTension);
        capaRespiracion.volume = Mathf.MoveTowards(capaRespiracion.volume, volTarget, Time.deltaTime * 0.5f);

        float pitchTarget = Mathf.Lerp(0.9f, 1.4f, nivelTension);
        capaRespiracion.pitch = Mathf.MoveTowards(capaRespiracion.pitch, pitchTarget, Time.deltaTime * 0.3f);

        if (nivelTension > 0.7f && respiracionPanico != null && capaRespiracion.clip != respiracionPanico)
        {
            CambiarClipRespiracion(respiracionPanico);
        }
        else if (nivelTension > 0.3f && nivelTension <= 0.7f && respiracionAgitada != null && capaRespiracion.clip != respiracionAgitada)
        {
            CambiarClipRespiracion(respiracionAgitada);
        }
        else if (nivelTension <= 0.3f && respiracionNormal != null && capaRespiracion.clip != respiracionNormal)
        {
            CambiarClipRespiracion(respiracionNormal);
        }
    }

    void CambiarClipRespiracion(AudioClip nuevoClip)
    {
        if (capaRespiracion == null) return;
        StartCoroutine(CrossfadeClip(capaRespiracion, nuevoClip, 1.5f));
    }

    IEnumerator CrossfadeClip(AudioSource source, AudioClip nuevoClip, float duracion)
    {
        float volOriginal = source.volume;
        float t = 0f;

        while (t < duracion * 0.5f)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(volOriginal, 0f, t / (duracion * 0.5f));
            yield return null;
        }

        source.clip = nuevoClip;
        source.Play();

        t = 0f;
        while (t < duracion * 0.5f)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, volOriginal, t / (duracion * 0.5f));
            yield return null;
        }
    }

    void ActualizarDetalles()
    {
        timerDetalle -= Time.deltaTime;
        if (timerDetalle > 0f) return;

        float delayBase = Mathf.Lerp(detalleMaxDelay, detalleMinDelay, nivelTension);
        timerDetalle = Random.Range(delayBase * 0.6f, delayBase * 1.4f);

        ReproducirDetalleAleatorio();
    }

    void ReproducirDetalleAleatorio()
    {
        if (capaDetalles == null || todosDetalles == null) return;

        AudioClip[] grupoElegido = null;
        int intentos = 0;
        while (grupoElegido == null && intentos < 10)
        {
            var candidato = todosDetalles[Random.Range(0, todosDetalles.Length)];
            if (candidato != null && candidato.Length > 0)
                grupoElegido = candidato;
            intentos++;
        }
        if (grupoElegido == null) return;

        AudioClip clip = grupoElegido[Random.Range(0, grupoElegido.Length)];
        float volumen = volumenDetalles * Random.Range(0.6f, 1f);
        float pitch = Random.Range(0.85f, 1.15f);

        capaDetalles.pitch = pitch;
        capaDetalles.volume = volumen;
        capaDetalles.PlayOneShot(clip);
    }

    // === API Publica ===

    public void EstablecerTension(float valor)
    {
        nivelTensionTarget = Mathf.Clamp01(valor);
    }

    public void CambiarAmbienteBase(AudioClip nuevoClip)
    {
        if (capaBase == null || nuevoClip == null) return;
        StartCoroutine(CrossfadeClip(capaBase, nuevoClip, crossfadeDuration));
    }

    public void ReproducirSonidoEspacial(AudioClip clip, Vector3 posicion, float volumen = 0.5f)
    {
        if (clip == null) return;
        GameObject tempObj = new GameObject("SFX_Temp");
        tempObj.transform.position = posicion;
        AudioSource tempSrc = tempObj.AddComponent<AudioSource>();
        tempSrc.spatialBlend = 1f;
        tempSrc.maxDistance = 15f;
        tempSrc.rolloffMode = AudioRolloffMode.Linear;
        tempSrc.volume = volumen;
        tempSrc.PlayOneShot(clip);
        Destroy(tempObj, clip.length + 0.5f);
    }

    public void PulsoTension(float cantidad, float duracion)
    {
        StartCoroutine(PulsoTensionCoroutine(cantidad, duracion));
    }

    IEnumerator PulsoTensionCoroutine(float cantidad, float duracion)
    {
        float original = nivelTensionTarget;
        nivelTensionTarget = Mathf.Clamp01(original + cantidad);
        yield return new WaitForSeconds(duracion);
        nivelTensionTarget = original;
    }

    public float NivelTension => nivelTension;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
