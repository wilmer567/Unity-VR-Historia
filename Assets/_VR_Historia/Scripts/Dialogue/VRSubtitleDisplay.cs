using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VRSubtitleDisplay : MonoBehaviour
{
    [Header("Panel de Subtitulos")]
    [SerializeField] private Canvas subtitleCanvas;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image emotionIcon;

    [Header("Panel de Opciones")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Panel de Mensajes")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Configuracion")]
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private bool useTypewriter = true;

    private Coroutine typewriterCoroutine;
    private Coroutine messageCoroutine;

    void Start()
    {
        OcultarTodo();
    }

    public void MostrarSubtitulo(string speaker, string text, EmotionType emotion)
    {
        if (subtitleCanvas != null)
            subtitleCanvas.gameObject.SetActive(true);

        if (speakerText != null)
            speakerText.text = speaker ?? "";

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        if (useTypewriter && dialogueText != null)
        {
            typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
        }
        else if (dialogueText != null)
        {
            dialogueText.text = text;
        }

        OcultarOpciones();
    }

    IEnumerator TypewriterEffect(string text)
    {
        if (dialogueText == null) yield break;

        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    public void MostrarOpciones(List<DialogueChoice> choices, Action<DialogueChoice> onSelected)
    {
        OcultarOpciones();
        if (choicesContainer == null || choiceButtonPrefab == null) return;

        choicesContainer.gameObject.SetActive(true);

        foreach (var choice in choices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = choice.label;

            var btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                var capturedChoice = choice;
                btn.onClick.AddListener(() =>
                {
                    onSelected?.Invoke(capturedChoice);
                    OcultarOpciones();
                });
            }
        }
    }

    public void MostrarMensajeTemporal(string text, float duration)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(MensajeTemporalCoroutine(text, duration));
    }

    IEnumerator MensajeTemporalCoroutine(string text, float duration)
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = text;
        }
        yield return new WaitForSeconds(duration);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    void OcultarOpciones()
    {
        if (choicesContainer == null) return;
        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);
        choicesContainer.gameObject.SetActive(false);
    }

    public void OcultarTodo()
    {
        if (subtitleCanvas != null)
            subtitleCanvas.gameObject.SetActive(false);
        if (messageText != null)
            messageText.gameObject.SetActive(false);
        OcultarOpciones();
    }
}
