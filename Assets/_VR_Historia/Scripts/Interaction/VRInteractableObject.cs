using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Collider))]
public class VRInteractableObject : MonoBehaviour
{
    [Header("Perfil")]
    [SerializeField] private InteractionProfile profile;

    [Header("Visual Feedback")]
    [SerializeField] private Material materialResaltado;
    [SerializeField] private float escalaResaltado = 1.05f;
    [SerializeField] private Color colorEmisionHover = new Color(0.3f, 0.5f, 1f);
    [SerializeField] private float emisionIntensidad = 0.5f;

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip sonidoHover;
    [SerializeField] private AudioClip sonidoDenegado;
    [SerializeField] private float volumenFeedback = 0.3f;

    [Header("Item (si es recogible)")]
    [SerializeField] private ItemVR itemData;

    [Header("Animacion Pickup")]
    [SerializeField] private float pickupDuracion = 0.4f;
    [SerializeField] private float pickupEscalaFinal = 0f;
    [SerializeField] private ParticleSystem particulasInteraccion;

    private Renderer objectRenderer;
    private Material materialOriginal;
    private Vector3 escalaOriginal;
    private bool interactuado;
    private bool hover;
    private AudioSource audioFeedback;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    public InteractionProfile Profile => profile;
    public bool Interactuado => interactuado;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
            materialOriginal = objectRenderer.material;
        escalaOriginal = transform.localScale;

        audioFeedback = GetComponent<AudioSource>();
        if (audioFeedback == null)
        {
            audioFeedback = gameObject.AddComponent<AudioSource>();
            audioFeedback.spatialBlend = 1f;
            audioFeedback.maxDistance = 8f;
            audioFeedback.playOnAwake = false;
        }
    }

    public void OnHoverEnter()
    {
        if (interactuado || hover) return;
        hover = true;

        if (objectRenderer != null && materialResaltado != null)
        {
            objectRenderer.material = materialResaltado;
        }
        else if (objectRenderer != null)
        {
            objectRenderer.material.EnableKeyword("_EMISSION");
            objectRenderer.material.SetColor(EmissionColor, colorEmisionHover * emisionIntensidad);
        }

        transform.localScale = escalaOriginal * escalaResaltado;

        if (sonidoHover != null && audioFeedback != null)
            audioFeedback.PlayOneShot(sonidoHover, volumenFeedback);
    }

    public void OnHoverExit()
    {
        if (interactuado || !hover) return;
        hover = false;

        if (objectRenderer != null && materialOriginal != null)
        {
            objectRenderer.material = materialOriginal;
        }
        else if (objectRenderer != null)
        {
            objectRenderer.material.SetColor(EmissionColor, Color.black);
        }

        transform.localScale = escalaOriginal;
    }

    public void Interactuar()
    {
        if (interactuado || profile == null) return;

        if (!string.IsNullOrEmpty(profile.requiredItemID))
        {
            if (InventarioVR.Instance == null || !InventarioVR.Instance.TieneItem(profile.requiredItemID))
            {
                ReproducirDenegado();
                return;
            }
        }

        if (profile.audioClip != null && audioFeedback != null)
        {
            audioFeedback.PlayOneShot(profile.audioClip);
        }

        if (particulasInteraccion != null)
            particulasInteraccion.Play();

        switch (profile.interactionType)
        {
            case InteractionType.Grab:
                EjecutarGrab();
                break;
            case InteractionType.Inspect:
                EjecutarInspect();
                break;
            case InteractionType.Use:
                EjecutarUse();
                break;
            case InteractionType.Trigger:
                EjecutarTrigger();
                break;
        }

        if (!string.IsNullOrEmpty(profile.flagAlCompletar) && GameManagerVR.Instance != null)
            GameManagerVR.Instance.ActivarFlag(profile.flagAlCompletar);

        profile.onInteract?.Invoke();

        if (profile.interactionType == InteractionType.Trigger ||
            profile.interactionType == InteractionType.Grab)
            interactuado = true;
    }

    void EjecutarGrab()
    {
        if (itemData != null && InventarioVR.Instance != null)
        {
            InventarioVR.Instance.AgregarItem(itemData);
            StartCoroutine(AnimarPickup());
        }
    }

    IEnumerator AnimarPickup()
    {
        float t = 0f;
        Vector3 escalaInicio = transform.localScale;
        Vector3 posInicio = transform.position;
        Vector3 posTarget = Camera.main != null
            ? Camera.main.transform.position + Camera.main.transform.forward * 0.3f
            : posInicio + Vector3.up * 0.5f;

        while (t < pickupDuracion)
        {
            t += Time.deltaTime;
            float progreso = t / pickupDuracion;
            float curva = 1f - Mathf.Pow(1f - progreso, 3f);

            transform.localScale = Vector3.Lerp(escalaInicio, escalaInicio * pickupEscalaFinal, curva);
            transform.position = Vector3.Lerp(posInicio, posTarget, curva);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    void EjecutarInspect()
    {
        if (VRDialogueManager.Instance != null && !string.IsNullOrEmpty(profile.mensajeInspeccion))
        {
            VRDialogueManager.Instance.MostrarMensaje(profile.mensajeInspeccion, 4f);
        }
    }

    void EjecutarUse()
    {
        if (VRDialogueManager.Instance != null && !string.IsNullOrEmpty(profile.descripcionUI))
        {
            VRDialogueManager.Instance.MostrarMensaje(profile.descripcionUI, 3f);
        }
    }

    void EjecutarTrigger()
    {
        if (TerrorPsychologyManager.Instance != null)
        {
            TerrorPsychologyManager.Instance.PulsoTerror(0.2f, 0.3f);
        }
    }

    void ReproducirDenegado()
    {
        if (sonidoDenegado != null && audioFeedback != null)
            audioFeedback.PlayOneShot(sonidoDenegado, volumenFeedback);

        if (VRDialogueManager.Instance != null)
            VRDialogueManager.Instance.MostrarMensaje($"Necesitas: {profile.requiredItemID}", 2f);
    }
}
