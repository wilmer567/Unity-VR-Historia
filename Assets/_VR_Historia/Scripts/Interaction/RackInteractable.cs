using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class RackInteractable : MonoBehaviour
{
    [Header("Configuracion de Objetivo")]
    public string objectiveIdToComplete = "find_rack";
    public Key desktopInteractKey = Key.E;

    [Header("Evento de Terror al Interactuar")]
    public bool triggerScareOnInteract = true;
    public AudioSource scareSoundBehindPlayer;
    public Light[] lightsToBlackout;

    public UnityEvent onInteractEvent;

    private bool isCompleted = false;
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && !isCompleted &&
            Keyboard.current != null && Keyboard.current[desktopInteractKey].wasPressedThisFrame)
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (isCompleted) return;
        isCompleted = true;

        HorrorObjectiveHUD hud = FindFirstObjectByType<HorrorObjectiveHUD>();
        if (hud != null)
        {
            hud.CompleteObjective(objectiveIdToComplete);
        }

        onInteractEvent?.Invoke();

        if (triggerScareOnInteract)
        {
            StartCoroutine(MicroBlackoutEvent());
        }
    }

    private IEnumerator MicroBlackoutEvent()
    {
        foreach (Light l in lightsToBlackout)
            if (l != null) l.enabled = false;

        if (scareSoundBehindPlayer != null)
            scareSoundBehindPlayer.Play();

        yield return new WaitForSeconds(1.2f);

        foreach (Light l in lightsToBlackout)
            if (l != null) l.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}
