using System.Collections;
using UnityEngine;

public class SimulacionISTRController : MonoBehaviour
{
    [Header("=== Simulacion ISTR ===")]
    [SerializeField] private GestorHistoriaISTR gestorHistoria;

    [Header("Dialogos por Fase")]
    [SerializeField] private DialogueGraph dialogoIntroPiso1;
    [SerializeField] private DialogueGraph dialogoErrorServidor;
    [SerializeField] private DialogueGraph dialogoGradas;
    [SerializeField] private DialogueGraph dialogoPiso3;
    [SerializeField] private DialogueGraph dialogoClimax;

    [Header("Zonas de Teleport")]
    [SerializeField] private string zonaPiso1 = "istr_piso1";
    [SerializeField] private string zonaGradas = "istr_gradas";
    [SerializeField] private string zonaPiso3 = "istr_piso3";
    [SerializeField] private string zonaLaboratorio = "istr_laboratorio";

    [Header("Objetos Interactuables VR")]
    [SerializeField] private VRInteractableObject rackServidor;
    [SerializeField] private VRInteractableObject terminalError;
    [SerializeField] private VRInteractableObject palancaReinicio;
    [SerializeField] private VRInteractableObject computadoraAnomala;
    [SerializeField] private VRInteractableObject interruptorTransmision;

    [Header("UI")]
    [SerializeField] private VRFadeScreen fadeScreen;

    private bool suscrito;

    void Start()
    {
        SuscribirEventos();
        ConfigurarZonas();
        StartCoroutine(IniciarSimulacion());
    }

    void SuscribirEventos()
    {
        if (gestorHistoria == null)
            gestorHistoria = GestorHistoriaISTR.Instance;

        if (gestorHistoria != null)
        {
            gestorHistoria.OnFaseCambiada += OnFaseCambiada;
            gestorHistoria.OnMensaje += OnMensaje;
            gestorHistoria.OnObjetivoCompletado += OnObjetivoCompletado;
            gestorHistoria.OnHistoriaCompleta += OnHistoriaCompleta;
            suscrito = true;
        }
    }

    void ConfigurarZonas()
    {
        if (VRTeleportManager.Instance != null)
        {
            VRTeleportManager.Instance.DesbloquearZona(zonaPiso1);
        }
    }

    IEnumerator IniciarSimulacion()
    {
        if (VRHUD.Instance != null)
        {
            VRHUD.Instance.EstablecerObjetivos("Simulacion ISTR", new[]
            {
                "Piso 1: Localizar el rack averiado",
                "Piso 1: Leer el error de la terminal",
                "Piso 1: Reiniciar el rack manualmente",
                "Gradas: Subir al tercer piso",
                "Piso 3: Encontrar el laboratorio",
                "Piso 3: Investigar la computadora",
                "Piso 3: Cortar la transmision anomala"
            });
        }

        yield return new WaitForSeconds(1f);

        if (VRDialogueManager.Instance != null && dialogoIntroPiso1 != null)
        {
            VRDialogueManager.Instance.IniciarDialogo(dialogoIntroPiso1);
        }
    }

    void OnFaseCambiada(FaseISTR fase)
    {
        switch (fase)
        {
            case FaseISTR.Piso1_Exploracion:
                if (VRHUD.Instance != null)
                    VRHUD.Instance.MostrarNotificacion("Explora el cuarto de maquinas");
                if (AmbientSoundSystem.Instance != null)
                    AmbientSoundSystem.Instance.EstablecerTension(0.1f);
                break;

            case FaseISTR.Piso1_ErrorServidor:
                if (VRDialogueManager.Instance != null && dialogoErrorServidor != null)
                    VRDialogueManager.Instance.IniciarDialogo(dialogoErrorServidor);
                if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(1);
                if (AmbientSoundSystem.Instance != null)
                    AmbientSoundSystem.Instance.PulsoTension(0.2f, 2f);
                break;

            case FaseISTR.Piso1_ReinicioManual:
                if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(2);
                if (TerrorPsychologyManager.Instance != null)
                    TerrorPsychologyManager.Instance.PulsoTerror(0.15f, 0.5f);
                break;

            case FaseISTR.Piso1_SalidaGradas:
                if (VRTeleportManager.Instance != null)
                    VRTeleportManager.Instance.DesbloquearZona(zonaGradas);
                break;

            case FaseISTR.Gradas_Subida:
                if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(3);
                if (VRDialogueManager.Instance != null && dialogoGradas != null)
                    VRDialogueManager.Instance.IniciarDialogo(dialogoGradas);
                if (AmbientSoundSystem.Instance != null)
                    AmbientSoundSystem.Instance.EstablecerTension(0.3f);
                break;

            case FaseISTR.Gradas_Completado:
                if (VRTeleportManager.Instance != null)
                    VRTeleportManager.Instance.DesbloquearZona(zonaPiso3);
                break;

            case FaseISTR.Piso3_Entrada:
                if (VRDialogueManager.Instance != null && dialogoPiso3 != null)
                    VRDialogueManager.Instance.IniciarDialogo(dialogoPiso3);
                if (AmbientSoundSystem.Instance != null)
                    AmbientSoundSystem.Instance.EstablecerTension(0.5f);
                break;

            case FaseISTR.Piso3_Exploracion:
                if (VRTeleportManager.Instance != null)
                    VRTeleportManager.Instance.DesbloquearZona(zonaLaboratorio);
                break;

            case FaseISTR.Piso3_AnomaliaFinal:
                if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(5);
                if (VRDialogueManager.Instance != null && dialogoClimax != null)
                    VRDialogueManager.Instance.IniciarDialogo(dialogoClimax);
                if (AmbientSoundSystem.Instance != null)
                    AmbientSoundSystem.Instance.EstablecerTension(0.8f);
                if (TerrorPsychologyManager.Instance != null)
                    TerrorPsychologyManager.Instance.PulsoTerror(0.3f, 1f);
                break;

            case FaseISTR.Piso3_Climax:
                if (VRHUD.Instance != null) VRHUD.Instance.CompletarObjetivo(6);
                if (AmbientSoundSystem.Instance != null)
                    AmbientSoundSystem.Instance.EstablecerTension(1f);
                if (TerrorPsychologyManager.Instance != null)
                    TerrorPsychologyManager.Instance.ForzarPerdidaCordura(0.3f);
                break;
        }
    }

    void OnMensaje(string texto)
    {
        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje(texto, 4f);
    }

    void OnObjetivoCompletado(ObjetivoISTR objetivo)
    {
        if (objetivo.id == "localizar_rack" && VRHUD.Instance != null)
            VRHUD.Instance.CompletarObjetivo(0);
        else if (objetivo.id == "encontrar_lab" && VRHUD.Instance != null)
            VRHUD.Instance.CompletarObjetivo(4);
    }

    void OnHistoriaCompleta()
    {
        StartCoroutine(FinalizarSimulacion());
    }

    IEnumerator FinalizarSimulacion()
    {
        if (VRHUD.Instance != null)
            VRHUD.Instance.MostrarNotificacion("Simulacion ISTR completada");

        yield return new WaitForSeconds(5f);

        if (fadeScreen != null)
            yield return fadeScreen.FadeOut(2f);

        UnityEngine.SceneManagement.SceneManager.LoadScene("0_MainMenu_Scene");
    }

    // === Puentes para XR Interactables ===

    public void XR_RackInteractuado()
    {
        if (gestorHistoria != null)
            gestorHistoria.Evento_RackLocalizado();
    }

    public void XR_TerminalLeida()
    {
        if (gestorHistoria != null)
            gestorHistoria.Evento_ErrorLeido();
    }

    public void XR_ReinicioManual()
    {
        if (gestorHistoria != null)
            gestorHistoria.Evento_ReinicioManual();
    }

    public void XR_EntrarGradas()
    {
        if (gestorHistoria != null)
            gestorHistoria.Evento_JugadorEntraGradas();
    }

    public void XR_LaboratorioEncontrado()
    {
        if (gestorHistoria != null)
            gestorHistoria.Evento_LaboratorioEncontrado();
    }

    public void XR_ComputadoraInvestigada()
    {
        if (gestorHistoria != null)
            gestorHistoria.Evento_ComputadoraInvestigada();
    }

    public void XR_TransmisionCortada()
    {
        if (gestorHistoria != null)
            gestorHistoria.Evento_TransmisionCortada();
    }

    void OnDestroy()
    {
        if (suscrito && gestorHistoria != null)
        {
            gestorHistoria.OnFaseCambiada -= OnFaseCambiada;
            gestorHistoria.OnMensaje -= OnMensaje;
            gestorHistoria.OnObjetivoCompletado -= OnObjetivoCompletado;
            gestorHistoria.OnHistoriaCompleta -= OnHistoriaCompleta;
        }
    }
}
