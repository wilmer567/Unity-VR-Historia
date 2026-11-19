#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SyntroScenographyBuilder : EditorWindow
{
    [MenuItem("SYNTRO/Construir Escenografía Subterránea")]
    public static void BuildScenography()
    {
        GameObject root = GameObject.Find("SYNTRO_Scenography_Props");
        if (root != null) DestroyImmediate(root);
        root = new GameObject("SYNTRO_Scenography_Props");

        // Materiales base oscuros e industriales
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        Material darkMetalMat = new Material(litShader);
        darkMetalMat.color = new Color(0.10f, 0.11f, 0.12f);
        darkMetalMat.SetFloat("_Metallic", 0.75f);
        darkMetalMat.SetFloat("_Smoothness", 0.35f);

        Material rustedPipeMat = new Material(litShader);
        rustedPipeMat.color = new Color(0.18f, 0.12f, 0.10f);
        rustedPipeMat.SetFloat("_Metallic", 0.6f);
        rustedPipeMat.SetFloat("_Smoothness", 0.25f);

        Material ledRedMat = new Material(litShader);
        ledRedMat.color = Color.red;
        ledRedMat.EnableKeyword("_EMISSION");
        ledRedMat.SetColor("_EmissionColor", Color.red * 2.5f);

        // 1. Crear Racks de Servidores en los laterales
        for (int i = 0; i < 3; i++)
        {
            CreateServerRack(root.transform, new Vector3(2.8f, 1.1f, 1.5f + (i * 1.4f)), darkMetalMat, ledRedMat, $"ServerRack_Right_{i + 1}");
        }

        // 2. Crear Tuberías Industriales en el techo
        for (int i = 0; i < 3; i++)
        {
            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = $"Ceiling_Pipe_{i + 1}";
            pipe.transform.SetParent(root.transform);
            pipe.transform.position = new Vector3(-0.8f + (i * 0.25f), 2.75f, 3.0f);
            pipe.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            pipe.transform.localScale = new Vector3(0.08f, 5.0f, 0.08f);
            pipe.GetComponent<Renderer>().sharedMaterial = rustedPipeMat;
        }

        // 3. Crear Vigas Estructurales para romper la monotonía del techo
        for (int i = 0; i < 4; i++)
        {
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = $"Structural_Beam_{i + 1}";
            beam.transform.SetParent(root.transform);
            beam.transform.position = new Vector3(0.5f, 2.85f, i * 2.5f);
            beam.transform.localScale = new Vector3(6.5f, 0.22f, 0.25f);
            beam.GetComponent<Renderer>().sharedMaterial = darkMetalMat;
        }

        // 4. Apagar luces quemadas que causan la pared blanca
        Light[] sceneLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in sceneLights)
        {
            if (l.intensity > 3.0f)
            {
                l.intensity = 0.8f;
                l.range = 6.0f;
            }
        }

        Undo.RegisterCreatedObjectUndo(root, "Construir Escenografía SYNTRO");
        Debug.Log("<color=red><b>[SYNTRO]</b></color> Escenografía industrial generada. Ajusta las posiciones de 'SYNTRO_Scenography_Props' a las medidas exactas de tu cuarto.");
    }

    private static void CreateServerRack(Transform parent, Vector3 pos, Material bodyMat, Material ledMat, string rackName)
    {
        GameObject rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rack.name = rackName;
        rack.transform.SetParent(parent);
        rack.transform.position = pos;
        rack.transform.localScale = new Vector3(0.85f, 2.2f, 1.0f);
        rack.GetComponent<Renderer>().sharedMaterial = bodyMat;

        // Añadir luz LED indicadora de fallo en el rack
        GameObject led = GameObject.CreatePrimitive(PrimitiveType.Cube);
        led.name = "StatusLED";
        led.transform.SetParent(rack.transform);
        led.transform.localPosition = new Vector3(-0.51f, 0.3f, 0.2f);
        led.transform.localScale = new Vector3(0.05f, 0.04f, 0.15f);
        led.GetComponent<Renderer>().sharedMaterial = ledMat;
    }
}
#endif
