#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SyntroSpawnFixer : EditorWindow
{
    [MenuItem("SYNTRO/Corregir Pantalla Inicial, Luces y Colliders")]
    public static void FixSpawnAndOverexposure()
    {
        FixBlindingLights();
        FixWallMaterials();
        UpgradeHUDContrast();
        FixMissingColliders();

        Debug.Log("<color=red><b>[SYNTRO]</b></color> Luces, contraste y colliders corregidos en la escena.");
    }

    private static void FixBlindingLights()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        int fixedCount = 0;

        foreach (Light l in lights)
        {
            if (l.intensity > 3.5f)
            {
                l.intensity = 1.2f;
                l.color = new Color(0.85f, 0.25f, 0.25f);
                fixedCount++;
            }

            if (l.type == LightType.Point && l.range > 12f)
            {
                l.range = 7.5f;
            }
        }

        RenderSettings.skybox = null;
        RenderSettings.ambientLight = new Color(0.25f, 0.25f, 0.3f);
        RenderSettings.reflectionIntensity = 0.8f;

        Debug.Log($"[SYNTRO] Se regularon {fixedCount} luces que quemaban la escena.");
    }

    private static void FixWallMaterials()
    {
        MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach (MeshRenderer r in renderers)
        {
            foreach (Material mat in r.sharedMaterials)
            {
                if (mat != null && mat.HasProperty("_Smoothness"))
                {
                    if (mat.GetFloat("_Smoothness") > 0.45f)
                        mat.SetFloat("_Smoothness", 0.2f);
                }
            }
        }
    }

    private static void UpgradeHUDContrast()
    {
        Image[] uiImages = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image img in uiImages)
        {
            if (img.gameObject.name.ToLower().Contains("panel") || img.gameObject.name.ToLower().Contains("bg"))
            {
                img.color = new Color(0f, 0f, 0f, 0f);
            }
        }

        TextMeshProUGUI[] texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (TextMeshProUGUI tmp in texts)
        {
            tmp.color = new Color(0.92f, 0.92f, 0.92f, 1f);
            tmp.outlineWidth = 0.22f;
            tmp.outlineColor = new Color32(0, 0, 0, 220);
        }
    }

    private static void FixMissingColliders()
    {
        MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int addedCount = 0;

        foreach (MeshRenderer r in renderers)
        {
            if (r.gameObject.name.Contains("UI") || r.gameObject.name.Contains("Canvas") || r.gameObject.name.Contains("Particle") || r.gameObject.name.Contains("Volume") || r.gameObject.name.Contains("Atmosfera"))
                continue;

            Collider col = r.GetComponent<Collider>();
            if (col == null)
            {
                r.gameObject.AddComponent<BoxCollider>();
                addedCount++;
            }
        }

        Debug.Log($"[SYNTRO] Se agregaron BoxColliders a {addedCount} objetos en la escena que no tenian colision.");
    }
}
#endif
