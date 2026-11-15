using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRDialogueManager : MonoBehaviour
{
    public static VRDialogueManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource voiceSource;

    [Header("UI")]
    [SerializeField] private VRSubtitleDisplay subtitleDisplay;

    [Header("Configuracion")]
    [SerializeField] private float delayEntreNodos = 0.5f;

    private DialogueGraph currentGraph;
    private DialogueNode currentNode;
    private bool inDialogue;
    private Coroutine dialogueCoroutine;

    public bool InDialogue => inDialogue;
    public event Action<DialogueNode> OnNodeStarted;
    public event Action<DialogueGraph> OnDialogueStarted;
    public event Action OnDialogueEnded;
    public event Action<string> OnFlagSet;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void IniciarDialogo(DialogueGraph graph)
    {
        if (graph == null || inDialogue) return;

        currentGraph = graph;
        inDialogue = true;
        OnDialogueStarted?.Invoke(graph);

        DialogueNode start = graph.GetStartNode();
        if (start != null)
        {
            dialogueCoroutine = StartCoroutine(ProcesarNodo(start));
        }
        else
        {
            TerminarDialogo();
        }
    }

    public void IniciarDialogo(DialogueGraph graph, Vector3 posicionNPC)
    {
        if (voiceSource != null)
        {
            voiceSource.transform.position = posicionNPC;
            voiceSource.spatialBlend = 1f;
        }
        IniciarDialogo(graph);
    }

    IEnumerator ProcesarNodo(DialogueNode node)
    {
        currentNode = node;
        OnNodeStarted?.Invoke(node);

        if (subtitleDisplay != null)
            subtitleDisplay.MostrarSubtitulo(node.speakerName, node.text, node.emotion);

        if (voiceSource != null && node.audioClip != null)
        {
            voiceSource.clip = node.audioClip;
            voiceSource.Play();
        }

        if (!string.IsNullOrEmpty(node.flagToSet) && GameManagerVR.Instance != null)
        {
            GameManagerVR.Instance.ActivarFlag(node.flagToSet);
            OnFlagSet?.Invoke(node.flagToSet);
        }

        if (node.choices != null && node.choices.Count > 0)
        {
            var validChoices = FiltrarOpciones(node.choices);
            if (validChoices.Count > 0)
            {
                if (subtitleDisplay != null)
                    subtitleDisplay.MostrarOpciones(validChoices, OnChoiceSelected);
                yield break;
            }
        }

        float waitTime = node.autoAdvanceDelay;
        if (waitTime <= 0f && node.audioClip != null)
            waitTime = node.audioClip.length + 0.5f;
        else if (waitTime <= 0f)
            waitTime = Mathf.Max(2f, node.text.Length * 0.06f);

        yield return new WaitForSeconds(waitTime);

        if (!string.IsNullOrEmpty(node.nextNodeID))
        {
            DialogueNode next = currentGraph.GetNode(node.nextNodeID);
            if (next != null)
            {
                yield return new WaitForSeconds(delayEntreNodos);
                dialogueCoroutine = StartCoroutine(ProcesarNodo(next));
                yield break;
            }
        }

        TerminarDialogo();
    }

    List<DialogueChoice> FiltrarOpciones(List<DialogueChoice> choices)
    {
        var result = new List<DialogueChoice>();
        foreach (var choice in choices)
        {
            if (string.IsNullOrEmpty(choice.requiredFlag) ||
                (GameManagerVR.Instance != null && GameManagerVR.Instance.TieneFlag(choice.requiredFlag)))
            {
                result.Add(choice);
            }
        }
        return result;
    }

    void OnChoiceSelected(DialogueChoice choice)
    {
        if (choice == null || currentGraph == null) return;

        DialogueNode next = currentGraph.GetNode(choice.nextNodeID);
        if (next != null)
        {
            dialogueCoroutine = StartCoroutine(ProcesarNodo(next));
        }
        else
        {
            TerminarDialogo();
        }
    }

    public void TerminarDialogo()
    {
        if (dialogueCoroutine != null)
            StopCoroutine(dialogueCoroutine);

        if (voiceSource != null && voiceSource.isPlaying)
            StartCoroutine(FadeOutVoice(voiceSource, 0.3f));

        if (subtitleDisplay != null)
            subtitleDisplay.OcultarTodo();

        currentGraph = null;
        currentNode = null;
        inDialogue = false;
        OnDialogueEnded?.Invoke();
    }

    System.Collections.IEnumerator FadeOutVoice(AudioSource source, float duracion)
    {
        float volInicial = source.volume;
        float t = 0f;
        while (t < duracion && source.isPlaying)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(volInicial, 0f, t / duracion);
            yield return null;
        }
        source.Stop();
        source.volume = volInicial;
    }

    public void MostrarMensaje(string texto, float duracion)
    {
        if (subtitleDisplay != null)
            subtitleDisplay.MostrarMensajeTemporal(texto, duracion);
    }
}
