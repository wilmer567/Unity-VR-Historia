using UnityEngine;

public enum TransitionEffect
{
    Fade,
    Dissolve,
    Portal
}

[CreateAssetMenu(fileName = "NewTeleportZone", menuName = "VR Historia/Teleport Zone")]
public class TeleportZone : ScriptableObject
{
    public string zoneID;
    public string displayName;
    public bool isUnlocked;
    public string requiredFlag;
    public int sceneIndex = -1;
    public string sceneName;
    public Vector3 arrivalPosition;
    public float arrivalYRotation;
    public TransitionEffect transitionEffect = TransitionEffect.Fade;
    [Range(0.5f, 3f)] public float transitionDuration = 1f;
    [TextArea(1, 3)] public string description;
}
