#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Linq;

public class SyntroCorredorEnhancer : EditorWindow
{
    private static readonly string RUTA_MAT = "Assets/_Unity Essentials/Materials/Realistas";

    [MenuItem("Syntro Terror/Mejorar Corredor y HUD (Paso Final)")]
    public static void MejorarCorredor()
    {
        Debug.Log("[Syntro] === INICIO: Mejoras de Corredor, HUD e Iluminación ===");

        MejorarHUD();
        MejorarIluminacionYAtmosfera();
        MejorarPuertasYDetallesParedes();

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[Syntro] === COMPLETADO: Corredor y HUD actualizados con éxito ===");
    }

    // =================================================================
    //  1. CORREGIR HUD RECORTADO
    // =================================================================

    private static void MejorarHUD()
    {
        // Buscar todos los Canvas en la escena
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        // Buscar el Canvas que contiene el HUD de objetivos
        Canvas hudCanvas = null;
        foreach (var c in canvases)
        {
            string name = c.gameObject.name.ToLower();
            if (name.Contains("objetivo") || name.Contains("hud") || name.Contains("mision") || name.Contains("canvas"))
            {
                TextMeshProUGUI[] textos = c.GetComponentsInChildren<TextMeshProUGUI>(true);
                if (textos.Length > 0)
                {
                    hudCanvas = c;
                    break;
                }
            }
        }

        // Si no encontramos Canvas con nombre, buscar cualquier Canvas con TMP de objetivos
        if (hudCanvas == null)
        {
            foreach (var c in canvases)
            {
                TextMeshProUGUI[] textos = c.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in textos)
                {
                    string txt = t.text.ToLower();
                    if (txt.Contains("objetivo") || txt.Contains("sala") || txt.Contains("servidor") ||
                        txt.Contains("subsuelo") || txt.Contains("localiza") || txt.Contains("encuentra"))
                    {
                        hudCanvas = c;
                        break;
                    }
                }
                if (hudCanvas != null) break;
            }
        }

        if (hudCanvas == null)
        {
            Debug.LogWarning("[Syntro] No se encontró Canvas de HUD de objetivos. Creando uno nuevo...");
            hudCanvas = CrearCanvasHUD();
        }

        // Eliminar TODOS los fondos grises/translúcidos del Canvas
        Image[] imagenes = hudCanvas.GetComponentsInChildren<Image>(true);
        foreach (var img in imagenes)
        {
            string imgName = img.gameObject.name.ToLower();
            if (imgName.Contains("panel") || imgName.Contains("bg") || imgName.Contains("background") ||
                imgName.Contains("fondo") || img.color.a < 0.95f)
            {
                img.color = new Color(0f, 0f, 0f, 0f);
                EditorUtility.SetDirty(img);
            }
        }

        // Configurar los textos TMP
        TextMeshProUGUI[] allTexts = hudCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);

        // Buscar o crear el texto de título (header) y el de objetivos
        TextMeshProUGUI headerText = null;
        TextMeshProUGUI objectivesText = null;

        foreach (var tmp in allTexts)
        {
            string goName = tmp.gameObject.name.ToLower();
            if (goName.Contains("titulo") || goName.Contains("header") || goName.Contains("location") ||
                goName.Contains("sector") || tmp.fontStyle == FontStyles.Bold)
            {
                headerText = tmp;
            }
            else
            {
                objectivesText = tmp;
            }
        }

        // Si solo hay un texto, usarlo para todo
        if (allTexts.Length == 1)
        {
            objectivesText = allTexts[0];
            headerText = null;
        }

        // Configurar texto de título si existe separado
        if (headerText != null)
        {
            ConfigurarRectTransformHUD(headerText.GetComponent<RectTransform>());
            headerText.text = "SUBSUELO 01 // CORREDOR DE CONTENCIÓN";
            headerText.fontSize = 20;
            headerText.fontStyle = FontStyles.Bold;
            headerText.color = new Color(0.92f, 0.92f, 0.92f, 1f);
            headerText.outlineWidth = 0.25f;
            headerText.outlineColor = new Color32(0, 0, 0, 240);
            headerText.textWrappingMode = TextWrappingModes.NoWrap;
            headerText.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(headerText);
        }

        // Configurar texto de objetivos
        if (objectivesText != null)
        {
            RectTransform rt = objectivesText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(45f, -40f);
            rt.sizeDelta = new Vector2(500f, 200f);

            if (headerText == null)
            {
                objectivesText.text =
                    "<b>SUBSUELO 01 // CORREDOR DE CONTENCIÓN</b>\n\n" +
                    "[ ] Localiza la sala de servidores con brecha de energía\n" +
                    "[ ] Revisa el registro de anomalías en la terminal\n" +
                    "[ ] Fuerza el reinicio antes del apagón total";
            }
            else
            {
                objectivesText.text =
                    "[ ] Localiza la sala de servidores con brecha de energía\n" +
                    "[ ] Revisa el registro de anomalías en la terminal\n" +
                    "[ ] Fuerza el reinicio antes del apagón total";

                RectTransform objRt = objectivesText.GetComponent<RectTransform>();
                objRt.anchoredPosition = new Vector2(45f, -70f);
            }

            objectivesText.fontSize = 18;
            objectivesText.color = new Color(0.92f, 0.92f, 0.92f, 1f);
            objectivesText.outlineWidth = 0.2f;
            objectivesText.outlineColor = new Color32(0, 0, 0, 240);
            objectivesText.textWrappingMode = TextWrappingModes.Normal;
            objectivesText.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(objectivesText);
        }

        // Configurar HorrorObjectiveHUD si existe
        HorrorObjectiveHUD hudScript = Object.FindAnyObjectByType<HorrorObjectiveHUD>();
        if (hudScript != null)
        {
            hudScript.sectorName = "SUBSUELO 01 // CORREDOR DE CONTENCIÓN";
            EditorUtility.SetDirty(hudScript);
        }

        // Eliminar fondos grises en TODOS los paneles UI de la escena (no solo el canvas HUD)
        Image[] allImages = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (var img in allImages)
        {
            string n = img.gameObject.name.ToLower();
            if ((n.Contains("panel") || n.Contains("bg") || n.Contains("hud")) &&
                img.color.a > 0f && img.color.a < 0.98f)
            {
                img.color = new Color(0f, 0f, 0f, 0f);
                EditorUtility.SetDirty(img);
            }
        }

        Debug.Log("[Syntro] HUD superior izquierdo corregido: fondo eliminado, anchor Top-Left, textos terror tecnológico.");
    }

    static void ConfigurarRectTransformHUD(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(45f, -40f);
        rt.sizeDelta = new Vector2(500f, 40f);
    }

    static Canvas CrearCanvasHUD()
    {
        GameObject canvasGO = new GameObject("Canvas_HUD_Objetivos");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject textGO = new GameObject("Text_Objetivos");
        textGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "<b>SUBSUELO 01 // CORREDOR DE CONTENCIÓN</b>\n\n" +
                   "[ ] Localiza la sala de servidores con brecha de energía\n" +
                   "[ ] Revisa el registro de anomalías en la terminal\n" +
                   "[ ] Fuerza el reinicio antes del apagón total";

        RectTransform rt = tmp.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(45f, -40f);
        rt.sizeDelta = new Vector2(500f, 200f);

        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear Canvas HUD");
        return canvas;
    }

    // =================================================================
    //  2. ROMPER ILUMINACIÓN UNIFORME DEL PASILLO
    // =================================================================

    private static void MejorarIluminacionYAtmosfera()
    {
        // --- Fog exponencial oscuro ---
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.03f);
        RenderSettings.fogDensity = 0.05f;

        // --- URP Global Volume: Vignette + Film Grain ---
        ConfigurarVolumeGlobal();

        // --- Configurar las 5 lámparas del pasillo ---
        ConfigurarLamparasPasillo();

        Debug.Log("[Syntro] Iluminación diferenciada (5 lámparas), fog oscuro y post-processing terror configurados.");
    }

    static void ConfigurarVolumeGlobal()
    {
        Volume volume = Object.FindFirstObjectByType<Volume>();
        if (volume == null)
        {
            GameObject volObj = new GameObject("Global_Volume_Atmosfera");
            volume = volObj.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            Undo.RegisterCreatedObjectUndo(volObj, "Crear Volume Global");
        }

        VolumeProfile profile = volume.sharedProfile;
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            string path = "Assets/Settings/CorredorTerrorProfile.asset";
            AssetDatabase.CreateAsset(profile, path);
            volume.profile = profile;
        }

        // Vignette
        if (!profile.TryGet<Vignette>(out var vignette))
            vignette = profile.Add<Vignette>(true);
        vignette.active = true;
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0.45f;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = 0.35f;
        vignette.color.overrideState = true;
        vignette.color.value = Color.black;

        // Film Grain
        if (!profile.TryGet<FilmGrain>(out var filmGrain))
            filmGrain = profile.Add<FilmGrain>(true);
        filmGrain.active = true;
        filmGrain.type.overrideState = true;
        filmGrain.type.value = FilmGrainLookup.Medium3;
        filmGrain.intensity.overrideState = true;
        filmGrain.intensity.value = 0.35f;

        // Bloom suave para que la luz roja de emergencia brille
        if (!profile.TryGet<Bloom>(out var bloom))
            bloom = profile.Add<Bloom>(true);
        bloom.active = true;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = 0.8f;
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 0.9f;
        bloom.scatter.overrideState = true;
        bloom.scatter.value = 0.7f;

        EditorUtility.SetDirty(profile);
        EditorUtility.SetDirty(volume);
    }

    static void ConfigurarLamparasPasillo()
    {
        // Recolectar luces de techo del corredor principal (Point lights a Y > 2.0)
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        List<Light> lamparasTecho = new List<Light>();

        foreach (var l in allLights)
        {
            if (l.type == LightType.Point && l.transform.position.y > 2.0f)
            {
                // Filtrar solo las del pasillo principal (verificar que pertenezcan al corredor)
                Transform parent = l.transform.parent;
                bool esDePasillo = false;
                while (parent != null)
                {
                    string pName = parent.name.ToLower();
                    if (pName.Contains("pasillo") || pName.Contains("corredor") || pName.Contains("piso1"))
                    {
                        esDePasillo = true;
                        break;
                    }
                    parent = parent.parent;
                }

                // Si no tiene parent de pasillo, aceptarla igual si está en rango razonable
                if (!esDePasillo && l.transform.position.y < 4.5f && l.transform.position.y > 2.5f)
                    esDePasillo = true;

                if (esDePasillo)
                    lamparasTecho.Add(l);
            }
        }

        // Ordenar por posición X (de izquierda a derecha en el pasillo)
        lamparasTecho.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // Tomar las primeras 5 (o todas si hay menos)
        int count = Mathf.Min(lamparasTecho.Count, 5);
        if (count == 0)
        {
            Debug.LogWarning("[Syntro] No se encontraron lámparas de techo en el pasillo. Saltando configuración de lámparas.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Light l = lamparasTecho[i];
            LimpiarComponentesFlicker(l.gameObject);

            switch (i)
            {
                case 0: // 1ª lámpara: TENUE
                    l.enabled = true;
                    l.intensity = 0.35f;
                    l.range = 4.5f;
                    l.color = new Color(0.75f, 0.68f, 0.55f);
                    l.shadows = LightShadows.Soft;
                    break;

                case 1: // 2ª lámpara: PARPADEO ELÉCTRICO IRREGULAR
                    l.enabled = true;
                    l.intensity = 1.2f;
                    l.range = 6.0f;
                    l.color = new Color(0.9f, 0.85f, 0.7f);
                    l.shadows = LightShadows.Soft;
                    var flicker = l.gameObject.AddComponent<LuzFlickerSyntro>();
                    flicker.modo = LuzFlickerSyntro.ModoFlicker.Irregular;
                    flicker.intensidadMin = 0.05f;
                    flicker.intensidadMax = 1.4f;
                    flicker.intervaloMin = 0.04f;
                    flicker.intervaloMax = 0.2f;
                    flicker.probabilidadApagon = 0.18f;
                    break;

                case 2: // 3ª lámpara: COMPLETAMENTE APAGADA / ROTA
                    l.enabled = false;
                    l.intensity = 0f;
                    break;

                case 3: // 4ª lámpara: TONO FRÍO PARPADEANTE
                    l.enabled = true;
                    l.intensity = 0.9f;
                    l.range = 5.5f;
                    l.color = new Color(0.55f, 0.7f, 0.95f);
                    l.shadows = LightShadows.Soft;
                    var flickerFrio = l.gameObject.AddComponent<LuzFlickerSyntro>();
                    flickerFrio.modo = LuzFlickerSyntro.ModoFlicker.FrioParpadeante;
                    flickerFrio.intensidadMin = 0.15f;
                    flickerFrio.intensidadMax = 1.0f;
                    flickerFrio.velocidadPulso = 4.0f;
                    flickerFrio.probabilidadMicroCorte = 0.1f;
                    break;

                case 4: // 5ª lámpara: LUZ ROJA DE EMERGENCIA → apunta a la última puerta
                    l.enabled = true;
                    l.intensity = 2.5f;
                    l.range = 8.0f;
                    l.color = new Color(1f, 0.08f, 0.05f);
                    l.shadows = LightShadows.Soft;

                    // Cambiar a Spot para apuntar hacia la última puerta
                    l.type = LightType.Spot;
                    l.spotAngle = 90f;
                    l.innerSpotAngle = 40f;
                    l.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

                    // Agregar pulso de emergencia
                    var emergencia = l.gameObject.AddComponent<LuzEmergenciaTitilante>();
                    var soEmergencia = new SerializedObject(emergencia);
                    SetSerializedFloat(soEmergencia, "pulsoMinIntensidad", 0.8f);
                    SetSerializedFloat(soEmergencia, "pulsoMaxIntensidad", 2.8f);
                    SetSerializedFloat(soEmergencia, "pulsoPorSegundo", 1.2f);
                    SetSerializedFloat(soEmergencia, "velocidadRotacion", 0f);
                    SetSerializedBool(soEmergencia, "flickerActivo", true);
                    SetSerializedFloat(soEmergencia, "flickerProbabilidad", 0.1f);
                    soEmergencia.ApplyModifiedProperties();
                    break;
            }

            EditorUtility.SetDirty(l.gameObject);
        }

        // Apagar lámparas restantes (>5) para mantener oscuridad
        for (int i = 5; i < lamparasTecho.Count; i++)
        {
            lamparasTecho[i].intensity = Mathf.Min(lamparasTecho[i].intensity, 0.3f);
            EditorUtility.SetDirty(lamparasTecho[i].gameObject);
        }
    }

    static void LimpiarComponentesFlicker(GameObject go)
    {
        foreach (var comp in go.GetComponents<LuzFlickerSyntro>())
            Object.DestroyImmediate(comp);
        foreach (var comp in go.GetComponents<HorrorLightFlicker>())
            Object.DestroyImmediate(comp);
        foreach (var comp in go.GetComponents<LuzEmergenciaTitilante>())
            Object.DestroyImmediate(comp);
    }

    static void SetSerializedFloat(SerializedObject so, string prop, float val)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.floatValue = val;
    }

    static void SetSerializedBool(SerializedObject so, string prop, bool val)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.boolValue = val;
    }

    // =================================================================
    //  3. IDENTIDAD DE PUERTAS Y DETALLES DE PAREDES
    // =================================================================

    private static void MejorarPuertasYDetallesParedes()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        // --- Reducir Smoothness de suelos y paredes ---
        ReducirReflejos();

        // --- Agregar carteles/placas numeradas y LEDs a puertas ---
        AgregarIdentidadPuertas(litShader);

        // --- Agregar zócalos y tuberías al pasillo ---
        AgregarDetallesEstructurales(litShader);

        Debug.Log("[Syntro] Puertas identificadas (SALA 01-04), LEDs, zócalos y tuberías añadidos.");
    }

    static void ReducirReflejos()
    {
        MeshRenderer[] renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        HashSet<Material> procesados = new HashSet<Material>();

        foreach (var r in renderers)
        {
            foreach (Material mat in r.sharedMaterials)
            {
                if (mat == null || procesados.Contains(mat)) continue;
                procesados.Add(mat);

                string matName = mat.name.ToLower();
                bool esSueloOPared = matName.Contains("piso") || matName.Contains("pared") ||
                                     matName.Contains("techo") || matName.Contains("floor") ||
                                     matName.Contains("wall") || matName.Contains("ceiling");

                if (mat.HasProperty("_Smoothness"))
                {
                    float sm = mat.GetFloat("_Smoothness");
                    if (esSueloOPared && sm > 0.25f)
                    {
                        mat.SetFloat("_Smoothness", 0.12f);
                        EditorUtility.SetDirty(mat);
                    }
                    else if (sm > 0.5f)
                    {
                        mat.SetFloat("_Smoothness", 0.25f);
                        EditorUtility.SetDirty(mat);
                    }
                }
            }
        }
    }

    static void AgregarIdentidadPuertas(Shader litShader)
    {
        // Buscar el contenedor de puertas/identidad existente y eliminarlo
        GameObject existente = GameObject.Find("Corredor_IdentidadPuertas");
        if (existente != null) Object.DestroyImmediate(existente);

        GameObject root = new GameObject("Corredor_IdentidadPuertas");
        Undo.RegisterCreatedObjectUndo(root, "Identidad Puertas");

        // Materiales
        Material matCartel = CrearMaterialEditor("Mat_CartelPuerta", litShader,
            new Color(0.08f, 0.08f, 0.1f), 0.7f, 0.4f);
        Material matTextoCartel = CrearMaterialEmisivoEditor("Mat_TextoCartel", litShader,
            new Color(0.6f, 0.55f, 0.4f), new Color(0.2f, 0.18f, 0.08f));
        Material matLedRojo = CrearMaterialEmisivoEditor("Mat_LED_Rojo", litShader,
            new Color(0.8f, 0.05f, 0.05f), Color.red * 3f);
        Material matLedVerdeParpadeante = CrearMaterialEmisivoEditor("Mat_LED_Parpadeo", litShader,
            new Color(0.05f, 0.8f, 0.2f), new Color(0.1f, 1.5f, 0.3f));

        // Buscar puertas en la escena por nombre
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<Transform> puertas = new List<Transform>();

        foreach (var go in allObjects)
        {
            string goName = go.name.ToLower();
            if (goName.Contains("puerta_") || goName.Contains("door"))
            {
                // Verificar que tenga una Hoja (la parte de la puerta)
                Transform hoja = go.transform.Find("Hoja");
                if (hoja != null)
                    puertas.Add(go.transform);
            }
        }

        // Ordenar por posición X para numerar secuencialmente
        puertas.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        // Tomar las primeras 4 puertas del pasillo principal (o las que tengan "puerta_" en el nombre)
        // Filtrar las que están en el pasillo del Piso 1
        List<Transform> puertasPasillo = new List<Transform>();
        foreach (var p in puertas)
        {
            Transform ancestor = p.parent;
            while (ancestor != null)
            {
                string aName = ancestor.name.ToLower();
                if (aName.Contains("pasillo_p1") || aName.Contains("pasillo") ||
                    aName.Contains("piso1") || aName.Contains("corredor"))
                {
                    puertasPasillo.Add(p);
                    break;
                }
                ancestor = ancestor.parent;
            }
        }

        // Si no encontramos puertas de pasillo específicas, usar las primeras 4 encontradas
        if (puertasPasillo.Count == 0)
            puertasPasillo = puertas.Take(4).ToList();

        int salaNum = 1;
        int puertaObjetivo = puertasPasillo.Count > 0 ? puertasPasillo.Count - 1 : -1;

        for (int i = 0; i < Mathf.Min(puertasPasillo.Count, 4); i++)
        {
            Transform puerta = puertasPasillo[i];

            // --- Placa/Cartel sobre la puerta ---
            GameObject cartel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cartel.name = $"Cartel_SALA_{salaNum:D2}";
            cartel.transform.SetParent(root.transform);
            cartel.transform.position = puerta.position + new Vector3(0f, 2.25f, 0f);
            cartel.transform.rotation = puerta.rotation;
            cartel.transform.localScale = new Vector3(0.45f, 0.15f, 0.02f);
            cartel.GetComponent<Renderer>().sharedMaterial = matCartel;
            cartel.isStatic = true;

            // Texto del cartel (placa superpuesta)
            GameObject textoPlaca = GameObject.CreatePrimitive(PrimitiveType.Cube);
            textoPlaca.name = $"Texto_SALA_{salaNum:D2}";
            textoPlaca.transform.SetParent(cartel.transform);
            textoPlaca.transform.localPosition = new Vector3(0f, 0f, -0.55f);
            textoPlaca.transform.localScale = new Vector3(0.85f, 0.7f, 0.1f);
            textoPlaca.GetComponent<Renderer>().sharedMaterial = matTextoCartel;
            Object.DestroyImmediate(textoPlaca.GetComponent<Collider>());

            // --- LED indicador sobre el marco ---
            GameObject led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            led.name = $"LED_Puerta_{salaNum:D2}";
            led.transform.SetParent(root.transform);
            led.transform.position = puerta.position + new Vector3(0.35f, 2.35f, 0f);
            led.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            Object.DestroyImmediate(led.GetComponent<Collider>());

            bool esPuertaObjetivo = (i == puertaObjetivo);

            if (esPuertaObjetivo)
            {
                led.GetComponent<Renderer>().sharedMaterial = matLedVerdeParpadeante;

                // Agregar luz parpadeante al LED de la puerta objetivo
                GameObject ledLuz = new GameObject($"LedLight_Objetivo_{salaNum:D2}");
                ledLuz.transform.SetParent(led.transform);
                ledLuz.transform.localPosition = Vector3.zero;
                Light luzLed = ledLuz.AddComponent<Light>();
                luzLed.type = LightType.Point;
                luzLed.color = new Color(0.2f, 1f, 0.3f);
                luzLed.intensity = 0.5f;
                luzLed.range = 2.0f;
                luzLed.shadows = LightShadows.None;

                var flickerLed = ledLuz.AddComponent<LuzFlickerSyntro>();
                flickerLed.modo = LuzFlickerSyntro.ModoFlicker.Irregular;
                flickerLed.intensidadMin = 0.1f;
                flickerLed.intensidadMax = 0.6f;
                flickerLed.intervaloMin = 0.3f;
                flickerLed.intervaloMax = 0.8f;
            }
            else
            {
                led.GetComponent<Renderer>().sharedMaterial = matLedRojo;

                // Luz tenue roja estática
                GameObject ledLuz = new GameObject($"LedLight_Bloqueada_{salaNum:D2}");
                ledLuz.transform.SetParent(led.transform);
                ledLuz.transform.localPosition = Vector3.zero;
                Light luzLed = ledLuz.AddComponent<Light>();
                luzLed.type = LightType.Point;
                luzLed.color = new Color(1f, 0.1f, 0.05f);
                luzLed.intensity = 0.15f;
                luzLed.range = 1.0f;
                luzLed.shadows = LightShadows.None;
            }

            salaNum++;
        }
    }

    static void AgregarDetallesEstructurales(Shader litShader)
    {
        // Buscar contenedor existente
        GameObject existente = GameObject.Find("Corredor_DetallesEstructurales");
        if (existente != null) Object.DestroyImmediate(existente);

        GameObject root = new GameObject("Corredor_DetallesEstructurales");
        Undo.RegisterCreatedObjectUndo(root, "Detalles Estructurales");

        Material matZocalo = CrearMaterialEditor("Mat_Zocalo_Corredor", litShader,
            new Color(0.06f, 0.06f, 0.07f), 0f, 0.2f);
        Material matTuberia = CrearMaterialEditor("Mat_Tuberia_Corredor", litShader,
            new Color(0.2f, 0.14f, 0.1f), 0.65f, 0.3f);

        // Buscar las paredes del pasillo para posicionar zócalos y tuberías
        GameObject pasillo = GameObject.Find("Pasillo_P1_Principal");
        if (pasillo == null)
        {
            // Buscar alternativas
            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var go in all)
            {
                if (go.name.ToLower().Contains("pasillo") && go.name.ToLower().Contains("p1"))
                {
                    pasillo = go;
                    break;
                }
            }
        }

        if (pasillo == null)
        {
            Debug.LogWarning("[Syntro] No se encontró 'Pasillo_P1_Principal'. Generando detalles en posiciones estimadas.");
            // Generar en posiciones por defecto basadas en la geometría del EdificioSyntro
            GenerarDetallesEnPosicion(root.transform, Vector3.zero, 20f, 2.4f, 3.5f, matZocalo, matTuberia);
        }
        else
        {
            Vector3 pasilloPos = pasillo.transform.position;

            // Obtener dimensiones del pasillo desde el piso si existe
            Transform pisoT = pasillo.transform.Find("Piso_Pasillo_P1");
            float largo = 20f;
            float ancho = 2.4f;
            float alto = 3.5f;

            if (pisoT != null)
            {
                largo = pisoT.localScale.x;
                ancho = pisoT.localScale.z;
            }

            GenerarDetallesEnPosicion(root.transform, pasilloPos, largo, ancho, alto, matZocalo, matTuberia);
        }
    }

    static void GenerarDetallesEnPosicion(Transform root, Vector3 centro, float largo, float ancho, float alto,
        Material matZocalo, Material matTuberia)
    {
        float halfAncho = ancho / 2f;
        float halfLargo = largo / 2f;

        // --- ZÓCALOS INFERIORES (ambos lados del pasillo) ---
        GameObject zocaloNorte = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zocaloNorte.name = "Zocalo_Norte_Corredor";
        zocaloNorte.transform.SetParent(root);
        zocaloNorte.transform.position = centro + new Vector3(0f, 0.08f, halfAncho - 0.03f);
        zocaloNorte.transform.localScale = new Vector3(largo, 0.16f, 0.04f);
        zocaloNorte.GetComponent<Renderer>().sharedMaterial = matZocalo;
        zocaloNorte.isStatic = true;
        Object.DestroyImmediate(zocaloNorte.GetComponent<Collider>());

        GameObject zocaloSur = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zocaloSur.name = "Zocalo_Sur_Corredor";
        zocaloSur.transform.SetParent(root);
        zocaloSur.transform.position = centro + new Vector3(0f, 0.08f, -halfAncho + 0.03f);
        zocaloSur.transform.localScale = new Vector3(largo, 0.16f, 0.04f);
        zocaloSur.GetComponent<Renderer>().sharedMaterial = matZocalo;
        zocaloSur.isStatic = true;
        Object.DestroyImmediate(zocaloSur.GetComponent<Collider>());

        // --- TUBERÍAS EN LA PARTE SUPERIOR DE LA PARED ---
        // 3 tuberías paralelas a lo largo del pasillo
        for (int i = 0; i < 3; i++)
        {
            float offsetX = -0.15f + i * 0.15f;
            float offsetZ = halfAncho - 0.06f;

            // Tubería lado norte
            GameObject tubN = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tubN.name = $"Tuberia_Norte_{i + 1}";
            tubN.transform.SetParent(root);
            tubN.transform.position = centro + new Vector3(0f, alto - 0.12f - i * 0.08f, offsetZ);
            tubN.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            tubN.transform.localScale = new Vector3(0.06f + i * 0.01f, halfLargo, 0.06f + i * 0.01f);
            tubN.GetComponent<Renderer>().sharedMaterial = matTuberia;
            tubN.isStatic = true;
            Object.DestroyImmediate(tubN.GetComponent<Collider>());

            // Tubería lado sur
            GameObject tubS = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tubS.name = $"Tuberia_Sur_{i + 1}";
            tubS.transform.SetParent(root);
            tubS.transform.position = centro + new Vector3(0f, alto - 0.12f - i * 0.08f, -offsetZ);
            tubS.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            tubS.transform.localScale = new Vector3(0.06f + i * 0.01f, halfLargo, 0.06f + i * 0.01f);
            tubS.GetComponent<Renderer>().sharedMaterial = matTuberia;
            tubS.isStatic = true;
            Object.DestroyImmediate(tubS.GetComponent<Collider>());
        }

        // Soportes de tubería cada cierta distancia
        for (int s = 0; s < 6; s++)
        {
            float x = -halfLargo + 2f + s * (largo / 5f);

            GameObject soporteN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            soporteN.name = $"Soporte_Tub_N_{s}";
            soporteN.transform.SetParent(root);
            soporteN.transform.position = centro + new Vector3(x, alto - 0.2f, halfAncho - 0.06f);
            soporteN.transform.localScale = new Vector3(0.04f, 0.35f, 0.08f);
            soporteN.GetComponent<Renderer>().sharedMaterial = matTuberia;
            soporteN.isStatic = true;
            Object.DestroyImmediate(soporteN.GetComponent<Collider>());

            GameObject soporteS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            soporteS.name = $"Soporte_Tub_S_{s}";
            soporteS.transform.SetParent(root);
            soporteS.transform.position = centro + new Vector3(x, alto - 0.2f, -halfAncho + 0.06f);
            soporteS.transform.localScale = new Vector3(0.04f, 0.35f, 0.08f);
            soporteS.GetComponent<Renderer>().sharedMaterial = matTuberia;
            soporteS.isStatic = true;
            Object.DestroyImmediate(soporteS.GetComponent<Collider>());
        }
    }

    // =================================================================
    //  UTILIDADES DE MATERIALES
    // =================================================================

    static Material CrearMaterialEditor(string nombre, Shader shader, Color color, float metallic, float smoothness)
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

    static Material CrearMaterialEmisivoEditor(string nombre, Shader shader, Color color, Color emission)
    {
        Material mat = CrearMaterialEditor(nombre, shader, color, 0f, 0.3f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emission);
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
#endif
