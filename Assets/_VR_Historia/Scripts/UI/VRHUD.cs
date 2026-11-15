using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VRHUD : MonoBehaviour
{
    public static VRHUD Instance { get; private set; }

    [Header("Panel Objetivos")]
    [SerializeField] private Transform objetivosContainer;
    [SerializeField] private GameObject objetivoPrefab;
    [SerializeField] private TextMeshProUGUI tituloCapitulo;

    [Header("Panel Notificaciones")]
    [SerializeField] private TextMeshProUGUI notificacionText;
    [SerializeField] private CanvasGroup notificacionGroup;
    [SerializeField] private float notificacionDuration = 3f;

    [Header("Panel Inventario")]
    [SerializeField] private Transform inventarioContainer;
    [SerializeField] private GameObject inventarioItemPrefab;

    [Header("Indicador de Zona")]
    [SerializeField] private TextMeshProUGUI zonaText;

    private Coroutine notificacionCoroutine;
    private readonly List<GameObject> objetivoUIItems = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (notificacionGroup != null)
            notificacionGroup.alpha = 0f;

        SuscribirEventos();
    }

    void SuscribirEventos()
    {
        if (GameManagerVR.Instance != null)
        {
            GameManagerVR.Instance.OnCapituloCambiado += OnCapituloCambiado;
            GameManagerVR.Instance.OnFlagActivado += OnFlagActivado;
        }

        if (VRTeleportManager.Instance != null)
        {
            VRTeleportManager.Instance.OnZoneChanged += OnZonaCambiada;
            VRTeleportManager.Instance.OnZoneUnlocked += OnZonaDesbloqueada;
        }

        if (InventarioVR.Instance != null)
        {
            InventarioVR.Instance.OnItemAgregado += OnItemAgregado;
            InventarioVR.Instance.OnItemRemovido += OnItemRemovido;
        }
    }

    void OnCapituloCambiado(Capitulo cap)
    {
        if (tituloCapitulo != null)
            tituloCapitulo.text = $"Capitulo {(int)cap}: {cap}";

        MostrarNotificacion($"Capitulo {(int)cap}: {cap}");
    }

    void OnFlagActivado(string flag)
    {
        ActualizarObjetivos();
    }

    void OnZonaCambiada(TeleportZone zone)
    {
        if (zonaText != null && zone != null)
            zonaText.text = zone.displayName;
    }

    void OnZonaDesbloqueada(TeleportZone zone)
    {
        if (zone != null)
            MostrarNotificacion($"Zona desbloqueada: {zone.displayName}");
    }

    void OnItemAgregado(ItemVR item)
    {
        MostrarNotificacion($"Obtenido: {item.nombre}");
        ActualizarInventarioUI();
    }

    void OnItemRemovido(ItemVR item)
    {
        ActualizarInventarioUI();
    }

    public void EstablecerObjetivos(string titulo, string[] objetivos)
    {
        if (tituloCapitulo != null)
            tituloCapitulo.text = titulo;

        LimpiarObjetivos();
        if (objetivosContainer == null || objetivoPrefab == null) return;

        foreach (var obj in objetivos)
        {
            var go = Instantiate(objetivoPrefab, objetivosContainer);
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = $"  {obj}";
            objetivoUIItems.Add(go);
        }
    }

    public void CompletarObjetivo(int index)
    {
        if (index < 0 || index >= objetivoUIItems.Count) return;
        var tmp = objetivoUIItems[index].GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = tmp.text.Replace("  ", "  ");
            tmp.fontStyle = FontStyles.Strikethrough;
        }
    }

    void LimpiarObjetivos()
    {
        foreach (var go in objetivoUIItems)
            if (go != null) Destroy(go);
        objetivoUIItems.Clear();
    }

    void ActualizarObjetivos()
    {
        // Los controladores de capítulo manejan la actualización específica
    }

    void ActualizarInventarioUI()
    {
        if (inventarioContainer == null || inventarioItemPrefab == null) return;

        foreach (Transform child in inventarioContainer)
            Destroy(child.gameObject);

        if (InventarioVR.Instance == null) return;

        foreach (var item in InventarioVR.Instance.Items)
        {
            var go = Instantiate(inventarioItemPrefab, inventarioContainer);
            var img = go.GetComponent<Image>();
            if (img != null && item.icono != null) img.sprite = item.icono;
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = item.nombre;
        }
    }

    public void MostrarNotificacion(string texto)
    {
        if (notificacionCoroutine != null)
            StopCoroutine(notificacionCoroutine);
        notificacionCoroutine = StartCoroutine(NotificacionCoroutine(texto));
    }

    IEnumerator NotificacionCoroutine(string texto)
    {
        if (notificacionText != null)
            notificacionText.text = texto;

        if (notificacionGroup != null)
        {
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                notificacionGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
                yield return null;
            }

            yield return new WaitForSeconds(notificacionDuration);

            t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                notificacionGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.5f);
                yield return null;
            }
            notificacionGroup.alpha = 0f;
        }
    }

    void OnDestroy()
    {
        if (GameManagerVR.Instance != null)
        {
            GameManagerVR.Instance.OnCapituloCambiado -= OnCapituloCambiado;
            GameManagerVR.Instance.OnFlagActivado -= OnFlagActivado;
        }
    }
}
