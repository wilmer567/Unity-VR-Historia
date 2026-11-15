using System.Collections;
using UnityEngine;

public class CapituloRevelacion : MonoBehaviour
{
    [Header("=== Capitulo 3: Revelacion ===")]
    [SerializeField] private DialogueGraph dialogoFinal;
    [SerializeField] private DialogueGraph dialogoEntidad;
    [SerializeField] private DialogueGraph dialogoDecisionFinal;

    [Header("Objetos Interactuables")]
    [SerializeField] private VRInteractableObject terminalCentral;
    [SerializeField] private VRInteractableObject interruptorEmergencia;
    [SerializeField] private VRInteractableObject artefactoFinal;

    [Header("Ambiente")]
    [SerializeField] private AudioSource musicaAmbiente;
    [SerializeField] private AudioClip clipRevelacion;
    [SerializeField] private AudioClip clipClimax;
    [SerializeField] private AudioClip clipFinal;
    [SerializeField] private Light[] lucesEscena;
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorAlerta = Color.red;
    [SerializeField] private Color colorFinal = new Color(0.5f, 0f, 1f);

    [Header("Efectos")]
    [SerializeField] private ParticleSystem efectoPortal;
    [SerializeField] private ParticleSystem efectoExplosion;
    [SerializeField] private ParticleSystem particulasAmbientales;
    [SerializeField] private GameObject entidadFinal;
    [SerializeField] private Animator entidadAnimator;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip sonidoTerminalAcceso;
    [SerializeField] private AudioClip sonidoAlertaSistema;
    [SerializeField] private AudioClip sonidoEntidadAparece;
    [SerializeField] private AudioClip sonidoExplosionFinal;
    [SerializeField] private AudioClip sonidoDecision;

    [Header("Escenario Final")]
    [SerializeField] private GameObject salaSecreto;
    [SerializeField] private Transform puntoClimax;

    [Header("Creditos")]
    [SerializeField] private string escenaCreditos = "0_MainMenu_Scene";

    private enum FaseRevelacion
    {
        Inicio,
        Exploracion,
        TerminalAccedida,
        EntidadRevelada,
        DecisionFinal,
        Desenlace,
        Completado
    }

    private FaseRevelacion faseActual = FaseRevelacion.Inicio;
    private AudioSource sfxSource;

    void Start()
    {
        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.EstablecerCapitulo(Capitulo.Revelacion);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 0f;
        sfxSource.playOnAwake = false;

        if (entidadFinal != null) entidadFinal.SetActive(false);
        if (salaSecreto != null) salaSecreto.SetActive(false);

        StartCoroutine(IniciarCapitulo());
    }

    IEnumerator IniciarCapitulo()
    {
        if (VRHUD.Instance != null)
        {
            VRHUD.Instance.EstablecerObjetivos("Capitulo 3: Revelacion", new[]
            {
                "Acceder a la terminal central",
                "Descifrar la transmision",
                "Confrontar la entidad",
                "Tomar la decision final"
            });
        }

        if (musicaAmbiente != null && clipRevelacion != null)
        {
            musicaAmbiente.clip = clipRevelacion;
            musicaAmbiente.loop = true;
            musicaAmbiente.volume = 0f;
            musicaAmbiente.Play();
            StartCoroutine(FadeAudio(musicaAmbiente, 0f, 0.4f, 3f));
        }

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0.3f);

        if (particulasAmbientales != null)
            particulasAmbientales.Play();

        StartCoroutine(TransicionIluminacion(colorNormal, 1f, 2f));

        yield return new WaitForSeconds(2f);

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje(
                "El tercer piso. El aire esta cargado de electricidad. La terminal central parpadea.", 5f);

        faseActual = FaseRevelacion.Exploracion;
    }

    public void OnTerminalAccedida()
    {
        if (faseActual != FaseRevelacion.Exploracion) return;
        faseActual = FaseRevelacion.TerminalAccedida;

        if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(0);

        StartCoroutine(SecuenciaTerminal());
    }

    IEnumerator SecuenciaTerminal()
    {
        if (sonidoTerminalAcceso != null)
            sfxSource.PlayOneShot(sonidoTerminalAcceso);

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje(
                "La terminal muestra coordenadas... datos de transmision... un patron que se repite.", 4f);

        yield return new WaitForSeconds(5f);

        if (sonidoAlertaSistema != null)
            sfxSource.PlayOneShot(sonidoAlertaSistema, 0.7f);

        StartCoroutine(TransicionIluminacion(colorAlerta, 0.5f, 3f));

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0.6f);

        yield return CrossfadeMusica(musicaAmbiente, clipClimax, 3f, 0.5f);

        if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(1);

        yield return new WaitForSeconds(2f);

        yield return SecuenciaAparicionEntidad();
    }

    IEnumerator SecuenciaAparicionEntidad()
    {
        if (TerrorPsychologyManager.Instance != null)
            TerrorPsychologyManager.Instance.PulsoTerror(0.4f, 1f);

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0.85f);

        var fade = FindAnyObjectByType<VRFadeScreen>();
        if (fade != null) fade.FlashTerror(0.6f, 0.5f);

        if (sonidoEntidadAparece != null)
            sfxSource.PlayOneShot(sonidoEntidadAparece);

        yield return new WaitForSeconds(0.3f);

        if (entidadFinal != null)
        {
            entidadFinal.SetActive(true);
            StartCoroutine(AparecerEntidad());
        }

        if (efectoPortal != null) efectoPortal.Play();

        faseActual = FaseRevelacion.EntidadRevelada;

        if (VRHUD.Instance != null)
            VRHUD.Instance.MostrarNotificacion("Algo ha aparecido...");

        yield return new WaitForSeconds(2f);

        if (VRDialogueManager.Instance != null && dialogoEntidad != null)
        {
            VRDialogueManager.Instance.IniciarDialogo(dialogoEntidad);
            yield return new WaitUntil(() => !VRDialogueManager.Instance.InDialogue);
        }
        else if (VRDialogueManager.Instance != null)
        {
            VRDialogueManager.Instance.MostrarMensaje(
                "\"Te hemos estado esperando. Ahora debes elegir.\"", 4f);
            yield return new WaitForSeconds(4f);
        }

        if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(2);

        faseActual = FaseRevelacion.DecisionFinal;

        yield return new WaitForSeconds(1f);

        if (VRDialogueManager.Instance != null && dialogoDecisionFinal != null)
        {
            VRDialogueManager.Instance.IniciarDialogo(dialogoDecisionFinal);
        }
        else
        {
            IniciarDesenlace("default");
        }
    }

    IEnumerator AparecerEntidad()
    {
        if (entidadFinal == null) yield break;

        Vector3 escalaFinal = entidadFinal.transform.localScale;
        entidadFinal.transform.localScale = Vector3.zero;
        float t = 0f;

        while (t < 1.5f)
        {
            t += Time.deltaTime;
            float progreso = Mathf.Clamp01(t / 1.5f);
            float curva = 1f - Mathf.Pow(1f - progreso, 3f);
            float oscilacion = 1f + Mathf.Sin(progreso * 20f) * (1f - progreso) * 0.15f;
            entidadFinal.transform.localScale = escalaFinal * curva * oscilacion;
            yield return null;
        }
        entidadFinal.transform.localScale = escalaFinal;
    }

    public void IniciarDesenlace(string decision)
    {
        if (faseActual != FaseRevelacion.DecisionFinal) return;
        faseActual = FaseRevelacion.Desenlace;

        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.ActivarFlag("decision_" + decision);

        if (sonidoDecision != null)
            sfxSource.PlayOneShot(sonidoDecision);

        StartCoroutine(SecuenciaDesenlace(decision));
    }

    IEnumerator SecuenciaDesenlace(string decision)
    {
        if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(3);

        StartCoroutine(TransicionIluminacion(colorFinal, 2f, 4f));

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(1f);

        yield return CrossfadeMusica(musicaAmbiente, clipFinal, 3f, 0.6f);

        string mensaje = decision switch
        {
            "aceptar" => "Aceptas tu destino. La realidad se reconfigura a tu alrededor.",
            "rechazar" => "Te niegas. La entidad se desvanece, pero las preguntas permanecen.",
            "sacrificio" => "El sacrificio rompe el ciclo. Todo termina... y todo comienza de nuevo.",
            _ => "Tu decision resuena en el vacio. Nada volvera a ser igual."
        };

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje(mensaje, 6f);

        yield return new WaitForSeconds(3f);

        if (TerrorPsychologyManager.Instance != null)
            TerrorPsychologyManager.Instance.PulsoTerror(0.6f, 1.5f);

        if (sonidoExplosionFinal != null)
            sfxSource.PlayOneShot(sonidoExplosionFinal);

        if (efectoExplosion != null) efectoExplosion.Play();

        var fade = FindAnyObjectByType<VRFadeScreen>();
        if (fade != null) fade.FlashTerror(1f, 0.8f);

        yield return new WaitForSeconds(3f);

        StartCoroutine(TransicionIluminacion(Color.black, 0f, 3f));

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0f);

        faseActual = FaseRevelacion.Completado;

        if (GameManagerVR.Instance != null)
        {
            GameManagerVR.Instance.ActivarFlag("historia_completada");
            GameManagerVR.Instance.GuardarProgreso();
        }

        yield return new WaitForSeconds(2f);

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje("[FIN DE LA HISTORIA]", 5f);

        yield return new WaitForSeconds(5f);

        if (fade != null) yield return fade.FadeOut(3f);

        UnityEngine.SceneManagement.SceneManager.LoadScene(escenaCreditos);
    }

    // === Utilidades ===

    IEnumerator TransicionIluminacion(Color color, float intensity, float duracion)
    {
        if (lucesEscena == null) yield break;

        Color[] coloresInicio = new Color[lucesEscena.Length];
        float[] intensidadesInicio = new float[lucesEscena.Length];

        for (int i = 0; i < lucesEscena.Length; i++)
        {
            if (lucesEscena[i] != null)
            {
                coloresInicio[i] = lucesEscena[i].color;
                intensidadesInicio[i] = lucesEscena[i].intensity;
            }
        }

        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float progreso = t / duracion;
            float curva = progreso * progreso * (3f - 2f * progreso);

            for (int i = 0; i < lucesEscena.Length; i++)
            {
                if (lucesEscena[i] == null) continue;
                lucesEscena[i].color = Color.Lerp(coloresInicio[i], color, curva);
                lucesEscena[i].intensity = Mathf.Lerp(intensidadesInicio[i], intensity, curva);
            }
            yield return null;
        }
    }

    IEnumerator FadeAudio(AudioSource source, float from, float to, float duracion)
    {
        if (source == null) yield break;
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
