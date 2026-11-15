using System.Collections;
using UnityEngine;

public class CapituloExploracion : MonoBehaviour
{
    [Header("=== Capitulo 2: Exploracion ===")]
    [SerializeField] private DialogueGraph dialogoCientifico;
    [SerializeField] private DialogueGraph dialogoFantasma;
    [SerializeField] private DialogueGraph dialogoTerminal;

    [Header("Objetos Interactuables")]
    [SerializeField] private VRInteractableObject tarjetaAcceso;
    [SerializeField] private VRInteractableObject documentoSecreto;
    [SerializeField] private VRInteractableObject palancaEnergia;
    [SerializeField] private VRInteractableObject terminalDatos;

    [Header("NPCs")]
    [SerializeField] private Transform npcCientifico;
    [SerializeField] private Transform npcFantasma;

    [Header("Zonas")]
    [SerializeField] private string zonaLaboratorio = "zona_laboratorio";
    [SerializeField] private string zonaSotano = "zona_sotano";
    [SerializeField] private string zonaArchivo = "zona_archivo";
    [SerializeField] private string zonaRevelacion = "zona_revelacion";

    [Header("Ambiente")]
    [SerializeField] private AudioSource musicaAmbiente;
    [SerializeField] private AudioClip clipExploracion;
    [SerializeField] private AudioClip clipTension;
    [SerializeField] private AudioClip clipDescubrimiento;
    [SerializeField] private ParticleSystem particulasPolvo;
    [SerializeField] private Light luzEmergencia;

    [Header("Iluminacion")]
    [SerializeField] private Light[] lucesLaboratorio;
    [SerializeField] private Light[] lucesSotano;
    [SerializeField] private Color colorExploracion = new Color(0.3f, 0.4f, 0.5f);
    [SerializeField] private Color colorTension = new Color(0.5f, 0.2f, 0.15f);
    [SerializeField] private Color colorEnergia = new Color(0.2f, 0.6f, 0.3f);

    [Header("Puertas y Bloqueos")]
    [SerializeField] private GameObject puertaLaboratorio;
    [SerializeField] private GameObject puertaSotano;
    [SerializeField] private GameObject barreraEnergia;

    [Header("Audio Eventos")]
    [SerializeField] private AudioClip sonidoPuertaAbre;
    [SerializeField] private AudioClip sonidoBarreraDesactiva;
    [SerializeField] private AudioClip sonidoGeneradorEncendiendose;

    private bool tarjetaObtenida;
    private bool documentoLeido;
    private bool energiaRestaurada;
    private AudioSource sfxSource;

    void Start()
    {
        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.EstablecerCapitulo(Capitulo.Exploracion);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 0f;
        sfxSource.playOnAwake = false;

        BloquearPuertas();
        StartCoroutine(IniciarCapitulo());
    }

    void BloquearPuertas()
    {
        if (puertaLaboratorio != null) puertaLaboratorio.SetActive(false);
        if (puertaSotano != null) puertaSotano.SetActive(false);
        if (barreraEnergia != null) barreraEnergia.SetActive(true);

        if (lucesSotano != null)
        {
            foreach (var l in lucesSotano)
                if (l != null) l.intensity = 0f;
        }
    }

    IEnumerator IniciarCapitulo()
    {
        if (VRHUD.Instance != null)
        {
            VRHUD.Instance.EstablecerObjetivos("Capitulo 2: Exploracion", new[]
            {
                "Encontrar la tarjeta de acceso",
                "Acceder al laboratorio",
                "Leer el documento clasificado",
                "Restaurar la energia del sotano",
                "Descubrir la verdad en los archivos"
            });
        }

        if (musicaAmbiente != null && clipExploracion != null)
        {
            musicaAmbiente.clip = clipExploracion;
            musicaAmbiente.loop = true;
            musicaAmbiente.volume = 0f;
            musicaAmbiente.Play();
            StartCoroutine(FadeAudio(musicaAmbiente, 0f, 0.25f, 3f));
        }

        if (particulasPolvo != null) particulasPolvo.Play();

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0.1f);

        yield return new WaitForSeconds(2f);

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje(
                "Un edificio abandonado. Algo paso aqui... Necesitas encontrar respuestas.", 4f);

        yield return new WaitForSeconds(5f);

        if (VRTeleportManager.Instance != null)
            VRTeleportManager.Instance.DesbloquearZona(zonaLaboratorio);
    }

    void Update()
    {
        VerificarProgreso();
    }

    void VerificarProgreso()
    {
        if (GameManagerVR.Instance == null) return;

        if (!tarjetaObtenida && GameManagerVR.Instance.TieneFlag("item_tarjeta_acceso"))
        {
            tarjetaObtenida = true;
            OnTarjetaObtenida();
        }

        if (!documentoLeido && GameManagerVR.Instance.TieneFlag("documento_leido"))
        {
            documentoLeido = true;
            OnDocumentoLeido();
        }

        if (!energiaRestaurada && GameManagerVR.Instance.TieneFlag("energia_restaurada"))
        {
            energiaRestaurada = true;
            OnEnergiaRestaurada();
        }
    }

    void OnTarjetaObtenida()
    {
        if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(0);

        StartCoroutine(AbrirPuerta(puertaLaboratorio, sonidoPuertaAbre));

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje("Tarjeta de acceso al laboratorio obtenida.", 3f);

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.PulsoTension(0.15f, 1.5f);
    }

    void OnDocumentoLeido()
    {
        if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(2);

        if (VRTeleportManager.Instance != null)
            VRTeleportManager.Instance.DesbloquearZona(zonaSotano);

        StartCoroutine(TransicionATension());

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje(
                "El documento revela experimentos prohibidos. Hay mas informacion en el sotano.", 5f);
    }

    IEnumerator TransicionATension()
    {
        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0.4f);

        if (TerrorPsychologyManager.Instance != null)
            TerrorPsychologyManager.Instance.PulsoTerror(0.2f, 0.5f);

        yield return CrossfadeMusica(musicaAmbiente, clipTension, 4f, 0.3f);

        if (lucesLaboratorio != null)
        {
            foreach (var l in lucesLaboratorio)
            {
                if (l != null)
                    StartCoroutine(TransicionLuz(l, colorTension, l.intensity * 0.7f, 3f));
            }
        }
    }

    void OnEnergiaRestaurada()
    {
        if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(3);

        if (barreraEnergia != null) barreraEnergia.SetActive(false);

        if (VRTeleportManager.Instance != null)
            VRTeleportManager.Instance.DesbloquearZona(zonaArchivo);

        StartCoroutine(SecuenciaRestauracionEnergia());
    }

    IEnumerator SecuenciaRestauracionEnergia()
    {
        if (sonidoGeneradorEncendiendose != null && sfxSource != null)
            sfxSource.PlayOneShot(sonidoGeneradorEncendiendose);

        yield return new WaitForSeconds(0.5f);

        if (lucesSotano != null)
        {
            foreach (var l in lucesSotano)
            {
                if (l != null)
                    StartCoroutine(EncenderLuzProgresiva(l, colorEnergia, 1.5f, 2f));
            }
        }

        if (luzEmergencia != null)
            StartCoroutine(TransicionLuz(luzEmergencia, Color.green, 2f, 1.5f));

        if (sonidoBarreraDesactiva != null && sfxSource != null)
        {
            yield return new WaitForSeconds(1f);
            sfxSource.PlayOneShot(sonidoBarreraDesactiva);
        }

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje(
                "Energia restaurada. Los archivos del sotano son accesibles.", 3f);
    }

    IEnumerator EncenderLuzProgresiva(Light luz, Color color, float intensidadFinal, float duracion)
    {
        luz.color = color;
        luz.intensity = 0f;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            float progreso = t / duracion;
            float flicker = 1f + Mathf.Sin(progreso * 30f) * (1f - progreso) * 0.5f;
            luz.intensity = Mathf.Lerp(0f, intensidadFinal, progreso) * flicker;
            yield return null;
        }
        luz.intensity = intensidadFinal;
    }

    public void OnCientificoInteractuado()
    {
        if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(1);

        if (VRDialogueManager.Instance != null && dialogoCientifico != null && npcCientifico != null)
            VRDialogueManager.Instance.IniciarDialogo(dialogoCientifico, npcCientifico.position);
    }

    public void OnFantasmaInteractuado()
    {
        if (TerrorPsychologyManager.Instance != null)
            TerrorPsychologyManager.Instance.ForzarPerdidaCordura(0.15f);

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.PulsoTension(0.4f, 3f);

        if (VRDialogueManager.Instance != null && dialogoFantasma != null && npcFantasma != null)
            VRDialogueManager.Instance.IniciarDialogo(dialogoFantasma, npcFantasma.position);
    }

    public void OnArchivosDescubiertos()
    {
        if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(4);

        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.ActivarFlag("cap2_completado");

        StartCoroutine(TransicionCapitulo3());
    }

    IEnumerator TransicionCapitulo3()
    {
        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0.6f);

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje(
                "Los archivos revelan una ubicacion final. Todo lleva al tercer piso.", 4f);

        yield return CrossfadeMusica(musicaAmbiente, clipDescubrimiento, 3f, 0.35f);

        yield return new WaitForSeconds(3f);

        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.AvanzarCapitulo();

        if (VRTeleportManager.Instance != null)
        {
            VRTeleportManager.Instance.DesbloquearZona(zonaRevelacion);
            VRTeleportManager.Instance.TeleportarAZona(zonaRevelacion);
        }
    }

    // === Utilidades de Iluminacion y Audio ===

    IEnumerator AbrirPuerta(GameObject puerta, AudioClip sonido)
    {
        if (sonido != null && sfxSource != null)
            sfxSource.PlayOneShot(sonido, 0.6f);

        yield return new WaitForSeconds(0.3f);

        if (puerta != null)
            puerta.SetActive(true);
    }

    IEnumerator TransicionLuz(Light luz, Color colorTarget, float intensidadTarget, float duracion)
    {
        Color colorInicio = luz.color;
        float intensidadInicio = luz.intensity;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            float progreso = t / duracion;
            float curva = progreso * progreso * (3f - 2f * progreso);
            luz.color = Color.Lerp(colorInicio, colorTarget, curva);
            luz.intensity = Mathf.Lerp(intensidadInicio, intensidadTarget, curva);
            yield return null;
        }
    }

    IEnumerator FadeAudio(AudioSource source, float from, float to, float duracion)
    {
        source.volume = from;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, t / duracion);
            yield return null;
        }
        source.volume = to;
    }

    IEnumerator CrossfadeMusica(AudioSource source, AudioClip nuevoClip, float duracion, float volTarget)
    {
        if (source == null || nuevoClip == null) yield break;
        float volOriginal = source.volume;
        yield return FadeAudio(source, volOriginal, 0f, duracion * 0.4f);
        source.clip = nuevoClip;
        source.Play();
        yield return FadeAudio(source, 0f, volTarget, duracion * 0.6f);
    }
}
