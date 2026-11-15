#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class LaboratorioPiso3Terror : EditorWindow
{
    private const string RUTA_MAT = "Assets/_Unity Essentials/Materials/Realistas";

    static Material matSueloLab, matParedLab, matTechoLab;
    static Material matMesaLab, matSillaLab, matMetalOscuro;
    static Material matPantallaApagada, matPantallaGlitch, matPantallaVerde, matPantallaRoja;
    static Material matCableNegro, matCableBio, matSombraLab;
    static Material matOjosLab, matEscrituraVerde, matEscrituraRoja;
    static Material matNieblaLab, matVidrioRotoLab, matOxidoLab;
    static Material matOrganico, matLuzVerdeDebil, matLuzRojaDebil;
    static Material matPlacaCircuito, matTelaranaLab, matPapelLab;

    [MenuItem("ISTR Terror/Construir Laboratorio Piso 3 TERROR")]
    public static void Ejecutar()
    {
        Debug.Log("[LabTerror] === INICIO: Escenografia de terror para Laboratorio Piso 3 ===");

        GameObject piso3 = GameObject.Find("Piso3_Laboratorio");
        if (piso3 == null)
        {
            piso3 = new GameObject("Piso3_Laboratorio");
            piso3.transform.position = new Vector3(0f, 8f, 18f);
            Undo.RegisterCreatedObjectUndo(piso3, "Crear Piso3_Laboratorio");
            Debug.Log("[LabTerror] Piso3_Laboratorio creado en (0, 8, 18)");
        }

        AsegurarCarpetas();
        CrearMaterialesLabTerror();

        ConstruirEstructuraLab(piso3);
        CrearColisionesLab(piso3);
        CrearEstacionesTrabajo(piso3);
        CrearComputadoraAnomala(piso3);
        CrearRacksLab(piso3);
        CrearPantallasGlitch(piso3);
        CrearCablesTentaculo(piso3);
        CrearCrecimientoBioDigital(piso3);
        CrearSiluetasLab(piso3);
        CrearEscriturasMensajes(piso3);
        CrearVigilancia(piso3);
        CrearDesorden(piso3);
        CrearIluminacionLab(piso3);
        CrearNieblaLab(piso3);
        CrearElementosSustoLab(piso3);

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[LabTerror] === COMPLETADO: El laboratorio del Piso 3 esta listo para aterrorizar ===");
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
    //  MATERIALES DE LABORATORIO TERROR
    // =====================================================

    static void CrearMaterialesLabTerror()
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");

        matSueloLab = Mat("Mat_SueloLab_Terror", lit, new Color(0.04f, 0.04f, 0.05f), 0f, 0.4f);
        matParedLab = Mat("Mat_ParedLab_Terror", lit, new Color(0.08f, 0.08f, 0.09f), 0f, 0.1f);
        matTechoLab = Mat("Mat_TechoLab_Terror", lit, new Color(0.06f, 0.06f, 0.07f), 0f, 0.15f);
        matMesaLab = Mat("Mat_MesaLab_Terror", lit, new Color(0.1f, 0.1f, 0.12f), 0.3f, 0.4f);
        matSillaLab = Mat("Mat_SillaLab_Terror", lit, new Color(0.03f, 0.03f, 0.035f), 0f, 0.2f);
        matMetalOscuro = Mat("Mat_MetalOscuroLab_Terror", lit, new Color(0.05f, 0.05f, 0.06f), 0.8f, 0.5f);

        matPantallaApagada = Mat("Mat_PantallaOff_Lab", lit, new Color(0.015f, 0.015f, 0.02f), 0f, 0.85f);
        matPantallaGlitch = MatEmisivo("Mat_PantallaGlitch_Lab", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(0.6f, 0.08f, 0.03f) * 1.5f);
        matPantallaVerde = MatEmisivo("Mat_PantallaVerde_Lab", lit,
            new Color(0.005f, 0.02f, 0.005f), new Color(0.05f, 0.8f, 0.12f) * 2f);
        matPantallaRoja = MatEmisivo("Mat_PantallaRoja_Lab", lit,
            new Color(0.02f, 0.005f, 0.005f), new Color(1.2f, 0.05f, 0.02f) * 2.5f);

        matCableNegro = Mat("Mat_CableNegro_Lab", lit, new Color(0.02f, 0.02f, 0.02f), 0.1f, 0.2f);
        matCableBio = MatEmisivo("Mat_CableBio_Lab", lit,
            new Color(0.02f, 0.04f, 0.02f), new Color(0.03f, 0.12f, 0.02f) * 0.8f);
        matSombraLab = Mat("Mat_SombraLab_Terror", lit, new Color(0.006f, 0.006f, 0.008f), 0f, 0.05f);
        matOjosLab = MatEmisivo("Mat_OjosLab_Terror", lit,
            new Color(0.3f, 0.5f, 0.2f), new Color(0.5f, 1.2f, 0.2f) * 3f);

        matEscrituraVerde = MatEmisivo("Mat_EscrituraVerde_Lab", lit,
            new Color(0.01f, 0.05f, 0.01f), new Color(0.05f, 0.4f, 0.05f) * 1.5f);
        matEscrituraRoja = MatEmisivo("Mat_EscrituraRoja_Lab", lit,
            new Color(0.08f, 0.01f, 0.005f), new Color(0.3f, 0.02f, 0.01f) * 1.5f);

        matNieblaLab = Mat("Mat_NieblaLab_Terror", lit, new Color(0.03f, 0.04f, 0.05f), 0f, 0f);
        SetTransparente(matNieblaLab, 0.12f);

        matVidrioRotoLab = Mat("Mat_VidrioRotoLab_Terror", lit, new Color(0.03f, 0.04f, 0.05f), 0f, 0.75f);
        matOxidoLab = Mat("Mat_OxidoLab_Terror", lit, new Color(0.2f, 0.1f, 0.04f), 0.4f, 0.2f);

        matOrganico = MatEmisivo("Mat_Organico_Lab", lit,
            new Color(0.04f, 0.06f, 0.02f), new Color(0.02f, 0.06f, 0.01f) * 0.5f);
        matLuzVerdeDebil = MatEmisivo("Mat_LuzVerdeDebil_Lab", lit,
            new Color(0.005f, 0.015f, 0.005f), new Color(0.02f, 0.1f, 0.02f));
        matLuzRojaDebil = MatEmisivo("Mat_LuzRojaDebil_Lab", lit,
            new Color(0.015f, 0.003f, 0.003f), new Color(0.8f, 0.02f, 0.01f) * 2f);

        matPlacaCircuito = Mat("Mat_PlacaCircuito_Lab", lit, new Color(0.02f, 0.06f, 0.02f), 0.2f, 0.3f);
        matTelaranaLab = Mat("Mat_TelaranaLab_Terror", lit, new Color(0.3f, 0.3f, 0.28f), 0f, 0.1f);
        SetTransparente(matTelaranaLab, 0.3f);
        matPapelLab = Mat("Mat_PapelLab_Terror", lit, new Color(0.12f, 0.11f, 0.1f), 0f, 0.15f);

        Debug.Log("[LabTerror] 24 materiales de laboratorio terror creados");
    }

    static void SetTransparente(Material mat, float alpha)
    {
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_Blend", 0);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0);
        mat.SetFloat("_AlphaClip", 0);
        mat.renderQueue = 3000;
        Color c = mat.GetColor("_BaseColor");
        c.a = alpha;
        mat.SetColor("_BaseColor", c);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        EditorUtility.SetDirty(mat);
    }

    static Material Mat(string n, Shader s, Color c, float met, float smooth)
    {
        string r = RUTA_MAT + "/" + n + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(r);
        if (m == null) { m = new Material(s); m.name = n; AssetDatabase.CreateAsset(m, r); }
        m.SetColor("_BaseColor", c); m.SetFloat("_Metallic", met); m.SetFloat("_Smoothness", smooth);
        EditorUtility.SetDirty(m); return m;
    }

    static Material MatEmisivo(string n, Shader s, Color c, Color em)
    {
        Material m = Mat(n, s, c, 0f, 0.3f);
        m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", em);
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        EditorUtility.SetDirty(m); return m;
    }

    // =====================================================
    //  HELPERS
    // =====================================================

    static GameObject C(string n, Transform p, Vector3 pos, Vector3 s, Material m)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = n; go.transform.SetParent(p); go.transform.localPosition = pos; go.transform.localScale = s;
        go.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        Undo.RegisterCreatedObjectUndo(go, n); return go;
    }

    static GameObject Cyl(string n, Transform p, Vector3 pos, Vector3 s, Material m)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = n; go.transform.SetParent(p); go.transform.localPosition = pos; go.transform.localScale = s;
        go.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        Undo.RegisterCreatedObjectUndo(go, n); return go;
    }

    static GameObject Sph(string n, Transform p, Vector3 pos, Vector3 s, Material m)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = n; go.transform.SetParent(p); go.transform.localPosition = pos; go.transform.localScale = s;
        go.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        Undo.RegisterCreatedObjectUndo(go, n); return go;
    }

    static GameObject Box(string n, Transform p, Vector3 pos)
    {
        var go = new GameObject(n); go.transform.SetParent(p); go.transform.localPosition = pos;
        Undo.RegisterCreatedObjectUndo(go, n); return go;
    }

    static Light LuzPuntual(string n, Transform p, Vector3 pos, Color c, float intens, float rango)
    {
        var go = new GameObject(n); go.transform.SetParent(p); go.transform.localPosition = pos;
        var l = go.AddComponent<Light>(); l.type = LightType.Point;
        l.color = c; l.intensity = intens; l.range = rango; l.shadows = LightShadows.Soft;
        Undo.RegisterCreatedObjectUndo(go, n); return l;
    }

    // =====================================================
    //  ESTRUCTURA DEL LABORATORIO
    // =====================================================

    static void ConstruirEstructuraLab(GameObject lab)
    {
        if (lab.transform.Find("Estructura_Lab") != null) return;
        GameObject est = Box("Estructura_Lab", lab.transform, Vector3.zero);

        float ancho = 12f;
        float largo = 10f;
        float alto = 3.5f;

        // Suelo
        C("Suelo_Lab", est.transform, new Vector3(0f, -0.05f, largo / 2f),
            new Vector3(ancho, 0.1f, largo), matSueloLab);

        // Techo
        C("Techo_Lab", est.transform, new Vector3(0f, alto, largo / 2f),
            new Vector3(ancho, 0.1f, largo), matTechoLab);

        // Pared fondo (norte)
        C("Pared_Fondo_Lab", est.transform, new Vector3(0f, alto / 2f, largo),
            new Vector3(ancho, alto, 0.2f), matParedLab);

        // Pared izquierda
        C("Pared_Izq_Lab", est.transform, new Vector3(-ancho / 2f, alto / 2f, largo / 2f),
            new Vector3(0.2f, alto, largo), matParedLab);

        // Pared derecha
        C("Pared_Der_Lab", est.transform, new Vector3(ancho / 2f, alto / 2f, largo / 2f),
            new Vector3(0.2f, alto, largo), matParedLab);

        // Pared entrada (sur) con hueco de puerta
        C("Pared_Entrada_Izq", est.transform, new Vector3(-ancho / 4f - 0.5f, alto / 2f, 0f),
            new Vector3(ancho / 2f - 1f, alto, 0.2f), matParedLab);
        C("Pared_Entrada_Der", est.transform, new Vector3(ancho / 4f + 0.5f, alto / 2f, 0f),
            new Vector3(ancho / 2f - 1f, alto, 0.2f), matParedLab);
        C("Dintel_Entrada", est.transform, new Vector3(0f, alto - 0.3f, 0f),
            new Vector3(2f, 0.6f, 0.2f), matParedLab);

        // Pared divisoria interna (separa zona principal de zona anomalia)
        C("Divisoria_Lab", est.transform, new Vector3(0f, alto / 2f, 5.5f),
            new Vector3(ancho * 0.6f, alto, 0.12f), matParedLab);
        // Hueco en divisoria (paso)
        C("Divisoria_Hueco", est.transform, new Vector3(3.5f, 1.05f, 5.5f),
            new Vector3(2.2f, 2.1f, 0.14f), matSueloLab);

        // Columnas estructurales
        C("Columna_1", est.transform, new Vector3(-4f, alto / 2f, 3f),
            new Vector3(0.3f, alto, 0.3f), matMetalOscuro);
        C("Columna_2", est.transform, new Vector3(4f, alto / 2f, 3f),
            new Vector3(0.3f, alto, 0.3f), matMetalOscuro);
        C("Columna_3", est.transform, new Vector3(-4f, alto / 2f, 7f),
            new Vector3(0.3f, alto, 0.3f), matMetalOscuro);
        C("Columna_4", est.transform, new Vector3(4f, alto / 2f, 7f),
            new Vector3(0.3f, alto, 0.3f), matMetalOscuro);

        Debug.Log("[LabTerror] Estructura: sala 12x10m con divisoria y columnas");
    }

    // =====================================================
    //  COLISIONES (suelo y paredes caminables)
    // =====================================================

    static void CrearColisionesLab(GameObject lab)
    {
        if (lab.transform.Find("Colisiones_Lab") != null) return;
        GameObject col = Box("Colisiones_Lab", lab.transform, Vector3.zero);

        float ancho = 12f;
        float largo = 10f;
        float alto = 3.5f;

        // Suelo caminable
        CrearColisionBox("Col_Suelo", col.transform,
            new Vector3(0f, -0.05f, largo / 2f), new Vector3(ancho, 0.1f, largo));

        // Paredes
        CrearColisionBox("Col_Fondo", col.transform,
            new Vector3(0f, alto / 2f, largo), new Vector3(ancho, alto, 0.2f));
        CrearColisionBox("Col_Izq", col.transform,
            new Vector3(-ancho / 2f, alto / 2f, largo / 2f), new Vector3(0.2f, alto, largo));
        CrearColisionBox("Col_Der", col.transform,
            new Vector3(ancho / 2f, alto / 2f, largo / 2f), new Vector3(0.2f, alto, largo));
        CrearColisionBox("Col_Entrada_Izq", col.transform,
            new Vector3(-ancho / 4f - 0.5f, alto / 2f, 0f), new Vector3(ancho / 2f - 1f, alto, 0.2f));
        CrearColisionBox("Col_Entrada_Der", col.transform,
            new Vector3(ancho / 4f + 0.5f, alto / 2f, 0f), new Vector3(ancho / 2f - 1f, alto, 0.2f));

        // Techo
        CrearColisionBox("Col_Techo", col.transform,
            new Vector3(0f, alto, largo / 2f), new Vector3(ancho, 0.1f, largo));

        // Divisoria
        CrearColisionBox("Col_Divisoria", col.transform,
            new Vector3(0f, alto / 2f, 5.5f), new Vector3(ancho * 0.6f, alto, 0.12f));

        // Columnas
        CrearColisionBox("Col_Columna1", col.transform,
            new Vector3(-4f, alto / 2f, 3f), new Vector3(0.3f, alto, 0.3f));
        CrearColisionBox("Col_Columna2", col.transform,
            new Vector3(4f, alto / 2f, 3f), new Vector3(0.3f, alto, 0.3f));
        CrearColisionBox("Col_Columna3", col.transform,
            new Vector3(-4f, alto / 2f, 7f), new Vector3(0.3f, alto, 0.3f));
        CrearColisionBox("Col_Columna4", col.transform,
            new Vector3(4f, alto / 2f, 7f), new Vector3(0.3f, alto, 0.3f));

        // Conexion con pasillo (rampa/suelo entre gradas y lab)
        CrearColisionBox("Col_Pasillo_Lab", col.transform,
            new Vector3(0f, -0.05f, -1f), new Vector3(3f, 0.1f, 2.5f));

        Debug.Log("[LabTerror] Colisiones: suelo, paredes, techo, divisoria, columnas, pasillo");
    }

    static void CrearColisionBox(string nombre, Transform padre, Vector3 pos, Vector3 tamano)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.size = tamano;
        bc.center = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(go, nombre);
    }

    // =====================================================
    //  ESTACIONES DE TRABAJO (mesas con monitores apagados)
    // =====================================================

    static void CrearEstacionesTrabajo(GameObject lab)
    {
        if (lab.transform.Find("Estaciones_Lab") != null) return;
        GameObject est = Box("Estaciones_Lab", lab.transform, Vector3.zero);

        // Fila izquierda de escritorios (zona principal, antes de divisoria)
        for (int i = 0; i < 4; i++)
        {
            float z = 1.5f + i * 1.2f;
            CrearEscritorio($"Escritorio_Izq_{i}", est.transform,
                new Vector3(-4f, 0f, z), 90f, i % 3 == 0);
        }

        // Fila derecha
        for (int i = 0; i < 4; i++)
        {
            float z = 1.5f + i * 1.2f;
            CrearEscritorio($"Escritorio_Der_{i}", est.transform,
                new Vector3(4f, 0f, z), -90f, i == 2);
        }

        // Escritorios en zona de anomalia (fondo)
        for (int i = 0; i < 3; i++)
        {
            float x = -3f + i * 3f;
            CrearEscritorio($"Escritorio_Fondo_{i}", est.transform,
                new Vector3(x, 0f, 7f), 0f, i != 1);
        }

        Debug.Log("[LabTerror] 11 estaciones de trabajo con monitores apagados");
    }

    static void CrearEscritorio(string nombre, Transform padre, Vector3 pos, float rotY, bool volcado)
    {
        GameObject desk = Box(nombre, padre, pos);
        desk.transform.localEulerAngles = new Vector3(0f, rotY, 0f);

        if (volcado)
        {
            // Escritorio volcado
            GameObject mesa = C("Mesa", desk.transform,
                new Vector3(0f, 0.38f, 0f), new Vector3(1.2f, 0.04f, 0.6f), matMesaLab);
            desk.transform.localEulerAngles = new Vector3(
                Random.Range(-5f, 15f), rotY + Random.Range(-20f, 20f), Random.Range(-10f, 40f));

            // Monitor caido
            C("Monitor_Caido", desk.transform,
                new Vector3(Random.Range(-0.3f, 0.3f), 0.08f, Random.Range(-0.5f, 0.3f)),
                new Vector3(0.4f, 0.28f, 0.03f), matPantallaApagada);
        }
        else
        {
            // Mesa en pie
            C("Tablero", desk.transform,
                new Vector3(0f, 0.74f, 0f), new Vector3(1.2f, 0.04f, 0.6f), matMesaLab);
            // Patas
            C("Pata_FL", desk.transform, new Vector3(-0.55f, 0.37f, -0.25f), new Vector3(0.04f, 0.74f, 0.04f), matMetalOscuro);
            C("Pata_FR", desk.transform, new Vector3(0.55f, 0.37f, -0.25f), new Vector3(0.04f, 0.74f, 0.04f), matMetalOscuro);
            C("Pata_BL", desk.transform, new Vector3(-0.55f, 0.37f, 0.25f), new Vector3(0.04f, 0.74f, 0.04f), matMetalOscuro);
            C("Pata_BR", desk.transform, new Vector3(0.55f, 0.37f, 0.25f), new Vector3(0.04f, 0.74f, 0.04f), matMetalOscuro);

            // Monitor apagado
            C("PantallaMon", desk.transform,
                new Vector3(0f, 1.0f, 0.1f), new Vector3(0.42f, 0.28f, 0.02f), matPantallaApagada);
            C("BaseMon", desk.transform,
                new Vector3(0f, 0.78f, 0.1f), new Vector3(0.15f, 0.04f, 0.12f), matMetalOscuro);
            C("PieMon", desk.transform,
                new Vector3(0f, 0.84f, 0.1f), new Vector3(0.03f, 0.12f, 0.03f), matMetalOscuro);

            // Teclado
            C("Teclado", desk.transform,
                new Vector3(0f, 0.77f, -0.1f), new Vector3(0.35f, 0.015f, 0.12f), matSillaLab);

            // Silla (algunas volcadas)
            if (Random.value > 0.4f)
            {
                GameObject silla = Box("Silla", desk.transform, new Vector3(0f, 0f, -0.5f));
                C("Asiento_S", silla.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.4f, 0.04f, 0.4f), matSillaLab);
                C("Respaldo_S", silla.transform, new Vector3(0f, 0.75f, 0.18f), new Vector3(0.38f, 0.55f, 0.03f), matSillaLab);
                Cyl("PieCentral_S", silla.transform, new Vector3(0f, 0.22f, 0f), new Vector3(0.04f, 0.22f, 0.04f), matMetalOscuro);

                if (Random.value > 0.5f)
                    silla.transform.localEulerAngles = new Vector3(80f, Random.Range(0f, 360f), 0f);
            }
        }
    }

    // =====================================================
    //  COMPUTADORA ANOMALA (pieza central)
    // =====================================================

    static void CrearComputadoraAnomala(GameObject lab)
    {
        if (lab.transform.Find("Computadora_Anomala") != null) return;
        // Z=5 local para alinear con zona de interaccion de SimulacionCompletaISTR
        GameObject comp = Box("Computadora_Anomala", lab.transform, new Vector3(0f, 0f, 5f));

        // Mesa especial (mas grande, metalica)
        C("Mesa_Anomala", comp.transform, new Vector3(0f, 0.74f, 0f),
            new Vector3(1.5f, 0.05f, 0.8f), matMetalOscuro);
        C("PataM_FL", comp.transform, new Vector3(-0.7f, 0.37f, -0.35f), new Vector3(0.05f, 0.74f, 0.05f), matMetalOscuro);
        C("PataM_FR", comp.transform, new Vector3(0.7f, 0.37f, -0.35f), new Vector3(0.05f, 0.74f, 0.05f), matMetalOscuro);
        C("PataM_BL", comp.transform, new Vector3(-0.7f, 0.37f, 0.35f), new Vector3(0.05f, 0.74f, 0.05f), matMetalOscuro);
        C("PataM_BR", comp.transform, new Vector3(0.7f, 0.37f, 0.35f), new Vector3(0.05f, 0.74f, 0.05f), matMetalOscuro);

        // Monitor principal (ENCENDIDO — verde terminal)
        C("Pantalla_Anomala", comp.transform,
            new Vector3(0f, 1.1f, 0.15f), new Vector3(0.55f, 0.38f, 0.025f), matPantallaVerde);
        C("Marco_Monitor", comp.transform,
            new Vector3(0f, 1.1f, 0.16f), new Vector3(0.6f, 0.42f, 0.02f), matMetalOscuro);
        C("Base_Monitor", comp.transform,
            new Vector3(0f, 0.78f, 0.15f), new Vector3(0.2f, 0.04f, 0.15f), matMetalOscuro);
        C("Pie_Monitor", comp.transform,
            new Vector3(0f, 0.87f, 0.15f), new Vector3(0.04f, 0.15f, 0.04f), matMetalOscuro);

        // "Lineas de codigo" en la pantalla (cubitos horizontales emisivos)
        for (int l = 0; l < 12; l++)
        {
            float y = 0.95f + l * 0.025f;
            float ancho = Random.Range(0.1f, 0.45f);
            float xOff = Random.Range(-0.2f, 0.1f);
            C($"Linea_Codigo_{l}", comp.transform,
                new Vector3(xOff, y, 0.135f),
                new Vector3(ancho, 0.008f, 0.001f), matEscrituraVerde);
        }

        // Monitor secundario (rojo, datos erraticos)
        C("Pantalla_Secundaria", comp.transform,
            new Vector3(-0.5f, 1.05f, 0.2f), new Vector3(0.35f, 0.25f, 0.02f), matPantallaRoja);
        C("Marco_MonSec", comp.transform,
            new Vector3(-0.5f, 1.05f, 0.21f), new Vector3(0.38f, 0.28f, 0.015f), matMetalOscuro);

        // CPU torre junto a la mesa
        C("CPU_Torre", comp.transform,
            new Vector3(0.6f, 0.35f, 0.2f), new Vector3(0.2f, 0.45f, 0.4f), matMetalOscuro);
        // LEDs de la CPU (parpadeantes)
        Sph("LED_CPU_1", comp.transform,
            new Vector3(0.6f, 0.45f, -0.01f), new Vector3(0.015f, 0.015f, 0.01f), matLuzVerdeDebil);
        Sph("LED_CPU_2", comp.transform,
            new Vector3(0.6f, 0.4f, -0.01f), new Vector3(0.015f, 0.015f, 0.01f), matLuzRojaDebil);

        // Cables que salen de la CPU hacia todos lados
        for (int c = 0; c < 8; c++)
        {
            float angulo = c * 45f * Mathf.Deg2Rad;
            float dx = Mathf.Cos(angulo) * 0.8f;
            float dz = Mathf.Sin(angulo) * 0.8f;
            Cyl($"Cable_CPU_{c}", comp.transform,
                new Vector3(0.6f + dx / 2f, 0.1f, 0.2f + dz / 2f),
                new Vector3(0.012f, 0.5f, 0.012f), matCableNegro);
            comp.transform.Find($"Cable_CPU_{c}").localEulerAngles =
                new Vector3(0f, 0f, Random.Range(40f, 75f));
        }

        // Silla intacta frente a la computadora (como si alguien acaba de levantarse)
        GameObject silla = Box("Silla_Anomala", comp.transform, new Vector3(0f, 0f, -0.6f));
        C("Asiento_SA", silla.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.42f, 0.04f, 0.42f), matSillaLab);
        C("Respaldo_SA", silla.transform, new Vector3(0f, 0.75f, 0.19f), new Vector3(0.4f, 0.55f, 0.03f), matSillaLab);
        Cyl("PieCentral_SA", silla.transform, new Vector3(0f, 0.22f, 0f), new Vector3(0.04f, 0.22f, 0.04f), matMetalOscuro);
        silla.transform.localEulerAngles = new Vector3(0f, 15f, 0f);

        Debug.Log("[LabTerror] Computadora anomala: monitor verde, pantalla roja, CPU con cables");
    }

    // =====================================================
    //  RACKS DE SERVIDOR EN EL LAB
    // =====================================================

    static void CrearRacksLab(GameObject lab)
    {
        if (lab.transform.Find("Racks_Lab") != null) return;
        GameObject racks = Box("Racks_Lab", lab.transform, Vector3.zero);

        // Racks contra la pared derecha (apagados, algunos abiertos)
        for (int i = 0; i < 4; i++)
        {
            float z = 1.5f + i * 2f;
            GameObject rack = Box($"Rack_Lab_{i}", racks.transform, new Vector3(5.3f, 0f, z));

            C("Cuerpo_R", rack.transform, new Vector3(0f, 1.1f, 0f),
                new Vector3(0.65f, 2.0f, 0.7f), matMetalOscuro);

            // Puerta del rack (abierta en algunos)
            if (i % 2 == 0)
            {
                GameObject puerta = C("Puerta_Rack", rack.transform,
                    new Vector3(-0.35f, 1.1f, -0.35f), new Vector3(0.6f, 1.8f, 0.025f), matMetalOscuro);
                puerta.transform.localEulerAngles = new Vector3(0f, -50f + Random.Range(-10f, 10f), 0f);
            }

            // Interior visible (discos, placas)
            if (i % 2 == 0)
            {
                for (int d = 0; d < 6; d++)
                {
                    float y = 0.3f + d * 0.25f;
                    C($"Disco_{d}", rack.transform,
                        new Vector3(0f, y, 0f), new Vector3(0.5f, 0.08f, 0.55f), matPlacaCircuito);
                }
            }

            // LED apagado
            Sph($"LED_Rack_{i}", rack.transform,
                new Vector3(0f, 2.0f, -0.36f), new Vector3(0.02f, 0.02f, 0.01f),
                i == 2 ? matLuzRojaDebil : matPantallaApagada);
        }

        // Rack especial contra la pared del fondo (conectado a la computadora anomala)
        GameObject rackEspecial = Box("Rack_Conectado", racks.transform, new Vector3(3f, 0f, 9.3f));
        C("Cuerpo_RE", rackEspecial.transform, new Vector3(0f, 1.1f, 0f),
            new Vector3(0.7f, 2.2f, 0.75f), matMetalOscuro);
        // Este rack tiene luz verde activa
        LuzPuntual("Luz_RackConectado", rackEspecial.transform,
            new Vector3(0f, 1f, -0.5f), new Color(0.1f, 0.6f, 0.15f), 0.5f, 2f);
        // Cables que lo conectan a la computadora anomala
        for (int c = 0; c < 5; c++)
        {
            float y = 0.3f + c * 0.35f;
            Cyl($"Cable_RC_{c}", rackEspecial.transform,
                new Vector3(-0.4f, y, -0.38f), new Vector3(0.01f, 1.2f, 0.01f), matCableBio);
            rackEspecial.transform.Find($"Cable_RC_{c}").localEulerAngles =
                new Vector3(0f, 0f, 70f + Random.Range(-10f, 10f));
        }

        Debug.Log("[LabTerror] 4 racks + 1 rack conectado a la anomalia");
    }

    // =====================================================
    //  PANTALLAS GLITCH (monitores con estatica)
    // =====================================================

    static void CrearPantallasGlitch(GameObject lab)
    {
        if (lab.transform.Find("Pantallas_Glitch") != null) return;
        GameObject pg = Box("Pantallas_Glitch", lab.transform, Vector3.zero);

        // Monitor grande en la pared (muestra datos erraticos)
        C("MonitorPared_1", pg.transform,
            new Vector3(-3f, 1.8f, 9.9f), new Vector3(0.7f, 0.45f, 0.03f), matPantallaGlitch);
        C("MarcoMP_1", pg.transform,
            new Vector3(-3f, 1.8f, 9.91f), new Vector3(0.75f, 0.5f, 0.02f), matMetalOscuro);

        // Monitor que muestra "TRANSMISION ACTIVA" (rojo)
        C("MonitorPared_2", pg.transform,
            new Vector3(3f, 1.8f, 9.9f), new Vector3(0.7f, 0.45f, 0.03f), matPantallaRoja);
        C("MarcoMP_2", pg.transform,
            new Vector3(3f, 1.8f, 9.91f), new Vector3(0.75f, 0.5f, 0.02f), matMetalOscuro);

        // Pantalla en el suelo (caida, aun encendida con glitch)
        GameObject monSuelo = C("Monitor_Suelo", pg.transform,
            new Vector3(2f, 0.15f, 3f), new Vector3(0.45f, 0.3f, 0.025f), matPantallaGlitch);
        monSuelo.transform.localEulerAngles = new Vector3(80f, 25f, 0f);

        // Pantalla colgando del techo (cable roto)
        GameObject monColgante = Box("Monitor_Colgante", pg.transform, new Vector3(-2f, 2.2f, 4f));
        C("Pantalla_MC", monColgante.transform, Vector3.zero,
            new Vector3(0.4f, 0.28f, 0.025f), matPantallaGlitch);
        Cyl("Cable_MC", monColgante.transform,
            new Vector3(0f, 0.5f, 0f), new Vector3(0.015f, 0.5f, 0.015f), matCableNegro);
        monColgante.transform.localEulerAngles = new Vector3(0f, 0f, 12f);

        // Proyeccion holografica simulada (cubos semi-transparentes apilados)
        Material matHolo = MatEmisivo("Mat_Holograma_Lab", Shader.Find("Universal Render Pipeline/Lit"),
            new Color(0.01f, 0.03f, 0.01f), new Color(0.03f, 0.15f, 0.04f) * 0.8f);
        SetTransparente(matHolo, 0.2f);

        GameObject holo = Box("Holograma_Datos", pg.transform, new Vector3(0f, 1.5f, 7.5f));
        for (int h = 0; h < 8; h++)
        {
            float y = -0.3f + h * 0.12f;
            float ancho = Random.Range(0.3f, 0.8f);
            C($"HoloLinea_{h}", holo.transform,
                new Vector3(Random.Range(-0.2f, 0.2f), y, -0.6f),
                new Vector3(ancho, 0.04f, 0.005f), matHolo);
        }

        Debug.Log("[LabTerror] Pantallas glitch, monitor colgante, holograma de datos");
    }

    // =====================================================
    //  CABLES COMO TENTACULOS
    // =====================================================

    static void CrearCablesTentaculo(GameObject lab)
    {
        if (lab.transform.Find("Cables_Tentaculo") != null) return;
        GameObject cab = Box("Cables_Tentaculo", lab.transform, Vector3.zero);

        // Marana de cables desde el techo (zona anomalia)
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(-4f, 4f);
            float z = Random.Range(6f, 9.5f);
            float largo = Random.Range(0.5f, 2.5f);
            float grosor = Random.Range(0.008f, 0.02f);
            bool bioCable = Random.value > 0.6f;

            GameObject cable = Cyl($"Tentaculo_{i}", cab.transform,
                new Vector3(x, 3.5f - largo / 2f, z),
                new Vector3(grosor, largo / 2f, grosor),
                bioCable ? matCableBio : matCableNegro);
            cable.transform.localEulerAngles = new Vector3(
                Random.Range(-15f, 15f), 0f, Random.Range(-15f, 15f));
        }

        // Cables que crecen por las paredes (como raices)
        for (int i = 0; i < 10; i++)
        {
            bool paredIzq = Random.value > 0.5f;
            float x = paredIzq ? -5.8f : 5.8f;
            float y = Random.Range(0.2f, 3f);
            float z = Random.Range(5.5f, 9f);
            float largo = Random.Range(0.4f, 1.5f);

            Cyl($"CablePared_{i}", cab.transform,
                new Vector3(x, y, z),
                new Vector3(0.012f, largo / 2f, 0.012f), matCableBio);
        }

        // Cables por el suelo (zona anomalia, dificultan el paso)
        for (int i = 0; i < 12; i++)
        {
            float x = Random.Range(-3f, 3f);
            float z = Random.Range(5.5f, 9f);
            float largo = Random.Range(0.5f, 2f);
            float angulo = Random.Range(0f, 360f);

            GameObject cableSuelo = Cyl($"CableSuelo_{i}", cab.transform,
                new Vector3(x, 0.02f, z),
                new Vector3(Random.Range(0.01f, 0.025f), largo / 2f, Random.Range(0.01f, 0.025f)),
                Random.value > 0.5f ? matCableNegro : matCableBio);
            cableSuelo.transform.localEulerAngles = new Vector3(90f, angulo, 0f);
        }

        Debug.Log("[LabTerror] 15 tentaculos de techo + 10 en paredes + 12 en suelo");
    }

    // =====================================================
    //  CRECIMIENTO BIO-DIGITAL
    // =====================================================

    static void CrearCrecimientoBioDigital(GameObject lab)
    {
        if (lab.transform.Find("BioDigital_Lab") != null) return;
        GameObject bio = Box("BioDigital_Lab", lab.transform, Vector3.zero);

        // Protuberancias organicas en paredes (como si algo crece desde los cables)
        Vector3[] posiciones = {
            new Vector3(-5.85f, 1.5f, 7f), new Vector3(-5.85f, 2.2f, 8.5f),
            new Vector3(5.85f, 1.0f, 6.5f), new Vector3(5.85f, 2.5f, 8f),
            new Vector3(0f, 3.4f, 7.5f), new Vector3(2f, 3.4f, 8.5f),
            new Vector3(-1f, 0.1f, 8f), new Vector3(1.5f, 0.1f, 9f)
        };

        for (int i = 0; i < posiciones.Length; i++)
        {
            float escala = Random.Range(0.1f, 0.35f);
            Sph($"Bio_Nodo_{i}", bio.transform, posiciones[i],
                new Vector3(escala, escala * Random.Range(0.6f, 1.2f), escala), matOrganico);

            // Ramificaciones desde cada nodo
            int ramas = Random.Range(2, 5);
            for (int r = 0; r < ramas; r++)
            {
                Vector3 dir = Random.onUnitSphere * Random.Range(0.1f, 0.3f);
                Cyl($"Bio_Rama_{i}_{r}", bio.transform,
                    posiciones[i] + dir / 2f,
                    new Vector3(0.015f, dir.magnitude / 2f, 0.015f), matCableBio);
            }
        }

        // Placas de circuito expuestas (como piel digital)
        for (int p = 0; p < 6; p++)
        {
            float x = Random.Range(-4f, 4f);
            float z = Random.Range(6f, 9.5f);
            bool enPared = Random.value > 0.5f;
            Vector3 pos = enPared
                ? new Vector3(Random.value > 0.5f ? -5.88f : 5.88f, Random.Range(0.5f, 2.5f), z)
                : new Vector3(x, 0.01f, z);

            C($"PlacaCircuito_{p}", bio.transform, pos,
                new Vector3(Random.Range(0.15f, 0.4f), 0.005f, Random.Range(0.1f, 0.3f)),
                matPlacaCircuito);
        }

        Debug.Log("[LabTerror] Crecimiento bio-digital: 8 nodos organicos + placas de circuito");
    }

    // =====================================================
    //  SILUETAS EN EL LABORATORIO
    // =====================================================

    static void CrearSiluetasLab(GameObject lab)
    {
        if (lab.transform.Find("Siluetas_Lab") != null) return;
        GameObject sil = Box("Siluetas_Lab", lab.transform, Vector3.zero);

        // Silueta sentada frente a un monitor apagado
        GameObject sentada = CrearFiguraHumanoide("Silueta_Sentada", sil.transform,
            new Vector3(-4f, 0.4f, 2.5f), 0.85f);
        sentada.transform.localEulerAngles = new Vector3(-15f, 90f, 0f);

        // Silueta de pie detras de la divisoria (apenas visible por el hueco)
        CrearFiguraHumanoide("Silueta_Divisoria", sil.transform,
            new Vector3(2f, 0f, 5.8f), 0.95f);

        // Silueta pegada al techo en zona de anomalia
        GameObject techoFig = CrearFiguraHumanoide("Silueta_Techo_Lab", sil.transform,
            new Vector3(-1f, 3.2f, 8f), 0.8f);
        techoFig.transform.localEulerAngles = new Vector3(180f, 0f, 0f);

        // Silueta agachada detras de un rack
        GameObject agachada = CrearFiguraHumanoide("Silueta_Rack", sil.transform,
            new Vector3(5.5f, 0f, 5f), 0.55f);
        agachada.transform.localEulerAngles = new Vector3(40f, -90f, 0f);

        // Ojos brillantes en la oscuridad del fondo
        Sph("Ojo_Lab_L1", sil.transform,
            new Vector3(-5f, 1.6f, 9.2f), new Vector3(0.035f, 0.035f, 0.02f), matOjosLab);
        Sph("Ojo_Lab_R1", sil.transform,
            new Vector3(-4.85f, 1.6f, 9.2f), new Vector3(0.035f, 0.035f, 0.02f), matOjosLab);

        // Ojos entre cables del techo
        Sph("Ojo_Techo_L", sil.transform,
            new Vector3(1f, 2.8f, 7.5f), new Vector3(0.04f, 0.04f, 0.02f), matOjosLab);
        Sph("Ojo_Techo_R", sil.transform,
            new Vector3(1.18f, 2.8f, 7.5f), new Vector3(0.04f, 0.04f, 0.02f), matOjosLab);

        // Ojos dentro de un rack abierto
        Sph("Ojo_Rack_L", sil.transform,
            new Vector3(5.3f, 1.0f, 1.5f), new Vector3(0.03f, 0.03f, 0.015f), matOjosLab);
        Sph("Ojo_Rack_R", sil.transform,
            new Vector3(5.45f, 1.0f, 1.5f), new Vector3(0.03f, 0.03f, 0.015f), matOjosLab);

        Debug.Log("[LabTerror] 4 siluetas humanoides + 3 pares de ojos brillantes");
    }

    static GameObject CrearFiguraHumanoide(string nombre, Transform padre, Vector3 pos, float escala)
    {
        GameObject fig = Box(nombre, padre, pos);

        C("Torso", fig.transform, new Vector3(0f, 0.7f * escala, 0f),
            new Vector3(0.35f * escala, 0.6f * escala, 0.2f * escala), matSombraLab);
        Sph("Cabeza", fig.transform, new Vector3(0f, 1.15f * escala, 0f),
            new Vector3(0.22f * escala, 0.25f * escala, 0.2f * escala), matSombraLab);
        C("Pierna_L", fig.transform, new Vector3(-0.08f * escala, 0.2f * escala, 0f),
            new Vector3(0.1f * escala, 0.55f * escala, 0.1f * escala), matSombraLab);
        C("Pierna_R", fig.transform, new Vector3(0.08f * escala, 0.2f * escala, 0f),
            new Vector3(0.1f * escala, 0.55f * escala, 0.1f * escala), matSombraLab);
        C("Brazo_L", fig.transform, new Vector3(-0.25f * escala, 0.65f * escala, 0f),
            new Vector3(0.08f * escala, 0.5f * escala, 0.08f * escala), matSombraLab);
        C("Brazo_R", fig.transform, new Vector3(0.25f * escala, 0.65f * escala, 0f),
            new Vector3(0.08f * escala, 0.5f * escala, 0.08f * escala), matSombraLab);
        Sph("Ojo_L", fig.transform, new Vector3(-0.05f * escala, 1.18f * escala, -0.1f * escala),
            new Vector3(0.025f, 0.025f, 0.01f), matOjosLab);
        Sph("Ojo_R", fig.transform, new Vector3(0.05f * escala, 1.18f * escala, -0.1f * escala),
            new Vector3(0.025f, 0.025f, 0.01f), matOjosLab);

        return fig;
    }

    // =====================================================
    //  ESCRITURAS Y MENSAJES
    // =====================================================

    static void CrearEscriturasMensajes(GameObject lab)
    {
        if (lab.transform.Find("Escrituras_Lab") != null) return;
        GameObject esc = Box("Escrituras_Lab", lab.transform, Vector3.zero);

        // "TRANSMISION ACTIVA" en pared del fondo (verde digital)
        CrearTextoDigital("TRANSMISION ACTIVA", esc.transform,
            new Vector3(-4f, 2.5f, 9.88f), 0f, 0.12f, matEscrituraVerde);

        // "ERROR 0x7F" repetido en la pared izquierda
        CrearTextoDigital("ERROR 0x7F", esc.transform,
            new Vector3(-5.88f, 1.5f, 4f), 90f, 0.1f, matEscrituraRoja);
        CrearTextoDigital("ERROR 0x7F", esc.transform,
            new Vector3(-5.88f, 2.2f, 6f), 90f, 0.08f, matEscrituraRoja);

        // "YA SABEMOS" garabateado (presagio del climax)
        CrearTextoDigital("YA SABEMOS", esc.transform,
            new Vector3(2f, 1.3f, 9.88f), 0f, 0.15f, matEscrituraRoja);

        // Numeros binarios en el techo
        CrearTextoDigital("01101000 01100101", esc.transform,
            new Vector3(0f, 3.44f, 7f), 0f, 0.08f, matEscrituraVerde);
        CrearTextoDigital("01101100 01110000", esc.transform,
            new Vector3(0f, 3.44f, 7.5f), 0f, 0.08f, matEscrituraVerde);

        // "HELP" en binario en la pared (para los que decodifiquen)
        // Conteo regresivo en la divisoria
        CrearTextoDigital("3 2 1 0", esc.transform,
            new Vector3(-1f, 2.5f, 5.44f), 0f, 0.12f, matEscrituraRoja);

        // IP address repetida
        CrearTextoDigital("192.168.1.666", esc.transform,
            new Vector3(5.88f, 2f, 7f), -90f, 0.09f, matEscrituraVerde);

        Debug.Log("[LabTerror] Escrituras: TRANSMISION ACTIVA, ERROR 0x7F, YA SABEMOS, binarios, IP");
    }

    static void CrearTextoDigital(string texto, Transform padre, Vector3 posInicio, float rotY, float tamLetra, Material mat)
    {
        GameObject textoGO = Box("Texto_" + texto.Replace(" ", "_").Substring(0, Mathf.Min(texto.Length, 15)),
            padre, posInicio);
        textoGO.transform.localEulerAngles = new Vector3(0f, rotY, 0f);

        float cursor = 0f;
        foreach (char ch in texto)
        {
            if (ch == ' ') { cursor += tamLetra * 0.5f; continue; }

            float jitter = Random.Range(-0.005f, 0.005f);
            CrearLetraDigital(ch, textoGO.transform,
                new Vector3(cursor, jitter, 0f), tamLetra, mat);
            cursor += tamLetra * 0.65f;
        }
    }

    static void CrearLetraDigital(char ch, Transform padre, Vector3 offset, float s, Material mat)
    {
        float t = s * 0.07f;
        float h = s;
        float w = s * 0.45f;

        switch (char.ToUpper(ch))
        {
            case 'T':
                C("H", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("V", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(t, h, t), mat);
                break;
            case 'R':
                C("V", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("R", padre, offset + new Vector3(w, h * 0.75f, 0), new Vector3(t, h / 2, t), mat);
                break;
            case 'A':
                C("L", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                break;
            case 'N':
                C("V1", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("V2", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                C("D", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(t, h * 1.2f, t), mat)
                    .transform.localEulerAngles = new Vector3(0, 0, 30);
                break;
            case 'S':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("TL", padre, offset + new Vector3(0, h * 0.75f, 0), new Vector3(t, h / 2, t), mat);
                C("BR", padre, offset + new Vector3(w, h * 0.25f, 0), new Vector3(t, h / 2, t), mat);
                break;
            case 'M':
                C("L", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("C", padre, offset + new Vector3(w / 2, h * 0.65f, 0), new Vector3(t, h * 0.3f, t), mat);
                break;
            case 'I':
                C("V", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w * 0.6f, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w * 0.6f, t, t), mat);
                break;
            case 'O':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("L", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                break;
            case 'C':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("L", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                break;
            case 'V':
                C("L", padre, offset + new Vector3(w * 0.15f, h / 2, 0), new Vector3(t, h, t), mat)
                    .transform.localEulerAngles = new Vector3(0, 0, -8);
                C("R", padre, offset + new Vector3(w * 0.85f, h / 2, 0), new Vector3(t, h, t), mat)
                    .transform.localEulerAngles = new Vector3(0, 0, 8);
                break;
            case 'E':
                C("V", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                break;
            case 'Y':
                C("TL", padre, offset + new Vector3(w * 0.15f, h * 0.75f, 0), new Vector3(t, h / 2, t), mat)
                    .transform.localEulerAngles = new Vector3(0, 0, -15);
                C("TR", padre, offset + new Vector3(w * 0.85f, h * 0.75f, 0), new Vector3(t, h / 2, t), mat)
                    .transform.localEulerAngles = new Vector3(0, 0, 15);
                C("B", padre, offset + new Vector3(w / 2, h * 0.25f, 0), new Vector3(t, h / 2, t), mat);
                break;
            case 'B':
                C("V", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                break;
            case 'D':
                C("V", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w * 0.3f, h, 0), new Vector3(w * 0.6f, t, t), mat);
                C("B", padre, offset + new Vector3(w * 0.3f, 0, 0), new Vector3(w * 0.6f, t, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h * 0.8f, t), mat);
                break;
            case 'L':
                C("V", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                break;
            case 'P':
                C("V", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("R", padre, offset + new Vector3(w, h * 0.75f, 0), new Vector3(t, h / 2, t), mat);
                break;
            case 'H':
                C("L", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                break;
            case 'X':
                C("D1", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(t, h * 1.1f, t), mat)
                    .transform.localEulerAngles = new Vector3(0, 0, 35);
                C("D2", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(t, h * 1.1f, t), mat)
                    .transform.localEulerAngles = new Vector3(0, 0, -35);
                break;
            case 'F':
                C("V", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                break;
            case 'G':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("L", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h * 0.25f, 0), new Vector3(t, h / 2, t), mat);
                C("HM", padre, offset + new Vector3(w * 0.7f, h / 2, 0), new Vector3(w * 0.6f, t, t), mat);
                break;
            case '0':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("L", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                break;
            case '1':
                C("V", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(t, h, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w * 0.6f, t, t), mat);
                break;
            case '2':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("TR", padre, offset + new Vector3(w, h * 0.75f, 0), new Vector3(t, h / 2, t), mat);
                C("BL", padre, offset + new Vector3(0, h * 0.25f, 0), new Vector3(t, h / 2, t), mat);
                break;
            case '3':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                break;
            case '6':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("L", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("BR", padre, offset + new Vector3(w, h * 0.25f, 0), new Vector3(t, h / 2, t), mat);
                break;
            case '7':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("R", padre, offset + new Vector3(w * 0.7f, h / 2, 0), new Vector3(t, h, t), mat)
                    .transform.localEulerAngles = new Vector3(0, 0, 8);
                break;
            case '8':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("L", padre, offset + new Vector3(0, h / 2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                break;
            case '9':
                C("T", padre, offset + new Vector3(w / 2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w / 2, 0, 0), new Vector3(w, t, t), mat);
                C("R", padre, offset + new Vector3(w, h / 2, 0), new Vector3(t, h, t), mat);
                C("TL", padre, offset + new Vector3(0, h * 0.75f, 0), new Vector3(t, h / 2, t), mat);
                break;
            case '.':
                C("Dot", padre, offset + new Vector3(w / 2, t / 2, 0), new Vector3(t * 1.5f, t * 1.5f, t), mat);
                break;
            default:
                C("X", padre, offset + new Vector3(w / 2, h / 2, 0), new Vector3(w, h, t), mat);
                break;
        }
    }

    // =====================================================
    //  VIGILANCIA (camaras, pantallas de monitoreo)
    // =====================================================

    static void CrearVigilancia(GameObject lab)
    {
        if (lab.transform.Find("Vigilancia_Lab") != null) return;
        GameObject vig = Box("Vigilancia_Lab", lab.transform, Vector3.zero);

        // Camaras de seguridad (apuntando al jugador)
        Vector3[] posCams = {
            new Vector3(-5.5f, 3.1f, 1f),
            new Vector3(5.5f, 3.1f, 9f),
            new Vector3(0f, 3.1f, 5.5f)
        };
        Vector3[] rotCams = {
            new Vector3(25f, 45f, 0f),
            new Vector3(25f, -135f, 0f),
            new Vector3(30f, 180f, 0f)
        };

        for (int i = 0; i < posCams.Length; i++)
        {
            GameObject cam = Box($"Camara_Lab_{i}", vig.transform, posCams[i]);
            cam.transform.localEulerAngles = rotCams[i];

            Cyl("Soporte_C", cam.transform, new Vector3(0f, 0.06f, 0f),
                new Vector3(0.025f, 0.06f, 0.025f), matMetalOscuro);
            C("Cuerpo_C", cam.transform, Vector3.zero,
                new Vector3(0.06f, 0.05f, 0.1f), matSillaLab);
            Cyl("Lente_C", cam.transform, new Vector3(0f, 0f, -0.07f),
                new Vector3(0.03f, 0.015f, 0.03f), matMetalOscuro);
            cam.transform.Find("Lente_C").localRotation = Quaternion.Euler(90f, 0f, 0f);

            // LED rojo siempre encendido
            Sph($"LED_Cam_{i}", cam.transform,
                new Vector3(0.02f, 0.02f, -0.05f), new Vector3(0.01f, 0.01f, 0.01f), matLuzRojaDebil);
        }

        // Panel de monitoreo (multiples pantallas pequenas en pared)
        GameObject panel = Box("Panel_Monitoreo", vig.transform, new Vector3(-5.85f, 1.5f, 2f));
        panel.transform.localEulerAngles = new Vector3(0f, 90f, 0f);

        for (int fila = 0; fila < 2; fila++)
        {
            for (int col = 0; col < 3; col++)
            {
                float x = -0.35f + col * 0.35f;
                float y = -0.2f + fila * 0.3f;
                Material pantMat = (fila == 0 && col == 1) ? matPantallaGlitch : matPantallaApagada;
                C($"MiniPantalla_{fila}_{col}", panel.transform,
                    new Vector3(x, y, 0f), new Vector3(0.28f, 0.2f, 0.02f), pantMat);
            }
        }
        C("Marco_Panel", panel.transform, Vector3.zero,
            new Vector3(1.1f, 0.7f, 0.03f), matMetalOscuro);

        Debug.Log("[LabTerror] 3 camaras de vigilancia + panel de monitoreo");
    }

    // =====================================================
    //  DESORDEN Y DESTRUCCION
    // =====================================================

    static void CrearDesorden(GameObject lab)
    {
        if (lab.transform.Find("Desorden_Lab") != null) return;
        GameObject des = Box("Desorden_Lab", lab.transform, Vector3.zero);

        // Papeles esparcidos por el suelo
        for (int p = 0; p < 15; p++)
        {
            float x = Random.Range(-5f, 5f);
            float z = Random.Range(0.5f, 9f);
            GameObject papel = C($"Papel_{p}", des.transform,
                new Vector3(x, 0.01f, z),
                new Vector3(0.2f, 0.002f, 0.28f), matPapelLab);
            papel.transform.localEulerAngles = new Vector3(
                Random.Range(-3f, 3f), Random.Range(0f, 360f), Random.Range(-3f, 3f));
        }

        // Tazas de cafe volcadas
        for (int t = 0; t < 3; t++)
        {
            float x = Random.Range(-4f, 4f);
            float z = Random.Range(1f, 4.5f);
            GameObject taza = Cyl($"Taza_{t}", des.transform,
                new Vector3(x, 0.04f, z), new Vector3(0.06f, 0.04f, 0.06f), matParedLab);
            taza.transform.localEulerAngles = new Vector3(85f, Random.Range(0f, 360f), 0f);
            // Charco de cafe
            C($"Charco_Cafe_{t}", des.transform,
                new Vector3(x + 0.05f, 0.005f, z), new Vector3(0.15f, 0.002f, 0.12f),
                matSueloLab);
        }

        // Vidrios rotos en el suelo
        for (int v = 0; v < 8; v++)
        {
            float x = Random.Range(-5f, 5f);
            float z = Random.Range(0.5f, 9f);
            C($"Vidrio_Lab_{v}", des.transform,
                new Vector3(x, 0.008f, z),
                new Vector3(Random.Range(0.05f, 0.2f), 0.003f, Random.Range(0.04f, 0.15f)),
                matVidrioRotoLab);
        }

        // Pizarra blanca rota/volcada
        GameObject pizarra = Box("Pizarra_Rota", des.transform, new Vector3(3f, 0.3f, 2f));
        C("Tablero_Piz", pizarra.transform, Vector3.zero,
            new Vector3(1.2f, 0.9f, 0.03f), matParedLab);
        C("Marco_Piz", pizarra.transform, Vector3.zero,
            new Vector3(1.25f, 0.95f, 0.02f), matOxidoLab);
        pizarra.transform.localEulerAngles = new Vector3(75f, 20f, -5f);

        // Telaranas en esquinas
        Vector3[] esquinasTela = {
            new Vector3(-5.5f, 3.3f, 0.5f), new Vector3(5.5f, 3.3f, 0.5f),
            new Vector3(-5.5f, 3.3f, 9.5f), new Vector3(5.5f, 3.3f, 9.5f)
        };
        for (int i = 0; i < esquinasTela.Length; i++)
        {
            C($"Telarana_Lab_{i}", des.transform, esquinasTela[i],
                new Vector3(Random.Range(0.5f, 1f), 0.003f, Random.Range(0.4f, 0.8f)),
                matTelaranaLab);
        }

        Debug.Log("[LabTerror] Desorden: 15 papeles, tazas, vidrios, pizarra rota, telaranas");
    }

    // =====================================================
    //  ILUMINACION DEL LABORATORIO
    // =====================================================

    static void CrearIluminacionLab(GameObject lab)
    {
        if (lab.transform.Find("Luces_Lab") != null) return;
        GameObject luces = Box("Luces_Lab", lab.transform, Vector3.zero);

        // ZONA PRINCIPAL: Casi a oscuras, solo tenue luz ambiental
        LuzPuntual("Luz_Entrada", luces.transform,
            new Vector3(0f, 2.8f, 1f), new Color(0.3f, 0.35f, 0.5f), 0.15f, 4f);

        // Luz parpadeante en zona principal (unica fuente)
        Light flickerPrincipal = LuzPuntual("Luz_Flicker_Principal", luces.transform,
            new Vector3(-2f, 3.0f, 3f), new Color(1f, 0.7f, 0.3f), 0.25f, 5f);
        C("Foco_Flicker", luces.transform,
            new Vector3(-2f, 3.2f, 3f), new Vector3(0.08f, 0.06f, 0.08f), matLuzVerdeDebil);
        Cyl("Cable_Foco", luces.transform,
            new Vector3(-2f, 3.35f, 3f), new Vector3(0.01f, 0.12f, 0.01f), matCableNegro);

        // ZONA ANOMALIA: Iluminada solo por la computadora
        LuzPuntual("Luz_Computadora", luces.transform,
            new Vector3(0f, 1.3f, 5f), new Color(0.1f, 0.7f, 0.15f), 1.5f, 4f);

        // Contraluz rojo detras de los racks
        LuzPuntual("Luz_Roja_Fondo", luces.transform,
            new Vector3(3f, 1.5f, 9.5f), new Color(0.8f, 0.05f, 0.02f), 0.4f, 3.5f);

        // Luces rojas de emergencia tenues
        LuzPuntual("LuzEmergencia_1", luces.transform,
            new Vector3(-5f, 3.0f, 5f), new Color(1f, 0.03f, 0.01f), 0.2f, 3f);
        LuzPuntual("LuzEmergencia_2", luces.transform,
            new Vector3(5f, 3.0f, 5f), new Color(1f, 0.03f, 0.01f), 0.2f, 3f);

        // Paneles de emergencia
        C("PanelEmerg_Lab_1", luces.transform,
            new Vector3(-5f, 3.1f, 5f), new Vector3(0.2f, 0.06f, 0.2f), matLuzRojaDebil);
        C("PanelEmerg_Lab_2", luces.transform,
            new Vector3(5f, 3.1f, 5f), new Vector3(0.2f, 0.06f, 0.2f), matLuzRojaDebil);

        // Paneles LED en techo (APAGADOS — solo estructura visible)
        for (int i = 0; i < 4; i++)
        {
            float x = -3f + i * 2f;
            C($"PanelLED_Apagado_{i}", luces.transform,
                new Vector3(x, 3.38f, 3f), new Vector3(0.8f, 0.02f, 0.3f), matPantallaApagada);
        }
        for (int i = 0; i < 3; i++)
        {
            float x = -2f + i * 2f;
            C($"PanelLED_Apagado_F_{i}", luces.transform,
                new Vector3(x, 3.38f, 7.5f), new Vector3(0.8f, 0.02f, 0.3f), matPantallaApagada);
        }

        Debug.Log("[LabTerror] Iluminacion: casi total oscuridad, luz verde de PC, luces rojas tenues");
    }

    // =====================================================
    //  NIEBLA DEL LABORATORIO
    // =====================================================

    static void CrearNieblaLab(GameObject lab)
    {
        if (lab.transform.Find("Niebla_Lab") != null) return;
        GameObject niebla = Box("Niebla_Lab", lab.transform, Vector3.zero);

        // Niebla rastrera (mas densa en zona de anomalia)
        for (int i = 0; i < 6; i++)
        {
            float x = Random.Range(-4f, 4f);
            float z = Random.Range(0.5f, 5f);
            C($"Niebla_Principal_{i}", niebla.transform,
                new Vector3(x, 0.12f + i * 0.03f, z),
                new Vector3(Random.Range(2f, 4f), 0.06f, Random.Range(2f, 3.5f)), matNieblaLab);
        }

        // Niebla densa en zona anomalia
        for (int i = 0; i < 8; i++)
        {
            float x = Random.Range(-4f, 4f);
            float z = Random.Range(5.5f, 9.5f);
            C($"Niebla_Anomalia_{i}", niebla.transform,
                new Vector3(x, 0.08f + i * 0.04f, z),
                new Vector3(Random.Range(2.5f, 5f), 0.1f, Random.Range(2f, 4f)), matNieblaLab);
        }

        // "Aliento" frio cerca de la computadora
        Material matAliento = Mat("Mat_Aliento_Lab", Shader.Find("Universal Render Pipeline/Lit"),
            new Color(0.15f, 0.18f, 0.22f), 0f, 0f);
        SetTransparente(matAliento, 0.08f);

        C("Aliento_Frio", niebla.transform,
            new Vector3(0f, 1f, 4.5f), new Vector3(1.5f, 0.3f, 0.8f), matAliento);

        Debug.Log("[LabTerror] Niebla: 6 capas principales + 8 en zona anomalia + aliento frio");
    }

    // =====================================================
    //  ELEMENTOS DE SUSTO
    // =====================================================

    static void CrearElementosSustoLab(GameObject lab)
    {
        if (lab.transform.Find("Sustos_Lab") != null) return;
        GameObject sustos = Box("Sustos_Lab", lab.transform, Vector3.zero);

        // Mano saliendo de debajo de un escritorio
        GameObject mano = Box("Mano_Escritorio", sustos.transform, new Vector3(-4f, 0.08f, 3.5f));
        C("Palma_M", mano.transform, Vector3.zero, new Vector3(0.1f, 0.15f, 0.06f), matSombraLab);
        for (int d = 0; d < 4; d++)
        {
            float dx = -0.035f + d * 0.023f;
            C($"Dedo_M_{d}", mano.transform,
                new Vector3(dx, 0.1f, 0f), new Vector3(0.018f, 0.1f, 0.018f), matSombraLab);
        }
        C("Pulgar_M", mano.transform,
            new Vector3(-0.06f, 0.03f, 0f), new Vector3(0.02f, 0.07f, 0.02f), matSombraLab);

        // Cara en una pantalla apagada (apenas perceptible)
        GameObject cara = Box("Cara_Pantalla", sustos.transform, new Vector3(4f, 1.0f, 2.1f));
        cara.transform.localEulerAngles = new Vector3(0f, -90f, 0f);
        // Ojos
        Sph("OjoCara_L", cara.transform,
            new Vector3(-0.06f, 0.03f, -0.015f), new Vector3(0.025f, 0.02f, 0.005f), matOjosLab);
        Sph("OjoCara_R", cara.transform,
            new Vector3(0.06f, 0.03f, -0.015f), new Vector3(0.025f, 0.02f, 0.005f), matOjosLab);
        // Boca
        C("BocaCara", cara.transform,
            new Vector3(0f, -0.04f, -0.015f), new Vector3(0.06f, 0.008f, 0.003f), matOjosLab);

        // Huellas de manos en el suelo (arrastrándose hacia la computadora)
        for (int h = 0; h < 5; h++)
        {
            float z = 3.5f + h * 0.4f;
            float x = Random.Range(-0.3f, 0.3f);
            GameObject huella = Box($"Huella_Suelo_{h}", sustos.transform,
                new Vector3(x, 0.005f, z));
            C("Palma_HS", huella.transform, Vector3.zero,
                new Vector3(0.08f, 0.002f, 0.1f), matEscrituraRoja);
            for (int d = 0; d < 4; d++)
            {
                float dx = -0.025f + d * 0.016f;
                C($"DedoHS_{d}", huella.transform,
                    new Vector3(dx, 0f, 0.06f), new Vector3(0.01f, 0.002f, 0.04f), matEscrituraRoja);
            }
            huella.transform.localEulerAngles = new Vector3(0f, Random.Range(-15f, 15f), 0f);
        }

        // Espiral de datos en el suelo (marca ritual tecnologica)
        int segmentos = 36;
        Vector3 centroEspiral = new Vector3(0f, 0.006f, 5f);
        for (int s = 0; s < segmentos; s++)
        {
            float angulo = (float)s / segmentos * Mathf.PI * 4f;
            float radio = 0.3f + s * 0.03f;
            float x = Mathf.Cos(angulo) * radio + centroEspiral.x;
            float z = Mathf.Sin(angulo) * radio + centroEspiral.z;

            C($"Espiral_{s}", sustos.transform,
                new Vector3(x, centroEspiral.y, z),
                new Vector3(0.04f, 0.003f, 0.04f), matEscrituraVerde);
        }

        // "Ojo" central de la espiral (placa de circuito con luz)
        Sph("OjoEspiral", sustos.transform,
            new Vector3(0f, 0.01f, 5f), new Vector3(0.15f, 0.01f, 0.15f), matLuzVerdeDebil);

        Debug.Log("[LabTerror] Sustos: mano bajo escritorio, cara en pantalla, huellas, espiral ritual");
    }
}
#endif
