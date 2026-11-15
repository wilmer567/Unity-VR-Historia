#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class ConfiguradorSimulacionSyntro
{
    [MenuItem("Syntro Terror/Configurar Simulacion Jugable")]
    public static void Ejecutar()
    {
        Debug.Log("[Syntro Simulacion] Configurando simulacion completa jugable...");

        GameObject raiz = BuscarOCrear("Sistema_Simulacion_Syntro", null);

        GameObject hud = CrearHUD(raiz);
        CrearPuntosReferencia(raiz);
        CrearRampasGradas(raiz);
        CrearObjetosPiso3();

        ConfigurarSimulacionComponent(raiz, hud);

        EditorUtility.SetDirty(raiz);
        Debug.Log("[Syntro Simulacion] Simulacion configurada. Presiona Play para jugar.");
        Debug.Log("[Syntro Simulacion] Controles: WASD mover, Mouse mirar, Shift correr, E interactuar.");
    }

    // ==========================================
    //  HUD CANVAS
    // ==========================================

    static GameObject CrearHUD(GameObject raiz)
    {
        GameObject canvasObj = BuscarOCrear("HUD_Syntro", raiz.transform);

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvasObj.GetComponent<GraphicRaycaster>() == null)
            canvasObj.AddComponent<GraphicRaycaster>();

        // --- Panel Fade (pantalla negra completa) ---
        GameObject fadeObj = BuscarOCrear("PanelFade_Syntro", canvasObj.transform);
        Image fadeImg = fadeObj.GetComponent<Image>();
        if (fadeImg == null) fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.color = Color.black;
        fadeImg.raycastTarget = false;
        ConfigurarRect(fadeObj, Vector2.zero, Vector2.one);
        CanvasGroup fadeCG = fadeObj.GetComponent<CanvasGroup>();
        if (fadeCG == null) fadeCG = fadeObj.AddComponent<CanvasGroup>();
        fadeCG.alpha = 1f;
        fadeCG.blocksRaycasts = false;
        fadeCG.interactable = false;

        // --- Panel Mensaje (barra inferior) ---
        GameObject msgPanel = BuscarOCrear("PanelMensaje_Syntro", canvasObj.transform);
        Image msgBg = msgPanel.GetComponent<Image>();
        if (msgBg == null) msgBg = msgPanel.AddComponent<Image>();
        msgBg.color = new Color(0f, 0f, 0f, 0.78f);
        msgBg.raycastTarget = false;
        ConfigurarRect(msgPanel, new Vector2(0.1f, 0.01f), new Vector2(0.9f, 0.11f));
        CanvasGroup msgCG = msgPanel.GetComponent<CanvasGroup>();
        if (msgCG == null) msgCG = msgPanel.AddComponent<CanvasGroup>();
        msgCG.alpha = 0f;
        msgCG.blocksRaycasts = false;

        GameObject msgTxt = BuscarOCrear("TextoMensaje", msgPanel.transform);
        TextMeshProUGUI textoMsg = msgTxt.GetComponent<TextMeshProUGUI>();
        if (textoMsg == null) textoMsg = msgTxt.AddComponent<TextMeshProUGUI>();
        textoMsg.fontSize = 22;
        textoMsg.color = new Color(0.85f, 0.92f, 0.85f);
        textoMsg.alignment = TextAlignmentOptions.Center;
        textoMsg.fontStyle = FontStyles.Italic;
        textoMsg.textWrappingMode = TextWrappingModes.Normal;
        ConfigurarRect(msgTxt, new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.92f));

        // --- Panel Objetivos (esquina superior izquierda) ---
        GameObject objPanel = BuscarOCrear("PanelObjetivos_Syntro", canvasObj.transform);
        Image objBg = objPanel.GetComponent<Image>();
        if (objBg == null) objBg = objPanel.AddComponent<Image>();
        objBg.color = new Color(0f, 0f, 0f, 0.5f);
        objBg.raycastTarget = false;
        ConfigurarRect(objPanel, new Vector2(0.005f, 0.72f), new Vector2(0.24f, 0.995f));

        GameObject objTxt = BuscarOCrear("TextoObjetivos", objPanel.transform);
        TextMeshProUGUI textoObj = objTxt.GetComponent<TextMeshProUGUI>();
        if (textoObj == null) textoObj = objTxt.AddComponent<TextMeshProUGUI>();
        textoObj.fontSize = 15;
        textoObj.color = new Color(0.7f, 0.85f, 1f);
        textoObj.alignment = TextAlignmentOptions.TopLeft;
        textoObj.textWrappingMode = TextWrappingModes.Normal;
        textoObj.richText = true;
        ConfigurarRect(objTxt, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.96f));

        // --- Texto Interaccion (centro de pantalla) ---
        GameObject intTxt = BuscarOCrear("TextoInteraccion_Syntro", canvasObj.transform);
        TextMeshProUGUI textoInt = intTxt.GetComponent<TextMeshProUGUI>();
        if (textoInt == null) textoInt = intTxt.AddComponent<TextMeshProUGUI>();
        textoInt.fontSize = 20;
        textoInt.color = new Color(1f, 1f, 0.7f);
        textoInt.alignment = TextAlignmentOptions.Center;
        textoInt.text = "";
        ConfigurarRect(intTxt, new Vector2(0.3f, 0.42f), new Vector2(0.7f, 0.48f));

        // --- Reticula (punto central) ---
        GameObject crossObj = BuscarOCrear("Reticula_Syntro", canvasObj.transform);
        Image crossImg = crossObj.GetComponent<Image>();
        if (crossImg == null) crossImg = crossObj.AddComponent<Image>();
        crossImg.color = new Color(1f, 1f, 1f, 0.35f);
        crossImg.raycastTarget = false;
        RectTransform crossRect = crossObj.GetComponent<RectTransform>();
        crossRect.anchorMin = new Vector2(0.5f, 0.5f);
        crossRect.anchorMax = new Vector2(0.5f, 0.5f);
        crossRect.sizeDelta = new Vector2(4f, 4f);
        crossRect.anchoredPosition = Vector2.zero;

        return canvasObj;
    }

    // ==========================================
    //  PUNTOS DE REFERENCIA
    // ==========================================

    static void CrearPuntosReferencia(GameObject raiz)
    {
        GameObject spawn = BuscarOCrear("SpawnInicial_Syntro", raiz.transform);
        spawn.transform.position = new Vector3(0f, 0.12f, -2f);

        GameObject retorno = BuscarOCrear("PuntoRetorno_Gradas", raiz.transform);
        retorno.transform.position = new Vector3(0f, 0.15f, 5.5f);

        GameObject llegada = BuscarOCrear("PuntoLlegada_Piso3", raiz.transform);
        llegada.transform.position = new Vector3(0f, 8.15f, 18.5f);

        GameObject servidorFin = BuscarOCrear("PuntoServidor_Final", raiz.transform);
        servidorFin.transform.position = new Vector3(0f, 0.15f, 0f);
    }

    // ==========================================
    //  RAMPAS DE GRADAS (geometria caminable)
    // ==========================================

    static void CrearRampasGradas(GameObject raiz)
    {
        GameObject contenedorRampas = BuscarOCrear("Gradas_Rampas_Syntro", raiz.transform);

        float anguloRampa = Mathf.Atan2(4f, 5f) * Mathf.Rad2Deg;
        float largoRampa = Mathf.Sqrt(16f + 25f);

        // Rampa inferior: Y=0→4, Z=5→10
        CrearRampa(contenedorRampas.transform, "Rampa_Inferior",
            new Vector3(0f, 2f, 7.5f),
            new Vector3(-anguloRampa, 0f, 0f),
            new Vector3(2.8f, 0.12f, largoRampa));

        // Descanso: Y=4, Z=10→11
        CrearCajaColision(contenedorRampas.transform, "Descanso_Medio",
            new Vector3(0f, 3.94f, 10.5f),
            new Vector3(2.8f, 0.12f, 1.4f));

        // Rampa superior: Y=4→8, Z=11→16
        CrearRampa(contenedorRampas.transform, "Rampa_Superior",
            new Vector3(0f, 6f, 13.5f),
            new Vector3(-anguloRampa, 0f, 0f),
            new Vector3(2.8f, 0.12f, largoRampa));

        // Pasillo conectando gradas con Piso3: Y=8, Z=16→18.5
        CrearCajaColision(contenedorRampas.transform, "Pasillo_Gradas_Piso3",
            new Vector3(0f, 7.94f, 17.25f),
            new Vector3(2.8f, 0.12f, 3f));

        // Paredes laterales
        CrearCajaColision(contenedorRampas.transform, "Pared_Gradas_Izq",
            new Vector3(-1.5f, 4f, 11f),
            new Vector3(0.12f, 9f, 13f));

        CrearCajaColision(contenedorRampas.transform, "Pared_Gradas_Der",
            new Vector3(1.5f, 4f, 11f),
            new Vector3(0.12f, 9f, 13f));

        // Techo de gradas
        CrearCajaColision(contenedorRampas.transform, "Techo_Gradas",
            new Vector3(0f, 8.8f, 11f),
            new Vector3(3.2f, 0.12f, 13f));

        Debug.Log("[Syntro Simulacion] Rampas de gradas creadas (2 rampas + descanso + paredes).");
    }

    static void CrearRampa(Transform padre, string nombre, Vector3 pos, Vector3 rot, Vector3 tamano)
    {
        GameObject obj = BuscarOCrear(nombre, padre);
        obj.transform.position = pos;
        obj.transform.eulerAngles = rot;
        BoxCollider col = obj.GetComponent<BoxCollider>();
        if (col == null) col = obj.AddComponent<BoxCollider>();
        col.size = tamano;
        col.center = Vector3.zero;
    }

    static void CrearCajaColision(Transform padre, string nombre, Vector3 pos, Vector3 tamano)
    {
        GameObject obj = BuscarOCrear(nombre, padre);
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.identity;
        BoxCollider col = obj.GetComponent<BoxCollider>();
        if (col == null) col = obj.AddComponent<BoxCollider>();
        col.size = tamano;
        col.center = Vector3.zero;
    }

    // ==========================================
    //  OBJETOS PISO 3 (interactivos)
    // ==========================================

    static void CrearObjetosPiso3()
    {
        Transform piso3 = null;
        GameObject piso3Obj = GameObject.Find("Piso3_Laboratorio");
        if (piso3Obj != null) piso3 = piso3Obj.transform;

        Vector3 base3 = piso3 != null ? piso3.position : new Vector3(0f, 8f, 18f);

        // Luz monitor anomalo
        GameObject luzMon = BuscarOCrear("LuzMonitorAnomalo_Syntro", piso3);
        luzMon.transform.position = base3 + new Vector3(0f, 1.2f, 5f);
        Light luz = luzMon.GetComponent<Light>();
        if (luz == null) luz = luzMon.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(0.2f, 0.9f, 0.3f);
        luz.intensity = 2.5f;
        luz.range = 4f;
        luz.enabled = false;

        // Objeto anomalia final (cubo emissivo rojo)
        GameObject anomalia = BuscarOCrear("ObjetoAnomalia_Syntro", piso3);
        anomalia.transform.position = base3 + new Vector3(0f, 1.3f, 5.5f);
        anomalia.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        if (anomalia.GetComponent<MeshFilter>() == null)
        {
            MeshFilter mf = anomalia.AddComponent<MeshFilter>();
            MeshRenderer mr = anomalia.AddComponent<MeshRenderer>();
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mf.sharedMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            Material matAnomalia = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            matAnomalia.name = "Mat_Anomalia_Syntro";
            matAnomalia.SetColor("_BaseColor", new Color(0.15f, 0f, 0f));
            matAnomalia.SetColor("_EmissionColor", new Color(3f, 0f, 0f));
            matAnomalia.EnableKeyword("_EMISSION");
            mr.sharedMaterial = matAnomalia;
        }
        anomalia.SetActive(false);

        // Wiring a GestorHistoriaSyntro
        GestorHistoriaSyntro gestor = Object.FindAnyObjectByType<GestorHistoriaSyntro>();
        if (gestor != null)
        {
            SerializedObject so = new SerializedObject(gestor);

            var propLuz = so.FindProperty("luzMonitorAnomalo");
            if (propLuz != null && propLuz.objectReferenceValue == null)
                propLuz.objectReferenceValue = luz;

            var propAnomalia = so.FindProperty("objetoAnomaliaFinal");
            if (propAnomalia != null && propAnomalia.objectReferenceValue == null)
                propAnomalia.objectReferenceValue = anomalia;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(gestor);
        }

        Debug.Log("[Syntro Simulacion] Objetos Piso 3 creados (luz monitor, anomalia).");
    }

    // ==========================================
    //  COMPONENTE SIMULACION
    // ==========================================

    static void ConfigurarSimulacionComponent(GameObject raiz, GameObject hud)
    {
        SimulacionCompletaSyntro sim = raiz.GetComponent<SimulacionCompletaSyntro>();
        if (sim == null) sim = raiz.AddComponent<SimulacionCompletaSyntro>();

        // HUD references
        Transform canvasT = hud.transform;
        sim.panelFade = canvasT.Find("PanelFade_Syntro")?.GetComponent<CanvasGroup>();
        sim.panelMensaje = canvasT.Find("PanelMensaje_Syntro")?.GetComponent<CanvasGroup>();

        Transform msgPanel = canvasT.Find("PanelMensaje_Syntro");
        if (msgPanel != null)
            sim.textoMensaje = msgPanel.Find("TextoMensaje")?.GetComponent<TextMeshProUGUI>();

        Transform objPanel = canvasT.Find("PanelObjetivos_Syntro");
        if (objPanel != null)
            sim.textoObjetivos = objPanel.Find("TextoObjetivos")?.GetComponent<TextMeshProUGUI>();

        sim.textoInteraccion = canvasT.Find("TextoInteraccion_Syntro")?.GetComponent<TextMeshProUGUI>();

        // Reference points
        sim.spawnInicial = raiz.transform.Find("SpawnInicial_Syntro");
        sim.puntoServidorFinal = raiz.transform.Find("PuntoServidor_Final");

        EditorUtility.SetDirty(sim);

        Debug.Log("[Syntro Simulacion] SimulacionCompletaSyntro configurada con todas las referencias.");
    }

    // ==========================================
    //  UTILIDADES
    // ==========================================

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

        RectTransform rt = nuevo.GetComponent<RectTransform>();
        if (rt == null && padre != null && padre.GetComponent<Canvas>() != null)
        {
            Object.DestroyImmediate(nuevo.GetComponent<Transform>());
            nuevo.AddComponent<RectTransform>();
        }

        return nuevo;
    }

    static void ConfigurarRect(GameObject obj, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt == null) rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif
