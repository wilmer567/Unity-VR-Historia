#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrepararJuegoCompletoSyntro
{
    [MenuItem("Syntro Terror/▶ JUEGO COMPLETO (Construir + Simular)")]
    public static void Ejecutar()
    {
        Debug.Log("=============================================================");
        Debug.Log("[Syntro] ▶ PREPARANDO JUEGO COMPLETO...");
        Debug.Log("=============================================================");

        ConstruirTodo();
        ConstruirGradasVisuales();
        CrearObjetosInteractivos();
        CrearManagersRuntime();
        ConfiguradorSimulacionSyntro.Ejecutar();
        ConectarReferencias();

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.ClearProgressBar();

        Debug.Log("=============================================================");
        Debug.Log("[Syntro] ▶ JUEGO COMPLETO LISTO");
        Debug.Log("[Syntro] Presiona PLAY para jugar la experiencia completa.");
        Debug.Log("[Syntro] Controles: WASD=mover, Mouse=mirar, Shift=correr, E=interactuar");
        Debug.Log("[Syntro] Flujo: Piso 1 > Gradas > Piso 3 > Climax > FIN");
        Debug.Log("=============================================================");
    }

    // ============================================================
    //  PASO 1: CONSTRUIR TODA LA GEOMETRIA
    // ============================================================

    static void ConstruirTodo()
    {
        string[] nombres = {
            "Texturas PBR", "Graficos Realistas", "Cuadros Fotorrealistas",
            "Sala de Servidores", "Edificio Syntro", "Terror Piso 1", "Laboratorio Piso 3",
            "Atmosfera Visual Terror", "Audio Procedural Terror"
        };
        System.Action[] acciones = {
            AplicarTexturasPBR.Ejecutar,
            ConfiguradorGraficosRealistas.Ejecutar,
            ConfiguradorCuadrosFotorrealistas.Ejecutar,
            DisenoSalaServidores.Ejecutar,
            EdificioSyntro.Ejecutar,
            SalaServidoresTerror.Ejecutar,
            LaboratorioPiso3Terror.Ejecutar,
            AtmosferaVisualTerrorSyntro.Ejecutar,
            GeneradorAudioTerrorSyntro.Ejecutar
        };

        for (int i = 0; i < nombres.Length; i++)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Syntro Juego Completo",
                    $"Paso {i + 1}/{nombres.Length}: {nombres[i]}...", (float)i / nombres.Length);
                Debug.Log($"[Syntro] Paso {i + 1}/{nombres.Length}: {nombres[i]}...");
                acciones[i]();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Syntro] Error en {nombres[i]}: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    // ============================================================
    //  PASO 2: GRADAS VISUALES (escalones + paredes + atmosfera)
    // ============================================================

    static void ConstruirGradasVisuales()
    {
        if (GameObject.Find("Bloque_Gradas_Terror") != null)
        {
            Debug.Log("[Syntro] Gradas visuales ya existen, omitiendo.");
            return;
        }

        EditorUtility.DisplayProgressBar("Syntro Juego Completo", "Construyendo gradas visuales...", 0.75f);

        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        Material matEscalon = CrearMat("Mat_Escalon_Gradas", lit, new Color(0.06f, 0.06f, 0.07f), 0.1f, 0.3f);
        Material matParedG = CrearMat("Mat_ParedGradas", lit, new Color(0.04f, 0.04f, 0.05f), 0f, 0.1f);
        Material matBarandal = CrearMat("Mat_BarandalGradas", lit, new Color(0.08f, 0.08f, 0.09f), 0.7f, 0.4f);
        Material matSombraG = CrearMat("Mat_SombraGradas", lit, new Color(0.01f, 0.01f, 0.015f), 0f, 0.05f);
        Material matSilueta = CrearMat("Mat_SiluetaGradas", lit, new Color(0.005f, 0.005f, 0.008f), 0f, 0.05f);

        Material matMarcaG = new Material(lit);
        matMarcaG.name = "Mat_MarcaGradas";
        matMarcaG.SetColor("_BaseColor", new Color(0.01f, 0.03f, 0.01f));
        matMarcaG.SetColor("_EmissionColor", new Color(0.03f, 0.08f, 0.02f));
        matMarcaG.EnableKeyword("_EMISSION");

        GameObject gradas = new GameObject("Bloque_Gradas_Terror");
        Undo.RegisterCreatedObjectUndo(gradas, "Crear Gradas Terror");

        // --- ESCALONES ---
        int n = 15;
        float escH = 4f / n;
        float escD = 5f / n;
        float anchoEsc = 2.5f;

        // Tramo inferior: Y 0->4, Z 5->10
        for (int i = 0; i < n; i++)
        {
            float y = (i + 0.5f) * escH;
            float z = 5f + (i + 0.5f) * escD;
            Cubo($"Esc_Inf_{i}", gradas.transform,
                new Vector3(0f, y, z), new Vector3(anchoEsc, escH, escD), matEscalon);
        }

        // Descanso Y~4, Z 10->11
        Cubo("Descanso_Visual", gradas.transform,
            new Vector3(0f, 3.93f, 10.5f), new Vector3(anchoEsc, 0.15f, 1.4f), matEscalon);

        // Tramo superior: Y 4->8, Z 11->16
        for (int i = 0; i < n; i++)
        {
            float y = 4f + (i + 0.5f) * escH;
            float z = 11f + (i + 0.5f) * escD;
            Cubo($"Esc_Sup_{i}", gradas.transform,
                new Vector3(0f, y, z), new Vector3(anchoEsc, escH, escD), matEscalon);
        }

        // Pasillo conexion a Piso 3: Y~8, Z 16->18.5
        Cubo("Pasillo_Visual_P3", gradas.transform,
            new Vector3(0f, 7.93f, 17.25f), new Vector3(anchoEsc, 0.15f, 3f), matEscalon);

        // --- PAREDES ---
        Cubo("ParedIzq_Gradas", gradas.transform,
            new Vector3(-1.38f, 4f, 11f), new Vector3(0.08f, 8.5f, 13.5f), matParedG);
        Cubo("ParedDer_Gradas", gradas.transform,
            new Vector3(1.38f, 4f, 11f), new Vector3(0.08f, 8.5f, 13.5f), matParedG);
        Cubo("ParedFondo_Gradas", gradas.transform,
            new Vector3(0f, 1.8f, 4.9f), new Vector3(2.8f, 3.6f, 0.1f), matParedG);
        Cubo("TechoVisual_Gradas", gradas.transform,
            new Vector3(0f, 8.7f, 11f), new Vector3(2.85f, 0.1f, 13.5f), matParedG);

        // --- BARANDALES ---
        float[] tramoZ = { 7.5f, 13.5f };
        float[] tramoYBase = { 0f, 4f };
        for (int t = 0; t < 2; t++)
        {
            string sufijo = t == 0 ? "Inf" : "Sup";
            float yMid = tramoYBase[t] + 2.5f;
            Cubo($"BarandalIzq_{sufijo}", gradas.transform,
                new Vector3(-1.2f, yMid, tramoZ[t]), new Vector3(0.04f, 1f, 5f), matBarandal);
            Cubo($"BarandalDer_{sufijo}", gradas.transform,
                new Vector3(1.2f, yMid, tramoZ[t]), new Vector3(0.04f, 1f, 5f), matBarandal);
        }

        // --- LUCES (4, para TrampaGradasSyntro) ---
        CrearLuzGradas("Luz_Gradas_0", gradas.transform, new Vector3(0f, 1.5f, 7f));
        CrearLuzGradas("Luz_Gradas_1", gradas.transform, new Vector3(0f, 4.8f, 10.5f));
        CrearLuzGradas("Luz_Gradas_2", gradas.transform, new Vector3(0f, 6.5f, 13.5f));
        CrearLuzGradas("Luz_Gradas_3", gradas.transform, new Vector3(0f, 8.3f, 17f));

        // --- CARTEL DE PISO ---
        GameObject cartel = new GameObject("Cartel_Piso_Gradas");
        cartel.transform.SetParent(gradas.transform);
        cartel.transform.position = new Vector3(1.3f, 4.8f, 10.5f);
        cartel.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        Cubo("FondoCartel", cartel.transform,
            Vector3.zero, new Vector3(0.5f, 0.35f, 0.02f), matBarandal);

        GameObject textoObj = new GameObject("TextoCartel_Piso");
        textoObj.transform.SetParent(cartel.transform);
        textoObj.transform.localPosition = new Vector3(0f, 0f, -0.015f);
        textoObj.transform.localRotation = Quaternion.identity;
        var tmp = textoObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = "PISO 1";
        tmp.fontSize = 4f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(0.7f, 0.7f, 0.5f);
        var rect = textoObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0.5f, 0.35f);

        // --- SILUETA FANTASMA ---
        GameObject silueta = new GameObject("Silueta_Fantasma_Gradas");
        silueta.transform.SetParent(gradas.transform);
        silueta.transform.position = new Vector3(0f, 6f, 14f);

        Cubo("Cuerpo_Sil", silueta.transform,
            Vector3.zero, new Vector3(0.4f, 1.2f, 0.2f), matSilueta);

        GameObject cabeza = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cabeza.name = "Cabeza_Sil";
        cabeza.transform.SetParent(silueta.transform);
        cabeza.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        cabeza.transform.localScale = new Vector3(0.25f, 0.3f, 0.25f);
        cabeza.GetComponent<Renderer>().sharedMaterial = matSilueta;

        silueta.SetActive(false);

        // Posiciones para aparicion de silueta
        CrearPuntoSilueta("PosSilueta_1", gradas.transform, new Vector3(0f, 2f, 9f), 0f);
        CrearPuntoSilueta("PosSilueta_2", gradas.transform, new Vector3(0.8f, 6f, 14f), -30f);
        CrearPuntoSilueta("PosSilueta_3", gradas.transform, new Vector3(-0.5f, 4.2f, 10.5f), 15f);

        // --- MANCHAS Y MARCAS EN PAREDES ---
        for (int i = 0; i < 6; i++)
        {
            float ry = Random.Range(1f, 7.5f);
            float rz = Random.Range(5.5f, 17f);
            float lado = (i % 2 == 0) ? -1.33f : 1.33f;
            Cubo($"Mancha_G_{i}", gradas.transform,
                new Vector3(lado, ry, rz),
                new Vector3(0.01f, Random.Range(0.3f, 0.8f), Random.Range(0.1f, 0.4f)), matSombraG);
        }

        // Escrituras escalofriantes (marcas emisivas)
        Cubo("Marca_Sube", gradas.transform,
            new Vector3(-1.33f, 2.5f, 8f), new Vector3(0.01f, 0.06f, 0.5f), matMarcaG);
        Cubo("Marca_NoSalir", gradas.transform,
            new Vector3(1.33f, 5.5f, 12f), new Vector3(0.01f, 0.06f, 0.7f), matMarcaG);
        Cubo("Marca_Ojos", gradas.transform,
            new Vector3(-1.33f, 7f, 15f), new Vector3(0.01f, 0.1f, 0.1f), matMarcaG);

        Debug.Log("[Syntro] Gradas visuales: 30 escalones, paredes, 4 luces, cartel, silueta fantasma");
    }

    static Light CrearLuzGradas(string nombre, Transform padre, Vector3 pos)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre);
        obj.transform.position = pos;
        Light luz = obj.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(0.85f, 0.75f, 0.55f);
        luz.intensity = 0.6f;
        luz.range = 5f;
        luz.shadows = LightShadows.Soft;
        return luz;
    }

    static void CrearPuntoSilueta(string nombre, Transform padre, Vector3 pos, float rotY)
    {
        GameObject p = new GameObject(nombre);
        p.transform.SetParent(padre);
        p.transform.position = pos;
        p.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
    }

    // ============================================================
    //  PASO 3: OBJETOS INTERACTIVOS FALTANTES
    // ============================================================

    static void CrearObjetosInteractivos()
    {
        EditorUtility.DisplayProgressBar("Syntro Juego Completo", "Creando objetos interactivos...", 0.8f);

        Shader lit = Shader.Find("Universal Render Pipeline/Lit");

        // --- Puerta hacia gradas ---
        if (GameObject.Find("Puerta_Pasillo") == null)
        {
            Material matPuerta = CrearMat("Mat_PuertaGradas", lit,
                new Color(0.15f, 0.1f, 0.06f), 0.1f, 0.2f);
            Material matCartelP = new Material(lit);
            matCartelP.name = "Mat_CartelPuerta";
            matCartelP.SetColor("_BaseColor", new Color(0.02f, 0.02f, 0.03f));
            matCartelP.SetColor("_EmissionColor", new Color(0.1f, 0.3f, 0.1f) * 0.5f);
            matCartelP.EnableKeyword("_EMISSION");

            GameObject puerta = new GameObject("Puerta_Pasillo");
            puerta.transform.position = new Vector3(0f, 1.5f, 4.5f);

            Cubo("Marco_Puerta", puerta.transform,
                Vector3.zero, new Vector3(1.2f, 2.4f, 0.15f), matPuerta);
            Cubo("Cartel_Puerta", puerta.transform,
                new Vector3(0f, 1.5f, -0.1f), new Vector3(0.4f, 0.15f, 0.02f), matCartelP);

            Undo.RegisterCreatedObjectUndo(puerta, "Crear Puerta Gradas");
            puerta.SetActive(false);
        }

        // --- Terminal de mantenimiento ---
        if (GameObject.Find("Terminal_Mantenimiento") == null)
        {
            Material matTermPantalla = new Material(lit);
            matTermPantalla.name = "Mat_TerminalPantalla";
            matTermPantalla.SetColor("_BaseColor", new Color(0.02f, 0.02f, 0.03f));
            matTermPantalla.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 0.2f) * 1.5f);
            matTermPantalla.EnableKeyword("_EMISSION");

            Material matTermBase = CrearMat("Mat_TerminalBase", lit,
                new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.3f);

            GameObject terminal = new GameObject("Terminal_Mantenimiento");
            terminal.transform.position = new Vector3(-3f, 0.9f, -2f);

            Cubo("Pantalla_Term", terminal.transform,
                new Vector3(0f, 0.3f, 0f), new Vector3(0.5f, 0.4f, 0.03f), matTermPantalla);
            Cubo("Base_Term", terminal.transform,
                new Vector3(0f, -0.1f, 0f), new Vector3(0.3f, 0.05f, 0.25f), matTermBase);

            Undo.RegisterCreatedObjectUndo(terminal, "Crear Terminal");
        }

        Debug.Log("[Syntro] Objetos interactivos verificados (Puerta_Pasillo, Terminal_Mantenimiento)");
    }

    // ============================================================
    //  PASO 4: MANAGERS DE RUNTIME
    // ============================================================

    static void CrearManagersRuntime()
    {
        EditorUtility.DisplayProgressBar("Syntro Juego Completo", "Creando managers de runtime...", 0.85f);

        GameObject managers = BuscarOCrear("Managers_Syntro_Runtime", null);

        // --- GestorHistoriaSyntro ---
        GestorHistoriaSyntro gestor = Object.FindAnyObjectByType<GestorHistoriaSyntro>();
        if (gestor == null)
        {
            GameObject obj = BuscarOCrear("GestorHistoria_Syntro", managers.transform);
            gestor = obj.AddComponent<GestorHistoriaSyntro>();

            GameObject musicaObj = new GameObject("MusicaSource");
            musicaObj.transform.SetParent(obj.transform);
            musicaObj.transform.localPosition = Vector3.zero;
            AudioSource musicaSrc = musicaObj.AddComponent<AudioSource>();
            musicaSrc.playOnAwake = false;
            musicaSrc.loop = true;
            musicaSrc.spatialBlend = 0f;

            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(obj.transform);
            sfxObj.transform.localPosition = Vector3.zero;
            AudioSource sfxSrc = sfxObj.AddComponent<AudioSource>();
            sfxSrc.playOnAwake = false;
            sfxSrc.spatialBlend = 0f;
        }

        // --- TrampaGradasSyntro ---
        TrampaGradasSyntro trampa = Object.FindAnyObjectByType<TrampaGradasSyntro>();
        if (trampa == null)
        {
            GameObject obj = BuscarOCrear("TrampaGradas_Syntro", managers.transform);
            trampa = obj.AddComponent<TrampaGradasSyntro>();

            AudioSource audioG = obj.AddComponent<AudioSource>();
            audioG.playOnAwake = false;
            audioG.spatialBlend = 0.8f;
        }

        // --- ControladorSentidosVR ---
        ControladorSentidosVR sentidos = Object.FindAnyObjectByType<ControladorSentidosVR>();
        if (sentidos == null)
        {
            GameObject obj = BuscarOCrear("ControladorSentidos_Syntro", managers.transform);
            sentidos = obj.AddComponent<ControladorSentidosVR>();
        }

        // --- EscaleraParadojaEscher ---
        EscaleraParadojaEscher escher = Object.FindAnyObjectByType<EscaleraParadojaEscher>();
        if (escher == null)
        {
            GameObject obj = BuscarOCrear("EscaleraEscher_Syntro", managers.transform);
            escher = obj.AddComponent<EscaleraParadojaEscher>();
        }

        // --- MenuSistemaSyntro ---
        MenuSistemaSyntro menu = Object.FindAnyObjectByType<MenuSistemaSyntro>();
        if (menu == null)
        {
            GameObject obj = BuscarOCrear("MenuSistema_Syntro", managers.transform);
            menu = obj.AddComponent<MenuSistemaSyntro>();
        }

        // --- AmbienciaAudioSyntro ---
        AmbienciaAudioSyntro ambience = Object.FindAnyObjectByType<AmbienciaAudioSyntro>();
        if (ambience == null)
        {
            GameObject obj = BuscarOCrear("AmbienciaAudio_Syntro", managers.transform);
            ambience = obj.AddComponent<AmbienciaAudioSyntro>();
        }

        Debug.Log("[Syntro] 6 managers: GestorHistoria, TrampaGradas, Sentidos, Escher, Menu, Ambientacion");
    }

    // ============================================================
    //  PASO 6: CONECTAR TODAS LAS REFERENCIAS
    // ============================================================

    static void ConectarReferencias()
    {
        EditorUtility.DisplayProgressBar("Syntro Juego Completo", "Conectando referencias...", 0.92f);

        ConectarGestorHistoria();
        ConectarTrampaGradas();
        ConectarSentidosVR();
        ConectarEscaleraEscher();

        Debug.Log("[Syntro] Todas las referencias conectadas entre managers y objetos.");
    }

    static void ConectarGestorHistoria()
    {
        GestorHistoriaSyntro gestor = Object.FindAnyObjectByType<GestorHistoriaSyntro>();
        if (gestor == null) return;

        SerializedObject so = new SerializedObject(gestor);

        SetRefSiNull(so, "puertaHaciaGradas", GameObject.Find("Puerta_Pasillo"));

        GameObject luzMon = GameObject.Find("LuzMonitorAnomalo_Syntro");
        if (luzMon != null)
            SetRefSiNull(so, "luzMonitorAnomalo", luzMon.GetComponent<Light>());

        SetRefSiNull(so, "objetoAnomaliaFinal", GameObject.Find("ObjetoAnomalia_Syntro"));

        Transform musicaT = gestor.transform.Find("MusicaSource");
        if (musicaT != null)
            SetRefSiNull(so, "musicaSource", musicaT.GetComponent<AudioSource>());

        Transform sfxT = gestor.transform.Find("SFXSource");
        if (sfxT != null)
            SetRefSiNull(so, "sfxSource", sfxT.GetComponent<AudioSource>());

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(gestor);
    }

    static void ConectarTrampaGradas()
    {
        TrampaGradasSyntro trampa = Object.FindAnyObjectByType<TrampaGradasSyntro>();
        if (trampa == null) return;

        SerializedObject so = new SerializedObject(trampa);

        GameObject retorno = GameObject.Find("PuntoRetorno_Gradas");
        if (retorno != null) SetRefSiNull(so, "puntoRetornoInferior", retorno.transform);

        GameObject llegada = GameObject.Find("PuntoLlegada_Piso3");
        if (llegada != null) SetRefSiNull(so, "puntoLlegadaPiso3", llegada.transform);

        GameObject fade = GameObject.Find("PanelFade_Syntro");
        if (fade != null) SetRefSiNull(so, "panelFadeNegro", fade.GetComponent<CanvasGroup>());

        GameObject cartelTxt = GameObject.Find("TextoCartel_Piso");
        if (cartelTxt != null)
            SetRefSiNull(so, "textoCartelPiso", cartelTxt.GetComponent<TMPro.TextMeshPro>());

        SetRefSiNull(so, "siluetaFantasma", GameObject.Find("Silueta_Fantasma_Gradas"));

        // lucesGradas array
        var propLuces = so.FindProperty("lucesGradas");
        if (propLuces != null && propLuces.arraySize == 0)
        {
            GameObject bloqueG = GameObject.Find("Bloque_Gradas_Terror");
            if (bloqueG != null)
            {
                Light[] luces = bloqueG.GetComponentsInChildren<Light>();
                propLuces.arraySize = luces.Length;
                for (int i = 0; i < luces.Length; i++)
                    propLuces.GetArrayElementAtIndex(i).objectReferenceValue = luces[i];
            }
        }

        // posicionesSilueta array
        var propPos = so.FindProperty("posicionesSilueta");
        if (propPos != null && propPos.arraySize == 0)
        {
            List<Transform> posiciones = new List<Transform>();
            for (int i = 1; i <= 3; i++)
            {
                GameObject p = GameObject.Find($"PosSilueta_{i}");
                if (p != null) posiciones.Add(p.transform);
            }
            propPos.arraySize = posiciones.Count;
            for (int i = 0; i < posiciones.Count; i++)
                propPos.GetArrayElementAtIndex(i).objectReferenceValue = posiciones[i];
        }

        // audioGradas
        AudioSource audioG = trampa.GetComponent<AudioSource>();
        if (audioG != null) SetRefSiNull(so, "audioGradas", audioG);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(trampa);
    }

    static void ConectarSentidosVR()
    {
        ControladorSentidosVR sentidos = Object.FindAnyObjectByType<ControladorSentidosVR>();
        if (sentidos == null) return;

        SerializedObject so = new SerializedObject(sentidos);

        var propLuces = so.FindProperty("lucesControladas");
        if (propLuces != null && propLuces.arraySize == 0)
        {
            Light[] todasLuces = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            List<Light> validas = new List<Light>();
            foreach (Light l in todasLuces)
            {
                if (l.type == LightType.Point && l.intensity < 5f)
                    validas.Add(l);
            }
            int max = Mathf.Min(validas.Count, 20);
            propLuces.arraySize = max;
            for (int i = 0; i < max; i++)
                propLuces.GetArrayElementAtIndex(i).objectReferenceValue = validas[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(sentidos);
    }

    static void ConectarEscaleraEscher()
    {
        EscaleraParadojaEscher escher = Object.FindAnyObjectByType<EscaleraParadojaEscher>();
        if (escher == null) return;

        SerializedObject so = new SerializedObject(escher);

        GameObject bloqueG = GameObject.Find("Bloque_Gradas_Terror");
        if (bloqueG != null)
            SetRefSiNull(so, "contenedorGradas", bloqueG.transform);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(escher);
    }

    // ============================================================
    //  UTILIDADES
    // ============================================================

    static void SetRefSiNull(SerializedObject so, string prop, Object valor)
    {
        if (valor == null) return;
        var p = so.FindProperty(prop);
        if (p != null && p.objectReferenceValue == null)
            p.objectReferenceValue = valor;
    }

    static GameObject BuscarOCrear(string nombre, Transform padre)
    {
        if (padre != null)
        {
            Transform existente = padre.Find(nombre);
            if (existente != null) return existente.gameObject;
        }
        else
        {
            GameObject existente = GameObject.Find(nombre);
            if (existente != null) return existente;
        }

        GameObject nuevo = new GameObject(nombre);
        if (padre != null) nuevo.transform.SetParent(padre, false);
        return nuevo;
    }

    static GameObject Cubo(string nombre, Transform padre, Vector3 posLocal, Vector3 tamano, Material mat)
    {
        GameObject cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubo.name = nombre;
        cubo.transform.SetParent(padre);
        cubo.transform.localPosition = posLocal;
        cubo.transform.localScale = tamano;
        if (mat != null)
            cubo.GetComponent<Renderer>().sharedMaterial = mat;
        return cubo;
    }

    static Material CrearMat(string nombre, Shader shader, Color color, float metallic, float smoothness)
    {
        Material mat = new Material(shader);
        mat.name = nombre;
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }
}
#endif
