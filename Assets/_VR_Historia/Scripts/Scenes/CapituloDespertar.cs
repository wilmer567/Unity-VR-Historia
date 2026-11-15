using System.Collections;
using UnityEngine;

public class CapituloDespertar : MonoBehaviour
{
    [Header("=== Capitulo 1: Despertar ===")]
    [SerializeField] private DialogueGraph dialogoIntroduccion;
    [SerializeField] private DialogueGraph dialogoGuardia;
    [SerializeField] private DialogueGraph dialogoDiario;

    [Header("Objetos Interactuables")]
    [SerializeField] private VRInteractableObject diarioObjeto;
    [SerializeField] private VRInteractableObject llaveObjeto;
    [SerializeField] private VRInteractableObject puertaPrincipal;
    [SerializeField] private VRInteractableObject terminalObjeto;

    [Header("NPCs")]
    [SerializeField] private Transform npcGuardia;

    [Header("Zonas de Teleport")]
    [SerializeField] private string zonaRecepcion = "zona_recepcion";
    [SerializeField] private string zonaPasillo = "zona_pasillo";
    [SerializeField] private string zonaOficina = "zona_oficina";

    [Header("Ambiente")]
    [SerializeField] private AudioSource musicaAmbiente;
    [SerializeField] private AudioClip clipAmbienteDesespertar;
    [SerializeField] private AudioClip clipAmbienteTension;
    [SerializeField] private Light[] lucesFlicker;
    [SerializeField] private Light luzPrincipal;

    [Header("Iluminacion Dinamica")]
    [SerializeField] private Color colorLuzInicial = new Color(0.4f, 0.5f, 0.6f);
    [SerializeField] private Color colorLuzTension = new Color(0.6f, 0.3f, 0.2f);
    [SerializeField] private float intensidadLuzTension = 0.3f;
    [SerializeField] private float transicionLuzDuracion = 5f;

    [Header("Patrones de Flicker")]
    [SerializeField] private AnimationCurve flickerCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1.5f);

    [Header("Tiempos")]
    [SerializeField] private float delayInicio = 3f;
    [SerializeField] private float delayDialogoGuardia = 8f;

    private bool capituloiniciado;
    private bool diarioRecogido;
    private bool llaveObtenida;
    private float[] flickerIntensidadesOriginales;
    private float flickerProgresion;

    void Start()
    {
        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.EstablecerCapitulo(Capitulo.Despertar);

        GuardarIntensidadesFlicker();
        StartCoroutine(IniciarCapitulo());
    }

    void GuardarIntensidadesFlicker()
    {
        if (lucesFlicker == null) return;
        flickerIntensidadesOriginales = new float[lucesFlicker.Length];
        for (int i = 0; i < lucesFlicker.Length; i++)
        {
            if (lucesFlicker[i] != null)
                flickerIntensidadesOriginales[i] = lucesFlicker[i].intensity;
        }
    }

    IEnumerator IniciarCapitulo()
    {
        if (VRHUD.Instance != null)
        {
            VRHUD.Instance.EstablecerObjetivos("Capitulo 1: Despertar", new[]
            {
                "Explorar la habitacion",
                "Recoger el diario",
                "Hablar con el guardia",
                "Encontrar la llave",
                "Abrir la puerta principal"
            });
        }

        if (musicaAmbiente != null && clipAmbienteDesespertar != null)
        {
            musicaAmbiente.clip = clipAmbienteDesespertar;
            musicaAmbiente.loop = true;
            musicaAmbiente.volume = 0f;
            musicaAmbiente.Play();
            StartCoroutine(FadeAudio(musicaAmbiente, 0f, 0.3f, 3f));
        }

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0.05f);

        yield return new WaitForSeconds(delayInicio);

        if (VRDialogueManager.Instance != null && dialogoIntroduccion != null)
        {
            VRDialogueManager.Instance.IniciarDialogo(dialogoIntroduccion);
            yield return new WaitUntil(() => !VRDialogueManager.Instance.InDialogue);
        }
        else
        {
            if (VRDialogueManager.Instance != null)
                VRDialogueManager.Instance.MostrarMensaje(
                    "Despiertas en una habitacion desconocida. Todo esta en silencio.", 4f);
            yield return new WaitForSeconds(4f);
        }

        capituloiniciado = true;
        StartCoroutine(FlickerLucesRealista());

        if (VRTeleportManager.Instance != null)
            VRTeleportManager.Instance.DesbloquearZona(zonaRecepcion);

        yield return new WaitForSeconds(delayDialogoGuardia);

        if (!diarioRecogido)
        {
            if (VRDialogueManager.Instance != null)
                VRDialogueManager.Instance.MostrarMensaje(
                    "Hay algo brillante en la mesa... un diario antiguo.", 3f);
        }
    }

    void Update()
    {
        if (!capituloiniciado) return;
        VerificarProgreso();
    }

    void VerificarProgreso()
    {
        if (GameManagerVR.Instance == null) return;

        if (!diarioRecogido && GameManagerVR.Instance.TieneFlag("diario_recogido"))
        {
            diarioRecogido = true;
            OnDiarioRecogido();
        }

        if (!llaveObtenida && GameManagerVR.Instance.TieneFlag("item_llave_oficina"))
        {
            llaveObtenida = true;
            OnLlaveObtenida();
        }
    }

    void OnDiarioRecogido()
    {
        if (VRHUD.Instance != null)
            VRHUD.Instance.CompletarObjetivo(1);

        if (VRTeleportManager.Instance != null)
            VRTeleportManager.Instance.DesbloquearZona(zonaPasillo);

        if (VRDialogueManager.Instance != null && dialogoDiario != null)
        {
            VRDialogueManager.Instance.IniciarDialogo(dialogoDiario);
        }
        else if (VRDialogueManager.Instance != null)
        {
            VRDialogueManager.Instance.MostrarMensaje(
                "El diario tiene paginas arrancadas. Una nota dice: 'No confies en nadie.'", 5f);
        }

        if (VRHUD.Instance != null)
            VRHUD.Instance.MostrarNotificacion("Diario recogido — Zona pasillo desbloqueada");

        StartCoroutine(TransicionTension());
    }

    IEnumerator TransicionTension()
    {
        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0.25f);

        flickerProgresion = 0.3f;

        if (luzPrincipal != null)
            StartCoroutine(TransicionLuz(luzPrincipal, colorLuzTension, intensidadLuzTension, transicionLuzDuracion));

        if (musicaAmbiente != null && clipAmbienteTension != null)
            yield return CrossfadeMusica(musicaAmbiente, clipAmbienteTension, 3f, 0.35f);
    }

    void OnLlaveObtenida()
    {
        if (VRHUD.Instance != null)
            VRHUD.Instance.CompletarObjetivo(3);

        if (VRTeleportManager.Instance != null)
            VRTeleportManager.Instance.DesbloquearZona(zonaOficina);

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje(
                "Llave obtenida. La puerta principal puede abrirse ahora.", 3f);

        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.PulsoTension(0.3f, 2f);

        flickerProgresion = 0.6f;
    }

    public void OnGuardiaInteractuado()
    {
        if (VRHUD.Instance != null)
            VRHUD.Instance.CompletarObjetivo(2);

        if (VRDialogueManager.Instance != null && dialogoGuardia != null && npcGuardia != null)
        {
            VRDialogueManager.Instance.IniciarDialogo(dialogoGuardia, npcGuardia.position);
        }
    }

    public void OnPuertaAbierta()
    {
        if (VRHUD.Instance != null)
            VRHUD.Instance.CompletarObjetivo(4);

        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.ActivarFlag("cap1_completado");

        StartCoroutine(TransicionCapitulo2());
    }

    IEnumerator TransicionCapitulo2()
    {
        if (AmbientSoundSystem.Instance != null)
            AmbientSoundSystem.Instance.EstablecerTension(0.5f);

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje("La puerta se abre. Un corredor oscuro te espera.", 3f);

        if (TerrorPsychologyManager.Instance != null)
            TerrorPsychologyManager.Instance.PulsoTerror(0.15f, 1f);

        yield return new WaitForSeconds(4f);

        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.AvanzarCapitulo();

        if (VRTeleportManager.Instance != null)
            VRTeleportManager.Instance.TeleportarAZona("zona_exploracion_inicio");
    }

    // === Iluminacion Realista ===

    IEnumerator FlickerLucesRealista()
    {
        while (capituloiniciado && lucesFlicker != null)
        {
            for (int i = 0; i < lucesFlicker.Length; i++)
            {
                if (lucesFlicker[i] == null) continue;

                float base_i = flickerIntensidadesOriginales[i];
                float ruido = Mathf.PerlinNoise(Time.time * 3f + i * 100f, i * 50f);
                float flickerAmount = flickerCurve.Evaluate(flickerProgresion);

                float intensidad = base_i * Mathf.Lerp(1f, ruido * flickerAmount, flickerProgresion);

                if (flickerProgresion > 0.4f && Random.value < 0.02f * flickerProgresion)
                    intensidad *= Random.Range(0f, 0.1f);

                lucesFlicker[i].intensity = Mathf.MoveTowards(
                    lucesFlicker[i].intensity, intensidad, Time.deltaTime * 8f);
            }

            yield return null;
        }
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

    // === Audio ===

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
        float volOriginal = source.volume;
        yield return FadeAudio(source, volOriginal, 0f, duracion * 0.4f);
        source.clip = nuevoClip;
        source.Play();
        yield return FadeAudio(source, 0f, volTarget, duracion * 0.6f);
    }
}
