using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

public class MenuSistemaSyntro : MonoBehaviour
{
    static readonly Color CRIMSON = new Color(0.863f, 0.149f, 0.149f);
    static readonly Color CRIMSON_DARK = new Color(0.27f, 0.04f, 0.04f);
    static readonly Color VOID_BG = new Color(0.016f, 0.016f, 0.02f);
    static readonly Color NEUTRAL_400 = new Color(0.64f, 0.64f, 0.64f);
    static readonly Color NEUTRAL_500 = new Color(0.45f, 0.45f, 0.45f);
    static readonly Color NEUTRAL_600 = new Color(0.35f, 0.35f, 0.35f);
    static readonly Color AMBER_500 = new Color(0.96f, 0.72f, 0.16f);

    Canvas menuCanvas;
    GameObject panelInicio, panelPausa, panelFinal;
    CanvasGroup cgInicio, cgPausa, cgFinal;
    bool juegoIniciado;
    bool enPausa;

    // Efectos visuales del inicio
    TextMeshProUGUI tituloGlitch, glitchClon1, glitchClon2;
    TextMeshProUGUI terminalTMP;
    TextMeshProUGUI latenciaTMP, oxigenoTMP;
    TextMeshProUGUI estadoConexion;
    Image scanlineOverlay, vignetteOverlay;
    Image[] hudCorners = new Image[4];
    List<RectTransform> particulas = new List<RectTransform>();
    GameObject panelTransicion;
    CanvasGroup cgTransicion;
    TextMeshProUGUI textoTransicion, textoTransicionSub;
    Button btnIniciar;
    TextMeshProUGUI btnIniciarLabel;
    int oxygenLevel = 64;
    float glitchTimer;
    bool glitchActivo;

    void Awake()
    {
        ConstruirUI();
    }

    void Start()
    {
        panelInicio.SetActive(true);
        panelPausa.SetActive(false);
        panelFinal.SetActive(false);
        panelTransicion.SetActive(false);

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

        StartCoroutine(TerminalTypewriter());
        StartCoroutine(TelemetriaLoop());
        StartCoroutine(ParticulasLoop());
        StartCoroutine(ScanlineFlicker());
    }

    void Update()
    {
        // Glitch del titulo
        if (tituloGlitch != null && panelInicio.activeSelf)
        {
            glitchTimer += Time.unscaledDeltaTime;
            if (!glitchActivo && Random.value < 0.003f)
            {
                glitchActivo = true;
                glitchTimer = 0f;
            }
            if (glitchActivo)
            {
                float offset = Random.Range(-4f, 4f);
                glitchClon1.rectTransform.anchoredPosition = new Vector2(2f + offset, Random.Range(-1f, 1f));
                glitchClon2.rectTransform.anchoredPosition = new Vector2(-2f - offset, Random.Range(-1f, 1f));
                glitchClon1.alpha = 0.7f;
                glitchClon2.alpha = 0.7f;

                if (glitchTimer > 0.15f)
                {
                    glitchActivo = false;
                    glitchClon1.alpha = 0f;
                    glitchClon2.alpha = 0f;
                    glitchClon1.rectTransform.anchoredPosition = Vector2.zero;
                    glitchClon2.rectTransform.anchoredPosition = Vector2.zero;
                }
            }
        }

        if (juegoIniciado && !panelFinal.activeSelf &&
            UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (enPausa) Reanudar();
            else Pausar();
        }
    }

    // ===========================================
    //  EFECTOS ANIMADOS (coroutines con unscaledTime)
    // ===========================================

    IEnumerator TerminalTypewriter()
    {
        yield return new WaitForSecondsRealtime(0.8f);

        string[] lineas = {
            "<color=#EF4444>> Verificando enlace con la superficie... [ERROR 503]</color>",
            "<color=#F59E0B>> ADVERTENCIA: Anomalia detectada en el rack de memoria 09.</color>",
            "<color=#E5E5E5>> La conexion externa se ha perdido. Algo sigue dentro de la red.</color>"
        };

        terminalTMP.text = "";
        foreach (string linea in lineas)
        {
            string textoPlano = linea;
            string acumulado = terminalTMP.text;
            string lineaSinTags = System.Text.RegularExpressions.Regex.Replace(linea, "<.*?>", "");

            // Construir caracter por caracter con tags de color
            string colorOpen = "";
            string colorClose = "";
            if (linea.StartsWith("<color="))
            {
                int cierre = linea.IndexOf('>');
                colorOpen = linea.Substring(0, cierre + 1);
                colorClose = "</color>";
                lineaSinTags = linea.Substring(cierre + 1);
                lineaSinTags = lineaSinTags.Replace("</color>", "");
            }

            for (int i = 0; i < lineaSinTags.Length; i++)
            {
                string parcial = lineaSinTags.Substring(0, i + 1);
                terminalTMP.text = acumulado + colorOpen + parcial + "<color=#EF4444>_</color>" + colorClose;
                yield return new WaitForSecondsRealtime(0.025f + Random.Range(0f, 0.025f));
            }

            terminalTMP.text = acumulado + colorOpen + lineaSinTags + colorClose + "\n";
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }

    IEnumerator TelemetriaLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(2.5f);
            if (latenciaTMP == null) continue;

            int ms = Random.Range(11, 29);
            latenciaTMP.text = $"LATENCIA VR: <color={(ms > 24 ? "#EF4444" : "#F87171")}>{ms}ms</color>";

            if (Random.value > 0.75f && oxygenLevel > 12)
            {
                oxygenLevel--;
                string colorO2 = oxygenLevel < 30 ? "#EF4444" : "#F59E0B";
                oxigenoTMP.text = $"OXIGENO SECTOR: <color={colorO2}>{oxygenLevel}%</color>";
            }
        }
    }

    IEnumerator ParticulasLoop()
    {
        while (true)
        {
            yield return null;
            foreach (var p in particulas)
            {
                if (p == null) continue;
                p.anchoredPosition += new Vector2(
                    Random.Range(-0.15f, 0.15f),
                    Random.Range(0.1f, 0.4f)
                );
                if (p.anchoredPosition.y > 600f)
                    p.anchoredPosition = new Vector2(Random.Range(-900f, 900f), -550f);
            }
        }
    }

    IEnumerator ScanlineFlicker()
    {
        while (true)
        {
            if (scanlineOverlay != null)
            {
                float alpha = 0.06f + Random.Range(0f, 0.04f);
                if (Random.value < 0.05f) alpha = 0.15f;
                Color c = scanlineOverlay.color;
                c.a = alpha;
                scanlineOverlay.color = c;
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    // ===========================================
    //  ACCIONES DE MENU
    // ===========================================

    void IniciarProtocolo()
    {
        StartCoroutine(SecuenciaInicio());
    }

    IEnumerator SecuenciaInicio()
    {
        btnIniciar.interactable = false;
        btnIniciarLabel.text = "CONECTANDO...";

        // Fade out del panel inicio
        float t = 0f;
        while (t < 0.8f)
        {
            t += Time.unscaledDeltaTime;
            cgInicio.alpha = 1f - Mathf.Clamp01(t / 0.8f);
            yield return null;
        }

        // Mostrar transicion
        panelTransicion.SetActive(true);
        cgTransicion.alpha = 0f;
        textoTransicion.text = "ESTABLECIENDO ENLACE NEURONAL...";
        textoTransicionSub.text = "DESCENDIENDO AL NIVEL SUBTERRANEO";

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime;
            cgTransicion.alpha = Mathf.Clamp01(t / 1f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1.5f);
        textoTransicionSub.text = "ENLACE COMPLETADO — BIENVENIDO A SYNTRO";
        textoTransicionSub.color = CRIMSON;

        yield return new WaitForSecondsRealtime(1f);

        // Iniciar juego real
        panelInicio.SetActive(false);
        panelTransicion.SetActive(false);
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
    //  CONSTRUCCION DE UI COMPLETA
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

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();
        }

        ConstruirPanelInicio(canvasObj.transform);
        ConstruirPanelPausa(canvasObj.transform);
        ConstruirPanelFinal(canvasObj.transform);
        ConstruirPanelTransicion(canvasObj.transform);
    }

    // ------- PANEL INICIO (estilo terminal subterranea) -------

    void ConstruirPanelInicio(Transform padre)
    {
        panelInicio = CrearPanel("PanelInicio", padre, VOID_BG);
        cgInicio = panelInicio.GetComponent<CanvasGroup>();

        // Vineta oscura (textura radial)
        vignetteOverlay = CrearImagenProcedural("Vignette", panelInicio.transform,
            Vector2.zero, Vector2.one, CrearTexturaVineta(512));

        // Scanlines CRT
        scanlineOverlay = CrearImagenProcedural("Scanlines", panelInicio.transform,
            Vector2.zero, Vector2.one, CrearTexturaScanlines(4, 1080));
        scanlineOverlay.color = new Color(1f, 1f, 1f, 0.08f);

        // Esquinas HUD
        CrearEsquinasHUD(panelInicio.transform);

        // --- BARRA SUPERIOR: TELEMETRIA ---
        estadoConexion = CrearTexto("EstadoConexion", panelInicio.transform,
            "<color=#EF4444>●</color>  CONEXION: INESTABLE",
            14, CRIMSON, FontStyles.Bold,
            new Vector2(0.02f, 0.93f), new Vector2(0.4f, 0.98f));
        estadoConexion.alignment = TextAlignmentOptions.Left;

        CrearTexto("NodoInfo", panelInicio.transform,
            "NODO: SUB-04 // PROFUNDIDAD -420M",
            11, NEUTRAL_600, FontStyles.Normal,
            new Vector2(0.02f, 0.9f), new Vector2(0.4f, 0.93f)).alignment = TextAlignmentOptions.Left;

        latenciaTMP = CrearTexto("Latencia", panelInicio.transform,
            "LATENCIA VR: <color=#F87171>12ms</color>",
            13, new Color(0.82f, 0.82f, 0.82f), FontStyles.Normal,
            new Vector2(0.65f, 0.93f), new Vector2(0.98f, 0.98f));
        latenciaTMP.alignment = TextAlignmentOptions.Right;

        oxigenoTMP = CrearTexto("Oxigeno", panelInicio.transform,
            $"OXIGENO SECTOR: <color=#F59E0B>{oxygenLevel}%</color>",
            11, NEUTRAL_500, FontStyles.Normal,
            new Vector2(0.65f, 0.9f), new Vector2(0.98f, 0.93f));
        oxigenoTMP.alignment = TextAlignmentOptions.Right;

        // --- BADGE DE ADVERTENCIA ---
        GameObject badgeObj = new GameObject("Badge", typeof(RectTransform));
        badgeObj.transform.SetParent(panelInicio.transform, false);
        var badgeRT = badgeObj.GetComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0.25f, 0.78f);
        badgeRT.anchorMax = new Vector2(0.75f, 0.83f);
        badgeRT.offsetMin = Vector2.zero; badgeRT.offsetMax = Vector2.zero;
        var badgeImg = badgeObj.AddComponent<Image>();
        badgeImg.color = new Color(0.27f, 0.04f, 0.04f, 0.4f);
        CrearTexto("BadgeText", badgeObj.transform,
            "⚠  CONTENCION DE SERVIDORES COMPROMETIDA",
            12, CRIMSON, FontStyles.Normal,
            Vector2.zero, Vector2.one);

        // --- TITULO SYNTRO CON GLITCH ---
        GameObject tituloContainer = new GameObject("TituloContainer", typeof(RectTransform));
        tituloContainer.transform.SetParent(panelInicio.transform, false);
        var tcRT = tituloContainer.GetComponent<RectTransform>();
        tcRT.anchorMin = new Vector2(0.05f, 0.55f);
        tcRT.anchorMax = new Vector2(0.95f, 0.78f);
        tcRT.offsetMin = Vector2.zero; tcRT.offsetMax = Vector2.zero;

        glitchClon1 = CrearTexto("GlitchR", tituloContainer.transform, "SYNTRO",
            90, new Color(1f, 0f, 0.24f, 0f), FontStyles.Bold,
            Vector2.zero, Vector2.one);
        glitchClon1.characterSpacing = 25f;

        glitchClon2 = CrearTexto("GlitchB", tituloContainer.transform, "SYNTRO",
            90, new Color(0f, 0.94f, 1f, 0f), FontStyles.Bold,
            Vector2.zero, Vector2.one);
        glitchClon2.characterSpacing = 25f;

        tituloGlitch = CrearTexto("Titulo", tituloContainer.transform, "SYNTRO",
            90, Color.white, FontStyles.Bold,
            Vector2.zero, Vector2.one);
        tituloGlitch.characterSpacing = 25f;

        // --- SUBTITULO ---
        CrearTexto("Subtitulo", panelInicio.transform,
            "Experiencia de Terror en Realidad Virtual",
            20, NEUTRAL_400, FontStyles.Italic,
            new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.56f));

        // --- TERMINAL LOG ---
        GameObject termBg = new GameObject("TerminalBG", typeof(RectTransform));
        termBg.transform.SetParent(panelInicio.transform, false);
        var termBgRT = termBg.GetComponent<RectTransform>();
        termBgRT.anchorMin = new Vector2(0.18f, 0.3f);
        termBgRT.anchorMax = new Vector2(0.82f, 0.49f);
        termBgRT.offsetMin = Vector2.zero; termBgRT.offsetMax = Vector2.zero;
        var termBgImg = termBg.AddComponent<Image>();
        termBgImg.color = new Color(0f, 0f, 0f, 0.75f);

        Outline termOutline = termBg.AddComponent<Outline>();
        termOutline.effectColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        termOutline.effectDistance = new Vector2(1, -1);

        CrearTexto("TermHeader", termBg.transform,
            "TERMINAL_SYS_LOG.SH                              <color=#EF4444>● REC</color>",
            10, NEUTRAL_500, FontStyles.Normal,
            new Vector2(0.03f, 0.78f), new Vector2(0.97f, 0.97f)).alignment = TextAlignmentOptions.Left;

        // Linea separadora del header
        GameObject lineaSep = new GameObject("LineaSep", typeof(RectTransform));
        lineaSep.transform.SetParent(termBg.transform, false);
        var lsRT = lineaSep.GetComponent<RectTransform>();
        lsRT.anchorMin = new Vector2(0.02f, 0.76f);
        lsRT.anchorMax = new Vector2(0.98f, 0.77f);
        lsRT.offsetMin = Vector2.zero; lsRT.offsetMax = Vector2.zero;
        var lsImg = lineaSep.AddComponent<Image>();
        lsImg.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);

        terminalTMP = CrearTexto("TerminalLog", termBg.transform, "",
            13, new Color(0.82f, 0.82f, 0.82f), FontStyles.Normal,
            new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.74f));
        terminalTMP.alignment = TextAlignmentOptions.TopLeft;
        terminalTMP.richText = true;
        terminalTMP.overflowMode = TextOverflowModes.Truncate;

        // --- BOTON INICIAR PROTOCOLO ---
        GameObject btnObj = new GameObject("BtnIniciar", typeof(RectTransform));
        btnObj.transform.SetParent(panelInicio.transform, false);
        var btnRT = btnObj.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.3f, 0.16f);
        btnRT.anchorMax = new Vector2(0.7f, 0.25f);
        btnRT.offsetMin = Vector2.zero; btnRT.offsetMax = Vector2.zero;

        var btnBgImg = btnObj.AddComponent<Image>();
        btnBgImg.color = new Color(0.5f, 0.11f, 0.11f, 0.7f);

        Outline btnOutline = btnObj.AddComponent<Outline>();
        btnOutline.effectColor = new Color(0.86f, 0.15f, 0.15f, 0.6f);
        btnOutline.effectDistance = new Vector2(2, -2);

        btnIniciar = btnObj.AddComponent<Button>();
        ColorBlock cb = btnIniciar.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.4f, 1.1f, 1.1f);
        cb.pressedColor = new Color(0.6f, 0.6f, 0.6f);
        cb.selectedColor = new Color(1.2f, 1f, 1f);
        btnIniciar.colors = cb;
        btnIniciar.onClick.AddListener(IniciarProtocolo);

        btnIniciarLabel = CrearTexto("BtnLabel", btnObj.transform, "INICIAR PROTOCOLO",
            22, Color.white, FontStyles.Bold,
            Vector2.zero, Vector2.one);
        btnIniciarLabel.characterSpacing = 8f;

        // --- HINT ---
        CrearTexto("Hint", panelInicio.transform,
            "Se recomienda el uso de auriculares // Ponte el visor VR",
            11, NEUTRAL_500, FontStyles.Normal,
            new Vector2(0.15f, 0.08f), new Vector2(0.85f, 0.14f));

        // --- BARRA INFERIOR ---
        CrearTexto("SysVer", panelInicio.transform,
            "SYS.VER 4.09.2 // KERNEL CORRUPTO",
            10, NEUTRAL_600, FontStyles.Normal,
            new Vector2(0.02f, 0.01f), new Vector2(0.35f, 0.05f)).alignment = TextAlignmentOptions.Left;

        var advertencia = CrearTexto("AdvInferior", panelInicio.transform,
            "NO MIRES ATRAS CUANDO CAIGA LA RED",
            10, new Color(0.45f, 0.04f, 0.04f), FontStyles.Bold,
            new Vector2(0.35f, 0.01f), new Vector2(0.65f, 0.05f));

        CrearTexto("Reloj", panelInicio.transform,
            "03:14:00 AM",
            10, NEUTRAL_600, FontStyles.Normal,
            new Vector2(0.75f, 0.01f), new Vector2(0.98f, 0.05f)).alignment = TextAlignmentOptions.Right;

        // --- PARTICULAS ROJAS ---
        CrearParticulas(panelInicio.transform, 35);
    }

    void CrearEsquinasHUD(Transform padre)
    {
        float size = 20f;
        float margin = 15f;
        Color cColor = new Color(CRIMSON.r, CRIMSON.g, CRIMSON.b, 0.55f);

        // Top-Left
        hudCorners[0] = CrearEsquina("HUD_TL", padre,
            new Vector2(margin, -margin), new Vector2(0, 1), new Vector2(0, 1),
            size, cColor, true, false, true, false);
        // Top-Right
        hudCorners[1] = CrearEsquina("HUD_TR", padre,
            new Vector2(-margin, -margin), new Vector2(1, 1), new Vector2(1, 1),
            size, cColor, true, false, false, true);
        // Bottom-Left
        hudCorners[2] = CrearEsquina("HUD_BL", padre,
            new Vector2(margin, margin), new Vector2(0, 0), new Vector2(0, 0),
            size, cColor, false, true, true, false);
        // Bottom-Right
        hudCorners[3] = CrearEsquina("HUD_BR", padre,
            new Vector2(-margin, margin), new Vector2(1, 0), new Vector2(1, 0),
            size, cColor, false, true, false, true);
    }

    Image CrearEsquina(string nombre, Transform padre, Vector2 pos,
        Vector2 anchorMin, Vector2 anchorMax, float size, Color color,
        bool top, bool bottom, bool left, bool right)
    {
        GameObject obj = new GameObject(nombre, typeof(RectTransform));
        obj.transform.SetParent(padre, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = pos;

        var img = obj.AddComponent<Image>();
        img.color = Color.clear;
        img.raycastTarget = false;

        float thickness = 2.5f;
        if (top) CrearLineaSimple("T", obj.transform, new Vector2(0, size / 2f - thickness / 2f), new Vector2(size, thickness), color);
        if (bottom) CrearLineaSimple("B", obj.transform, new Vector2(0, -size / 2f + thickness / 2f), new Vector2(size, thickness), color);
        if (left) CrearLineaSimple("L", obj.transform, new Vector2(-size / 2f + thickness / 2f, 0), new Vector2(thickness, size), color);
        if (right) CrearLineaSimple("R", obj.transform, new Vector2(size / 2f - thickness / 2f, 0), new Vector2(thickness, size), color);

        return img;
    }

    void CrearLineaSimple(string nombre, Transform padre, Vector2 pos, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(nombre, typeof(RectTransform));
        obj.transform.SetParent(padre, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    void CrearParticulas(Transform padre, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject p = new GameObject($"Particula_{i}", typeof(RectTransform));
            p.transform.SetParent(padre, false);
            var rt = p.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(Random.Range(-900f, 900f), Random.Range(-540f, 540f));
            float s = Random.Range(1.5f, 4f);
            rt.sizeDelta = new Vector2(s, s);
            var img = p.AddComponent<Image>();
            img.color = new Color(CRIMSON.r, CRIMSON.g, CRIMSON.b, Random.Range(0.1f, 0.45f));
            img.raycastTarget = false;
            particulas.Add(rt);
        }
    }

    // ------- PANEL TRANSICION -------

    void ConstruirPanelTransicion(Transform padre)
    {
        panelTransicion = CrearPanel("PanelTransicion", padre, Color.black);
        cgTransicion = panelTransicion.GetComponent<CanvasGroup>();
        cgTransicion.alpha = 0f;

        textoTransicion = CrearTexto("TransTxt", panelTransicion.transform,
            "ESTABLECIENDO ENLACE NEURONAL...",
            20, CRIMSON, FontStyles.Bold,
            new Vector2(0.15f, 0.5f), new Vector2(0.85f, 0.58f));
        textoTransicion.characterSpacing = 10f;

        textoTransicionSub = CrearTexto("TransSub", panelTransicion.transform,
            "DESCENDIENDO AL NIVEL SUBTERRANEO",
            13, NEUTRAL_500, FontStyles.Normal,
            new Vector2(0.2f, 0.43f), new Vector2(0.8f, 0.49f));
        textoTransicionSub.characterSpacing = 6f;
    }

    // ------- PANEL PAUSA -------

    void ConstruirPanelPausa(Transform padre)
    {
        panelPausa = CrearPanel("PanelPausa", padre, new Color(0f, 0f, 0f, 0.88f));
        cgPausa = panelPausa.GetComponent<CanvasGroup>();

        CrearTexto("TituloPausa", panelPausa.transform, "PAUSA",
            52, Color.white, FontStyles.Bold,
            new Vector2(0.2f, 0.6f), new Vector2(0.8f, 0.75f)).characterSpacing = 15f;

        CrearBoton("BtnContinuar", panelPausa.transform, "CONTINUAR",
            24, new Color(0.05f, 0.3f, 0.05f),
            new Vector2(0.32f, 0.4f), new Vector2(0.68f, 0.5f),
            Reanudar);

        CrearBoton("BtnSalirPausa", panelPausa.transform, "SALIR",
            24, new Color(0.4f, 0.05f, 0.05f),
            new Vector2(0.32f, 0.26f), new Vector2(0.68f, 0.36f),
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
            16, NEUTRAL_500, FontStyles.Italic,
            new Vector2(0.2f, 0.24f), new Vector2(0.8f, 0.32f));

        CrearBoton("BtnSalirFinal", panelFinal.transform, "SALIR",
            24, new Color(0.4f, 0.05f, 0.05f),
            new Vector2(0.32f, 0.08f), new Vector2(0.68f, 0.18f),
            Salir);
    }

    // ===========================================
    //  TEXTURAS PROCEDURALES
    // ===========================================

    Texture2D CrearTexturaVineta(int res)
    {
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float center = res / 2f;
        float maxDist = center * 0.7f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float norm = Mathf.Clamp01((dist - maxDist) / (center - maxDist));
                float alpha = Mathf.Pow(norm, 1.5f) * 0.92f;
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }
        tex.Apply();
        return tex;
    }

    Texture2D CrearTexturaScanlines(int lineHeight, int totalHeight)
    {
        Texture2D tex = new Texture2D(1, lineHeight * 2, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        for (int y = 0; y < lineHeight; y++)
            tex.SetPixel(0, y, new Color(0f, 0f, 0f, 0f));
        for (int y = lineHeight; y < lineHeight * 2; y++)
            tex.SetPixel(0, y, new Color(0f, 0f, 0f, 0.45f));
        tex.Apply();
        return tex;
    }

    Image CrearImagenProcedural(string nombre, Transform padre,
        Vector2 anchorMin, Vector2 anchorMax, Texture2D tex)
    {
        GameObject obj = new GameObject(nombre, typeof(RectTransform));
        obj.transform.SetParent(padre, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.sprite = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));
        img.type = (tex.wrapMode == TextureWrapMode.Repeat) ? Image.Type.Tiled : Image.Type.Sliced;
        img.raycastTarget = false;
        return img;
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

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(bgColor.r * 1.5f, bgColor.g * 1.5f, bgColor.b * 1.5f, 0.5f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        var label = CrearTexto("Label", obj.transform, texto,
            tamano, Color.white, FontStyles.Bold,
            Vector2.zero, Vector2.one);
        label.characterSpacing = 6f;
    }
}
