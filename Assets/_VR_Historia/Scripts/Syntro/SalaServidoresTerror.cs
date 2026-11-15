#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class SalaServidoresTerror : EditorWindow
{
    private const string RUTA_MAT = "Assets/_Unity Essentials/Materials/Realistas";

    static Material matSueloViejo, matParedManchada, matTechoSucio, matOxido;
    static Material matSangre, matMarcaNegra, matCableRoto, matRackQuemado;
    static Material matLuzRoja, matLuzFlicker, matPantallaGlitch, matPantallaAzul;
    static Material matSombra, matOjos, matEscritura, matNiebla;
    static Material matRejaOxidada, matTuberiaOxidada, matVidrioCrack;
    static Material matLuzVerde, matTelarana, matPolvo;

    [MenuItem("Syntro Terror/Expandir Sala Servidores TERROR")]
    public static void Ejecutar()
    {
        Debug.Log("[Terror] === INICIO: Expansion siniestra de Sala de Servidores ===");

        GameObject sala = GameObject.Find("Piso1_CuartoMaquinas");
        if (sala == null)
        {
            Debug.LogError("[Terror] No se encontro Piso1_CuartoMaquinas");
            return;
        }

        AsegurarCarpetas();
        CrearMaterialesTerror();

        ExpandirSala(sala);
        CrearSeccionAbandonada(sala);
        CrearPasilloOscuro(sala);
        AgregarManchasYDanos(sala);
        CrearSiluetasOcultas(sala);
        CrearEscrituraPared(sala);
        CrearMonitoresGlitch(sala);
        CrearRacksMalditos(sala);
        CrearIluminacionTerror(sala);
        CrearDetallesAmbientales(sala);
        CrearNieblaRastrera(sala);
        CrearElementosSusto(sala);

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[Terror] === COMPLETADO: La sala ahora da escalofrios ===");
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
    //  MATERIALES DE TERROR
    // =====================================================

    static void CrearMaterialesTerror()
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");

        matSueloViejo = Mat("Mat_SueloViejo_Terror", lit, new Color(0.06f, 0.055f, 0.05f), 0f, 0.3f);
        matParedManchada = Mat("Mat_ParedManchada_Terror", lit, new Color(0.15f, 0.13f, 0.11f), 0f, 0.1f);
        matTechoSucio = Mat("Mat_TechoSucio_Terror", lit, new Color(0.12f, 0.11f, 0.1f), 0f, 0.15f);
        matOxido = Mat("Mat_Oxidado_Terror", lit, new Color(0.25f, 0.12f, 0.05f), 0.4f, 0.25f);
        matSangre = Mat("Mat_Sangre_Terror", lit, new Color(0.15f, 0.02f, 0.01f), 0f, 0.6f);
        matMarcaNegra = Mat("Mat_MarcaNegra_Terror", lit, new Color(0.02f, 0.02f, 0.015f), 0f, 0.1f);
        matCableRoto = Mat("Mat_CableRoto_Terror", lit, new Color(0.03f, 0.03f, 0.03f), 0.2f, 0.2f);
        matRackQuemado = Mat("Mat_RackQuemado_Terror", lit, new Color(0.04f, 0.035f, 0.03f), 0.5f, 0.15f);

        matLuzRoja = MatEmisivo("Mat_LuzRoja_Terror", lit,
            new Color(0.02f, 0.005f, 0.005f), new Color(1.5f, 0.03f, 0.01f) * 4f);
        matLuzFlicker = MatEmisivo("Mat_LuzFlicker_Terror", lit,
            new Color(0.04f, 0.03f, 0.01f), new Color(0.6f, 0.4f, 0.1f) * 0.5f);
        matPantallaGlitch = MatEmisivo("Mat_PantallaGlitch_Terror", lit,
            new Color(0.01f, 0.01f, 0.01f), new Color(0.8f, 0.1f, 0.05f) * 2f);
        matPantallaAzul = MatEmisivo("Mat_PantallaAzul_Terror", lit,
            new Color(0.01f, 0.01f, 0.02f), new Color(0.05f, 0.1f, 0.9f) * 1.2f);
        matLuzVerde = MatEmisivo("Mat_LuzVerdeDebil_Terror", lit,
            new Color(0.005f, 0.02f, 0.005f), new Color(0.02f, 0.15f, 0.02f));

        matSombra = Mat("Mat_Sombra_Terror", lit, new Color(0.008f, 0.008f, 0.01f), 0f, 0.05f);
        matOjos = MatEmisivo("Mat_Ojos_Terror", lit,
            new Color(0.5f, 0.5f, 0.2f), new Color(1.2f, 0.9f, 0.1f) * 3f);
        matEscritura = MatEmisivo("Mat_Escritura_Terror", lit,
            new Color(0.12f, 0.01f, 0.005f), new Color(0.3f, 0.02f, 0.01f) * 1.5f);
        matNiebla = Mat("Mat_Niebla_Terror", lit, new Color(0.06f, 0.07f, 0.09f), 0f, 0.0f);

        matRejaOxidada = Mat("Mat_Reja_Terror", lit, new Color(0.18f, 0.1f, 0.06f), 0.6f, 0.2f);
        matTuberiaOxidada = Mat("Mat_Tuberia_Terror", lit, new Color(0.2f, 0.1f, 0.04f), 0.5f, 0.2f);
        matVidrioCrack = Mat("Mat_VidrioRoto_Terror", lit, new Color(0.04f, 0.05f, 0.06f), 0f, 0.7f);
        matTelarana = Mat("Mat_Telarana_Terror", lit, new Color(0.4f, 0.4f, 0.38f), 0f, 0.1f);
        matPolvo = Mat("Mat_Polvo_Terror", lit, new Color(0.2f, 0.18f, 0.15f), 0f, 0.05f);

        // Hacer transparente la niebla y telaraña
        SetTransparente(matNiebla, 0.15f);
        SetTransparente(matTelarana, 0.35f);
        SetTransparente(matPolvo, 0.2f);
    }

    static void SetTransparente(Material mat, float alpha)
    {
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0);   // Alpha
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
    //  EXPANDIR SALA (hacer mas grande)
    // =====================================================

    static void ExpandirSala(GameObject sala)
    {
        // Expandir suelo, techo y paredes laterales
        ModificarEscala(sala, "Suelo_CM", new Vector3(18f, 0.2f, 14f), new Vector3(0f, -0.1f, 1f));
        ModificarEscala(sala, "Techo_CM", new Vector3(18f, 0.2f, 14f), new Vector3(0f, 3.6f, 1f));
        ModificarEscala(sala, "Pared_Fondo", new Vector3(18f, 3.5f, 0.2f), new Vector3(0f, 1.75f, 7f));
        ModificarEscala(sala, "Pared_Derecha", new Vector3(0.2f, 3.5f, 14f), new Vector3(9f, 1.75f, 1f));
        ModificarEscala(sala, "Pared_Izquierda", new Vector3(0.2f, 3.5f, 14f), new Vector3(-9f, 1.75f, 1f));
        ModificarEscala(sala, "Pared_Entrada", new Vector3(18f, 3.5f, 0.2f), new Vector3(0f, 1.75f, -6f));

        Debug.Log("[Terror] Sala expandida: 18x14m (antes 10x8m)");
    }

    static void ModificarEscala(GameObject sala, string nombre, Vector3 escala, Vector3 pos)
    {
        Transform t = sala.transform.Find(nombre);
        if (t == null) return;
        Undo.RecordObject(t, "Expandir " + nombre);
        t.localScale = escala;
        t.localPosition = pos;
    }

    // =====================================================
    //  SECCION ABANDONADA (al fondo de la sala expandida)
    // =====================================================

    static void CrearSeccionAbandonada(GameObject sala)
    {
        if (sala.transform.Find("Seccion_Abandonada") != null) return;
        GameObject sec = Box("Seccion_Abandonada", sala.transform, new Vector3(0f, 0f, 0f));

        // Reja oxidada que separa la seccion normal de la abandonada (Z=2)
        C("Reja_Separacion", sec.transform, new Vector3(0f, 1.75f, 2.5f), new Vector3(18f, 3.5f, 0.05f), matRejaOxidada);

        // Hueco roto en la reja (por donde se pasa)
        C("Reja_Hueco", sec.transform, new Vector3(0f, 1.0f, 2.5f), new Vector3(1.5f, 2.0f, 0.06f), matMarcaNegra);

        // --- Racks abandonados (volcados, quemados) ---
        GameObject rackCaido1 = Box("Rack_Caido_1", sec.transform, new Vector3(-5f, 0.4f, 5f));
        C("Cuerpo", rackCaido1.transform, Vector3.zero, new Vector3(0.7f, 2.2f, 0.8f), matRackQuemado);
        rackCaido1.transform.localEulerAngles = new Vector3(0f, 15f, 75f);

        GameObject rackCaido2 = Box("Rack_Caido_2", sec.transform, new Vector3(5.5f, 0.35f, 4f));
        C("Cuerpo", rackCaido2.transform, Vector3.zero, new Vector3(0.7f, 2.2f, 0.8f), matRackQuemado);
        rackCaido2.transform.localEulerAngles = new Vector3(10f, -20f, -80f);

        // Racks en pie pero destruidos
        for (int i = 0; i < 4; i++)
        {
            float x = -6f + i * 4f;
            GameObject rack = Box($"Rack_Destruido_{i}", sec.transform, new Vector3(x, 0f, 5.5f));
            C("Cuerpo", rack.transform, new Vector3(0f, 1.1f, 0f), new Vector3(0.7f, 2.2f, 0.8f), matRackQuemado);

            // Puerta arrancada
            if (i % 2 == 0)
            {
                GameObject puerta = C("Puerta_Arrancada", rack.transform,
                    new Vector3(0.5f, 0.3f, -0.5f), new Vector3(0.6f, 1.8f, 0.03f), matOxido);
                puerta.transform.localEulerAngles = new Vector3(5f, -40f, 10f);
            }

            // Cables colgando
            for (int c = 0; c < 6; c++)
            {
                float cx = Random.Range(-0.25f, 0.25f);
                float cy = Random.Range(0.3f, 1.8f);
                Cyl($"Cable_Colgante_{c}", rack.transform,
                    new Vector3(cx, cy, -0.45f),
                    new Vector3(0.01f, Random.Range(0.2f, 0.6f), 0.01f), matCableRoto);
            }

            // Marcas de quemadura en la base
            C($"Quemadura_{i}", sec.transform,
                new Vector3(x, 0.01f, 5.2f), new Vector3(1.2f, 0.005f, 0.8f), matMarcaNegra);
        }

        // Piso agrietado y sucio en la seccion abandonada
        C("Piso_Sucio_Abandonado", sec.transform,
            new Vector3(0f, 0.005f, 4.5f), new Vector3(16f, 0.005f, 5f), matSueloViejo);

        // Tuberia rota goteando
        Cyl("Tuberia_Rota", sec.transform,
            new Vector3(3f, 3.2f, 5f), new Vector3(0.08f, 3f, 0.08f), matTuberiaOxidada);
        sec.transform.Find("Tuberia_Rota").localRotation = Quaternion.Euler(0f, 0f, 90f);

        // Charco debajo de tuberia
        C("Charco_Agua", sec.transform,
            new Vector3(3f, 0.01f, 5f), new Vector3(1.5f, 0.005f, 1.2f), matSangre);

        // Silla volcada
        GameObject silla = Box("Silla_Volcada", sec.transform, new Vector3(-3f, 0.2f, 3.5f));
        C("Asiento", silla.transform, Vector3.zero, new Vector3(0.4f, 0.05f, 0.4f), matMarcaNegra);
        C("Respaldo", silla.transform, new Vector3(0f, 0.2f, 0.18f), new Vector3(0.38f, 0.4f, 0.03f), matMarcaNegra);
        Cyl("Pata", silla.transform, new Vector3(0f, -0.15f, 0f), new Vector3(0.03f, 0.15f, 0.03f), matOxido);
        silla.transform.localEulerAngles = new Vector3(85f, 30f, 0f);

        Debug.Log("[Terror] Seccion abandonada: racks destruidos, tuberia rota, silla volcada");
    }

    // =====================================================
    //  PASILLO OSCURO (extension lateral siniestra)
    // =====================================================

    static void CrearPasilloOscuro(GameObject sala)
    {
        if (sala.transform.Find("Pasillo_Oscuro") != null) return;
        GameObject pas = Box("Pasillo_Oscuro", sala.transform, new Vector3(-9f, 0f, 1f));

        float largo = 8f;
        float ancho = 2f;
        float alto = 3.5f;

        C("Piso_Pasillo", pas.transform, new Vector3(-ancho / 2f, -0.05f, largo / 2f),
            new Vector3(ancho, 0.1f, largo), matSueloViejo);
        C("Techo_Pasillo", pas.transform, new Vector3(-ancho / 2f, alto, largo / 2f),
            new Vector3(ancho, 0.1f, largo), matTechoSucio);
        C("Pared_Izq", pas.transform, new Vector3(-ancho, alto / 2f, largo / 2f),
            new Vector3(0.15f, alto, largo), matParedManchada);
        C("Pared_Der", pas.transform, new Vector3(0f, alto / 2f, largo / 2f),
            new Vector3(0.15f, alto, largo), matParedManchada);
        C("Pared_Fondo", pas.transform, new Vector3(-ancho / 2f, alto / 2f, largo),
            new Vector3(ancho, alto, 0.15f), matParedManchada);

        // Unica luz parpadeante al fondo
        LuzPuntual("Luz_Final_Pasillo", pas.transform,
            new Vector3(-ancho / 2f, 2.8f, largo - 1f),
            new Color(0.8f, 0.3f, 0.1f), 0.4f, 4f);

        // Puerta al final del pasillo (sellada, con cadena)
        C("Puerta_Sellada", pas.transform,
            new Vector3(-ancho / 2f, 1.05f, largo - 0.08f),
            new Vector3(0.9f, 2.1f, 0.05f), matOxido);
        // Cadena cruzada
        C("Cadena_H", pas.transform,
            new Vector3(-ancho / 2f, 1.2f, largo - 0.11f),
            new Vector3(0.7f, 0.03f, 0.03f), matRejaOxidada);
        C("Cadena_V", pas.transform,
            new Vector3(-ancho / 2f, 1.2f, largo - 0.11f),
            new Vector3(0.03f, 0.5f, 0.03f), matRejaOxidada);

        // Manchas en el piso del pasillo
        C("Mancha_Pasillo_1", pas.transform,
            new Vector3(-0.8f, 0.005f, 3f), new Vector3(0.6f, 0.003f, 1.2f), matSangre);
        C("Mancha_Pasillo_2", pas.transform,
            new Vector3(-1.2f, 0.005f, 5.5f), new Vector3(0.4f, 0.003f, 0.8f), matSangre);

        // Arrastres en la pared
        for (int i = 0; i < 5; i++)
        {
            float z = 2f + i * 1.2f;
            float y = 0.8f + Random.Range(-0.3f, 0.3f);
            C($"Arrastre_{i}", pas.transform,
                new Vector3(-ancho + 0.1f, y, z),
                new Vector3(0.01f, 0.08f, 0.6f), matSangre);
        }

        Debug.Log("[Terror] Pasillo oscuro con puerta sellada y manchas");
    }

    // =====================================================
    //  MANCHAS Y DANOS EN SALA PRINCIPAL
    // =====================================================

    static void AgregarManchasYDanos(GameObject sala)
    {
        if (sala.transform.Find("Manchas_Terror") != null) return;
        GameObject manchas = Box("Manchas_Terror", sala.transform, Vector3.zero);

        // Manchas oscuras en el piso
        Vector3[] posManchas = {
            new Vector3(-3f, 0.005f, -2f), new Vector3(4f, 0.005f, 0.5f),
            new Vector3(-1f, 0.005f, 1f), new Vector3(6f, 0.005f, -3f),
            new Vector3(-7f, 0.005f, 3f), new Vector3(2f, 0.005f, -4.5f)
        };
        for (int i = 0; i < posManchas.Length; i++)
        {
            float sx = Random.Range(0.3f, 1.2f);
            float sz = Random.Range(0.2f, 0.8f);
            Material m = (i < 2) ? matSangre : matMarcaNegra;
            C($"Mancha_{i}", manchas.transform, posManchas[i], new Vector3(sx, 0.003f, sz), m);
        }

        // Marcas de quemadura en paredes
        C("Quemadura_Pared1", manchas.transform,
            new Vector3(-2f, 1.5f, 6.95f), new Vector3(1.5f, 1.0f, 0.01f), matMarcaNegra);
        C("Quemadura_Pared2", manchas.transform,
            new Vector3(5f, 0.8f, 6.95f), new Vector3(0.8f, 0.6f, 0.01f), matMarcaNegra);

        // Vidrio roto en el piso
        for (int v = 0; v < 4; v++)
        {
            float x = Random.Range(-5f, 5f);
            float z = Random.Range(-3f, 2f);
            C($"Vidrio_Roto_{v}", manchas.transform,
                new Vector3(x, 0.01f, z),
                new Vector3(Random.Range(0.1f, 0.3f), 0.005f, Random.Range(0.1f, 0.2f)),
                matVidrioCrack);
        }

        Debug.Log("[Terror] Manchas de sangre, quemaduras y vidrio roto");
    }

    // =====================================================
    //  SILUETAS OCULTAS (aparecen entre racks)
    // =====================================================

    static void CrearSiluetasOcultas(GameObject sala)
    {
        if (sala.transform.Find("Siluetas_Terror") != null) return;
        GameObject sil = Box("Siluetas_Terror", sala.transform, Vector3.zero);

        // Silueta 1: entre racks del fondo, apenas visible
        CrearFiguraHumanoide("Silueta_EntreRacks", sil.transform,
            new Vector3(-1f, 0f, 6.2f), 0.9f);

        // Silueta 2: en la esquina de la seccion abandonada, agachada
        GameObject agachada = CrearFiguraHumanoide("Silueta_Agachada", sil.transform,
            new Vector3(7f, 0f, 5.5f), 0.6f);
        agachada.transform.localEulerAngles = new Vector3(30f, -90f, 0f);

        // Silueta 3: pegada al techo (invertida)
        GameObject techo = CrearFiguraHumanoide("Silueta_Techo", sil.transform,
            new Vector3(3f, 3.3f, 4f), 0.85f);
        techo.transform.localEulerAngles = new Vector3(180f, 0f, 0f);

        // Ojos brillantes en la oscuridad del pasillo oscuro
        Sph("Ojo_Izq_Pasillo", sil.transform,
            new Vector3(-10.5f, 1.6f, 7f), new Vector3(0.04f, 0.04f, 0.02f), matOjos);
        Sph("Ojo_Der_Pasillo", sil.transform,
            new Vector3(-10.3f, 1.6f, 7f), new Vector3(0.04f, 0.04f, 0.02f), matOjos);

        // Ojos detras de un rack destruido
        Sph("Ojo_Izq_Rack", sil.transform,
            new Vector3(-6.1f, 0.9f, 5.8f), new Vector3(0.03f, 0.03f, 0.015f), matOjos);
        Sph("Ojo_Der_Rack", sil.transform,
            new Vector3(-5.95f, 0.9f, 5.8f), new Vector3(0.03f, 0.03f, 0.015f), matOjos);

        // Ojos en el techo (miran hacia abajo)
        Sph("Ojo_Izq_Techo", sil.transform,
            new Vector3(0f, 3.45f, 0f), new Vector3(0.05f, 0.02f, 0.05f), matOjos);
        Sph("Ojo_Der_Techo", sil.transform,
            new Vector3(0.2f, 3.45f, 0f), new Vector3(0.05f, 0.02f, 0.05f), matOjos);

        Debug.Log("[Terror] 3 siluetas humanoides + 3 pares de ojos brillantes");
    }

    static GameObject CrearFiguraHumanoide(string nombre, Transform padre, Vector3 pos, float escala)
    {
        GameObject fig = Box(nombre, padre, pos);

        // Cuerpo
        C("Torso", fig.transform, new Vector3(0f, 0.7f * escala, 0f),
            new Vector3(0.35f * escala, 0.6f * escala, 0.2f * escala), matSombra);
        // Cabeza
        Sph("Cabeza", fig.transform, new Vector3(0f, 1.15f * escala, 0f),
            new Vector3(0.22f * escala, 0.25f * escala, 0.2f * escala), matSombra);
        // Piernas
        C("Pierna_L", fig.transform, new Vector3(-0.08f * escala, 0.2f * escala, 0f),
            new Vector3(0.1f * escala, 0.55f * escala, 0.1f * escala), matSombra);
        C("Pierna_R", fig.transform, new Vector3(0.08f * escala, 0.2f * escala, 0f),
            new Vector3(0.1f * escala, 0.55f * escala, 0.1f * escala), matSombra);
        // Brazos (ligeramente extendidos)
        C("Brazo_L", fig.transform, new Vector3(-0.25f * escala, 0.65f * escala, 0f),
            new Vector3(0.08f * escala, 0.5f * escala, 0.08f * escala), matSombra);
        C("Brazo_R", fig.transform, new Vector3(0.25f * escala, 0.65f * escala, 0f),
            new Vector3(0.08f * escala, 0.5f * escala, 0.08f * escala), matSombra);
        // Ojos tenues
        Sph("Ojo_L", fig.transform, new Vector3(-0.05f * escala, 1.18f * escala, -0.1f * escala),
            new Vector3(0.025f, 0.025f, 0.01f), matOjos);
        Sph("Ojo_R", fig.transform, new Vector3(0.05f * escala, 1.18f * escala, -0.1f * escala),
            new Vector3(0.025f, 0.025f, 0.01f), matOjos);

        return fig;
    }

    // =====================================================
    //  ESCRITURA EN PAREDES
    // =====================================================

    static void CrearEscrituraPared(GameObject sala)
    {
        if (sala.transform.Find("Escritura_Terror") != null) return;
        GameObject esc = Box("Escritura_Terror", sala.transform, Vector3.zero);

        // "NO SALGAS" en pared del fondo (letras con cubos)
        CrearTexto("NO SALGAS", esc.transform,
            new Vector3(-4f, 2.2f, 6.93f), 0f, 0.25f, matEscritura);

        // "EL TE VE" cerca de los racks
        CrearTexto("EL TE VE", esc.transform,
            new Vector3(3f, 1.5f, 6.93f), 0f, 0.18f, matSangre);

        // "AYUDA" escrito temblando en la pared izquierda
        CrearTexto("AYUDA", esc.transform,
            new Vector3(-8.93f, 1.8f, -2f), 90f, 0.2f, matEscritura);

        // Rayones erraticos (como uñas)
        for (int r = 0; r < 8; r++)
        {
            float y = 1.0f + r * 0.15f;
            float largo = Random.Range(0.3f, 0.8f);
            C($"Rayon_{r}", esc.transform,
                new Vector3(-8.93f, y, 2f + Random.Range(-0.5f, 0.5f)),
                new Vector3(0.005f, 0.02f, largo), matEscritura);
        }

        // Numeros repetidos "3 3 3 3" en techo
        CrearTexto("3333", esc.transform,
            new Vector3(0f, 3.55f, 3f), 0f, 0.15f, matSangre);

        Debug.Log("[Terror] Escrituras en paredes: NO SALGAS, EL TE VE, AYUDA, rayones");
    }

    static void CrearTexto(string texto, Transform padre, Vector3 posInicio, float rotY, float tamLetra, Material mat)
    {
        GameObject textoGO = Box("Texto_" + texto.Replace(" ", ""), padre, posInicio);
        textoGO.transform.localEulerAngles = new Vector3(0f, rotY, 0f);

        float cursor = 0f;
        foreach (char ch in texto)
        {
            if (ch == ' ') { cursor += tamLetra * 0.6f; continue; }

            // Cada letra = serie de cubos finos formando trazos
            float temblor = Random.Range(-0.02f, 0.02f);
            CrearLetra(ch, textoGO.transform,
                new Vector3(cursor, temblor, 0f), tamLetra, mat);
            cursor += tamLetra * 0.7f;
        }
    }

    static void CrearLetra(char ch, Transform padre, Vector3 offset, float s, Material mat)
    {
        float t = s * 0.08f; // grosor trazo
        float h = s;         // alto
        float w = s * 0.5f;  // ancho

        // Simplificacion: cada letra con trazos basicos
        switch (char.ToUpper(ch))
        {
            case 'N':
                C("V1", padre, offset + new Vector3(0, h/2, 0), new Vector3(t, h, t), mat);
                C("V2", padre, offset + new Vector3(w, h/2, 0), new Vector3(t, h, t), mat);
                C("D", padre, offset + new Vector3(w/2, h/2, 0), new Vector3(t, h*1.2f, t), mat).transform.localEulerAngles = new Vector3(0,0,30);
                break;
            case 'O':
                C("T", padre, offset + new Vector3(w/2, h, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w/2, 0, 0), new Vector3(w, t, t), mat);
                C("L", padre, offset + new Vector3(0, h/2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h/2, 0), new Vector3(t, h, t), mat);
                break;
            case 'S':
                C("T", padre, offset + new Vector3(w/2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w/2, h/2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w/2, 0, 0), new Vector3(w, t, t), mat);
                C("TL", padre, offset + new Vector3(0, h*0.75f, 0), new Vector3(t, h/2, t), mat);
                C("BR", padre, offset + new Vector3(w, h*0.25f, 0), new Vector3(t, h/2, t), mat);
                break;
            case 'A':
                C("L", padre, offset + new Vector3(0, h/2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h/2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w/2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w/2, h/2, 0), new Vector3(w, t, t), mat);
                break;
            case 'L':
                C("V", padre, offset + new Vector3(0, h/2, 0), new Vector3(t, h, t), mat);
                C("B", padre, offset + new Vector3(w/2, 0, 0), new Vector3(w, t, t), mat);
                break;
            case 'G':
                C("T", padre, offset + new Vector3(w/2, h, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w/2, 0, 0), new Vector3(w, t, t), mat);
                C("L", padre, offset + new Vector3(0, h/2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h*0.25f, 0), new Vector3(t, h/2, t), mat);
                C("M", padre, offset + new Vector3(w*0.7f, h/2, 0), new Vector3(w*0.6f, t, t), mat);
                break;
            case 'E':
                C("V", padre, offset + new Vector3(0, h/2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w/2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w/2, h/2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w/2, 0, 0), new Vector3(w, t, t), mat);
                break;
            case 'T':
                C("H", padre, offset + new Vector3(w/2, h, 0), new Vector3(w, t, t), mat);
                C("V", padre, offset + new Vector3(w/2, h/2, 0), new Vector3(t, h, t), mat);
                break;
            case 'V':
                C("L", padre, offset + new Vector3(w*0.15f, h/2, 0), new Vector3(t, h, t), mat).transform.localEulerAngles = new Vector3(0,0,-8);
                C("R", padre, offset + new Vector3(w*0.85f, h/2, 0), new Vector3(t, h, t), mat).transform.localEulerAngles = new Vector3(0,0,8);
                break;
            case 'Y':
                C("TL", padre, offset + new Vector3(w*0.15f, h*0.75f, 0), new Vector3(t, h/2, t), mat).transform.localEulerAngles = new Vector3(0,0,-15);
                C("TR", padre, offset + new Vector3(w*0.85f, h*0.75f, 0), new Vector3(t, h/2, t), mat).transform.localEulerAngles = new Vector3(0,0,15);
                C("B", padre, offset + new Vector3(w/2, h*0.25f, 0), new Vector3(t, h/2, t), mat);
                break;
            case 'U':
                C("L", padre, offset + new Vector3(0, h/2, 0), new Vector3(t, h, t), mat);
                C("R", padre, offset + new Vector3(w, h/2, 0), new Vector3(t, h, t), mat);
                C("B", padre, offset + new Vector3(w/2, 0, 0), new Vector3(w, t, t), mat);
                break;
            case 'D':
                C("V", padre, offset + new Vector3(0, h/2, 0), new Vector3(t, h, t), mat);
                C("T", padre, offset + new Vector3(w*0.3f, h, 0), new Vector3(w*0.6f, t, t), mat);
                C("B", padre, offset + new Vector3(w*0.3f, 0, 0), new Vector3(w*0.6f, t, t), mat);
                C("R", padre, offset + new Vector3(w, h/2, 0), new Vector3(t, h*0.8f, t), mat);
                break;
            case '3':
                C("T", padre, offset + new Vector3(w/2, h, 0), new Vector3(w, t, t), mat);
                C("M", padre, offset + new Vector3(w/2, h/2, 0), new Vector3(w, t, t), mat);
                C("B", padre, offset + new Vector3(w/2, 0, 0), new Vector3(w, t, t), mat);
                C("R", padre, offset + new Vector3(w, h/2, 0), new Vector3(t, h, t), mat);
                break;
            default:
                C("X", padre, offset + new Vector3(w/2, h/2, 0), new Vector3(w, h, t), mat);
                break;
        }
    }

    // =====================================================
    //  MONITORES CON GLITCH / PANTALLA AZUL
    // =====================================================

    static void CrearMonitoresGlitch(GameObject sala)
    {
        if (sala.transform.Find("Monitores_Terror") != null) return;
        GameObject mon = Box("Monitores_Terror", sala.transform, Vector3.zero);

        // Monitor glitch rojo en la pared (muestra estatica)
        C("Monitor_Glitch_1", mon.transform,
            new Vector3(6f, 1.5f, 6.93f), new Vector3(0.5f, 0.35f, 0.03f), matPantallaGlitch);
        C("MarcoMon_1", mon.transform,
            new Vector3(6f, 1.5f, 6.94f), new Vector3(0.55f, 0.4f, 0.02f), matRackQuemado);

        // Monitor BSOD (pantalla azul de la muerte)
        C("Monitor_BSOD", mon.transform,
            new Vector3(-6f, 1.5f, 6.93f), new Vector3(0.5f, 0.35f, 0.03f), matPantallaAzul);
        C("MarcoMon_2", mon.transform,
            new Vector3(-6f, 1.5f, 6.94f), new Vector3(0.55f, 0.4f, 0.02f), matRackQuemado);

        // Monitor en el suelo (caido, con grieta)
        GameObject monCaido = C("Monitor_Caido", mon.transform,
            new Vector3(2f, 0.15f, -3f), new Vector3(0.5f, 0.35f, 0.03f), matPantallaGlitch);
        monCaido.transform.localEulerAngles = new Vector3(85f, 15f, 0f);

        // Pantallas que parpadean verde en racks destruidos
        C("Pantalla_Rack_1", mon.transform,
            new Vector3(-6f, 1.0f, 5.1f), new Vector3(0.15f, 0.1f, 0.005f), matLuzVerde);
        C("Pantalla_Rack_2", mon.transform,
            new Vector3(2f, 0.8f, 5.1f), new Vector3(0.15f, 0.1f, 0.005f), matPantallaGlitch);

        Debug.Log("[Terror] Monitores glitch, BSOD y caidos");
    }

    // =====================================================
    //  RACKS MALDITOS (uno que "respira", uno con luz interior)
    // =====================================================

    static void CrearRacksMalditos(GameObject sala)
    {
        if (sala.transform.Find("Racks_Malditos") != null) return;
        GameObject rm = Box("Racks_Malditos", sala.transform, Vector3.zero);

        // Rack que brilla rojo por dentro (como si algo viviera dentro)
        GameObject rackVivo = Box("Rack_Vivo", rm.transform, new Vector3(7f, 0f, 5f));
        C("Cuerpo", rackVivo.transform, new Vector3(0f, 1.1f, 0f),
            new Vector3(0.7f, 2.2f, 0.8f), matRackQuemado);
        // Luz roja interna pulsante
        LuzPuntual("Luz_Interior_Rack", rackVivo.transform,
            new Vector3(0f, 1.0f, 0f), new Color(1f, 0.05f, 0.02f), 2f, 1.5f);
        // Grietas con luz
        C("Grieta_1", rackVivo.transform,
            new Vector3(0f, 1.3f, -0.41f), new Vector3(0.02f, 0.8f, 0.005f), matLuzRoja);
        C("Grieta_2", rackVivo.transform,
            new Vector3(0.1f, 0.8f, -0.41f), new Vector3(0.4f, 0.015f, 0.005f), matLuzRoja);
        C("Grieta_3", rackVivo.transform,
            new Vector3(-0.15f, 1.6f, -0.41f), new Vector3(0.3f, 0.012f, 0.005f), matLuzRoja);

        // Rack congelado (escarcha simulada)
        GameObject rackFrio = Box("Rack_Congelado", rm.transform, new Vector3(-7f, 0f, 4f));
        C("Cuerpo", rackFrio.transform, new Vector3(0f, 1.1f, 0f),
            new Vector3(0.7f, 2.2f, 0.8f), matParedManchada);
        // Escarcha (cubos blancos semi-transparentes)
        Material matEscarcha = Mat("Mat_Escarcha_Terror",
            Shader.Find("Universal Render Pipeline/Lit"),
            new Color(0.7f, 0.75f, 0.85f), 0f, 0.8f);
        SetTransparente(matEscarcha, 0.4f);

        for (int e = 0; e < 12; e++)
        {
            float ex = Random.Range(-0.35f, 0.35f);
            float ey = Random.Range(0.1f, 2.0f);
            C($"Escarcha_{e}", rackFrio.transform,
                new Vector3(ex, ey, -0.42f),
                new Vector3(Random.Range(0.05f, 0.2f), Random.Range(0.02f, 0.08f), 0.005f),
                matEscarcha);
        }
        // Luz azul fria
        LuzPuntual("Luz_Fria", rackFrio.transform,
            new Vector3(0f, 1f, -0.5f), new Color(0.3f, 0.4f, 0.9f), 0.8f, 2f);

        Debug.Log("[Terror] Rack 'vivo' con grietas rojas + Rack congelado con escarcha");
    }

    // =====================================================
    //  ILUMINACION TERROR
    // =====================================================

    static void CrearIluminacionTerror(GameObject sala)
    {
        if (sala.transform.Find("Luces_Terror") != null) return;
        GameObject luces = Box("Luces_Terror", sala.transform, Vector3.zero);

        // Luz roja de emergencia pulsante (zona principal)
        LuzPuntual("LuzRoja_Central", luces.transform,
            new Vector3(0f, 3.0f, -2f), new Color(1f, 0.05f, 0.02f), 1.2f, 8f);

        // Luces rojas de emergencia en esquinas
        LuzPuntual("LuzRoja_Esq1", luces.transform,
            new Vector3(-8f, 3.0f, -5f), new Color(1f, 0.03f, 0.01f), 0.6f, 5f);
        LuzPuntual("LuzRoja_Esq2", luces.transform,
            new Vector3(8f, 3.0f, -5f), new Color(1f, 0.03f, 0.01f), 0.6f, 5f);

        // Paneles de emergencia
        C("PanelEmergencia_1", luces.transform,
            new Vector3(-8f, 3.2f, -5f), new Vector3(0.3f, 0.08f, 0.3f), matLuzRoja);
        C("PanelEmergencia_2", luces.transform,
            new Vector3(8f, 3.2f, -5f), new Vector3(0.3f, 0.08f, 0.3f), matLuzRoja);

        // Luz tenue verde (salida de emergencia falsa)
        LuzPuntual("LuzVerde_Salida", luces.transform,
            new Vector3(0f, 3.2f, -5.8f), new Color(0.1f, 0.6f, 0.1f), 0.3f, 3f);

        // Seccion abandonada: casi sin luz
        LuzPuntual("LuzDebil_Abandonada", luces.transform,
            new Vector3(0f, 2.5f, 5f), new Color(0.8f, 0.3f, 0.1f), 0.15f, 6f);

        // Luz parpadeante solitaria en seccion abandonada
        Light flickerLight = LuzPuntual("LuzFlicker_Abandonada", luces.transform,
            new Vector3(-4f, 3.0f, 4.5f), new Color(1f, 0.7f, 0.3f), 0.4f, 4f);
        C("Foco_Colgante", luces.transform,
            new Vector3(-4f, 3.1f, 4.5f), new Vector3(0.08f, 0.08f, 0.08f), matLuzFlicker);
        Cyl("Cable_Foco", luces.transform,
            new Vector3(-4f, 3.35f, 4.5f), new Vector3(0.01f, 0.2f, 0.01f), matCableRoto);

        Debug.Log("[Terror] Iluminacion roja de emergencia + zonas oscuras");
    }

    // =====================================================
    //  DETALLES AMBIENTALES
    // =====================================================

    static void CrearDetallesAmbientales(GameObject sala)
    {
        if (sala.transform.Find("Ambiente_Terror") != null) return;
        GameObject amb = Box("Ambiente_Terror", sala.transform, Vector3.zero);

        // Telaranas en esquinas del techo
        Vector3[] esquinas = {
            new Vector3(-8.5f, 3.4f, 6.5f), new Vector3(8.5f, 3.4f, 6.5f),
            new Vector3(-8.5f, 3.4f, -5.5f), new Vector3(8.5f, 3.4f, -5.5f),
            new Vector3(-8.5f, 3.4f, 3f), new Vector3(8.5f, 3.4f, 3f)
        };
        for (int i = 0; i < esquinas.Length; i++)
        {
            C($"Telarana_{i}", amb.transform, esquinas[i],
                new Vector3(Random.Range(0.5f, 1.2f), 0.003f, Random.Range(0.5f, 1.0f)),
                matTelarana);
        }

        // Polvo flotante (particulas estaticas)
        for (int p = 0; p < 20; p++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-8f, 8f), Random.Range(0.5f, 3f), Random.Range(-5f, 6f));
            C($"Polvo_{p}", amb.transform, pos,
                new Vector3(Random.Range(0.02f, 0.06f), 0.002f, Random.Range(0.02f, 0.06f)),
                matPolvo);
        }

        // Tuberias oxidadas expuestas en techo
        for (int t = 0; t < 3; t++)
        {
            float z = -2f + t * 4f;
            Cyl($"Tuberia_Techo_{t}", amb.transform,
                new Vector3(0f, 3.4f, z), new Vector3(0.06f, 9f, 0.06f), matTuberiaOxidada);
            amb.transform.Find($"Tuberia_Techo_{t}").localRotation = Quaternion.Euler(0f, 0f, 90f);
        }

        // Papeles tirados en el suelo
        for (int p = 0; p < 6; p++)
        {
            float x = Random.Range(-7f, 7f);
            float z = Random.Range(-4f, 5f);
            float rot = Random.Range(0f, 360f);
            GameObject papel = C($"Papel_{p}", amb.transform,
                new Vector3(x, 0.01f, z), new Vector3(0.2f, 0.002f, 0.28f), matParedManchada);
            papel.transform.localEulerAngles = new Vector3(0f, rot, Random.Range(-5f, 5f));
        }

        Debug.Log("[Terror] Telaranas, polvo, tuberias oxidadas, papeles");
    }

    // =====================================================
    //  NIEBLA RASTRERA
    // =====================================================

    static void CrearNieblaRastrera(GameObject sala)
    {
        if (sala.transform.Find("Niebla_Terror") != null) return;
        GameObject niebla = Box("Niebla_Terror", sala.transform, Vector3.zero);

        // Capas de niebla a ras de suelo
        for (int i = 0; i < 8; i++)
        {
            float x = Random.Range(-7f, 7f);
            float z = Random.Range(-4f, 6f);
            float sx = Random.Range(2f, 5f);
            float sz = Random.Range(2f, 4f);
            C($"NieblaRastrera_{i}", niebla.transform,
                new Vector3(x, 0.15f + i * 0.04f, z),
                new Vector3(sx, 0.08f, sz), matNiebla);
        }

        // Niebla mas densa en la seccion abandonada
        for (int i = 0; i < 5; i++)
        {
            float x = Random.Range(-6f, 6f);
            float z = Random.Range(3f, 6.5f);
            C($"NieblaDensa_{i}", niebla.transform,
                new Vector3(x, 0.1f + i * 0.06f, z),
                new Vector3(Random.Range(3f, 6f), 0.12f, Random.Range(3f, 5f)), matNiebla);
        }

        Debug.Log("[Terror] Niebla rastrera en suelo y seccion abandonada");
    }

    // =====================================================
    //  ELEMENTOS DE SUSTO
    // =====================================================

    static void CrearElementosSusto(GameObject sala)
    {
        if (sala.transform.Find("Sustos_Terror") != null) return;
        GameObject sustos = Box("Sustos_Terror", sala.transform, Vector3.zero);

        // Mano saliendo de un rack (apenas visible)
        GameObject mano = Box("Mano_Rack", sustos.transform, new Vector3(-2f, 0.5f, 3.1f));
        C("Palma", mano.transform, Vector3.zero, new Vector3(0.1f, 0.15f, 0.06f), matSombra);
        for (int d = 0; d < 4; d++)
        {
            float dx = -0.035f + d * 0.023f;
            C($"Dedo_{d}", mano.transform,
                new Vector3(dx, 0.1f, 0f), new Vector3(0.018f, 0.1f, 0.018f), matSombra);
        }
        C("Pulgar", mano.transform,
            new Vector3(-0.06f, 0.03f, 0f), new Vector3(0.02f, 0.07f, 0.02f), matSombra);

        // Huella de mano ensangrentada en vidrio/pared
        GameObject huella = Box("Huella_Sangre", sustos.transform,
            new Vector3(3f, 1.3f, -5.95f));
        C("Palma_H", huella.transform, Vector3.zero, new Vector3(0.12f, 0.14f, 0.003f), matSangre);
        for (int d = 0; d < 4; d++)
        {
            float dx = -0.04f + d * 0.025f;
            C($"DedoH_{d}", huella.transform,
                new Vector3(dx, 0.1f, 0f), new Vector3(0.015f, 0.08f, 0.003f), matSangre);
        }
        C("ChorreoH", huella.transform,
            new Vector3(0f, -0.2f, 0f), new Vector3(0.04f, 0.3f, 0.003f), matSangre);

        // Cabeza de maniqui/muneco en el suelo (perturbador)
        Sph("Cabeza_Muneco", sustos.transform,
            new Vector3(-5f, 0.12f, -1f), new Vector3(0.22f, 0.25f, 0.2f), matParedManchada);
        // Un ojo del muneco
        Sph("Ojo_Muneco", sustos.transform,
            new Vector3(-5.04f, 0.16f, -1.1f), new Vector3(0.04f, 0.04f, 0.02f), matOjos);

        // Espejo roto en la pared (refleja algo que no deberia)
        C("Espejo_Roto_Marco", sustos.transform,
            new Vector3(8.93f, 1.5f, -1f), new Vector3(0.01f, 0.6f, 0.4f), matOxido);
        C("Espejo_Roto_Vidrio", sustos.transform,
            new Vector3(8.92f, 1.5f, -1f), new Vector3(0.005f, 0.5f, 0.35f), matVidrioCrack);
        // Grieta en el espejo
        C("Grieta_Espejo", sustos.transform,
            new Vector3(8.91f, 1.5f, -1f), new Vector3(0.003f, 0.45f, 0.02f), matMarcaNegra);

        // Marca circular en el suelo (como un ritual)
        int segmentos = 24;
        float radio = 1.2f;
        Vector3 centroRitual = new Vector3(0f, 0.006f, 5f);
        for (int s = 0; s < segmentos; s++)
        {
            float angulo = (float)s / segmentos * Mathf.PI * 2f;
            float x = Mathf.Cos(angulo) * radio + centroRitual.x;
            float z = Mathf.Sin(angulo) * radio + centroRitual.z;
            float nextAng = (float)(s + 1) / segmentos * Mathf.PI * 2f;
            float nx = Mathf.Cos(nextAng) * radio + centroRitual.x;
            float nz = Mathf.Sin(nextAng) * radio + centroRitual.z;

            Vector3 mid = new Vector3((x + nx) / 2f, centroRitual.y, (z + nz) / 2f);
            float dist = Vector3.Distance(new Vector3(x, 0, z), new Vector3(nx, 0, nz));
            float ang = Mathf.Atan2(nz - z, nx - x) * Mathf.Rad2Deg;

            GameObject seg_go = C($"Circulo_{s}", sustos.transform, mid,
                new Vector3(dist, 0.003f, 0.03f), matSangre);
            seg_go.transform.localEulerAngles = new Vector3(0f, -ang, 0f);
        }
        // Simbolo interior (estrella simplificada)
        for (int r = 0; r < 5; r++)
        {
            float ang1 = r * 72f * Mathf.Deg2Rad;
            float ang2 = ((r + 2) % 5) * 72f * Mathf.Deg2Rad;
            Vector3 p1 = centroRitual + new Vector3(Mathf.Cos(ang1) * radio * 0.9f, 0, Mathf.Sin(ang1) * radio * 0.9f);
            Vector3 p2 = centroRitual + new Vector3(Mathf.Cos(ang2) * radio * 0.9f, 0, Mathf.Sin(ang2) * radio * 0.9f);
            Vector3 mid = (p1 + p2) / 2f;
            float dist = Vector3.Distance(p1, p2);
            float ang = Mathf.Atan2(p2.z - p1.z, p2.x - p1.x) * Mathf.Rad2Deg;

            GameObject linea = C($"Estrella_{r}", sustos.transform, mid,
                new Vector3(dist, 0.004f, 0.025f), matEscritura);
            linea.transform.localEulerAngles = new Vector3(0f, -ang, 0f);
        }

        Debug.Log("[Terror] Mano en rack, huella sangre, cabeza muneco, espejo roto, circulo ritual");
    }
}
#endif
