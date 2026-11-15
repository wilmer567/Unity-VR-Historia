using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VRIntroCinematica : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string escenaMenuPrincipal = "MenuPrincipalVR";
    [SerializeField] private float duracionTotal = 18f;

    [Header("Audio")]
    [SerializeField] private AudioClip latidoClip;
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField] private AudioClip impactoClip;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Image fondoNegro;
    private TextMeshProUGUI textoNarrativo;
    private TextMeshProUGUI textoLogo;
    private TextMeshProUGUI textoSubtitulo;
    private Image lineaDecorativa;
    private AudioSource audioLatido;
    private AudioSource audioAmbience;

    void Start()
    {
        CrearUI();
        CrearAudio();
        StartCoroutine(SecuenciaIntro());
    }

    void CrearUI()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        GameObject fondoObj = new GameObject("Fondo");
        fondoObj.transform.SetParent(transform, false);
        fondoNegro = fondoObj.AddComponent<Image>();
        fondoNegro.color = Color.black;
        RectTransform fondoRT = fondoObj.GetComponent<RectTransform>();
        fondoRT.anchorMin = Vector2.zero;
        fondoRT.anchorMax = Vector2.one;
        fondoRT.sizeDelta = Vector2.zero;

        textoNarrativo = CrearTexto("Narrativo", 26, new Color(0.6f, 0.65f, 0.7f),
            FontStyles.Italic, TextAlignmentOptions.Center,
            new Vector2(0.15f, 0.4f), new Vector2(0.85f, 0.6f));
        textoNarrativo.alpha = 0f;

        lineaDecorativa = CrearLinea("Linea",
            new Color(0.8f, 0.15f, 0.1f, 0f),
            new Vector2(0.35f, 0.48f), new Vector2(0.65f, 0.485f));

        textoLogo = CrearTexto("Logo", 90, Color.white,
            FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.7f));
        textoLogo.alpha = 0f;
        textoLogo.characterSpacing = 25f;

        textoSubtitulo = CrearTexto("Subtitulo", 22, new Color(0.8f, 0.15f, 0.1f),
            FontStyles.Normal, TextAlignmentOptions.Center,
            new Vector2(0.2f, 0.32f), new Vector2(0.8f, 0.42f));
        textoSubtitulo.alpha = 0f;
        textoSubtitulo.characterSpacing = 12f;
    }

    TextMeshProUGUI CrearTexto(string nombre, float size, Color color,
        FontStyles style, TextAlignmentOptions align, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(transform, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.Normal;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return tmp;
    }

    Image CrearLinea(string nombre, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(transform, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return img;
    }

    void CrearAudio()
    {
        audioLatido = gameObject.AddComponent<AudioSource>();
        audioLatido.spatialBlend = 0f;
        audioLatido.loop = true;
        audioLatido.volume = 0f;
        audioLatido.playOnAwake = false;

        audioAmbience = gameObject.AddComponent<AudioSource>();
        audioAmbience.spatialBlend = 0f;
        audioAmbience.loop = true;
        audioAmbience.volume = 0f;
        audioAmbience.playOnAwake = false;
    }

    IEnumerator SecuenciaIntro()
    {
        fondoNegro.color = Color.black;
        yield return new WaitForSeconds(1f);

        // --- Fase 1: Ambiente + latido creciente ---
        if (ambienceClip != null)
        {
            audioAmbience.clip = ambienceClip;
            audioAmbience.Play();
        }
        if (latidoClip != null)
        {
            audioLatido.clip = latidoClip;
            audioLatido.pitch = 0.7f;
            audioLatido.Play();
        }

        yield return StartCoroutine(FadeAudio(audioAmbience, 0f, 0.15f, 2f));

        // --- Fase 2: Texto narrativo 1 ---
        textoNarrativo.text = "En las profundidades de un edificio abandonado...";
        yield return StartCoroutine(FadeTexto(textoNarrativo, 0f, 1f, 1.5f));
        yield return StartCoroutine(FadeAudio(audioLatido, 0f, 0.2f, 1.5f));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeTexto(textoNarrativo, 1f, 0f, 1f));

        // --- Fase 3: Texto narrativo 2 ---
        textoNarrativo.text = "algo despertó que no debería existir.";
        yield return StartCoroutine(FadeTexto(textoNarrativo, 0f, 1f, 1.5f));
        StartCoroutine(FadeAudio(audioLatido, 0.2f, 0.4f, 2f));
        audioLatido.pitch = Mathf.Lerp(0.7f, 0.9f, 0.5f);
        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(FadeTexto(textoNarrativo, 1f, 0f, 0.8f));

        // --- Fase 4: Texto narrativo 3 ---
        textoNarrativo.text = "Ahora te toca a ti descubrir la verdad.";
        yield return StartCoroutine(FadeTexto(textoNarrativo, 0f, 1f, 1.2f));
        StartCoroutine(FadeAudio(audioLatido, 0.4f, 0.6f, 1.5f));
        audioLatido.pitch = 1f;
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeTexto(textoNarrativo, 1f, 0f, 0.6f));

        yield return new WaitForSeconds(0.5f);

        // --- Fase 5: Logo SYNTRO con impacto ---
        StartCoroutine(FadeAudio(audioLatido, 0.6f, 0f, 0.3f));

        if (impactoClip != null)
        {
            AudioSource.PlayClipAtPoint(impactoClip, Camera.main != null ?
                Camera.main.transform.position : Vector3.zero, 0.8f);
        }

        textoLogo.text = "SYNTRO";
        yield return StartCoroutine(LogoReveal(1.2f));

        // Línea decorativa roja
        yield return StartCoroutine(FadeImagen(lineaDecorativa, 0f, 0.9f, 0.5f));

        // Subtítulo
        textoSubtitulo.text = "T E R R O R   V R   E X P E R I E N C E";
        yield return StartCoroutine(FadeTexto(textoSubtitulo, 0f, 1f, 0.8f));

        yield return new WaitForSeconds(2.5f);

        // --- Fase 6: Fade out total y cargar menú ---
        StartCoroutine(FadeTexto(textoLogo, 1f, 0f, 1.5f));
        StartCoroutine(FadeTexto(textoSubtitulo, 1f, 0f, 1.5f));
        StartCoroutine(FadeImagen(lineaDecorativa, 0.9f, 0f, 1.5f));
        StartCoroutine(FadeAudio(audioAmbience, audioAmbience.volume, 0f, 1.5f));
        yield return new WaitForSeconds(2f);

        SceneManager.LoadSceneAsync(escenaMenuPrincipal);
    }

    IEnumerator LogoReveal(float duracion)
    {
        float t = 0f;
        textoLogo.alpha = 0f;

        float scaleInicio = 1.3f;
        float scaleFin = 1f;
        RectTransform rt = textoLogo.GetComponent<RectTransform>();

        while (t < duracion)
        {
            t += Time.deltaTime;
            float progreso = t / duracion;

            float curva = 1f - Mathf.Pow(1f - progreso, 3f);

            textoLogo.alpha = curva;
            float escala = Mathf.Lerp(scaleInicio, scaleFin, curva);
            rt.localScale = Vector3.one * escala;

            Color c = textoLogo.color;
            float glowIntensidad = Mathf.Sin(progreso * Mathf.PI) * 0.3f;
            textoLogo.color = new Color(
                Mathf.Min(1f, c.r + glowIntensidad),
                Mathf.Min(1f, c.g + glowIntensidad * 0.3f),
                Mathf.Min(1f, c.b + glowIntensidad * 0.3f),
                c.a);

            yield return null;
        }

        textoLogo.alpha = 1f;
        textoLogo.color = Color.white;
        rt.localScale = Vector3.one;
    }

    IEnumerator FadeTexto(TextMeshProUGUI texto, float from, float to, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float progreso = Mathf.SmoothStep(0f, 1f, t / duracion);
            texto.alpha = Mathf.Lerp(from, to, progreso);
            yield return null;
        }
        texto.alpha = to;
    }

    IEnumerator FadeImagen(Image img, float from, float to, float duracion)
    {
        float t = 0f;
        Color c = img.color;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float progreso = Mathf.SmoothStep(0f, 1f, t / duracion);
            c.a = Mathf.Lerp(from, to, progreso);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }

    IEnumerator FadeAudio(AudioSource source, float from, float to, float duracion)
    {
        if (source == null || source.clip == null) yield break;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, t / duracion);
            yield return null;
        }
        source.volume = to;
        if (to <= 0.01f) source.Stop();
    }
}
