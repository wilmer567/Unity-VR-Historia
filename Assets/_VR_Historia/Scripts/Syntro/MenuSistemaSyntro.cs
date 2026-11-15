using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuSistemaSyntro : MonoBehaviour
{
    Canvas menuCanvas;
    GameObject panelInicio, panelPausa, panelFinal;
    CanvasGroup cgInicio, cgPausa, cgFinal;
    TextMeshProUGUI textoPulsante;
    bool juegoIniciado;
    bool enPausa;

    void Awake()
    {
        ConstruirUI();
    }

    void Start()
    {
        panelInicio.SetActive(true);
        panelPausa.SetActive(false);
        panelFinal.SetActive(false);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var sim = FindAnyObjectByType<SimulacionCompletaSyntro>();
        if (sim != null) sim.enabled = false;

        var gestor = GestorHistoriaSyntro.Instance;
        if (gestor != null)
        {
            gestor.enabled = false;
            gestor.OnFaseCambiada += OnFaseCambiada;
            gestor.OnHistoriaCompleta += MostrarPantallaFinal;
        }
    }

    void Update()
    {
        if (textoPulsante != null && panelInicio.activeSelf)
        {
            Color c = textoPulsante.color;
            c.a = 0.3f + 0.7f * Mathf.PingPong(Time.unscaledTime * 0.8f, 1f);
            textoPulsante.color = c;
        }

        if (juegoIniciado && !panelFinal.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            if (enPausa) Reanudar();
            else Pausar();
        }
    }

    // ===========================================
    //  ACCIONES DE MENU
    // ===========================================

    void IniciarJuego()
    {
        panelInicio.SetActive(false);
        juegoIniciado = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var gestor = GestorHistoriaSyntro.Instance;
        if (gestor != null) gestor.enabled = true;

        var sim = FindAnyObjectByType<SimulacionCompletaSyntro>();
        if (sim != null) sim.enabled = true;
    }

    void Pausar()
    {
        enPausa = true;
        panelPausa.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Reanudar()
    {
        enPausa = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnFaseCambiada(FaseSyntro fase)
    {
        if (fase == FaseSyntro.Completado)
            MostrarPantallaFinal();
    }

    void MostrarPantallaFinal()
    {
        panelFinal.SetActive(true);
        panelPausa.SetActive(false);
        enPausa = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(FadeInPanel(cgFinal, 2f));
    }

    IEnumerator FadeInPanel(CanvasGroup cg, float duracion)
    {
        cg.alpha = 0f;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(t / duracion);
            yield return null;
        }
        cg.alpha = 1f;
    }

    void OnDestroy()
    {
        var gestor = GestorHistoriaSyntro.Instance;
        if (gestor != null)
        {
            gestor.OnFaseCambiada -= OnFaseCambiada;
            gestor.OnHistoriaCompleta -= MostrarPantallaFinal;
        }
    }

    // ===========================================
    //  CONSTRUCCION DE UI
    // ===========================================

    void ConstruirUI()
    {
        GameObject canvasObj = new GameObject("MenuCanvas_Syntro");
        canvasObj.transform.SetParent(transform);
        menuCanvas = canvasObj.AddComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.sortingOrder = 200;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        ConstruirPanelInicio(canvasObj.transform);
        ConstruirPanelPausa(canvasObj.transform);
        ConstruirPanelFinal(canvasObj.transform);
    }

    // ------- PANEL INICIO -------

    void ConstruirPanelInicio(Transform padre)
    {
        panelInicio = CrearPanel("PanelInicio", padre, Color.black);
        cgInicio = panelInicio.GetComponent<CanvasGroup>();

        CrearTexto("Titulo", panelInicio.transform, "I.S.T.R.",
            72, Color.white, FontStyles.Bold,
            new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.75f));

        CrearTexto("Subtitulo", panelInicio.transform,
            "Syntro",
            24, new Color(0.6f, 0.6f, 0.6f), FontStyles.Italic,
            new Vector2(0.1f, 0.47f), new Vector2(0.9f, 0.55f));

        CrearTexto("Descripcion", panelInicio.transform,
            "SISTEMA DE SERVIDORES — NIVEL SUBTERRANEO",
            18, new Color(0.8f, 0.2f, 0.1f), FontStyles.Normal,
            new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.47f));

        CrearBoton("BtnIniciar", panelInicio.transform, "INICIAR",
            28, new Color(0.5f, 0.05f, 0.05f),
            new Vector2(0.35f, 0.18f), new Vector2(0.65f, 0.28f),
            IniciarJuego);

        textoPulsante = CrearTexto("TextoPulsante", panelInicio.transform,
            "Presiona INICIAR para comenzar",
            14, new Color(0.45f, 0.45f, 0.45f), FontStyles.Italic,
            new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.16f));
    }

    // ------- PANEL PAUSA -------

    void ConstruirPanelPausa(Transform padre)
    {
        panelPausa = CrearPanel("PanelPausa", padre, new Color(0f, 0f, 0f, 0.85f));
        cgPausa = panelPausa.GetComponent<CanvasGroup>();

        CrearTexto("TituloPausa", panelPausa.transform, "PAUSA",
            48, Color.white, FontStyles.Bold,
            new Vector2(0.2f, 0.6f), new Vector2(0.8f, 0.75f));

        CrearBoton("BtnContinuar", panelPausa.transform, "CONTINUAR",
            26, new Color(0.05f, 0.3f, 0.05f),
            new Vector2(0.35f, 0.4f), new Vector2(0.65f, 0.5f),
            Reanudar);

        CrearBoton("BtnSalirPausa", panelPausa.transform, "SALIR",
            26, new Color(0.4f, 0.05f, 0.05f),
            new Vector2(0.35f, 0.26f), new Vector2(0.65f, 0.36f),
            Salir);
    }

    // ------- PANEL FINAL -------

    void ConstruirPanelFinal(Transform padre)
    {
        panelFinal = CrearPanel("PanelFinal", padre, Color.black);
        cgFinal = panelFinal.GetComponent<CanvasGroup>();
        cgFinal.alpha = 0f;

        CrearTexto("TituloFinal", panelFinal.transform, "MISION COMPLETADA",
            52, new Color(0.2f, 0.9f, 0.3f), FontStyles.Bold,
            new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.82f));

        CrearTexto("CuerpoFinal", panelFinal.transform,
            "Has restaurado los sistemas de Syntro.\n\n" +
            "Los servidores estan operativos.\n" +
            "La anomalia ha sido contenida.\n\n" +
            "Pero... ¿que era esa transmision?",
            20, Color.white, FontStyles.Normal,
            new Vector2(0.15f, 0.35f), new Vector2(0.85f, 0.63f));

        CrearTexto("Creditos", panelFinal.transform,
            "Developed for Syntro",
            16, new Color(0.5f, 0.5f, 0.5f), FontStyles.Italic,
            new Vector2(0.2f, 0.24f), new Vector2(0.8f, 0.32f));

        CrearBoton("BtnSalirFinal", panelFinal.transform, "SALIR",
            26, new Color(0.4f, 0.05f, 0.05f),
            new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.18f),
            Salir);
    }

    // ===========================================
    //  HELPERS DE UI
    // ===========================================

    GameObject CrearPanel(string nombre, Transform padre, Color color)
    {
        GameObject obj = new GameObject(nombre, typeof(RectTransform));
        obj.transform.SetParent(padre, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;

        obj.AddComponent<CanvasGroup>();
        return obj;
    }

    TextMeshProUGUI CrearTexto(string nombre, Transform padre, string texto,
        int tamano, Color color, FontStyles estilo,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject(nombre, typeof(RectTransform));
        obj.transform.SetParent(padre, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.fontSize = tamano;
        tmp.color = color;
        tmp.fontStyle = estilo;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    void CrearBoton(string nombre, Transform padre, string texto,
        int tamano, Color bgColor,
        Vector2 anchorMin, Vector2 anchorMax,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = new GameObject(nombre, typeof(RectTransform));
        obj.transform.SetParent(padre, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        CrearTexto("Label", obj.transform, texto,
            tamano, Color.white, FontStyles.Bold,
            Vector2.zero, Vector2.one);
    }
}
