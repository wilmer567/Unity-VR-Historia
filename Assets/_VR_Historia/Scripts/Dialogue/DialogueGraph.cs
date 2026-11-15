using System;
using System.Collections.Generic;
using UnityEngine;

public enum EmotionType
{
    Neutral,
    Happy,
    Angry,
    Sad,
    Surprised,
    Fear
}

[Serializable]
public class DialogueChoice
{
    public string label;
    public string nextNodeID;
    public string requiredFlag;
}

[Serializable]
public class DialogueNode
{
    public string nodeID;
    public string speakerID;
    public string speakerName;
    [TextArea(2, 5)] public string text;
    public AudioClip audioClip;
    public EmotionType emotion = EmotionType.Neutral;
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    public string nextNodeID;
    public string flagToSet;
    public float autoAdvanceDelay;
}

[CreateAssetMenu(fileName = "NewDialogueGraph", menuName = "VR Historia/Dialogue Graph")]
public class DialogueGraph : ScriptableObject
{
    public string dialogueID;
    public string startNodeID;
    public List<DialogueNode> nodes = new List<DialogueNode>();

    public DialogueNode GetNode(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var node in nodes)
        {
            if (node.nodeID == id) return node;
        }
        return null;
    }

    public DialogueNode GetStartNode()
    {
        return GetNode(startNodeID);
    }
}
