using System.Collections;
using UnityEngine;

public class AmbienciaAudioSyntro : MonoBehaviour
{
    public static AmbienciaAudioSyntro Instance { get; private set; }

    [Header("Clips de Ambiente")]
    [SerializeField] private AudioClip ambienteMaquinas;
    [SerializeField] private AudioClip ambienteGradas;
    [SerializeField] private AudioClip ambienteLaboratorio;

    [Header("Configuracion")]
    [SerializeField] private float volumenBase = 0.3f;
    [SerializeField] private float tiempoCrossfade = 3f;

    private AudioSource sourceA, sourceB;
    private bool usandoA = true;
    private Coroutine crossfadeActual;
    private string zonaActual = "";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        ConfigurarSource(sourceA);
        ConfigurarSource(sourceB);
    }

    void Start()
    {
        if (GestorHistoriaSyntro.Instance != null)
            GestorHistoriaSyntro.Instance.OnFaseCambiada += OnFaseCambiada;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (GestorHistoriaSyntro.Instance != null)
            GestorHistoriaSyntro.Instance.OnFaseCambiada -= OnFaseCambiada;
    }

    void OnFaseCambiada(FaseSyntro fase)
    {
        string nombre = fase.ToString();
        if (nombre.StartsWith("Piso1_"))
            CambiarZona("maquinas");
        else if (nombre.StartsWith("Gradas_"))
            CambiarZona("gradas");
        else if (nombre.StartsWith("Piso3_"))
            CambiarZona("laboratorio");
        else if (fase == FaseSyntro.Completado)
            FadeOut();
    }

    public void CambiarZona(string zona)
    {
        if (zona == zonaActual) return;
        zonaActual = zona;

        AudioClip clip = null;
        switch (zona)
        {
            case "maquinas": clip = ambienteMaquinas; break;
            case "gradas": clip = ambienteGradas; break;
            case "laboratorio": clip = ambienteLaboratorio; break;
        }

        if (clip == null) return;

        if (crossfadeActual != null) StopCoroutine(crossfadeActual);
        crossfadeActual = StartCoroutine(Crossfade(clip));
    }

    IEnumerator Crossfade(AudioClip nuevoClip)
    {
        AudioSource viejo = usandoA ? sourceA : sourceB;
        AudioSource nuevo = usandoA ? sourceB : sourceA;
        usandoA = !usandoA;

        nuevo.clip = nuevoClip;
        nuevo.volume = 0f;
        nuevo.Play();

        float t = 0f;
        float volViejo = viejo.volume;
        while (t < tiempoCrossfade)
        {
            t += Time.deltaTime;
            float p = t / tiempoCrossfade;
            viejo.volume = Mathf.Lerp(volViejo, 0f, p);
            nuevo.volume = Mathf.Lerp(0f, volumenBase, p);
            yield return null;
        }

        viejo.Stop();
        viejo.volume = 0f;
        nuevo.volume = volumenBase;
        crossfadeActual = null;
    }

    public void FadeOut(float duracion = 2f)
    {
        zonaActual = "";
        if (crossfadeActual != null) StopCoroutine(crossfadeActual);
        crossfadeActual = StartCoroutine(FadeOutCoroutine(duracion));
    }

    IEnumerator FadeOutCoroutine(float duracion)
    {
        float volA = sourceA.volume;
        float volB = sourceB.volume;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float p = t / duracion;
            sourceA.volume = Mathf.Lerp(volA, 0f, p);
            sourceB.volume = Mathf.Lerp(volB, 0f, p);
            yield return null;
        }
        sourceA.Stop();
        sourceB.Stop();
        crossfadeActual = null;
    }

    public void SetVolumen(float vol)
    {
        volumenBase = vol;
        AudioSource activo = usandoA ? sourceA : sourceB;
        if (activo.isPlaying) activo.volume = vol;
    }

    static void ConfigurarSource(AudioSource src)
    {
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        src.volume = 0f;
    }
}
