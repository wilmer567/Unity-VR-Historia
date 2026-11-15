using UnityEngine;
using UnityEngine.Rendering;

public class PortalCamaraCuadro : MonoBehaviour
{
    public enum ModoPortal
    {
        VigilarEspalda,
        VentanaOtroPiso,
        ParadojaSubirBajar
    }

    [Header("Modo Psicológico")]
    public ModoPortal modo = ModoPortal.VigilarEspalda;

    [Header("Referencias")]
    public Transform jugador;
    public Renderer lienzoCuadro;
    [Tooltip("Solo para VentanaOtroPiso: punto donde se posiciona la cámara del portal")]
    public Transform puntoVistaOtroPiso;

    [Header("RenderTexture")]
    public int resolucion = 512;
    [Range(0, 2)]
    public int antiAliasing = 0;

    [Header("Rendimiento VR")]
    [Range(12, 30)]
    public int fpsObjetivo = 24;
    public float distanciaActivacion = 8f;
    public float distanciaRenderMaxima = 12f;

    [Header("Mezcla Foto Real")]
    [Tooltip("Velocidad de transicion foto->portal al acercarse")]
    public float velocidadMezcla = 2.5f;
    [Tooltip("Distancia a la que empieza la transicion a portal (entre activacion y este valor se interpola)")]
    public float distanciaInicioMezcla = 6f;
    [Tooltip("Distancia a la que el portal esta al 100%")]
    public float distanciaFullPortal = 2f;

    [Header("Layer Sombra")]
    [Tooltip("Layer extra que la cámara portal debe ver (ej. PortalSombra para silueta fantasma)")]
    public LayerMask layerExtraPortal;

    [Header("VigilarEspalda")]
    public float distanciaDetras = 2.5f;
    public float alturaOffset = 0.3f;
    public float suavizadoSeguimiento = 4f;
    public float retardoRotacion = 3f;

    [Header("ParadojaSubirBajar")]
    public float multiplicadorInversion = 1.5f;
    public float rollMaximo = 35f;
    public float pitchMaximo = 25f;
    public float alturaBaseGradas;
    public float alturaTopeGradas = 8f;

    Camera camaraPortal;
    RenderTexture renderTex;
    Material materialLienzo;
    float intervaloRender;
    float tiempoUltimoRender;
    Vector3 posicionSuavizada;
    Quaternion rotacionSuavizada;
    bool camaraActiva;
    bool usaShaderMezcla;
    float mezclaActual;

    static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");
    static readonly int EmissionMapID = Shader.PropertyToID("_EmissionMap");
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    static readonly int PortalTexID = Shader.PropertyToID("_PortalTex");
    static readonly int MezclaID = Shader.PropertyToID("_Mezcla");
    static readonly int BrilloEmisionID = Shader.PropertyToID("_BrilloEmision");

    void Awake()
    {
        CrearRenderTexture();
        CrearCamaraPortal();
        ConfigurarMaterialLienzo();
        intervaloRender = 1f / fpsObjetivo;
    }

    void Start()
    {
        if (jugador == null)
        {
            Camera cam = Camera.main;
            if (cam != null) jugador = cam.transform;
        }

        if (jugador != null)
        {
            posicionSuavizada = jugador.position - jugador.forward * distanciaDetras;
            rotacionSuavizada = jugador.rotation;
        }
    }

    void LateUpdate()
    {
        if (jugador == null || camaraPortal == null) return;

        float distancia = Vector3.Distance(jugador.position, transform.position);

        bool debeEstarActiva = distancia < distanciaActivacion;
        if (debeEstarActiva != camaraActiva)
        {
            camaraActiva = debeEstarActiva;
            camaraPortal.enabled = false;
        }

        ActualizarMezclaFotoPortal(distancia);

        if (!camaraActiva) return;

        if (Time.time - tiempoUltimoRender < intervaloRender) return;
        tiempoUltimoRender = Time.time;

        float proximidad = 1f - Mathf.Clamp01(distancia / distanciaRenderMaxima);

        switch (modo)
        {
            case ModoPortal.VigilarEspalda:
                ActualizarVigilarEspalda();
                break;
            case ModoPortal.VentanaOtroPiso:
                ActualizarVentanaOtroPiso();
                break;
            case ModoPortal.ParadojaSubirBajar:
                ActualizarParadojaSubirBajar(proximidad);
                break;
        }

        camaraPortal.Render();
    }

    void ActualizarMezclaFotoPortal(float distancia)
    {
        if (!usaShaderMezcla || materialLienzo == null) return;

        float mezclaObjetivo;
        if (distancia > distanciaInicioMezcla)
        {
            mezclaObjetivo = 0f;
        }
        else if (distancia < distanciaFullPortal)
        {
            mezclaObjetivo = 1f;
        }
        else
        {
            mezclaObjetivo = 1f - Mathf.Clamp01(
                (distancia - distanciaFullPortal) / (distanciaInicioMezcla - distanciaFullPortal));
        }

        mezclaActual = Mathf.Lerp(mezclaActual, mezclaObjetivo, Time.deltaTime * velocidadMezcla);
        materialLienzo.SetFloat(MezclaID, mezclaActual);

        float brilloExtra = mezclaActual * 0.4f;
        materialLienzo.SetFloat(BrilloEmisionID, 0.5f + brilloExtra);
    }

    void CrearRenderTexture()
    {
        int aa = antiAliasing == 0 ? 1 : (1 << antiAliasing);
        renderTex = new RenderTexture(resolucion, resolucion, 16, RenderTextureFormat.ARGB32);
        renderTex.antiAliasing = aa;
        renderTex.filterMode = FilterMode.Bilinear;
        renderTex.useMipMap = false;
        renderTex.Create();
    }

    void CrearCamaraPortal()
    {
        GameObject camGO = new GameObject("Camara_Portal");
        camGO.transform.SetParent(transform);
        camGO.transform.localPosition = Vector3.zero;
        camGO.transform.localRotation = Quaternion.identity;

        camaraPortal = camGO.AddComponent<Camera>();
        camaraPortal.targetTexture = renderTex;
        camaraPortal.fieldOfView = 60f;
        camaraPortal.nearClipPlane = 0.1f;
        camaraPortal.farClipPlane = 50f;
        camaraPortal.depth = -10;
        camaraPortal.enabled = false;
        camaraPortal.allowMSAA = antiAliasing > 0;

        if (layerExtraPortal != 0)
            camaraPortal.cullingMask |= layerExtraPortal;

        var additionalData = camGO.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        additionalData.renderShadows = false;
        additionalData.requiresColorOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
        additionalData.requiresDepthOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
        additionalData.renderPostProcessing = false;
    }

    void ConfigurarMaterialLienzo()
    {
        if (lienzoCuadro == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("Interior"))
                {
                    lienzoCuadro = child.GetComponent<Renderer>();
                    if (lienzoCuadro != null) break;
                }
            }
            if (lienzoCuadro == null)
                lienzoCuadro = GetComponentInChildren<Renderer>();
        }

        if (lienzoCuadro == null) return;

        materialLienzo = lienzoCuadro.material;

        usaShaderMezcla = materialLienzo.HasProperty(PortalTexID)
                          && materialLienzo.HasProperty(MezclaID);

        if (usaShaderMezcla)
        {
            materialLienzo.SetTexture(PortalTexID, renderTex);
            materialLienzo.SetFloat(MezclaID, 0f);
            mezclaActual = 0f;
        }
        else
        {
            materialLienzo.SetTexture(BaseMapID, renderTex);

            if (materialLienzo.HasProperty(EmissionMapID))
            {
                materialLienzo.EnableKeyword("_EMISSION");
                materialLienzo.SetTexture(EmissionMapID, renderTex);
                materialLienzo.SetColor(EmissionColorID, new Color(0.6f, 0.6f, 0.6f, 1f));
            }
        }
    }

    void ActualizarVigilarEspalda()
    {
        Vector3 detras = jugador.position
            - jugador.forward * distanciaDetras
            + Vector3.up * alturaOffset;

        posicionSuavizada = Vector3.Lerp(posicionSuavizada, detras,
            suavizadoSeguimiento * intervaloRender);

        Quaternion mirarAlJugador = Quaternion.LookRotation(
            jugador.position + Vector3.up * 0.2f - posicionSuavizada);

        rotacionSuavizada = Quaternion.Slerp(rotacionSuavizada, mirarAlJugador,
            retardoRotacion * intervaloRender);

        camaraPortal.transform.position = posicionSuavizada;
        camaraPortal.transform.rotation = rotacionSuavizada;
    }

    void ActualizarVentanaOtroPiso()
    {
        if (puntoVistaOtroPiso == null) return;

        camaraPortal.transform.position = puntoVistaOtroPiso.position;
        camaraPortal.transform.rotation = puntoVistaOtroPiso.rotation;
    }

    void ActualizarParadojaSubirBajar(float proximidad)
    {
        float alturaJugador = jugador.position.y;
        float progreso = Mathf.Clamp01(
            (alturaJugador - alturaBaseGradas) / (alturaTopeGradas - alturaBaseGradas));

        float rollInvertido = progreso * rollMaximo * multiplicadorInversion;
        float pitchInvertido = progreso * pitchMaximo * multiplicadorInversion;

        rollInvertido = Mathf.Clamp(rollInvertido * proximidad, -rollMaximo, rollMaximo);
        pitchInvertido = Mathf.Clamp(pitchInvertido * proximidad, -pitchMaximo, pitchMaximo);

        Vector3 posEnfrente = jugador.position + jugador.forward * 1.5f;
        camaraPortal.transform.position = posEnfrente + Vector3.up * 0.5f;

        Quaternion baseRot = Quaternion.LookRotation(
            jugador.position - camaraPortal.transform.position);
        Quaternion inversion = Quaternion.Euler(-pitchInvertido, 0f, rollInvertido);

        camaraPortal.transform.rotation = baseRot * inversion;
    }

    void OnDestroy()
    {
        if (renderTex != null)
        {
            renderTex.Release();
            Destroy(renderTex);
        }

        if (materialLienzo != null)
            Destroy(materialLienzo);
    }

    void OnValidate()
    {
        intervaloRender = 1f / Mathf.Max(fpsObjetivo, 1);
    }
}
