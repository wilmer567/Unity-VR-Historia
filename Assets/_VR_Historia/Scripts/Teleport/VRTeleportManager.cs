using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRTeleportManager : MonoBehaviour
{
    public static VRTeleportManager Instance { get; private set; }

    [Header("Configuracion")]
    [SerializeField] private WorldMap worldMap;
    [SerializeField] private VRFadeScreen fadeScreen;

    [Header("XR Rig")]
    [SerializeField] private Transform xrOrigin;

    private TeleportZone currentZone;
    private bool transitioning;

    public TeleportZone CurrentZone => currentZone;
    public event Action<TeleportZone> OnZoneChanged;
    public event Action<TeleportZone> OnZoneUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (worldMap != null && worldMap.startingZone != null)
            currentZone = worldMap.startingZone;
    }

    public void DesbloquearZona(string zoneID)
    {
        if (worldMap == null) return;

        foreach (var zone in worldMap.allZones)
        {
            if (zone != null && zone.zoneID == zoneID && !zone.isUnlocked)
            {
                zone.isUnlocked = true;
                OnZoneUnlocked?.Invoke(zone);
                Debug.Log($"[TeleportManager] Zona desbloqueada: {zone.displayName}");
            }
        }
    }

    public void TeleportarAZona(TeleportZone zone)
    {
        if (zone == null || transitioning) return;

        if (!zone.isUnlocked)
        {
            Debug.Log($"[TeleportManager] Zona bloqueada: {zone.displayName}");
            if (VRDialogueManager.Instance != null)
                VRDialogueManager.Instance.MostrarMensaje("Zona bloqueada", 2f);
            return;
        }

        if (!string.IsNullOrEmpty(zone.requiredFlag) && GameManagerVR.Instance != null)
        {
            if (!GameManagerVR.Instance.TieneFlag(zone.requiredFlag))
            {
                Debug.Log($"[TeleportManager] Flag requerido: {zone.requiredFlag}");
                return;
            }
        }

        StartCoroutine(EjecutarTeleport(zone));
    }

    public void TeleportarAZona(string zoneID)
    {
        if (worldMap == null) return;

        foreach (var zone in worldMap.allZones)
        {
            if (zone != null && zone.zoneID == zoneID)
            {
                TeleportarAZona(zone);
                return;
            }
        }
    }

    IEnumerator EjecutarTeleport(TeleportZone zone)
    {
        transitioning = true;

        if (fadeScreen != null)
            yield return fadeScreen.FadeOut(zone.transitionDuration * 0.5f);

        bool cambioEscena = !string.IsNullOrEmpty(zone.sceneName) &&
                            zone.sceneName != SceneManager.GetActiveScene().name;

        if (cambioEscena)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(zone.sceneName);
            while (!asyncLoad.isDone)
                yield return null;
        }

        if (xrOrigin != null)
        {
            xrOrigin.position = zone.arrivalPosition;
            xrOrigin.rotation = Quaternion.Euler(0f, zone.arrivalYRotation, 0f);
        }

        currentZone = zone;
        OnZoneChanged?.Invoke(zone);

        if (fadeScreen != null)
            yield return fadeScreen.FadeIn(zone.transitionDuration * 0.5f);

        transitioning = false;
        Debug.Log($"[TeleportManager] Teletransportado a: {zone.displayName}");
    }

    public void TeleportLocal(Vector3 position, float yRotation = 0f)
    {
        if (xrOrigin == null) return;
        StartCoroutine(TeleportLocalCoroutine(position, yRotation));
    }

    IEnumerator TeleportLocalCoroutine(Vector3 position, float yRotation)
    {
        if (fadeScreen != null)
            yield return fadeScreen.FadeOut(0.3f);

        xrOrigin.position = position;
        xrOrigin.rotation = Quaternion.Euler(0f, yRotation, 0f);

        if (fadeScreen != null)
            yield return fadeScreen.FadeIn(0.3f);
    }
}
