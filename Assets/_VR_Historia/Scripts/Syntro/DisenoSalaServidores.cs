#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class DisenoSalaServidores : EditorWindow
{
    private const string RUTA_MAT = "Assets/_Unity Essentials/Materials/Realistas";

    [MenuItem("Syntro Terror/Redisenar Sala de Servidores Realista")]
    public static void Ejecutar()
    {
        Debug.Log("[SalaServ] === INICIO: Rediseno fotorrealista de Sala de Servidores ===");

        GameObject sala = GameObject.Find("Piso1_CuartoMaquinas");
        if (sala == null)
        {
            Debug.LogError("[SalaServ] No se encontro Piso1_CuartoMaquinas en la escena");
            return;
        }

        AsegurarCarpetas();
        CrearMaterialesDataCenter();

        // Estructura fisica
        CrearPisoTecnico(sala);
        CrearTechoConCanaletas(sala);
        MejorarParedes(sala);

        // Racks adicionales
        CrearFilaRacksAdicionales(sala);
        AgregarDetallesARacks(sala);

        // Equipamiento
        CrearUPS(sala);
        CrearAireAcondicionado(sala);
        CrearPDU(sala);
        CrearPatchPanel(sala);

        // Cableado
        CrearCanaletasCables(sala);
        CrearCablesSueltos(sala);

        // Seguridad y senalizacion
        CrearCCTV(sala);
        CrearExtintor(sala);
        CrearPanelAcceso(sala);
        CrearSenalizacion(sala);
        CrearDisplayTemperatura(sala);

        // Iluminacion ambiental
        CrearIluminacionLED(sala);
        CrearLucesIndicadorasPiso(sala);

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[SalaServ] === COMPLETADO: Sala de servidores rediseñada con +40 elementos ===");
    }

    static void AsegurarCarpetas()
    {
        if (!AssetDatabase.IsValidFolder(RUTA_MAT))
        {
            string parent = Path.GetDirectoryName(RUTA_MAT).Replace("\\", "/");
            AssetDatabase.CreateFolder(parent, "Realistas");
        }
    }

    // =====================================================
    //  MATERIALES ESPECIFICOS DATA CENTER
    // =====================================================

    static Material matPisoTecnico, matTechoPerforado, matMetal, matMetalOscuro;
    static Material matCable, matLedVerde, matLedRojo, matLedAzul, matPantalla;
    static Material matSenal, matAC, matPlasticoNegro;

    static void CrearMaterialesDataCenter()
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");

        matPisoTecnico = CrearMat("Mat_PisoTecnico_DC", lit,
            new Color(0.55f, 0.55f, 0.52f), 0f, 0.45f);
        matTechoPerforado = CrearMat("Mat_TechoPerforado_DC", lit,
            new Color(0.85f, 0.85f, 0.83f), 0f, 0.3f);
        matMetal = CrearMat("Mat_MetalGris_DC", lit,
            new Color(0.35f, 0.36f, 0.37f), 0.7f, 0.5f);
        matMetalOscuro = CrearMat("Mat_MetalOscuro_DC", lit,
            new Color(0.06f, 0.06f, 0.07f), 0.85f, 0.55f);
        matCable = CrearMat("Mat_Cable_DC", lit,
            new Color(0.02f, 0.02f, 0.02f), 0f, 0.3f);
        matAC = CrearMat("Mat_AC_DC", lit,
            new Color(0.75f, 0.76f, 0.74f), 0f, 0.35f);
        matPlasticoNegro = CrearMat("Mat_PlasticoNegro_DC", lit,
            new Color(0.04f, 0.04f, 0.045f), 0f, 0.4f);
        matSenal = CrearMat("Mat_Senal_DC", lit,
            new Color(0.9f, 0.2f, 0.05f), 0f, 0.2f);

        matLedVerde = CrearMatEmisivo("Mat_LED_Verde_DC", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(0f, 2f, 0.2f) * 3f);
        matLedRojo = CrearMatEmisivo("Mat_LED_Rojo_DC", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(2.5f, 0.1f, 0.05f) * 3f);
        matLedAzul = CrearMatEmisivo("Mat_LED_Azul_DC", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(0.1f, 0.3f, 2.5f) * 2f);
        matPantalla = CrearMatEmisivo("Mat_Pantalla_DC", lit,
            new Color(0.02f, 0.02f, 0.03f), new Color(0.1f, 0.4f, 0.15f) * 1.5f);

        Debug.Log("[SalaServ] 12 materiales de data center creados");
    }

    static Material CrearMat(string nombre, Shader shader, Color color, float metallic, float smoothness)
    {
        string ruta = RUTA_MAT + "/" + nombre + ".mat";
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

    // =====================================================
    //  HELPERS GEOMETRIA
    // =====================================================

    static GameObject CrearCubo(string nombre, Transform padre, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
        return go;
    }

    static GameObject CrearCilindro(string nombre, Transform padre, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
        return go;
    }

    static GameObject CrearContenedor(string nombre, Transform padre, Vector3 pos)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
        return go;
    }

    static GameObject CrearEsfera(string nombre, Transform padre, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
        return go;
    }

    // =====================================================
    //  PISO TECNICO ELEVADO (raised floor)
    // =====================================================

    static void CrearPisoTecnico(GameObject sala)
    {
        if (sala.transform.Find("PisoTecnico_DC") != null) return;

        GameObject contenedor = CrearContenedor("PisoTecnico_DC", sala.transform, Vector3.zero);

        // Baldosas de piso tecnico 60x60cm
        int tilesX = 16, tilesZ = 13;
        float tileSize = 0.625f;
        float startX = -5f + tileSize / 2f;
        float startZ = -4f + tileSize / 2f;

        for (int tx = 0; tx < tilesX; tx++)
        {
            for (int tz = 0; tz < tilesZ; tz++)
            {
                float x = startX + tx * tileSize;
                float z = startZ + tz * tileSize;
                GameObject tile = CrearCubo($"Baldosa_{tx}_{tz}", contenedor.transform,
                    new Vector3(x, 0.01f, z),
                    new Vector3(tileSize - 0.01f, 0.04f, tileSize - 0.01f),
                    matPisoTecnico);
                tile.isStatic = true;
            }
        }

        // Pedestales visibles en esquinas (se ven por las juntas)
        for (int px = 0; px <= 4; px++)
        {
            for (int pz = 0; pz <= 3; pz++)
            {
                float x = -4f + px * 2.5f;
                float z = -3f + pz * 2.5f;
                CrearCilindro($"Pedestal_{px}_{pz}", contenedor.transform,
                    new Vector3(x, -0.08f, z),
                    new Vector3(0.06f, 0.08f, 0.06f),
                    matMetal);
            }
        }

        // Rejilla de ventilacion en piso (frente a racks)
        for (int i = 0; i < 3; i++)
        {
            float x = -2f + i * 2f;
            GameObject rejilla = CrearCubo($"RejillaPiso_{i}", contenedor.transform,
                new Vector3(x, 0.02f, 2.5f),
                new Vector3(0.6f, 0.02f, 0.6f),
                matMetal);
            rejilla.isStatic = true;

            for (int s = 0; s < 8; s++)
            {
                CrearCubo($"RejillaSlot_{i}_{s}", rejilla.transform,
                    new Vector3(0f, 0.02f, -0.25f + s * 0.065f),
                    new Vector3(0.5f, 0.005f, 0.02f),
                    matMetalOscuro);
            }
        }

        Debug.Log("[SalaServ] Piso tecnico elevado con baldosas y rejillas");
    }

    // =====================================================
    //  TECHO CON CANALETAS Y PANELES PERFORADOS
    // =====================================================

    static void CrearTechoConCanaletas(GameObject sala)
    {
        if (sala.transform.Find("TechoDetalle_DC") != null) return;

        GameObject contenedor = CrearContenedor("TechoDetalle_DC", sala.transform, Vector3.zero);
        float alturaTecho = 3.4f;

        // Paneles de techo tipo T-bar grid
        for (int i = 0; i < 8; i++)
        {
            float x = -4.375f + i * 1.25f;
            CrearCubo($"TBar_X_{i}", contenedor.transform,
                new Vector3(x, alturaTecho, 0f),
                new Vector3(0.02f, 0.04f, 8f),
                matMetal);
        }
        for (int j = 0; j < 6; j++)
        {
            float z = -3.125f + j * 1.25f;
            CrearCubo($"TBar_Z_{j}", contenedor.transform,
                new Vector3(0f, alturaTecho, z),
                new Vector3(10f, 0.02f, 0.02f),
                matMetal);
        }

        // Bandejas de cables en techo (cable trays)
        CrearCubo("BandejaCable_Central", contenedor.transform,
            new Vector3(0f, alturaTecho - 0.15f, 2f),
            new Vector3(0.5f, 0.04f, 7f), matMetal);
        CrearCubo("BandejaCable_Lat_L", contenedor.transform,
            new Vector3(0f, alturaTecho - 0.15f, 2f),
            new Vector3(0.5f, 0.15f, 0.02f), matMetal);
        CrearCubo("BandejaCable_Lat_R", contenedor.transform,
            new Vector3(0f, alturaTecho - 0.07f, 2f),
            new Vector3(0.52f, 0.02f, 7f), matMetal);

        // Bandeja lateral
        CrearCubo("BandejaCable_Izq", contenedor.transform,
            new Vector3(-3.5f, alturaTecho - 0.15f, 0f),
            new Vector3(0.4f, 0.04f, 7.5f), matMetal);

        Debug.Log("[SalaServ] Techo con grid T-bar y bandejas de cable");
    }

    // =====================================================
    //  MEJORAR PAREDES (zocalos, paneles, pintura)
    // =====================================================

    static void MejorarParedes(GameObject sala)
    {
        if (sala.transform.Find("DetalleParedes_DC") != null) return;

        GameObject contenedor = CrearContenedor("DetalleParedes_DC", sala.transform, Vector3.zero);

        // Zocalo metalico inferior en todas las paredes
        CrearCubo("Zocalo_Fondo", contenedor.transform,
            new Vector3(0f, 0.08f, 3.95f), new Vector3(9.9f, 0.16f, 0.05f), matMetal);
        CrearCubo("Zocalo_Entrada", contenedor.transform,
            new Vector3(0f, 0.08f, -3.95f), new Vector3(9.9f, 0.16f, 0.05f), matMetal);
        CrearCubo("Zocalo_Der", contenedor.transform,
            new Vector3(4.95f, 0.08f, 0f), new Vector3(0.05f, 0.16f, 7.9f), matMetal);
        CrearCubo("Zocalo_Izq", contenedor.transform,
            new Vector3(-4.95f, 0.08f, 0f), new Vector3(0.05f, 0.16f, 7.9f), matMetal);

        // Canaletas de cables en pared (verticales junto a racks)
        for (int i = 0; i < 4; i++)
        {
            float x = -3f + i * 2f;
            CrearCubo($"CanaletaPared_{i}", contenedor.transform,
                new Vector3(x, 1.75f, 3.92f),
                new Vector3(0.08f, 3.2f, 0.06f), matPlasticoNegro);
        }

        // Franja de pintura de advertencia (rayas amarillo/negro) en entrada
        for (int s = 0; s < 10; s++)
        {
            float x = -2.5f + s * 0.5f;
            Material m = (s % 2 == 0) ? matSenal : matMetalOscuro;
            CrearCubo($"FranjaAdv_{s}", contenedor.transform,
                new Vector3(x, 0.02f, -3.5f),
                new Vector3(0.24f, 0.005f, 0.8f), m);
        }

        Debug.Log("[SalaServ] Paredes mejoradas con zocalos y canaletas");
    }

    // =====================================================
    //  FILA ADICIONAL DE RACKS
    // =====================================================

    static void CrearFilaRacksAdicionales(GameObject sala)
    {
        if (sala.transform.Find("Racks_Fila2_DC") != null) return;

        GameObject contenedor = CrearContenedor("Racks_Fila2_DC", sala.transform, Vector3.zero);

        float[] posicionesX = { -3.5f, -1.5f, 0.5f, 2.5f, 4f };
        string[] nombres = { "Rack_Network_1", "Rack_Storage_1", "Rack_Storage_2", "Rack_Network_2", "Rack_PDU" };

        for (int i = 0; i < posicionesX.Length; i++)
        {
            GameObject rack = CrearContenedor(nombres[i], contenedor.transform,
                new Vector3(posicionesX[i], 0f, -2f));

            // Cuerpo del rack
            CrearCubo("Cuerpo", rack.transform,
                new Vector3(0f, 1.1f, 0f), new Vector3(0.6f, 2.0f, 0.7f), matMetalOscuro);

            // Marco frontal
            CrearCubo("Marco_L", rack.transform,
                new Vector3(-0.28f, 1.1f, -0.36f), new Vector3(0.03f, 2.0f, 0.02f), matMetal);
            CrearCubo("Marco_R", rack.transform,
                new Vector3(0.28f, 1.1f, -0.36f), new Vector3(0.03f, 2.0f, 0.02f), matMetal);

            // Unidades (dispositivos montados)
            int numUnidades = Random.Range(6, 10);
            for (int u = 0; u < numUnidades; u++)
            {
                float yUnit = 0.2f + u * 0.2f;
                float altUnit = Random.Range(0.04f, 0.12f);

                CrearCubo($"Unidad_{u}", rack.transform,
                    new Vector3(0f, yUnit, -0.34f),
                    new Vector3(0.52f, altUnit, 0.02f),
                    (u % 3 == 0) ? matMetal : matMetalOscuro);

                // LEDs por unidad
                int numLeds = Random.Range(2, 5);
                for (int l = 0; l < numLeds; l++)
                {
                    float ledX = -0.2f + l * 0.1f;
                    Material ledMat = (Random.value > 0.4f) ? matLedVerde : matLedRojo;
                    CrearEsfera($"LED_{u}_{l}", rack.transform,
                        new Vector3(ledX, yUnit, -0.36f),
                        new Vector3(0.015f, 0.015f, 0.015f), ledMat);
                }
            }

            // Etiqueta identificadora
            CrearCubo("Etiqueta", rack.transform,
                new Vector3(0f, 2.05f, -0.36f), new Vector3(0.25f, 0.06f, 0.005f), matSenal);
        }

        Debug.Log("[SalaServ] 5 racks adicionales (fila 2) con unidades y LEDs");
    }

    // =====================================================
    //  DETALLES EN RACKS EXISTENTES
    // =====================================================

    static void AgregarDetallesARacks(GameObject sala)
    {
        string[] racksExistentes = { "Rack_Servidor_1", "Rack_Servidor_2", "Rack_Servidor_3" };

        foreach (string nombre in racksExistentes)
        {
            Transform rack = sala.transform.Find(nombre);
            if (rack == null) continue;
            if (rack.Find("LEDs_DC") != null) continue;

            GameObject ledsContainer = CrearContenedor("LEDs_DC", rack, Vector3.zero);

            // LEDs en el frente
            for (int fila = 0; fila < 12; fila++)
            {
                float y = -0.35f + fila * 0.065f;
                for (int col = 0; col < 4; col++)
                {
                    float x = -0.15f + col * 0.08f;
                    Material ledMat;
                    float prob = Random.value;
                    if (prob < 0.5f) ledMat = matLedVerde;
                    else if (prob < 0.8f) ledMat = matLedRojo;
                    else ledMat = matLedAzul;

                    CrearEsfera($"LED_{fila}_{col}", ledsContainer.transform,
                        new Vector3(x, y, -0.52f),
                        new Vector3(0.012f, 0.012f, 0.012f), ledMat);
                }
            }

            // Patch cables en frente
            for (int c = 0; c < 6; c++)
            {
                float cx = -0.18f + c * 0.065f;
                float cy1 = -0.2f + Random.Range(0f, 0.15f);
                float cy2 = 0.1f + Random.Range(0f, 0.15f);
                CrearCubo($"PatchCable_{c}", ledsContainer.transform,
                    new Vector3(cx, (cy1 + cy2) / 2f, -0.51f),
                    new Vector3(0.008f, Mathf.Abs(cy2 - cy1), 0.008f), matCable);
            }
        }

        Debug.Log("[SalaServ] LEDs y patch cables agregados a racks existentes");
    }

    // =====================================================
    //  UPS (Sistema de alimentacion ininterrumpida)
    // =====================================================

    static void CrearUPS(GameObject sala)
    {
        if (sala.transform.Find("UPS_DC") != null) return;

        GameObject ups = CrearContenedor("UPS_DC", sala.transform, new Vector3(4.2f, 0f, 2.5f));

        // Cuerpo principal
        CrearCubo("Cuerpo_UPS", ups.transform,
            new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 1.0f, 0.6f), matMetalOscuro);

        // Panel frontal
        CrearCubo("Panel_UPS", ups.transform,
            new Vector3(0f, 0.7f, -0.31f), new Vector3(0.4f, 0.5f, 0.01f), matPlasticoNegro);

        // Pantalla LCD
        CrearCubo("LCD_UPS", ups.transform,
            new Vector3(0f, 0.8f, -0.32f), new Vector3(0.15f, 0.08f, 0.005f), matPantalla);

        // LEDs de estado
        CrearEsfera("LED_UPS_1", ups.transform,
            new Vector3(-0.1f, 0.55f, -0.32f), new Vector3(0.02f, 0.02f, 0.02f), matLedVerde);
        CrearEsfera("LED_UPS_2", ups.transform,
            new Vector3(0f, 0.55f, -0.32f), new Vector3(0.02f, 0.02f, 0.02f), matLedVerde);
        CrearEsfera("LED_UPS_3", ups.transform,
            new Vector3(0.1f, 0.55f, -0.32f), new Vector3(0.02f, 0.02f, 0.02f), matLedAzul);

        // Cables gruesos de salida
        CrearCilindro("Cable_UPS_1", ups.transform,
            new Vector3(0.15f, 1.1f, 0f), new Vector3(0.03f, 0.3f, 0.03f), matCable);
        CrearCilindro("Cable_UPS_2", ups.transform,
            new Vector3(-0.15f, 1.1f, 0f), new Vector3(0.03f, 0.3f, 0.03f), matCable);

        Debug.Log("[SalaServ] UPS creado");
    }

    // =====================================================
    //  AIRE ACONDICIONADO DE PRECISION
    // =====================================================

    static void CrearAireAcondicionado(GameObject sala)
    {
        if (sala.transform.Find("AC_DC") != null) return;

        GameObject ac = CrearContenedor("AC_DC", sala.transform, new Vector3(-4.5f, 0f, 0f));

        // Unidad CRAC (Computer Room Air Conditioning)
        CrearCubo("Cuerpo_AC", ac.transform,
            new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 1.8f, 0.8f), matAC);

        // Rejilla de ventilacion frontal
        for (int s = 0; s < 12; s++)
        {
            CrearCubo($"Rejilla_AC_{s}", ac.transform,
                new Vector3(0f, 0.3f + s * 0.12f, -0.41f),
                new Vector3(0.5f, 0.04f, 0.005f), matMetal);
        }

        // Panel de control
        CrearCubo("PanelControl_AC", ac.transform,
            new Vector3(0f, 1.6f, -0.41f), new Vector3(0.3f, 0.15f, 0.01f), matPlasticoNegro);

        // Display temperatura
        CrearCubo("Display_AC", ac.transform,
            new Vector3(0f, 1.62f, -0.42f), new Vector3(0.12f, 0.06f, 0.005f), matPantalla);

        // LED estado
        CrearEsfera("LED_AC", ac.transform,
            new Vector3(0.1f, 1.55f, -0.42f), new Vector3(0.015f, 0.015f, 0.015f), matLedVerde);

        // Tuberia de refrigerante
        CrearCilindro("Tuberia_AC_1", ac.transform,
            new Vector3(0.15f, 2.0f, 0.2f), new Vector3(0.05f, 0.5f, 0.05f), matMetal);
        CrearCilindro("Tuberia_AC_2", ac.transform,
            new Vector3(-0.15f, 2.0f, 0.2f), new Vector3(0.04f, 0.5f, 0.04f), matMetal);

        // Segunda unidad al fondo
        GameObject ac2 = CrearContenedor("AC_DC_2", sala.transform, new Vector3(-4.5f, 0f, 2.5f));
        CrearCubo("Cuerpo_AC2", ac2.transform,
            new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 1.8f, 0.8f), matAC);
        for (int s = 0; s < 12; s++)
        {
            CrearCubo($"Rejilla_AC2_{s}", ac2.transform,
                new Vector3(0f, 0.3f + s * 0.12f, -0.41f),
                new Vector3(0.5f, 0.04f, 0.005f), matMetal);
        }
        CrearCubo("Display_AC2", ac2.transform,
            new Vector3(0f, 1.62f, -0.42f), new Vector3(0.12f, 0.06f, 0.005f), matPantalla);

        Debug.Log("[SalaServ] 2 unidades de aire acondicionado CRAC");
    }

    // =====================================================
    //  PDU (Power Distribution Unit) en techo
    // =====================================================

    static void CrearPDU(GameObject sala)
    {
        if (sala.transform.Find("PDU_DC") != null) return;

        GameObject contenedor = CrearContenedor("PDU_DC", sala.transform, Vector3.zero);

        float[] posX = { -2f, 0f, 2f };
        for (int i = 0; i < posX.Length; i++)
        {
            GameObject pdu = CrearContenedor($"PDU_{i}", contenedor.transform,
                new Vector3(posX[i], 2.8f, 3.2f));

            CrearCubo("Cuerpo_PDU", pdu.transform,
                Vector3.zero, new Vector3(0.12f, 0.6f, 0.08f), matMetalOscuro);

            // Cables de distribucion
            for (int c = 0; c < 4; c++)
            {
                CrearCilindro($"CablePDU_{c}", pdu.transform,
                    new Vector3(0f, -0.35f - c * 0.08f, -0.04f),
                    new Vector3(0.015f, 0.06f, 0.015f), matCable);
            }

            CrearEsfera("LED_PDU", pdu.transform,
                new Vector3(0f, 0.25f, -0.045f), new Vector3(0.015f, 0.015f, 0.015f), matLedVerde);
        }

        Debug.Log("[SalaServ] 3 PDUs en techo");
    }

    // =====================================================
    //  PATCH PANEL
    // =====================================================

    static void CrearPatchPanel(GameObject sala)
    {
        if (sala.transform.Find("PatchPanel_DC") != null) return;

        GameObject pp = CrearContenedor("PatchPanel_DC", sala.transform,
            new Vector3(3.5f, 1.2f, 3.88f));

        // Panel montado en pared
        CrearCubo("Base_PP", pp.transform,
            Vector3.zero, new Vector3(0.8f, 0.3f, 0.05f), matMetal);

        // Puertos RJ45 (4 filas x 12)
        for (int fila = 0; fila < 4; fila++)
        {
            for (int col = 0; col < 12; col++)
            {
                float x = -0.33f + col * 0.06f;
                float y = 0.09f - fila * 0.06f;
                CrearCubo($"Puerto_{fila}_{col}", pp.transform,
                    new Vector3(x, y, -0.03f),
                    new Vector3(0.04f, 0.035f, 0.02f), matPlasticoNegro);
            }
        }

        // Cables conectados (algunos)
        for (int c = 0; c < 8; c++)
        {
            float cx = -0.3f + c * 0.08f + Random.Range(-0.02f, 0.02f);
            float cy = 0.05f - (c % 4) * 0.06f;
            CrearCilindro($"CablePP_{c}", pp.transform,
                new Vector3(cx, cy - 0.3f, -0.04f),
                new Vector3(0.008f, 0.25f, 0.008f), matCable);
        }

        Debug.Log("[SalaServ] Patch panel con 48 puertos");
    }

    // =====================================================
    //  CANALETAS DE CABLES
    // =====================================================

    static void CrearCanaletasCables(GameObject sala)
    {
        if (sala.transform.Find("Canaletas_DC") != null) return;

        GameObject contenedor = CrearContenedor("Canaletas_DC", sala.transform, Vector3.zero);

        // Canaleta horizontal sobre racks (backbone)
        CrearCubo("Canaleta_Backbone", contenedor.transform,
            new Vector3(0f, 2.5f, 3.3f), new Vector3(6f, 0.1f, 0.15f), matMetal);
        CrearCubo("Canaleta_BB_Tapa", contenedor.transform,
            new Vector3(0f, 2.56f, 3.3f), new Vector3(6f, 0.01f, 0.16f), matMetal);

        // Canaletas verticales entre racks
        for (int i = 0; i < 4; i++)
        {
            float x = -3f + i * 2f;
            CrearCubo($"Canaleta_V_{i}", contenedor.transform,
                new Vector3(x, 1.5f, 3.6f), new Vector3(0.1f, 2.5f, 0.08f), matPlasticoNegro);
        }

        Debug.Log("[SalaServ] Canaletas de cables backbone + verticales");
    }

    // =====================================================
    //  CABLES SUELTOS (realismo)
    // =====================================================

    static void CrearCablesSueltos(GameObject sala)
    {
        if (sala.transform.Find("Cables_DC") != null) return;

        GameObject contenedor = CrearContenedor("Cables_DC", sala.transform, Vector3.zero);

        // Mazos de cables entre racks y techo
        for (int i = 0; i < 3; i++)
        {
            float x = -2f + i * 2f;
            GameObject mazo = CrearContenedor($"MazoCables_{i}", contenedor.transform,
                new Vector3(x, 2.2f, 3.5f));

            int numCables = Random.Range(5, 10);
            for (int c = 0; c < numCables; c++)
            {
                float offsetX = Random.Range(-0.05f, 0.05f);
                float offsetZ = Random.Range(-0.03f, 0.03f);
                float longitud = Random.Range(0.6f, 1.2f);
                CrearCilindro($"Cable_{c}", mazo.transform,
                    new Vector3(offsetX, longitud / 2f, offsetZ),
                    new Vector3(0.008f, longitud / 2f, 0.008f), matCable);
            }
        }

        // Cables de fibra optica (amarillos) en piso
        Material matFibra = CrearMatEmisivo("Mat_FibraOptica_DC",
            Shader.Find("Universal Render Pipeline/Lit"),
            new Color(0.6f, 0.5f, 0.05f), new Color(0.3f, 0.25f, 0.02f));

        for (int f = 0; f < 4; f++)
        {
            float x1 = -1.5f + f * 1f;
            float z1 = 3.2f;
            float z2 = -1.5f;
            CrearCilindro($"Fibra_{f}", contenedor.transform,
                new Vector3(x1, 0.05f, (z1 + z2) / 2f),
                new Vector3(0.006f, Mathf.Abs(z1 - z2) / 2f, 0.006f), matFibra);
            // Curva: rotar para que se vea horizontal
            contenedor.transform.Find($"Fibra_{f}").localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        Debug.Log("[SalaServ] Mazos de cables y fibra optica");
    }

    // =====================================================
    //  CAMARA CCTV
    // =====================================================

    static void CrearCCTV(GameObject sala)
    {
        if (sala.transform.Find("CCTV_DC") != null) return;

        GameObject contenedor = CrearContenedor("CCTV_DC", sala.transform, Vector3.zero);

        Vector3[] posiciones = {
            new Vector3(-4.5f, 3.1f, -3.5f),
            new Vector3(4.5f, 3.1f, 3.5f)
        };
        Vector3[] rotaciones = {
            new Vector3(30f, 45f, 0f),
            new Vector3(30f, -135f, 0f)
        };

        for (int i = 0; i < posiciones.Length; i++)
        {
            GameObject cam = CrearContenedor($"CCTV_{i + 1}", contenedor.transform, posiciones[i]);
            cam.transform.localEulerAngles = rotaciones[i];

            // Soporte
            CrearCilindro("Soporte", cam.transform,
                new Vector3(0f, 0.08f, 0f), new Vector3(0.03f, 0.08f, 0.03f), matMetal);

            // Cuerpo camara
            CrearCubo("Cuerpo", cam.transform,
                Vector3.zero, new Vector3(0.06f, 0.06f, 0.12f), matPlasticoNegro);

            // Lente
            CrearCilindro("Lente", cam.transform,
                new Vector3(0f, 0f, -0.08f), new Vector3(0.035f, 0.02f, 0.035f), matMetalOscuro);
            cam.transform.Find("Lente").localRotation = Quaternion.Euler(90f, 0f, 0f);

            // LED IR
            CrearEsfera("LED_IR", cam.transform,
                new Vector3(0.02f, 0.02f, -0.06f), new Vector3(0.01f, 0.01f, 0.01f), matLedRojo);
        }

        Debug.Log("[SalaServ] 2 camaras CCTV");
    }

    // =====================================================
    //  EXTINTOR
    // =====================================================

    static void CrearExtintor(GameObject sala)
    {
        if (sala.transform.Find("Extintor_DC") != null) return;

        GameObject ext = CrearContenedor("Extintor_DC", sala.transform,
            new Vector3(4.8f, 0f, -1f));

        // Soporte de pared
        CrearCubo("Soporte_Ext", ext.transform,
            new Vector3(0f, 0.8f, 0f), new Vector3(0.02f, 0.2f, 0.08f), matMetal);

        // Tanque cilindrico rojo
        CrearCilindro("Tanque", ext.transform,
            new Vector3(0f, 0.55f, 0f), new Vector3(0.12f, 0.35f, 0.12f), matSenal);

        // Manguera
        CrearCilindro("Manguera", ext.transform,
            new Vector3(0.06f, 0.75f, 0f), new Vector3(0.015f, 0.1f, 0.015f), matCable);

        // Manometro
        CrearEsfera("Manometro", ext.transform,
            new Vector3(0f, 0.75f, -0.06f), new Vector3(0.03f, 0.03f, 0.01f), matMetal);

        // Señal de extintor en pared
        CrearCubo("Senal_Extintor", ext.transform,
            new Vector3(0f, 1.3f, 0f), new Vector3(0.2f, 0.2f, 0.005f), matSenal);

        Debug.Log("[SalaServ] Extintor con senalizacion");
    }

    // =====================================================
    //  PANEL DE ACCESO / CONTROL
    // =====================================================

    static void CrearPanelAcceso(GameObject sala)
    {
        if (sala.transform.Find("PanelAcceso_DC") != null) return;

        GameObject panel = CrearContenedor("PanelAcceso_DC", sala.transform,
            new Vector3(-1.5f, 1.2f, -3.9f));

        // Caja del panel
        CrearCubo("Caja_Panel", panel.transform,
            Vector3.zero, new Vector3(0.2f, 0.3f, 0.04f), matMetal);

        // Pantalla
        CrearCubo("Pantalla_Panel", panel.transform,
            new Vector3(0f, 0.04f, -0.025f), new Vector3(0.12f, 0.08f, 0.005f), matPantalla);

        // Lector de tarjeta
        CrearCubo("Lector_Tarjeta", panel.transform,
            new Vector3(0f, -0.08f, -0.025f), new Vector3(0.06f, 0.08f, 0.01f), matPlasticoNegro);

        // LED estado
        CrearEsfera("LED_Acceso", panel.transform,
            new Vector3(0.06f, 0.04f, -0.025f), new Vector3(0.012f, 0.012f, 0.012f), matLedVerde);

        Debug.Log("[SalaServ] Panel de control de acceso");
    }

    // =====================================================
    //  SENALIZACION
    // =====================================================

    static void CrearSenalizacion(GameObject sala)
    {
        if (sala.transform.Find("Senales_DC") != null) return;

        GameObject contenedor = CrearContenedor("Senales_DC", sala.transform, Vector3.zero);

        // Señal "CUARTO DE SERVIDORES - SOLO PERSONAL AUTORIZADO"
        CrearCubo("Senal_Puerta", contenedor.transform,
            new Vector3(0f, 2.8f, -3.92f), new Vector3(1.2f, 0.15f, 0.005f), matSenal);

        // Señal "NO APAGAR" en racks
        CrearCubo("Senal_NoApagar_1", contenedor.transform,
            new Vector3(-2f, 2.3f, 3.5f), new Vector3(0.3f, 0.1f, 0.005f), matSenal);
        CrearCubo("Senal_NoApagar_2", contenedor.transform,
            new Vector3(2f, 2.3f, 3.5f), new Vector3(0.3f, 0.1f, 0.005f), matSenal);

        // Señal de peligro electrico
        CrearCubo("Senal_PeligroElec", contenedor.transform,
            new Vector3(4.92f, 1.5f, 2f), new Vector3(0.005f, 0.25f, 0.25f), matSenal);

        // Senal salida emergencia (verde emisivo)
        Material matSalida = CrearMatEmisivo("Mat_SalidaEmergencia_DC",
            Shader.Find("Universal Render Pipeline/Lit"),
            new Color(0.02f, 0.15f, 0.02f), new Color(0f, 1.5f, 0f));
        CrearCubo("Senal_Salida", contenedor.transform,
            new Vector3(0f, 3.0f, -3.85f), new Vector3(0.4f, 0.12f, 0.01f), matSalida);

        Debug.Log("[SalaServ] Senalizacion de seguridad");
    }

    // =====================================================
    //  DISPLAY DE TEMPERATURA/HUMEDAD
    // =====================================================

    static void CrearDisplayTemperatura(GameObject sala)
    {
        if (sala.transform.Find("DisplayTemp_DC") != null) return;

        GameObject disp = CrearContenedor("DisplayTemp_DC", sala.transform,
            new Vector3(2f, 2f, -3.9f));

        CrearCubo("Caja_Display", disp.transform,
            Vector3.zero, new Vector3(0.25f, 0.15f, 0.03f), matPlasticoNegro);

        CrearCubo("Pantalla_Temp", disp.transform,
            new Vector3(0f, 0f, -0.02f), new Vector3(0.2f, 0.1f, 0.005f), matPantalla);

        // LED indicador
        CrearEsfera("LED_Temp", disp.transform,
            new Vector3(0.1f, 0.05f, -0.02f), new Vector3(0.01f, 0.01f, 0.01f), matLedVerde);

        Debug.Log("[SalaServ] Display de temperatura/humedad");
    }

    // =====================================================
    //  ILUMINACION LED REALISTA
    // =====================================================

    static void CrearIluminacionLED(GameObject sala)
    {
        if (sala.transform.Find("IluminacionLED_DC") != null) return;

        GameObject contenedor = CrearContenedor("IluminacionLED_DC", sala.transform, Vector3.zero);

        Material matLuzPanel = CrearMatEmisivo("Mat_PanelLuz_DC",
            Shader.Find("Universal Render Pipeline/Lit"),
            new Color(0.9f, 0.9f, 0.88f), new Color(1f, 0.95f, 0.85f) * 2f);

        // Paneles LED empotrados en techo (2x4 grid)
        float[] posXLuz = { -2.5f, 0f, 2.5f };
        float[] posZLuz = { -2f, 1f };

        int idx = 0;
        foreach (float lx in posXLuz)
        {
            foreach (float lz in posZLuz)
            {
                // Panel LED
                CrearCubo($"PanelLED_{idx}", contenedor.transform,
                    new Vector3(lx, 3.38f, lz),
                    new Vector3(1.0f, 0.02f, 0.4f), matLuzPanel);

                // Luz puntual asociada
                GameObject luzGO = new GameObject($"LuzLED_{idx}");
                luzGO.transform.SetParent(contenedor.transform);
                luzGO.transform.localPosition = new Vector3(lx, 3.2f, lz);
                Light luz = luzGO.AddComponent<Light>();
                luz.type = LightType.Point;
                luz.color = new Color(0.95f, 0.92f, 0.85f);
                luz.intensity = 1.8f;
                luz.range = 5f;
                luz.shadows = LightShadows.Soft;
                Undo.RegisterCreatedObjectUndo(luzGO, "Crear luz LED");
                idx++;
            }
        }

        Debug.Log("[SalaServ] 6 paneles LED con luces puntuales");
    }

    // =====================================================
    //  LUCES INDICADORAS EN PISO (guias)
    // =====================================================

    static void CrearLucesIndicadorasPiso(GameObject sala)
    {
        if (sala.transform.Find("LucesPiso_DC") != null) return;

        GameObject contenedor = CrearContenedor("LucesPiso_DC", sala.transform, Vector3.zero);

        // Tiras LED azules en el suelo marcando pasillos
        Material matTiraLed = CrearMatEmisivo("Mat_TiraLED_Piso_DC",
            Shader.Find("Universal Render Pipeline/Lit"),
            new Color(0.01f, 0.02f, 0.06f), new Color(0.05f, 0.15f, 0.8f) * 1.5f);

        // Pasillo central (entre filas de racks)
        CrearCubo("TiraLED_Central_L", contenedor.transform,
            new Vector3(-0.5f, 0.03f, 0.5f), new Vector3(0.02f, 0.005f, 5f), matTiraLed);
        CrearCubo("TiraLED_Central_R", contenedor.transform,
            new Vector3(0.5f, 0.03f, 0.5f), new Vector3(0.02f, 0.005f, 5f), matTiraLed);

        // Marcas de posicion de racks en piso
        for (int i = 0; i < 3; i++)
        {
            float x = -2f + i * 2f;
            CrearCubo($"MarcaPiso_{i}", contenedor.transform,
                new Vector3(x, 0.025f, 3.0f), new Vector3(0.7f, 0.003f, 0.02f), matTiraLed);
        }

        Debug.Log("[SalaServ] Tiras LED de piso y marcas");
    }
}
#endif
