using System;
using System.Collections;
using UnityEngine;

public enum FaseSyntro
{
    Inactivo,
    Piso1_Inicio,
    Piso1_Exploracion,
    Piso1_ErrorServidor,
    Piso1_ReinicioManual,
    Piso1_SalidaGradas,
    Gradas_Subida,
    Gradas_BuclePsicologico,
    Gradas_Completado,
    Piso3_Entrada,
    Piso3_Exploracion,
    Piso3_AnomaliaFinal,
    Piso3_Climax,
    Completado
}

[Serializable]
public class ObjetivoSyntro
{
    public string id;
    public string descripcion;
    public bool completado;
}

public class GestorHistoriaSyntro : MonoBehaviour
{
    public static GestorHistoriaSyntro Instance { get; private set; }

    [Header("=== Flujo General ===")]
    [SerializeField] private FaseSyntro faseInicial = FaseSyntro.Piso1_Inicio;

    [Header("=== Piso 1: Cuarto de Maquinas ===")]
    [SerializeField] private GameObject puertaHaciaGradas;
    [SerializeField] private AudioClip sonidoAlarmaServidor;
    [SerializeField] private AudioClip sonidoReinicioExitoso;
    [SerializeField] private AudioClip sonidoPuertaDesbloqueo;

    [Header("=== Gradas ===")]
    [SerializeField] private AudioClip musicaGradasTension;

    [Header("=== Piso 3: Laboratorio ===")]
    [SerializeField] private Light luzMonitorAnomalo;
    [SerializeField] private AudioClip sonidoAnomaliaFinal;
    [SerializeField] private AudioClip musicaClimaxFinal;
    [SerializeField] private GameObject objetoAnomaliaFinal;

    [Header("=== Audio ===")]
    [SerializeField] private AudioSource musicaSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float fadeDuracion = 2f;

    [Header("=== Tiempos ===")]
    [SerializeField] private float tiempoIntro = 5f;
    [SerializeField] private float tiempoTransicionGradas = 3f;

    [Header("=== Objetivos ===")]
    [SerializeField] private ObjetivoSyntro[] objetivosPiso1 = new ObjetivoSyntro[]
    {
        new ObjetivoSyntro { id = "localizar_rack", descripcion = "Localizar el rack del servidor averiado" },
        new ObjetivoSyntro { id = "leer_error", descripcion = "Identificar el error en la terminal" },
        new ObjetivoSyntro { id = "reiniciar_rack", descripcion = "Reiniciar el rack manualmente" }
    };
    [SerializeField] private ObjetivoSyntro[] objetivosPiso3 = new ObjetivoSyntro[]
    {
        new ObjetivoSyntro { id = "encontrar_lab", descripcion = "Encontrar el laboratorio en la oscuridad" },
        new ObjetivoSyntro { id = "investigar_pc", descripcion = "Investigar la computadora activa" },
        new ObjetivoSyntro { id = "cortar_anomalia", descripcion = "Detener la transmision anomala" }
    };

    private FaseSyntro faseActual = FaseSyntro.Inactivo;

    public FaseSyntro FaseActual => faseActual;

    public event Action<FaseSyntro> OnFaseCambiada;
    public event Action<string> OnMensaje;
    public event Action<ObjetivoSyntro> OnObjetivoCompletado;
    public event Action OnHistoriaCompleta;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (puertaHaciaGradas != null)
            puertaHaciaGradas.SetActive(false);

        if (objetoAnomaliaFinal != null)
            objetoAnomaliaFinal.SetActive(false);

        IniciarFase(faseInicial);
    }

    // ===========================================
    //  MOTOR DE FASES
    // ===========================================

    void IniciarFase(FaseSyntro fase)
    {
        faseActual = fase;
        OnFaseCambiada?.Invoke(fase);
        Debug.Log($"[GestorSyntro] >>> Fase: {fase}");

        switch (fase)
        {
            case FaseSyntro.Piso1_Inicio:
                StartCoroutine(Piso1_Inicio());
                break;
            case FaseSyntro.Gradas_Subida:
                StartCoroutine(Gradas_IniciarSubida());
                break;
            case FaseSyntro.Piso3_Entrada:
                StartCoroutine(Piso3_Entrada());
                break;
        }
    }

    // ===========================================
    //  PISO 1: CUARTO DE MAQUINAS
    // ===========================================

    IEnumerator Piso1_Inicio()
    {
        Mensaje("El turno de noche. El cuarto de maquinas zumba como siempre... hasta que algo falla.");

        yield return new WaitForSeconds(tiempoIntro);

        IniciarFase(FaseSyntro.Piso1_Exploracion);
        Mensaje("Encuentra el rack del servidor que esta fallando.");
    }

    public void Evento_RackLocalizado()
    {
        if (faseActual != FaseSyntro.Piso1_Exploracion) return;

        CompletarObjetivo(objetivosPiso1, "localizar_rack");
        Mensaje("Rack localizado. Las luces de error parpadean sin control.");

        if (sfxSource != null && sonidoAlarmaServidor != null)
            sfxSource.PlayOneShot(sonidoAlarmaServidor);
    }

    public void Evento_ErrorLeido()
    {
        if (faseActual != FaseSyntro.Piso1_Exploracion) return;

        CompletarObjetivo(objetivosPiso1, "leer_error");
        IniciarFase(FaseSyntro.Piso1_ErrorServidor);
        Mensaje("ERROR CRITICO: Kernel panic — Reinicio manual requerido. Abre el rack.");

        if (ControladorSentidosVR.Instance != null)
        {
            ControladorSentidosVR.Instance.ForzarAlerta(0.5f);
            ControladorSentidosVR.Instance.ActivarHumo(true);
        }
    }

    public void Evento_ReinicioManual()
    {
        if (faseActual != FaseSyntro.Piso1_ErrorServidor) return;

        CompletarObjetivo(objetivosPiso1, "reiniciar_rack");
        IniciarFase(FaseSyntro.Piso1_ReinicioManual);

        if (sfxSource != null && sonidoReinicioExitoso != null)
            sfxSource.PlayOneShot(sonidoReinicioExitoso);

        Mensaje("Servidor reiniciado... pero la terminal muestra: 'Transmision activa — Piso 3, Laboratorio B'.");

        StartCoroutine(HabilitarSalidaGradas());
    }

    IEnumerator HabilitarSalidaGradas()
    {
        yield return new WaitForSeconds(3f);

        IniciarFase(FaseSyntro.Piso1_SalidaGradas);

        if (puertaHaciaGradas != null)
            puertaHaciaGradas.SetActive(true);

        if (sfxSource != null && sonidoPuertaDesbloqueo != null)
            sfxSource.PlayOneShot(sonidoPuertaDesbloqueo);

        Mensaje("La puerta hacia las gradas se ha desbloqueado. Sube al tercer piso.");

        if (ControladorSentidosVR.Instance != null)
            ControladorSentidosVR.Instance.ActivarHumo(false);
    }

    public void Evento_JugadorEntraGradas()
    {
        if (faseActual != FaseSyntro.Piso1_SalidaGradas) return;
        IniciarFase(FaseSyntro.Gradas_Subida);
    }

    // ===========================================
    //  GRADAS: BUCLE PSICOLOGICO
    // ===========================================

    IEnumerator Gradas_IniciarSubida()
    {
        Mensaje("Las gradas estan oscuras. Debes llegar al tercer piso.");

        if (musicaSource != null && musicaGradasTension != null)
            StartCoroutine(FadeMusica(musicaGradasTension, 0.35f));

        yield return new WaitForSeconds(tiempoTransicionGradas);

        IniciarFase(FaseSyntro.Gradas_BuclePsicologico);

        if (TrampaGradasSyntro.Instance != null)
        {
            TrampaGradasSyntro.Instance.OnVueltaCompletada.AddListener(OnVueltaGradas);
            TrampaGradasSyntro.Instance.OnBucleTerminado.AddListener(OnBucleGradasCompleto);
            TrampaGradasSyntro.Instance.IniciarBucleGradas();
        }
        else
        {
            Debug.LogWarning("[GestorSyntro] TrampaGradasSyntro no encontrado. Saltando al Piso 3.");
            IniciarFase(FaseSyntro.Piso3_Entrada);
        }
    }

    void OnVueltaGradas()
    {
        if (TrampaGradasSyntro.Instance == null) return;

        int vuelta = TrampaGradasSyntro.Instance.VueltaActual;

        switch (vuelta)
        {
            case 1:
                Mensaje("Segundo piso... o eso dice el cartel. Sigue subiendo.");
                break;
            case 2:
                Mensaje("Otra vez el segundo piso? Algo no esta bien. Las gradas no terminan.");
                if (ControladorSentidosVR.Instance != null)
                    ControladorSentidosVR.Instance.ForzarAlerta(0.7f);
                break;
            case 3:
                Mensaje("El aire se siente diferente. El tercer piso, finalmente.");
                break;
        }
    }

    void OnBucleGradasCompleto()
    {
        IniciarFase(FaseSyntro.Gradas_Completado);

        if (TrampaGradasSyntro.Instance != null)
        {
            TrampaGradasSyntro.Instance.OnVueltaCompletada.RemoveListener(OnVueltaGradas);
            TrampaGradasSyntro.Instance.OnBucleTerminado.RemoveListener(OnBucleGradasCompleto);
        }

        StartCoroutine(TransicionAPiso3());
    }

    IEnumerator TransicionAPiso3()
    {
        yield return StartCoroutine(FadeOutMusica());
        yield return new WaitForSeconds(1f);
        IniciarFase(FaseSyntro.Piso3_Entrada);
    }

    // ===========================================
    //  PISO 3: LABORATORIO — ANOMALIA FINAL
    // ===========================================

    IEnumerator Piso3_Entrada()
    {
        Mensaje("Tercer piso. El pasillo esta en completa oscuridad. El laboratorio debe estar al fondo.");

        if (ControladorSentidosVR.Instance != null)
            ControladorSentidosVR.Instance.ForzarAlerta(0.3f);

        yield return new WaitForSeconds(3f);
        IniciarFase(FaseSyntro.Piso3_Exploracion);
        Mensaje("Encuentra el laboratorio.");
    }

    public void Evento_LaboratorioEncontrado()
    {
        if (faseActual != FaseSyntro.Piso3_Exploracion) return;

        CompletarObjetivo(objetivosPiso3, "encontrar_lab");
        Mensaje("El laboratorio. Una sola computadora sigue encendida en la oscuridad total.");

        if (luzMonitorAnomalo != null)
            luzMonitorAnomalo.enabled = true;
    }

    public void Evento_ComputadoraInvestigada()
    {
        if (faseActual != FaseSyntro.Piso3_Exploracion) return;

        CompletarObjetivo(objetivosPiso3, "investigar_pc");
        IniciarFase(FaseSyntro.Piso3_AnomaliaFinal);

        Mensaje("La pantalla muestra datos que no deberian existir. Un patron se repite... algo te esta mirando.");

        if (ControladorSentidosVR.Instance != null)
            ControladorSentidosVR.Instance.ForzarAlerta(0.85f);

        if (objetoAnomaliaFinal != null)
            objetoAnomaliaFinal.SetActive(true);

        if (sfxSource != null && sonidoAnomaliaFinal != null)
            sfxSource.PlayOneShot(sonidoAnomaliaFinal);

        if (musicaSource != null && musicaClimaxFinal != null)
            StartCoroutine(FadeMusica(musicaClimaxFinal, 0.5f));
    }

    public void Evento_TransmisionCortada()
    {
        if (faseActual != FaseSyntro.Piso3_AnomaliaFinal) return;

        CompletarObjetivo(objetivosPiso3, "cortar_anomalia");
        IniciarFase(FaseSyntro.Piso3_Climax);

        StartCoroutine(SecuenciaClimax());
    }

    IEnumerator SecuenciaClimax()
    {
        Mensaje("Transmision cortada.");

        if (ControladorSentidosVR.Instance != null)
            ControladorSentidosVR.Instance.ForzarAlerta(0f);

        yield return new WaitForSeconds(3f);

        Mensaje("...");

        yield return new WaitForSeconds(2f);

        Mensaje("\"YA SABEMOS DONDE ESTAS.\"");

        if (ControladorSentidosVR.Instance != null)
            ControladorSentidosVR.Instance.DisparoDeEmergencia();

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeOutMusica());

        IniciarFase(FaseSyntro.Completado);
        Mensaje("[FIN DE LA EXPERIENCIA SYNTRO]");
        OnHistoriaCompleta?.Invoke();
    }

    // ===========================================
    //  SISTEMA DE OBJETIVOS
    // ===========================================

    void CompletarObjetivo(ObjetivoSyntro[] lista, string id)
    {
        if (lista == null) return;
        foreach (ObjetivoSyntro obj in lista)
        {
            if (obj.id == id && !obj.completado)
            {
                obj.completado = true;
                OnObjetivoCompletado?.Invoke(obj);
                Debug.Log($"[GestorSyntro] Objetivo: {obj.descripcion}");
                return;
            }
        }
    }

    public float GetProgresoPiso1()
    {
        return CalcularProgreso(objetivosPiso1);
    }

    public float GetProgresoPiso3()
    {
        return CalcularProgreso(objetivosPiso3);
    }

    float CalcularProgreso(ObjetivoSyntro[] lista)
    {
        if (lista == null || lista.Length == 0) return 0f;
        int c = 0;
        foreach (ObjetivoSyntro o in lista)
            if (o.completado) c++;
        return (float)c / lista.Length;
    }

    // ===========================================
    //  AUDIO
    // ===========================================

    void Mensaje(string texto)
    {
        OnMensaje?.Invoke(texto);
        Debug.Log($"[GestorSyntro] {texto}");
    }

    IEnumerator FadeMusica(AudioClip clip, float volumenObjetivo)
    {
        if (musicaSource == null) yield break;

        if (musicaSource.isPlaying)
            yield return StartCoroutine(FadeOutMusica());

        musicaSource.clip = clip;
        musicaSource.volume = 0f;
        musicaSource.loop = true;
        musicaSource.Play();

        float t = 0f;
        while (t < fadeDuracion)
        {
            t += Time.deltaTime;
            musicaSource.volume = Mathf.Lerp(0f, volumenObjetivo, t / fadeDuracion);
            yield return null;
        }
    }

    IEnumerator FadeOutMusica()
    {
        if (musicaSource == null || !musicaSource.isPlaying) yield break;
        float volInicial = musicaSource.volume;
        float t = 0f;
        while (t < fadeDuracion)
        {
            t += Time.deltaTime;
            musicaSource.volume = Mathf.Lerp(volInicial, 0f, t / fadeDuracion);
            yield return null;
        }
        musicaSource.Stop();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}