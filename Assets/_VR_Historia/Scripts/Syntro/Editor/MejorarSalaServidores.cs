#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MejorarSalaServidores : EditorWindow
{
    [MenuItem("Syntro Terror/Mejorar Sala — Monitores y Escala")]
    static void Ejecutar()
    {
        GameObject sala = GameObject.Find("Piso1_CuartoMaquinas");
        if (sala == null)
        {
            Debug.LogError("[MejorarSala] No se encontro Piso1_CuartoMaquinas");
            return;
        }

        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        CrearEstacionesMonitoreo(sala, lit);
        CrearServidorCentral(sala, lit);
        CrearPanelMural(sala, lit);
        MejorarIluminacion(sala);

        Debug.Log("[MejorarSala] Sala de servidores mejorada con monitores grandes y servidor central");
    }

    static void CrearEstacionesMonitoreo(GameObject sala, Shader lit)
    {
        if (sala.transform.Find("Estaciones_Monitoreo") != null) return;

        GameObject contenedor = new GameObject("Estaciones_Monitoreo");
        contenedor.transform.SetParent(sala.transform);
        contenedor.transform.localPosition = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(contenedor, "Estaciones Monitoreo");

        Material matEscritorio = CrearMat("Mat_Escritorio_Monitor", lit,
            new Color(0.12f, 0.12f, 0.13f), 0.3f, 0.4f);
        Material matPantallaOff = CrearMat("Mat_PantallaOff", lit,
            new Color(0.02f, 0.02f, 0.03f), 0f, 0.85f);
        Material matPantallaVerde = CrearMatEmisivo("Mat_PantallaVerde_Est", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(0.05f, 0.8f, 0.15f) * 2f);
        Material matPantallaRoja = CrearMatEmisivo("Mat_PantallaRoja_Est", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(1.2f, 0.08f, 0.05f) * 2f);
        Material matPantallaAzul = CrearMatEmisivo("Mat_PantallaAzul_Est", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(0.1f, 0.3f, 1.5f) * 1.5f);
        Material matTeclado = CrearMat("Mat_Teclado_Est", lit,
            new Color(0.08f, 0.08f, 0.09f), 0f, 0.3f);
        Material matSilla = CrearMat("Mat_Silla_Est", lit,
            new Color(0.06f, 0.06f, 0.07f), 0f, 0.25f);

        Material[] pantallas = { matPantallaVerde, matPantallaRoja, matPantallaAzul, matPantallaOff };

        // 4 estaciones a lo largo de la pared izquierda
        for (int i = 0; i < 4; i++)
        {
            float z = -3f + i * 2.5f;
            GameObject est = new GameObject($"Estacion_{i + 1}");
            est.transform.SetParent(contenedor.transform);
            est.transform.localPosition = new Vector3(-7.5f, 0f, z);
            Undo.RegisterCreatedObjectUndo(est, $"Estacion {i + 1}");

            // Escritorio grande (1.6m x 0.8m)
            Cubo("Escritorio", est.transform,
                new Vector3(0f, 0.72f, 0f), new Vector3(1.6f, 0.04f, 0.8f), matEscritorio);
            // Patas
            Cubo("Pata_FL", est.transform,
                new Vector3(-0.72f, 0.36f, -0.32f), new Vector3(0.05f, 0.72f, 0.05f), matEscritorio);
            Cubo("Pata_FR", est.transform,
                new Vector3(0.72f, 0.36f, -0.32f), new Vector3(0.05f, 0.72f, 0.05f), matEscritorio);
            Cubo("Pata_BL", est.transform,
                new Vector3(-0.72f, 0.36f, 0.32f), new Vector3(0.05f, 0.72f, 0.05f), matEscritorio);
            Cubo("Pata_BR", est.transform,
                new Vector3(0.72f, 0.36f, 0.32f), new Vector3(0.05f, 0.72f, 0.05f), matEscritorio);

            // Monitor principal grande (27 pulgadas ~ 60cm ancho)
            Material matPantalla = pantallas[i % pantallas.Length];
            Cubo("Monitor_Pantalla", est.transform,
                new Vector3(0f, 1.18f, 0.2f), new Vector3(0.65f, 0.4f, 0.03f), matPantalla);
            // Bisel monitor
            Cubo("Monitor_Bisel", est.transform,
                new Vector3(0f, 1.18f, 0.215f), new Vector3(0.68f, 0.43f, 0.01f), matEscritorio);
            // Soporte monitor
            Cubo("Monitor_Soporte", est.transform,
                new Vector3(0f, 0.95f, 0.2f), new Vector3(0.06f, 0.22f, 0.06f), matEscritorio);
            // Base monitor
            Cubo("Monitor_Base", est.transform,
                new Vector3(0f, 0.76f, 0.2f), new Vector3(0.25f, 0.02f, 0.18f), matEscritorio);

            // Segundo monitor (más pequeño, lateral)
            if (i < 3)
            {
                Material matPant2 = pantallas[(i + 1) % pantallas.Length];
                Cubo("Monitor2_Pantalla", est.transform,
                    new Vector3(0.55f, 1.1f, 0.15f), new Vector3(0.45f, 0.3f, 0.02f), matPant2);
                Cubo("Monitor2_Bisel", est.transform,
                    new Vector3(0.55f, 1.1f, 0.165f), new Vector3(0.48f, 0.33f, 0.01f), matEscritorio);
                Cubo("Monitor2_Soporte", est.transform,
                    new Vector3(0.55f, 0.92f, 0.15f), new Vector3(0.04f, 0.18f, 0.04f), matEscritorio);

                // Rotar ligeramente hacia el operador
                est.transform.Find("Monitor2_Pantalla").localRotation = Quaternion.Euler(0f, -15f, 0f);
                est.transform.Find("Monitor2_Bisel").localRotation = Quaternion.Euler(0f, -15f, 0f);
            }

            // Teclado
            Cubo("Teclado", est.transform,
                new Vector3(-0.1f, 0.76f, -0.15f), new Vector3(0.44f, 0.015f, 0.14f), matTeclado);
            // Mouse
            Cubo("Mouse", est.transform,
                new Vector3(0.35f, 0.76f, -0.15f), new Vector3(0.06f, 0.02f, 0.1f), matTeclado);
            // Mousepad
            Cubo("Mousepad", est.transform,
                new Vector3(0.35f, 0.745f, -0.15f), new Vector3(0.25f, 0.003f, 0.22f),
                CrearMat("Mat_Mousepad", lit, new Color(0.05f, 0.05f, 0.06f), 0f, 0.15f));

            // Torre PC bajo escritorio
            Cubo("PC_Torre", est.transform,
                new Vector3(-0.5f, 0.25f, 0.15f), new Vector3(0.2f, 0.45f, 0.4f), matEscritorio);
            // LED de PC encendida
            var ledPC = Esfera("PC_LED", est.transform,
                new Vector3(-0.5f, 0.42f, -0.06f), new Vector3(0.01f, 0.01f, 0.01f),
                (i == 1) ? matPantallaRoja : matPantallaVerde);

            // Silla
            Cubo("Silla_Asiento", est.transform,
                new Vector3(0f, 0.45f, -0.6f), new Vector3(0.45f, 0.06f, 0.45f), matSilla);
            Cubo("Silla_Respaldo", est.transform,
                new Vector3(0f, 0.72f, -0.82f), new Vector3(0.42f, 0.48f, 0.04f), matSilla);
            Cubo("Silla_Pilar", est.transform,
                new Vector3(0f, 0.22f, -0.6f), new Vector3(0.06f, 0.44f, 0.06f), matSilla);

            // Luz puntual por monitor encendido
            if (matPantalla != matPantallaOff)
            {
                GameObject luzMon = new GameObject($"Luz_Monitor_{i}");
                luzMon.transform.SetParent(est.transform);
                luzMon.transform.localPosition = new Vector3(0f, 1.1f, -0.1f);
                Light luz = luzMon.AddComponent<Light>();
                luz.type = LightType.Point;
                luz.range = 2.5f;
                luz.intensity = 0.6f;
                if (matPantalla == matPantallaVerde) luz.color = new Color(0.1f, 0.8f, 0.2f);
                else if (matPantalla == matPantallaRoja) luz.color = new Color(0.9f, 0.1f, 0.05f);
                else luz.color = new Color(0.2f, 0.4f, 0.9f);
                Undo.RegisterCreatedObjectUndo(luzMon, "Luz monitor");
            }
        }

        Debug.Log("[MejorarSala] 4 estaciones de monitoreo con monitores duales, PCs y sillas");
    }

    static void CrearServidorCentral(GameObject sala, Shader lit)
    {
        if (sala.transform.Find("Servidor_Central") != null) return;

        GameObject serv = new GameObject("Servidor_Central");
        serv.transform.SetParent(sala.transform);
        serv.transform.localPosition = new Vector3(0f, 0f, 4f);
        Undo.RegisterCreatedObjectUndo(serv, "Servidor Central");

        Material matChassis = CrearMat("Mat_Chassis_Central", lit,
            new Color(0.04f, 0.04f, 0.05f), 0.9f, 0.6f);
        Material matLedVerde = CrearMatEmisivo("Mat_LED_Central_V", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(0f, 3f, 0.3f) * 2f);
        Material matLedRojo = CrearMatEmisivo("Mat_LED_Central_R", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(3f, 0.1f, 0.05f) * 2f);
        Material matPantallaStatus = CrearMatEmisivo("Mat_PantallaStatus", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(0.8f, 0.4f, 0f) * 1.5f);

        // Rack principal masivo (42U = 2m, ancho doble)
        Cubo("Rack_Cuerpo", serv.transform,
            new Vector3(0f, 1.1f, 0f), new Vector3(1.2f, 2.2f, 0.9f), matChassis);

        // Paneles frontales con unidades
        for (int u = 0; u < 14; u++)
        {
            float y = 0.15f + u * 0.15f;
            Cubo($"Unidad_{u}", serv.transform,
                new Vector3(0f, y, -0.46f), new Vector3(1.0f, 0.08f, 0.02f), matChassis);

            // LEDs por unidad
            for (int l = 0; l < 6; l++)
            {
                float lx = -0.35f + l * 0.14f;
                Material ledMat = (l < 4) ? matLedVerde : matLedRojo;
                Esfera($"LED_{u}_{l}", serv.transform,
                    new Vector3(lx, y, -0.48f), new Vector3(0.012f, 0.012f, 0.012f), ledMat);
            }
        }

        // Pantalla LCD de estado en la parte superior
        Cubo("LCD_Status", serv.transform,
            new Vector3(0f, 1.95f, -0.46f), new Vector3(0.5f, 0.2f, 0.02f), matPantallaStatus);
        Cubo("LCD_Bisel", serv.transform,
            new Vector3(0f, 1.95f, -0.47f), new Vector3(0.54f, 0.24f, 0.01f), matChassis);

        // Racks laterales (2 a cada lado)
        for (int side = -1; side <= 1; side += 2)
        {
            for (int r = 0; r < 2; r++)
            {
                float x = side * (1.0f + r * 0.8f);
                Cubo($"Rack_Lateral_{(side > 0 ? "R" : "L")}_{r}", serv.transform,
                    new Vector3(x, 1.0f, 0f), new Vector3(0.65f, 2.0f, 0.8f), matChassis);

                // LEDs aleatorios
                for (int led = 0; led < 8; led++)
                {
                    float y = 0.3f + led * 0.2f;
                    Material lm = (Random.value > 0.3f) ? matLedVerde : matLedRojo;
                    Esfera($"LED_Lat_{side}_{r}_{led}", serv.transform,
                        new Vector3(x, y, -0.42f), new Vector3(0.01f, 0.01f, 0.01f), lm);
                }
            }
        }

        // Luz ambiental del servidor
        GameObject luzServ = new GameObject("Luz_Servidor_Central");
        luzServ.transform.SetParent(serv.transform);
        luzServ.transform.localPosition = new Vector3(0f, 1.5f, -0.8f);
        Light luz = luzServ.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 4f;
        luz.intensity = 1.2f;
        luz.color = new Color(0.1f, 0.7f, 0.15f);
        Undo.RegisterCreatedObjectUndo(luzServ, "Luz servidor central");

        Debug.Log("[MejorarSala] Servidor central masivo con pantalla LCD y racks laterales");
    }

    static void CrearPanelMural(GameObject sala, Shader lit)
    {
        if (sala.transform.Find("Panel_Mural_Status") != null) return;

        GameObject panel = new GameObject("Panel_Mural_Status");
        panel.transform.SetParent(sala.transform);
        panel.transform.localPosition = new Vector3(0f, 1.5f, 6.8f);
        Undo.RegisterCreatedObjectUndo(panel, "Panel Mural");

        Material matMarco = CrearMat("Mat_Marco_Mural", lit,
            new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.5f);
        Material matPantallaGrande = CrearMatEmisivo("Mat_PantallaMural", lit,
            new Color(0.01f, 0.01f, 0.02f), new Color(0.15f, 0.5f, 0.8f) * 1.8f);

        // Pantalla mural grande (3m x 1.5m) — panel de monitoreo
        Cubo("Pantalla", panel.transform,
            Vector3.zero, new Vector3(3f, 1.5f, 0.04f), matPantallaGrande);
        Cubo("Marco", panel.transform,
            Vector3.zero, new Vector3(3.1f, 1.6f, 0.02f), matMarco);

        // Pantallas laterales más pequeñas
        Cubo("Pantalla_L", panel.transform,
            new Vector3(-2.2f, 0f, 0f), new Vector3(1.2f, 0.8f, 0.03f),
            CrearMatEmisivo("Mat_PantallaMural_L", lit,
                new Color(0.01f, 0.01f, 0.02f), new Color(0.8f, 0.2f, 0.05f) * 1.5f));
        Cubo("Marco_L", panel.transform,
            new Vector3(-2.2f, 0f, 0.01f), new Vector3(1.3f, 0.9f, 0.02f), matMarco);

        Cubo("Pantalla_R", panel.transform,
            new Vector3(2.2f, 0f, 0f), new Vector3(1.2f, 0.8f, 0.03f),
            CrearMatEmisivo("Mat_PantallaMural_R", lit,
                new Color(0.01f, 0.01f, 0.02f), new Color(0.05f, 0.9f, 0.15f) * 1.2f));
        Cubo("Marco_R", panel.transform,
            new Vector3(2.2f, 0f, 0.01f), new Vector3(1.3f, 0.9f, 0.02f), matMarco);

        // Luz del panel mural
        GameObject luzPanel = new GameObject("Luz_Panel_Mural");
        luzPanel.transform.SetParent(panel.transform);
        luzPanel.transform.localPosition = new Vector3(0f, 0f, -1.5f);
        Light luz = luzPanel.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 5f;
        luz.intensity = 0.8f;
        luz.color = new Color(0.2f, 0.5f, 0.8f);
        Undo.RegisterCreatedObjectUndo(luzPanel, "Luz panel mural");

        Debug.Log("[MejorarSala] Panel mural 3m + 2 pantallas laterales");
    }

    static void MejorarIluminacion(GameObject sala)
    {
        if (sala.transform.Find("Iluminacion_Mejorada") != null) return;

        GameObject contenedor = new GameObject("Iluminacion_Mejorada");
        contenedor.transform.SetParent(sala.transform);
        contenedor.transform.localPosition = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(contenedor, "Iluminacion Mejorada");

        // Luces de techo fluorescentes (tenues, parpadeantes)
        for (int i = 0; i < 4; i++)
        {
            float x = -5f + i * 3.5f;
            for (int j = 0; j < 3; j++)
            {
                float z = -2f + j * 3f;
                GameObject luzObj = new GameObject($"Fluorescente_{i}_{j}");
                luzObj.transform.SetParent(contenedor.transform);
                luzObj.transform.localPosition = new Vector3(x, 3.2f, z);
                Light luz = luzObj.AddComponent<Light>();
                luz.type = LightType.Point;
                luz.range = 4f;
                luz.intensity = 0.4f;
                luz.color = new Color(0.7f, 0.75f, 0.9f);
                Undo.RegisterCreatedObjectUndo(luzObj, "Fluorescente");
            }
        }

        // Luz roja de emergencia
        GameObject luzEmerg = new GameObject("Luz_Emergencia");
        luzEmerg.transform.SetParent(contenedor.transform);
        luzEmerg.transform.localPosition = new Vector3(4f, 3f, 0f);
        Light luzE = luzEmerg.AddComponent<Light>();
        luzE.type = LightType.Point;
        luzE.range = 8f;
        luzE.intensity = 0.3f;
        luzE.color = new Color(0.9f, 0.05f, 0.02f);
        Undo.RegisterCreatedObjectUndo(luzEmerg, "Luz emergencia");

        Debug.Log("[MejorarSala] 12 fluorescentes + luz de emergencia roja");
    }

    // === Helpers ===

    static GameObject Cubo(string n, Transform p, Vector3 pos, Vector3 s, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = n; go.transform.SetParent(p);
        go.transform.localPosition = pos; go.transform.localScale = s;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.isStatic = true;
        Undo.RegisterCreatedObjectUndo(go, n);
        return go;
    }

    static GameObject Esfera(string n, Transform p, Vector3 pos, Vector3 s, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = n; go.transform.SetParent(p);
        go.transform.localPosition = pos; go.transform.localScale = s;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        Undo.RegisterCreatedObjectUndo(go, n);
        return go;
    }

    static Material CrearMat(string nombre, Shader shader, Color color, float metallic, float smoothness)
    {
        string ruta = "Assets/_Unity Essentials/Materials/Realistas/" + nombre + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat == null)
        {
            mat = new Material(shader);
            mat.name = nombre;
            AssetDatabase.CreateAsset(mat, ruta);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material CrearMatEmisivo(string nombre, Shader shader, Color baseColor, Color emission)
    {
        Material mat = CrearMat(nombre, shader, baseColor, 0f, 0.4f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emission);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
#endif
