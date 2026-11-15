using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class VRMenuPrincipal : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button btnNuevaPartida;
    [SerializeField] private Button btnContinuar;
    [SerializeField] private Button btnSimulacionSyntro;
    [SerializeField] private Button btnCapitulo1;
    [SerializeField] private Button btnCapitulo2;
    [SerializeField] private Button btnCapitulo3;
    [SerializeField] private Button btnSalir;

    [Header("Paneles")]
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelCapitulos;

    [Header("Info")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Escenas")]
    [SerializeField] private string escenaCapitulo1 = "HistoriaCapitulo1_Despertar";
    [SerializeField] private string escenaCapitulo2 = "HistoriaCapitulo2_Exploracion";
    [SerializeField] private string escenaCapitulo3 = "HistoriaCapitulo3_Revelacion";
    [SerializeField] private string escenaSyntro = "SimulacionSyntro";

    [Header("Fade")]
    [SerializeField] private VRFadeScreen fadeScreen;

    void Start()
    {
        if (btnNuevaPartida != null)
            btnNuevaPartida.onClick.AddListener(NuevaPartida);
        if (btnContinuar != null)
            btnContinuar.onClick.AddListener(ContinuarPartida);
        if (btnSimulacionSyntro != null)
            btnSimulacionSyntro.onClick.AddListener(IniciarSyntro);
        if (btnCapitulo1 != null)
            btnCapitulo1.onClick.AddListener(() => CargarCapitulo(escenaCapitulo1, Capitulo.Despertar));
        if (btnCapitulo2 != null)
            btnCapitulo2.onClick.AddListener(() => CargarCapitulo(escenaCapitulo2, Capitulo.Exploracion));
        if (btnCapitulo3 != null)
            btnCapitulo3.onClick.AddListener(() => CargarCapitulo(escenaCapitulo3, Capitulo.Revelacion));
        if (btnSalir != null)
            btnSalir.onClick.AddListener(Salir);

        ActualizarEstadoContinuar();

        if (panelCapitulos != null)
            panelCapitulos.SetActive(false);
    }

    void ActualizarEstadoContinuar()
    {
        bool tieneProgreso = PlayerPrefs.HasKey("VR_Historia_Progress");
        if (btnContinuar != null)
            btnContinuar.interactable = tieneProgreso;

        if (statusText != null && tieneProgreso)
        {
            var data = JsonUtility.FromJson<ProgressData>(PlayerPrefs.GetString("VR_Historia_Progress"));
            if (data != null)
                statusText.text = $"Ultimo guardado: {data.fechaGuardado}\nCapitulo: {data.capituloActual}";
        }
    }

    void NuevaPartida()
    {
        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.BorrarProgreso();

        StartCoroutine(CargarEscena(escenaCapitulo1));
    }

    void ContinuarPartida()
    {
        if (GameManagerVR.Instance != null)
        {
            GameManagerVR.Instance.CargarProgreso();
            Capitulo cap = GameManagerVR.Instance.CapituloActual;
            string escena = cap switch
            {
                Capitulo.Despertar => escenaCapitulo1,
                Capitulo.Exploracion => escenaCapitulo2,
                Capitulo.Revelacion => escenaCapitulo3,
                _ => escenaCapitulo1
            };
            StartCoroutine(CargarEscena(escena));
        }
    }

    void IniciarSyntro()
    {
        StartCoroutine(CargarEscena(escenaSyntro));
    }

    void CargarCapitulo(string escena, Capitulo capitulo)
    {
        if (GameManagerVR.Instance != null)
            GameManagerVR.Instance.EstablecerCapitulo(capitulo);
        StartCoroutine(CargarEscena(escena));
    }

    IEnumerator CargarEscena(string nombre)
    {
        if (fadeScreen != null)
            yield return fadeScreen.FadeOut(1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nombre);
        while (!asyncLoad.isDone)
            yield return null;
    }

    public void MostrarPanelCapitulos()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (panelCapitulos != null) panelCapitulos.SetActive(true);
    }

    public void VolverAPrincipal()
    {
        if (panelCapitulos != null) panelCapitulos.SetActive(false);
        if (panelPrincipal != null) panelPrincipal.SetActive(true);
    }

    void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
