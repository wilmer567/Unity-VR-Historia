using UnityEngine;
using UnityEngine.Events;

public enum InteractionType
{
    Grab,
    Inspect,
    Use,
    Trigger
}

[System.Serializable]
public class HapticPreset
{
    [Range(0f, 1f)] public float amplitude = 0.3f;
    [Range(0f, 2f)] public float duration = 0.1f;
    public bool doublePulse;
    [Range(0f, 0.5f)] public float doublePulseDelay = 0.1f;
}

[CreateAssetMenu(fileName = "NewInteractionProfile", menuName = "VR Historia/Interaction Profile")]
public class InteractionProfile : ScriptableObject
{
    [Header("Tipo y Comportamiento")]
    public InteractionType interactionType = InteractionType.Grab;

    [Header("Feedback Sensorial")]
    public HapticPreset hapticFeedback;
    public AudioClip audioClip;
    public AudioClip audioClipSecundario;

    [Header("Requisitos")]
    public string requiredItemID;
    public string flagAlCompletar;

    [Header("UI")]
    public string descripcionUI;
    [TextArea(2, 4)] public string mensajeInspeccion;

    [Header("Evento")]
    public UnityEvent onInteract;
}
