using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRInteractionManager : MonoBehaviour
{
    public static VRInteractionManager Instance { get; private set; }

    [Header("Configuracion")]
    [SerializeField] private LayerMask interactableLayer;

    [Header("Haptic Feedback")]
    [SerializeField] private float hoverHapticAmplitude = 0.1f;
    [SerializeField] private float hoverHapticDuration = 0.05f;
    [SerializeField] private float selectHapticAmplitude = 0.4f;
    [SerializeField] private float selectHapticDuration = 0.15f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegistrarInteractable(XRBaseInteractable interactable)
    {
        if (interactable == null) return;

        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnSelectEnter);
    }

    public void DesregistrarInteractable(XRBaseInteractable interactable)
    {
        if (interactable == null) return;

        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
        interactable.selectEntered.RemoveListener(OnSelectEnter);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        var vrObj = args.interactableObject.transform.GetComponent<VRInteractableObject>();
        if (vrObj != null) vrObj.OnHoverEnter();

        if (args.interactorObject is XRBaseInputInteractor inputInteractor)
            inputInteractor.SendHapticImpulse(hoverHapticAmplitude, hoverHapticDuration);
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        var vrObj = args.interactableObject.transform.GetComponent<VRInteractableObject>();
        if (vrObj != null) vrObj.OnHoverExit();
    }

    void OnSelectEnter(SelectEnterEventArgs args)
    {
        var vrObj = args.interactableObject.transform.GetComponent<VRInteractableObject>();
        if (vrObj != null) vrObj.Interactuar();

        if (args.interactorObject is XRBaseInputInteractor inputInteractor)
        {
            var vrObjProfile = vrObj?.Profile;
            if (vrObjProfile?.hapticFeedback != null)
            {
                var haptic = vrObjProfile.hapticFeedback;
                inputInteractor.SendHapticImpulse(haptic.amplitude, haptic.duration);
            }
            else
            {
                inputInteractor.SendHapticImpulse(selectHapticAmplitude, selectHapticDuration);
            }
        }
    }
}
