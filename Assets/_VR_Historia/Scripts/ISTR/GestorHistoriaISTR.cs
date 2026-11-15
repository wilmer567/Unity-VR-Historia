using System;
using System.Collections;
using UnityEngine;

public enum FaseISTR
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
public class ObjetivoISTR
{
    public string id;
    public string descripcion;
    public bool completado;
}

public class GestorHistoriaISTR : MonoBehaviour
{
    public static GestorHistoriaISTR Instance { get; private set; }

    [Header("=== Flujo General ===")]
    [SerializeField] private FaseISTR faseInicial = FaseISTR.Piso1_Inicio;

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
    [SerializeField] private ObjetivoISTR[] objetivosPiso1 = new ObjetivoISTR[]
    {
        new ObjetivoISTR { id = "localizar_rack", descripcion = "Localizar el rack del servidor averiado" },
        new ObjetivoISTR { id = "leer_error", descripcion = "Identificar el error en la terminal" },
        new ObjetivoISTR { id = "reiniciar_rack", descripcion = "Reiniciar el rack manualmente" }
    };
    [SerializeField] private ObjetivoISTR[] objetivosPiso3 = new ObjetivoISTR[]
    {
        new ObjetivoISTR { id = "encontrar_lab", descripcion = "Encontrar el laboratorio en la oscuridad" },
        new ObjetivoISTR { id = "investigar_pc", descripcion = "Investigar la computadora activa" },
        new ObjetivoISTR { id = "cortar_anomalia", descripcion = "Detener la transmision anomala" }
    };

    private FaseISTR faseActual = FaseISTR.Inactivo;

    public FaseISTR FaseActual => faseActual;

    public event Action<FaseISTR> OnFaseCambiada;
    public event Action<string> OnMensaje;
    public event Action<ObjetivoISTR> OnObjetivoCompletado;
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

    void IniciarFase(FaseISTR fase)
    {
        faseActual = fase;
        OnFaseCambiada?.Invoke(fase);
        Debug.Log($"[GestorISTR] >>> Fase: {fase}");

        switch (fase)
        {
            case FaseISTR.Piso1_Inicio:
                StartCoroutine(Piso1_Inicio());
                break;
            case FaseISTR.Gradas_Subida:
                StartCoroutine(Gradas_IniciarSubida());
                break;
            case FaseISTR.Piso3_Entrada:
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

        IniciarFase(FaseISTR.Piso1_Exploracion);
        Mensaje("Encuentra el rack del servidor que esta fallando.");
    }

    public void Evento_RackLocalizado()
    {
        if (faseActual != FaseISTR.Piso1_Exploracion) return;

        CompletarObjetivo(objetivosPiso1, "localizar_rack");
        Mensaje("Rack localizado. Las luces de error parpadean sin control.");

        if (sfxSource != null && sonidoAlarmaServidor != null)
            sfxSource.PlayOneShot(sonidoAlarmaServidor);
    }

    public void Evento_ErrorLeido()
    {
        if (faseActual != FaseISTR.Piso1_Exploracion) return;

        CompletarObjetivo(objetivosPiso1, "leer_error");
        IniciarFase(FaseISTR.Piso1_ErrorServidor);
        Mensaje("ERROR CRITICO: Kernel panic — Reinicio manual requerido. Abre el rack.");

        if (ControladorSentidosVR.Instance != null)
        {
            ControladorSentidosVR.Instance.ForzarAlerta(0.5f);
            ControladorSentidosVR.Instance.ActivarHumo(true);
        }
    }

    public void Evento_ReinicioManual()
    {
        if (faseActual != FaseISTR.Piso1_ErrorServidor) return;

        CompletarObjetivo(objetivosPiso1, "reiniciar_rack");
        IniciarFase(FaseISTR.Piso1_ReinicioManual);

        if (sfxSource != null && sonidoReinicioExitoso != null)
            sfxSource.PlayOneShot(sonidoReinicioExitoso);

        Mensaje("Servidor reiniciado... pero la terminal muestra: 'Transmision activa — Piso 3, Laboratorio B'.");

        StartCoroutine(HabilitarSalidaGradas());
    }

    IEnumerator HabilitarSalidaGradas()
    {
        yield return new WaitForSeconds(3f);

        IniciarFase(FaseISTR.Piso1_SalidaGradas);

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
        if (faseActual != FaseISTR.Piso1_SalidaGradas) return;
        IniciarFase(FaseISTR.Gradas_Subida);
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

        IniciarFase(FaseISTR.Gradas_BuclePsicologico);

        if (TrampaGradasISTR.Instance != null)
        {
            TrampaGradasISTR.Instance.OnVueltaCompletada.AddListener(OnVueltaGradas);
            TrampaGradasISTR.Instance.OnBucleTerminado.AddListener(OnBucleGradasCompleto);
            TrampaGradasISTR.Instance.IniciarBucleGradas();
        }
        else
        {
            Debug.LogWarning("[GestorISTR] TrampaGradasISTR no encontrado. Saltando al Piso 3.");
            IniciarFase(FaseISTR.Piso3_Entrada);
        }
    }

    void OnVueltaGradas()
    {
        if (TrampaGradasISTR.Instance == null) return;

        int vuelta = TrampaGradasISTR.Instance.VueltaActual;

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
        IniciarFase(FaseISTR.Gradas_Completado);

        if (TrampaGradasISTR.Instance != null)
        {
            TrampaGradasISTR.Instance.OnVueltaCompletada.RemoveListener(OnVueltaGradas);
            TrampaGradasISTR.Instance.OnBucleTerminado.RemoveListener(OnBucleGradasCompleto);
        }

        StartCoroutine(TransicionAPiso3());
    }

    IEnumerator TransicionAPiso3()
    {
        yield return StartCoroutine(FadeOutMusica());
        yield return new WaitForSeconds(1f);
        IniciarFase(FaseISTR.Piso3_Entrada);
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
        IniciarFase(FaseISTR.Piso3_Exploracion);
        Mensaje("Encuentra el laboratorio.");
    }

    public void Evento_LaboratorioEncontrado()
    {
        if (faseActual != FaseISTR.Piso3_Exploracion) return;

        CompletarObjetivo(objetivosPiso3, "encontrar_lab");
        Mensaje("El laboratorio. Una sola computadora sigue encendida en la oscuridad total.");

        if (luzMonitorAnomalo != null)
            luzMonitorAnomalo.enabled = true;
    }

    public void Evento_ComputadoraInvestigada()
    {
        if (faseActual != FaseISTR.Piso3_Exploracion) return;

        CompletarObjetivo(objetivosPiso3, "investigar_pc");
        IniciarFase(FaseISTR.Piso3_AnomaliaFinal);

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
        if (faseActual != FaseISTR.Piso3_AnomaliaFinal) return;

        CompletarObjetivo(objetivosPiso3, "cortar_anomalia");
        IniciarFase(FaseISTR.Piso3_Climax);

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

        IniciarFase(FaseISTR.Completado);
        Mensaje("[FIN DE LA EXPERIENCIA ISTR]");
        OnHistoriaCompleta?.Invoke();
    }

    // ===========================================
    //  SISTEMA DE OBJETIVOS
    // ===========================================

    void CompletarObjetivo(ObjetivoISTR[] lista, string id)
    {
        if (lista == null) return;
        foreach (ObjetivoISTR obj in lista)
        {
            if (obj.id == id && !obj.completado)
            {
                obj.completado = true;
                OnObjetivoCompletado?.Invoke(obj);
                Debug.Log($"[GestorISTR] Objetivo: {obj.descripcion}");
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

    float CalcularProgreso(ObjetivoISTR[] lista)
    {
        if (lista == null || lista.Length == 0) return 0f;
        int c = 0;
        foreach (ObjetivoISTR o in lista)
            if (o.completado) c++;
        return (float)c / lista.Length;
    }

    // ===========================================
    //  AUDIO
    // ===========================================

    void Mensaje(string texto)
    {
        OnMensaje?.Invoke(texto);
        Debug.Log($"[GestorISTR] {texto}");
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