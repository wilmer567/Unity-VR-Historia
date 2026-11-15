#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class VRScenarioBuilder : EditorWindow
{
    private const string SCENES_PATH = "Assets/_Unity Essentials/Scenes/";
    private const string SO_PATH = "Assets/_VR_Historia/Data/";
    private const string PREFAB_PATH = "Assets/_VR_Historia/Prefabs/";

    [MenuItem("VR Historia/Construir TODOS los Escenarios", priority = 1)]
    public static void ConstruirTodo()
    {
        if (!EditorUtility.DisplayDialog("VR Historia — Construir Todo",
            "Esto creara/sobreescribira las escenas:\n" +
            "- HistoriaCapitulo1_Despertar\n" +
            "- HistoriaCapitulo2_Exploracion\n" +
            "- HistoriaCapitulo3_Revelacion\n" +
            "- SimulacionISTR\n" +
            "- MenuPrincipalVR\n\n" +
            "Ademas creara ScriptableObjects y prefabs de ejemplo.\n\n" +
            "Continuar?", "Construir", "Cancelar"))
            return;

        EnsureDirectories();
        CrearScriptableObjects();
        CrearPrefabs();
        ConstruirEscenaMenu();
        ConstruirEscenaCapitulo1();
        ConstruirEscenaCapitulo2();
        ConstruirEscenaCapitulo3();
        ConstruirEscenaISTR();
        AgregarEscenasAlBuild();

        EditorUtility.DisplayDialog("VR Historia", "Todos los escenarios han sido construidos.", "OK");
    }

    [MenuItem("VR Historia/Construir Solo Menu VR", priority = 10)]
    public static void SoloConstruirMenu() { EnsureDirectories(); ConstruirEscenaMenu(); }

    [MenuItem("VR Historia/Construir Solo Capitulo 1", priority = 11)]
    public static void SoloConstruirCap1() { EnsureDirectories(); ConstruirEscenaCapitulo1(); }

    [MenuItem("VR Historia/Construir Solo Capitulo 2", priority = 12)]
    public static void SoloConstruirCap2() { EnsureDirectories(); ConstruirEscenaCapitulo2(); }

    [MenuItem("VR Historia/Construir Solo Capitulo 3", priority = 13)]
    public static void SoloConstruirCap3() { EnsureDirectories(); ConstruirEscenaCapitulo3(); }

    [MenuItem("VR Historia/Construir Solo ISTR", priority = 14)]
    public static void SoloConstruirISTR() { EnsureDirectories(); ConstruirEscenaISTR(); }

    [MenuItem("VR Historia/Crear ScriptableObjects de Ejemplo", priority = 20)]
    public static void SoloCrearSOs() { EnsureDirectories(); CrearScriptableObjects(); }

    // ===========================================
    //  UTILIDADES
    // ===========================================

    static void EnsureDirectories()
    {
        string[] dirs = { SO_PATH, PREFAB_PATH,
            SO_PATH + "Interactions", SO_PATH + "Dialogues",
            SO_PATH + "Zones", SO_PATH + "Items" };
        foreach (var d in dirs)
        {
            if (!AssetDatabase.IsValidFolder(d))
            {
                string parent = Path.GetDirectoryName(d).Replace("\\", "/");
                string folder = Path.GetFileName(d.TrimEnd('/'));
                if (!string.IsNullOrEmpty(folder) && AssetDatabase.IsValidFolder(parent))
                    AssetDatabase.CreateFolder(parent, folder);
                else
                    Directory.CreateDirectory(d);
            }
        }
        AssetDatabase.Refresh();
    }

    static Scene CrearEscenaNueva(string nombre)
    {
        string path = SCENES_PATH + nombre + ".unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, path);
        return scene;
    }

    static GameObject CrearXROrigin()
    {
        var xrOrigin = new GameObject("XR Origin (XR Rig)");

        var cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.SetParent(xrOrigin.transform);
        cameraOffset.transform.localPosition = new Vector3(0f, 1.36144f, 0f);

        var mainCamera = new GameObject("Main Camera");
        mainCamera.transform.SetParent(cameraOffset.transform);
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.tag = "MainCamera";
        var cam = mainCamera.AddComponent<Camera>();
        cam.nearClipPlane = 0.01f;
        mainCamera.AddComponent<AudioListener>();

        var leftHand = new GameObject("Left Controller");
        leftHand.transform.SetParent(cameraOffset.transform);

        var rightHand = new GameObject("Right Controller");
        rightHand.transform.SetParent(cameraOffset.transform);

        var gazeInteractor = new GameObject("Gaze Interactor");
        gazeInteractor.transform.SetParent(cameraOffset.transform);

        return xrOrigin;
    }

    static GameObject CrearCanvasFade(Transform parent)
    {
        var canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(parent);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>();

        var imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform);
        var img = imageObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);
        img.raycastTarget = false;
        var rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        canvasObj.AddComponent<VRFadeScreen>();
        return canvasObj;
    }

    static GameObject CrearSubtitleDisplay(Transform parent)
    {
        var subtitleObj = new GameObject("SubtitleDisplay");
        subtitleObj.transform.SetParent(parent);
        var canvas = subtitleObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        subtitleObj.AddComponent<CanvasScaler>();

        var panelBg = new GameObject("SubtitlePanel");
        panelBg.transform.SetParent(subtitleObj.transform);
        var bgImg = panelBg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.7f);

        var speakerObj = new GameObject("SpeakerText");
        speakerObj.transform.SetParent(panelBg.transform);
        var speakerTMP = speakerObj.AddComponent<TextMeshProUGUI>();
        speakerTMP.fontSize = 24;
        speakerTMP.fontStyle = FontStyles.Bold;
        speakerTMP.alignment = TextAlignmentOptions.Left;

        var dialogueObj = new GameObject("DialogueText");
        dialogueObj.transform.SetParent(panelBg.transform);
        var dialogueTMP = dialogueObj.AddComponent<TextMeshProUGUI>();
        dialogueTMP.fontSize = 20;
        dialogueTMP.alignment = TextAlignmentOptions.Left;

        var choicesObj = new GameObject("ChoicesContainer");
        choicesObj.transform.SetParent(panelBg.transform);
        var vlg = choicesObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;

        var messageObj = new GameObject("MessageText");
        messageObj.transform.SetParent(subtitleObj.transform);
        var msgTMP = messageObj.AddComponent<TextMeshProUGUI>();
        msgTMP.fontSize = 28;
        msgTMP.alignment = TextAlignmentOptions.Center;
        messageObj.SetActive(false);

        subtitleObj.AddComponent<VRSubtitleDisplay>();
        return subtitleObj;
    }

    static GameObject CrearHUD(Transform parent)
    {
        var hudObj = new GameObject("HUD");
        hudObj.transform.SetParent(parent);
        var canvas = hudObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        hudObj.AddComponent<CanvasScaler>();

        var tituloObj = new GameObject("TituloCapitulo");
        tituloObj.transform.SetParent(hudObj.transform);
        var tituloTMP = tituloObj.AddComponent<TextMeshProUGUI>();
        tituloTMP.fontSize = 30;
        tituloTMP.fontStyle = FontStyles.Bold;
        tituloTMP.alignment = TextAlignmentOptions.Left;

        var objetivosContainer = new GameObject("ObjetivosContainer");
        objetivosContainer.transform.SetParent(hudObj.transform);
        var vlg = objetivosContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;

        var notifPanel = new GameObject("NotificacionPanel");
        notifPanel.transform.SetParent(hudObj.transform);
        var cg = notifPanel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        var notifText = new GameObject("NotificacionText");
        notifText.transform.SetParent(notifPanel.transform);
        notifText.AddComponent<TextMeshProUGUI>();

        var zonaText = new GameObject("ZonaText");
        zonaText.transform.SetParent(hudObj.transform);
        var ztmp = zonaText.AddComponent<TextMeshProUGUI>();
        ztmp.fontSize = 18;
        ztmp.alignment = TextAlignmentOptions.Right;

        hudObj.AddComponent<VRHUD>();
        return hudObj;
    }

    static GameObject CrearManagers()
    {
        var managers = new GameObject("--- VR MANAGERS ---");

        var gmObj = new GameObject("GameManagerVR");
        gmObj.transform.SetParent(managers.transform);
        gmObj.AddComponent<GameManagerVR>();

        var invObj = new GameObject("InventarioVR");
        invObj.transform.SetParent(managers.transform);
        invObj.AddComponent<InventarioVR>();

        var dialogueObj = new GameObject("VRDialogueManager");
        dialogueObj.transform.SetParent(managers.transform);
        dialogueObj.AddComponent<VRDialogueManager>();
        dialogueObj.AddComponent<AudioSource>();

        var teleportObj = new GameObject("VRTeleportManager");
        teleportObj.transform.SetParent(managers.transform);
        teleportObj.AddComponent<VRTeleportManager>();

        var interactionObj = new GameObject("VRInteractionManager");
        interactionObj.transform.SetParent(managers.transform);
        interactionObj.AddComponent<VRInteractionManager>();

        return managers;
    }

    static GameObject CrearSueloBasico(float size = 50f)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(size / 10f, 1f, size / 10f);
        floor.isStatic = true;
        return floor;
    }

    static void CrearIluminacionBase()
    {
        var dirLight = new GameObject("Directional Light");
        var light = dirLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.5f;
        light.color = new Color(0.8f, 0.85f, 1f);
        light.shadows = LightShadows.Soft;
        dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.2f);
    }

    static GameObject CrearObjetoInteractuable(string nombre, Vector3 pos, PrimitiveType forma, Color color)
    {
        var obj = GameObject.CreatePrimitive(forma);
        obj.name = nombre;
        obj.transform.position = pos;
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            renderer.material = mat;
        }
        obj.AddComponent<VRInteractableObject>();
        return obj;
    }

    static GameObject CrearNPC(string nombre, Vector3 pos, Color color)
    {
        var npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = "NPC_" + nombre;
        npc.transform.position = pos;
        npc.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        var renderer = npc.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            renderer.material = mat;
        }

        var nameLabel = new GameObject("Label");
        nameLabel.transform.SetParent(npc.transform);
        nameLabel.transform.localPosition = new Vector3(0f, 1.5f, 0f);

        return npc;
    }

    static GameObject CrearPared(string nombre, Vector3 pos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = nombre;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.isStatic = true;
        var renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.3f, 0.3f, 0.35f);
            renderer.material = mat;
        }
        return wall;
    }

    static Light CrearLuzPuntual(string nombre, Vector3 pos, Color color, float intensity, float range)
    {
        var lightObj = new GameObject(nombre);
        lightObj.transform.position = pos;
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        return light;
    }

    static GameObject CrearZonaTeleport(string nombre, Vector3 pos, Color color)
    {
        var zone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        zone.name = "TeleportZone_" + nombre;
        zone.transform.position = pos;
        zone.transform.localScale = new Vector3(2f, 0.02f, 2f);
        var renderer = zone.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Surface", 1); // transparent
            renderer.material = mat;
        }
        return zone;
    }

    static void CrearMusicaAmbiente(string nombre, Transform parent)
    {
        var audioObj = new GameObject(nombre);
        audioObj.transform.SetParent(parent);
        var src = audioObj.AddComponent<AudioSource>();
        src.loop = true;
        src.spatialBlend = 0f;
        src.playOnAwake = false;
        src.volume = 0.3f;
    }

    // ===========================================
    //  SCRIPTABLE OBJECTS
    // ===========================================

    static void CrearScriptableObjects()
    {
        // Interaction Profiles
        CrearInteractionProfile("IP_Grab_Llave", InteractionType.Grab, "llave_obtenida", "Llave");
        CrearInteractionProfile("IP_Grab_Diario", InteractionType.Grab, "diario_recogido", "Diario antiguo");
        CrearInteractionProfile("IP_Grab_TarjetaAcceso", InteractionType.Grab, "item_tarjeta_acceso", "Tarjeta de acceso");
        CrearInteractionProfile("IP_Inspect_Documento", InteractionType.Inspect, "documento_leido", "Documento clasificado");
        CrearInteractionProfile("IP_Use_Terminal", InteractionType.Use, "terminal_usada", "Terminal de datos");
        CrearInteractionProfile("IP_Use_Palanca", InteractionType.Use, "energia_restaurada", "Palanca de energia");
        CrearInteractionProfile("IP_Trigger_Puerta", InteractionType.Trigger, "puerta_abierta", "Puerta");
        CrearInteractionProfile("IP_Use_RackServidor", InteractionType.Use, "rack_interactuado", "Rack de servidor");
        CrearInteractionProfile("IP_Use_Interruptor", InteractionType.Use, "transmision_cortada", "Interruptor de emergencia");

        // Dialogue Graphs
        CrearDialogoEjemplo("DG_Intro_Cap1", "Intro Capitulo 1", new string[] {
            "...|Narrador|Despiertas en una habitacion que no reconoces.|Neutral",
            "intro_02|Narrador|El aire huele a humedad y metal oxidado.|Sad",
            "intro_03|Narrador|Un diario sobre la mesa parece llamarte.|Surprised"
        });

        CrearDialogoEjemplo("DG_Guardia", "Guardia", new string[] {
            "...|Guardia|Alto. No deberias estar aqui.|Angry",
            "guardia_02|Guardia|Hmm... o quizas si. Las cosas no son lo que parecen.|Neutral",
            "guardia_03|Guardia|Toma esta llave. La vas a necesitar.|Neutral"
        });

        CrearDialogoEjemplo("DG_Cientifico", "Cientifico", new string[] {
            "...|Dr. Mendez|Ah, un visitante. Hace tiempo que nadie viene por aqui.|Happy",
            "cient_02|Dr. Mendez|Los experimentos... se salieron de control.|Sad",
            "cient_03|Dr. Mendez|Si quieres respuestas, busca en los archivos del sotano.|Neutral"
        });

        CrearDialogoEjemplo("DG_Entidad", "Entidad Final", new string[] {
            "...|???|Te hemos observado desde el inicio.|Fear",
            "ent_02|???|Cada paso que diste... fue calculado.|Angry",
            "ent_03|???|Ahora debes elegir. No hay vuelta atras.|Surprised"
        });

        CrearDialogoEjemplo("DG_ISTR_Intro", "ISTR Intro", new string[] {
            "...|Sistema|Bienvenido al turno nocturno del edificio ISTR.|Neutral",
            "istr_02|Sistema|Su tarea: monitorear los servidores. Todo parece normal... por ahora.|Neutral"
        });

        // Teleport Zones
        CrearTeleportZone("zona_recepcion", "Recepcion", true, Vector3.zero, "HistoriaCapitulo1_Despertar");
        CrearTeleportZone("zona_pasillo", "Pasillo Principal", false, new Vector3(10, 0, 0), "HistoriaCapitulo1_Despertar");
        CrearTeleportZone("zona_oficina", "Oficina", false, new Vector3(20, 0, 0), "HistoriaCapitulo1_Despertar");
        CrearTeleportZone("zona_exploracion_inicio", "Entrada Edificio", true, Vector3.zero, "HistoriaCapitulo2_Exploracion");
        CrearTeleportZone("zona_laboratorio", "Laboratorio", false, new Vector3(0, 0, 15), "HistoriaCapitulo2_Exploracion");
        CrearTeleportZone("zona_sotano", "Sotano", false, new Vector3(-10, -3, 0), "HistoriaCapitulo2_Exploracion");
        CrearTeleportZone("zona_archivo", "Sala de Archivos", false, new Vector3(-10, -3, 15), "HistoriaCapitulo2_Exploracion");
        CrearTeleportZone("zona_revelacion", "Tercer Piso", false, Vector3.zero, "HistoriaCapitulo3_Revelacion");
        CrearTeleportZone("istr_piso1", "ISTR Piso 1", true, Vector3.zero, "SimulacionISTR");
        CrearTeleportZone("istr_gradas", "ISTR Gradas", false, new Vector3(0, 5, 0), "SimulacionISTR");
        CrearTeleportZone("istr_piso3", "ISTR Piso 3", false, new Vector3(0, 10, 0), "SimulacionISTR");
        CrearTeleportZone("istr_laboratorio", "ISTR Laboratorio", false, new Vector3(10, 10, 0), "SimulacionISTR");

        // World Map
        CrearWorldMap();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[VRScenarioBuilder] ScriptableObjects creados.");
    }

    static void CrearInteractionProfile(string nombre, InteractionType tipo, string flag, string desc)
    {
        string path = SO_PATH + "Interactions/" + nombre + ".asset";
        if (AssetDatabase.LoadAssetAtPath<InteractionProfile>(path) != null) return;

        var ip = CreateInstance<InteractionProfile>();
        ip.interactionType = tipo;
        ip.flagAlCompletar = flag;
        ip.descripcionUI = desc;
        ip.hapticFeedback = new HapticPreset { amplitude = 0.3f, duration = 0.15f };
        AssetDatabase.CreateAsset(ip, path);
    }

    static void CrearDialogoEjemplo(string nombre, string startSpeaker, string[] lines)
    {
        string path = SO_PATH + "Dialogues/" + nombre + ".asset";
        if (AssetDatabase.LoadAssetAtPath<DialogueGraph>(path) != null) return;

        var dg = CreateInstance<DialogueGraph>();
        dg.dialogueID = nombre;
        dg.nodes = new List<DialogueNode>();

        string prevID = null;
        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split('|');
            string nodeID = parts[0] == "..." ? nombre + "_node_" + i : parts[0];

            if (i == 0) dg.startNodeID = nodeID;

            var node = new DialogueNode
            {
                nodeID = nodeID,
                speakerID = parts[1].ToLower().Replace(" ", "_").Replace(".", ""),
                speakerName = parts[1],
                text = parts[2],
                emotion = System.Enum.TryParse<EmotionType>(parts[3], out var em) ? em : EmotionType.Neutral,
                autoAdvanceDelay = 4f,
                choices = new List<DialogueChoice>()
            };

            if (i < lines.Length - 1)
            {
                string nextParts0 = lines[i + 1].Split('|')[0];
                node.nextNodeID = nextParts0 == "..." ? nombre + "_node_" + (i + 1) : nextParts0;
            }

            dg.nodes.Add(node);
        }

        AssetDatabase.CreateAsset(dg, path);
    }

    static void CrearTeleportZone(string id, string displayName, bool unlocked, Vector3 pos, string sceneName)
    {
        string path = SO_PATH + "Zones/TZ_" + id + ".asset";
        if (AssetDatabase.LoadAssetAtPath<TeleportZone>(path) != null) return;

        var tz = CreateInstance<TeleportZone>();
        tz.zoneID = id;
        tz.displayName = displayName;
        tz.isUnlocked = unlocked;
        tz.arrivalPosition = pos;
        tz.sceneName = sceneName;
        tz.transitionEffect = TransitionEffect.Fade;
        tz.transitionDuration = 1f;
        AssetDatabase.CreateAsset(tz, path);
    }

    static void CrearWorldMap()
    {
        string path = SO_PATH + "Zones/WorldMap_Principal.asset";
        if (AssetDatabase.LoadAssetAtPath<WorldMap>(path) != null) return;

        var wm = CreateInstance<WorldMap>();
        wm.allZones = new List<TeleportZone>();
        wm.connections = new List<ZoneConnection>();

        string[] zoneFiles = Directory.GetFiles(Application.dataPath + "/../" + SO_PATH + "Zones/", "TZ_*.asset");
        foreach (var f in zoneFiles)
        {
            string assetPath = "Assets" + f.Replace(Application.dataPath, "").Replace("\\", "/");
            var tz = AssetDatabase.LoadAssetAtPath<TeleportZone>(assetPath);
            if (tz != null) wm.allZones.Add(tz);
        }

        if (wm.allZones.Count > 0) wm.startingZone = wm.allZones[0];

        AssetDatabase.CreateAsset(wm, path);
    }

    static void CrearPrefabs()
    {
        // Choice button prefab
        string btnPath = PREFAB_PATH + "ChoiceButton.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(btnPath) == null)
        {
            var btnObj = new GameObject("ChoiceButton");
            var btn = btnObj.AddComponent<Button>();
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            PrefabUtility.SaveAsPrefabAsset(btnObj, btnPath);
            DestroyImmediate(btnObj);
        }

        // Objetivo item prefab
        string objPath = PREFAB_PATH + "ObjetivoItem.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(objPath) == null)
        {
            var objItem = new GameObject("ObjetivoItem");
            var tmp = objItem.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 16;
            tmp.color = Color.white;
            PrefabUtility.SaveAsPrefabAsset(objItem, objPath);
            DestroyImmediate(objItem);
        }
    }

    // ===========================================
    //  ESCENA: MENU PRINCIPAL VR
    // ===========================================

    static void ConstruirEscenaMenu()
    {
        var scene = CrearEscenaNueva("MenuPrincipalVR");
        CrearIluminacionBase();

        var xrOrigin = CrearXROrigin();
        xrOrigin.transform.position = new Vector3(0f, 0f, -5f);

        var floor = CrearSueloBasico(30f);
        var renderer = floor.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.1f, 0.1f, 0.15f);
            renderer.material = mat;
        }

        // Panel Menu
        var menuCanvas = new GameObject("MenuCanvas");
        var canvas = menuCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        menuCanvas.AddComponent<CanvasScaler>();
        menuCanvas.AddComponent<GraphicRaycaster>();
        menuCanvas.transform.position = new Vector3(0f, 1.5f, 3f);
        menuCanvas.transform.localScale = Vector3.one * 0.01f;

        var menuBg = new GameObject("MenuBackground");
        menuBg.transform.SetParent(menuCanvas.transform);
        var bgImg = menuBg.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);
        var bgRect = bgImg.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(800, 600);

        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(menuBg.transform);
        var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "VR HISTORIA";
        titleTmp.fontSize = 48;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = Color.white;
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 220);
        titleRect.sizeDelta = new Vector2(700, 80);

        string[] btnNames = { "Nueva Partida", "Continuar", "Simulacion ISTR", "Capitulo 1", "Capitulo 2", "Capitulo 3", "Salir" };
        for (int i = 0; i < btnNames.Length; i++)
        {
            var btnObj = new GameObject("Btn_" + btnNames[i].Replace(" ", ""));
            btnObj.transform.SetParent(menuBg.transform);
            var btn = btnObj.AddComponent<Button>();
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);
            var btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0, 140 - i * 60);
            btnRect.sizeDelta = new Vector2(400, 50);

            var btnText = new GameObject("Text");
            btnText.transform.SetParent(btnObj.transform);
            var tmp = btnText.AddComponent<TextMeshProUGUI>();
            tmp.text = btnNames[i];
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var txtRect = btnText.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
        }

        var statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(menuBg.transform);
        var statusTmp = statusObj.AddComponent<TextMeshProUGUI>();
        statusTmp.fontSize = 16;
        statusTmp.alignment = TextAlignmentOptions.Center;
        statusTmp.color = new Color(0.7f, 0.7f, 0.7f);
        var statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchoredPosition = new Vector2(0, -260);
        statusRect.sizeDelta = new Vector2(700, 40);

        menuCanvas.AddComponent<VRMenuPrincipal>();
        CrearCanvasFade(xrOrigin.transform);

        // Decoracion
        CrearLuzPuntual("LuzMenu_1", new Vector3(-3, 3, 2), new Color(0.3f, 0.3f, 1f), 3f, 10f);
        CrearLuzPuntual("LuzMenu_2", new Vector3(3, 3, 2), new Color(1f, 0.3f, 0.3f), 3f, 10f);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[VRScenarioBuilder] Escena Menu construida.");
    }

    // ===========================================
    //  ESCENA: CAPITULO 1 — DESPERTAR
    // ===========================================

    static void ConstruirEscenaCapitulo1()
    {
        var scene = CrearEscenaNueva("HistoriaCapitulo1_Despertar");
        CrearIluminacionBase();

        var xrOrigin = CrearXROrigin();
        xrOrigin.transform.position = new Vector3(0f, 0f, 0f);

        var managers = CrearManagers();
        CrearCanvasFade(xrOrigin.transform);
        CrearSubtitleDisplay(xrOrigin.transform);
        CrearHUD(xrOrigin.transform);

        // === Habitacion Inicial ===
        CrearSueloBasico(20f);
        CrearPared("Pared_Norte", new Vector3(0, 1.5f, 10), new Vector3(20, 3, 0.2f));
        CrearPared("Pared_Sur", new Vector3(0, 1.5f, -10), new Vector3(20, 3, 0.2f));
        CrearPared("Pared_Este", new Vector3(10, 1.5f, 0), new Vector3(0.2f, 3, 20));
        CrearPared("Pared_Oeste", new Vector3(-10, 1.5f, 0), new Vector3(0.2f, 3, 20));

        // Techo
        var techo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        techo.name = "Techo";
        techo.transform.position = new Vector3(0, 3, 0);
        techo.transform.rotation = Quaternion.Euler(180, 0, 0);
        techo.transform.localScale = new Vector3(2, 1, 2);
        techo.isStatic = true;

        // Objetos interactuables
        var mesa = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesa.name = "Mesa";
        mesa.transform.position = new Vector3(2, 0.5f, 2);
        mesa.transform.localScale = new Vector3(1.5f, 0.05f, 0.8f);

        var diario = CrearObjetoInteractuable("Diario", new Vector3(2, 0.55f, 2), PrimitiveType.Cube, new Color(0.6f, 0.4f, 0.1f));
        diario.transform.localScale = new Vector3(0.2f, 0.03f, 0.15f);

        var llave = CrearObjetoInteractuable("Llave", new Vector3(-3, 0.3f, 5), PrimitiveType.Cylinder, Color.yellow);
        llave.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);

        var puerta = CrearObjetoInteractuable("PuertaPrincipal", new Vector3(0, 1.2f, 9.9f), PrimitiveType.Cube, new Color(0.4f, 0.25f, 0.1f));
        puerta.transform.localScale = new Vector3(1.2f, 2.4f, 0.1f);

        var terminal = CrearObjetoInteractuable("Terminal", new Vector3(-5, 0.9f, -8), PrimitiveType.Cube, new Color(0.1f, 0.1f, 0.1f));
        terminal.transform.localScale = new Vector3(0.5f, 0.4f, 0.05f);

        // NPC Guardia
        var guardia = CrearNPC("Guardia", new Vector3(5, 1f, 7), new Color(0.3f, 0.3f, 0.5f));

        // Iluminacion atmosferica
        var lucesFlicker = new List<Light>();
        lucesFlicker.Add(CrearLuzPuntual("Luz_Techo_1", new Vector3(0, 2.8f, 0), new Color(1f, 0.9f, 0.7f), 1.5f, 12f));
        lucesFlicker.Add(CrearLuzPuntual("Luz_Techo_2", new Vector3(-5, 2.8f, 5), new Color(1f, 0.85f, 0.6f), 1f, 8f));
        lucesFlicker.Add(CrearLuzPuntual("Luz_Mesa", new Vector3(2, 1.2f, 2), new Color(0.8f, 0.6f, 0.3f), 0.8f, 5f));

        // Zonas de teleport
        CrearZonaTeleport("Recepcion", new Vector3(0, 0.01f, 0), new Color(0, 1, 0, 0.3f));
        CrearZonaTeleport("Pasillo", new Vector3(0, 0.01f, 9.5f), new Color(0, 0.5f, 1, 0.3f));

        // Audio
        var audioParent = new GameObject("--- AUDIO ---");
        CrearMusicaAmbiente("MusicaAmbiente", audioParent.transform);
        CrearMusicaAmbiente("CanalDialogo", audioParent.transform);

        // Controller
        var controllerObj = new GameObject("CapituloDespertarController");
        controllerObj.AddComponent<CapituloDespertar>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[VRScenarioBuilder] Escena Capitulo 1 construida.");
    }

    // ===========================================
    //  ESCENA: CAPITULO 2 — EXPLORACION
    // ===========================================

    static void ConstruirEscenaCapitulo2()
    {
        var scene = CrearEscenaNueva("HistoriaCapitulo2_Exploracion");
        CrearIluminacionBase();

        var xrOrigin = CrearXROrigin();
        var managers = CrearManagers();
        CrearCanvasFade(xrOrigin.transform);
        CrearSubtitleDisplay(xrOrigin.transform);
        CrearHUD(xrOrigin.transform);

        // === Edificio Abandonado — Planta Baja ===
        CrearSueloBasico(40f);

        // Paredes del edificio
        CrearPared("Pared_N", new Vector3(0, 2, 20), new Vector3(40, 4, 0.3f));
        CrearPared("Pared_S", new Vector3(0, 2, -20), new Vector3(40, 4, 0.3f));
        CrearPared("Pared_E", new Vector3(20, 2, 0), new Vector3(0.3f, 4, 40));
        CrearPared("Pared_W", new Vector3(-20, 2, 0), new Vector3(0.3f, 4, 40));

        // Division: Pasillo central
        CrearPared("DivisionIzq", new Vector3(-8, 2, 0), new Vector3(0.2f, 4, 30));
        CrearPared("DivisionDer", new Vector3(8, 2, 0), new Vector3(0.2f, 4, 30));

        // Laboratorio (area derecha)
        CrearPared("LabPared1", new Vector3(14, 2, 10), new Vector3(12, 4, 0.2f));
        var puertaLab = CrearObjetoInteractuable("PuertaLaboratorio", new Vector3(14, 1.2f, 5), PrimitiveType.Cube, new Color(0.5f, 0.5f, 0.6f));
        puertaLab.transform.localScale = new Vector3(1.5f, 2.4f, 0.1f);
        puertaLab.SetActive(false);

        // Objetos interactuables
        var tarjeta = CrearObjetoInteractuable("TarjetaAcceso", new Vector3(-5, 0.8f, -10), PrimitiveType.Cube, Color.cyan);
        tarjeta.transform.localScale = new Vector3(0.1f, 0.01f, 0.06f);

        var documento = CrearObjetoInteractuable("DocumentoSecreto", new Vector3(14, 1.2f, 14), PrimitiveType.Cube, Color.white);
        documento.transform.localScale = new Vector3(0.2f, 0.01f, 0.3f);

        var palanca = CrearObjetoInteractuable("PalancaEnergia", new Vector3(-15, 1.2f, -15), PrimitiveType.Cube, Color.red);
        palanca.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);

        var terminalDatos = CrearObjetoInteractuable("TerminalDatos", new Vector3(14, 1f, 16), PrimitiveType.Cube, new Color(0.1f, 0.2f, 0.1f));
        terminalDatos.transform.localScale = new Vector3(0.5f, 0.4f, 0.05f);

        // Barrera de energia (sotano)
        var barrera = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrera.name = "BarreraEnergia";
        barrera.transform.position = new Vector3(-15, 1.5f, -5);
        barrera.transform.localScale = new Vector3(3f, 3f, 0.1f);
        var barrRenderer = barrera.GetComponent<Renderer>();
        if (barrRenderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            barrRenderer.material = mat;
        }

        // NPCs
        CrearNPC("DrMendez", new Vector3(12, 1, 12), new Color(0.8f, 0.8f, 0.9f));
        CrearNPC("Fantasma", new Vector3(-15, 1, 10), new Color(0.5f, 0.5f, 0.8f));

        // Luces
        CrearLuzPuntual("Luz_Pasillo_1", new Vector3(0, 3.5f, -10), new Color(0.7f, 0.7f, 0.8f), 1.2f, 15f);
        CrearLuzPuntual("Luz_Pasillo_2", new Vector3(0, 3.5f, 10), new Color(0.7f, 0.7f, 0.8f), 1.2f, 15f);
        CrearLuzPuntual("Luz_Lab", new Vector3(14, 3.5f, 14), new Color(0.4f, 0.8f, 0.4f), 1.5f, 10f);
        var luzEmergencia = CrearLuzPuntual("Luz_Emergencia", new Vector3(-15, 2, -10), Color.red, 2f, 8f);

        // Particulas
        var partObj = new GameObject("ParticulasPolvo");
        partObj.transform.position = new Vector3(0, 3, 0);
        partObj.AddComponent<ParticleSystem>();

        // Zonas teleport
        CrearZonaTeleport("EntradaEdificio", new Vector3(0, 0.01f, -18), new Color(0, 1, 0, 0.3f));
        CrearZonaTeleport("Laboratorio", new Vector3(14, 0.01f, 12), new Color(0, 0, 1, 0.3f));
        CrearZonaTeleport("Sotano", new Vector3(-15, 0.01f, -12), new Color(1, 0.5f, 0, 0.3f));
        CrearZonaTeleport("Archivos", new Vector3(-15, 0.01f, 12), new Color(1, 1, 0, 0.3f));

        // Audio
        var audioParent = new GameObject("--- AUDIO ---");
        CrearMusicaAmbiente("MusicaAmbiente", audioParent.transform);

        // Controller
        var controllerObj = new GameObject("CapituloExploracionController");
        controllerObj.AddComponent<CapituloExploracion>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[VRScenarioBuilder] Escena Capitulo 2 construida.");
    }

    // ===========================================
    //  ESCENA: CAPITULO 3 — REVELACION
    // ===========================================

    static void ConstruirEscenaCapitulo3()
    {
        var scene = CrearEscenaNueva("HistoriaCapitulo3_Revelacion");
        CrearIluminacionBase();

        var xrOrigin = CrearXROrigin();
        var managers = CrearManagers();
        CrearCanvasFade(xrOrigin.transform);
        CrearSubtitleDisplay(xrOrigin.transform);
        CrearHUD(xrOrigin.transform);

        // === Tercer Piso — Sala de la Revelacion ===
        CrearSueloBasico(30f);

        // Estructura circular (hexagonal simplificada)
        float radius = 12f;
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 2, Mathf.Sin(angle) * radius);
            float yRot = -i * 60f + 90f;
            var pared = CrearPared("Pared_Hex_" + i, pos, new Vector3(radius * 0.9f, 4, 0.3f));
            pared.transform.rotation = Quaternion.Euler(0, yRot, 0);
        }

        // Techo alto
        var techo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        techo.name = "Techo";
        techo.transform.position = new Vector3(0, 5, 0);
        techo.transform.rotation = Quaternion.Euler(180, 0, 0);
        techo.transform.localScale = new Vector3(3, 1, 3);
        techo.isStatic = true;

        // Terminal Central
        var terminalCentral = CrearObjetoInteractuable("TerminalCentral", new Vector3(0, 1.2f, 3), PrimitiveType.Cube, new Color(0.1f, 0.3f, 0.1f));
        terminalCentral.transform.localScale = new Vector3(1f, 0.8f, 0.1f);

        // Plataforma central
        var plataforma = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        plataforma.name = "PlataformaCentral";
        plataforma.transform.position = new Vector3(0, 0.1f, 0);
        plataforma.transform.localScale = new Vector3(4, 0.1f, 4);
        var platRenderer = plataforma.GetComponent<Renderer>();
        if (platRenderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.2f, 0.1f, 0.3f);
            platRenderer.material = mat;
        }

        // Interruptor de emergencia
        var interruptor = CrearObjetoInteractuable("InterruptorEmergencia", new Vector3(-3, 1.2f, -2), PrimitiveType.Cube, Color.red);
        interruptor.transform.localScale = new Vector3(0.3f, 0.2f, 0.1f);

        // Artefacto final (item)
        var artefacto = CrearObjetoInteractuable("ArtefactoFinal", new Vector3(3, 1.5f, -2), PrimitiveType.Sphere, new Color(0.5f, 0, 1f));
        artefacto.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        // Entidad (oculta inicialmente)
        var entidad = new GameObject("EntidadFinal");
        entidad.transform.position = new Vector3(0, 2, 0);
        var entidadVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        entidadVisual.name = "EntidadMesh";
        entidadVisual.transform.SetParent(entidad.transform);
        entidadVisual.transform.localPosition = Vector3.zero;
        entidadVisual.transform.localScale = new Vector3(2, 2, 2);
        var entRenderer = entidadVisual.GetComponent<Renderer>();
        if (entRenderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.3f, 0f, 0.5f);
            mat.SetFloat("_Smoothness", 0.9f);
            entRenderer.material = mat;
        }
        entidad.SetActive(false);

        // Sala secreta
        var salaSecreta = new GameObject("SalaSecreto");
        salaSecreta.SetActive(false);

        // Efectos
        var portalFX = new GameObject("EfectoPortal");
        portalFX.transform.position = new Vector3(0, 2, 0);
        portalFX.AddComponent<ParticleSystem>();

        var explosionFX = new GameObject("EfectoExplosion");
        explosionFX.transform.position = new Vector3(0, 2, 0);
        explosionFX.AddComponent<ParticleSystem>();

        // Iluminacion dramatica
        var luces = new List<Light>();
        luces.Add(CrearLuzPuntual("Luz_Central", new Vector3(0, 4, 0), new Color(0.5f, 0.3f, 1f), 2f, 15f));
        luces.Add(CrearLuzPuntual("Luz_Terminal", new Vector3(0, 2, 3), new Color(0, 1, 0), 1.5f, 5f));
        luces.Add(CrearLuzPuntual("Luz_Borde_1", new Vector3(-8, 2, 0), new Color(0.8f, 0, 0), 1f, 8f));
        luces.Add(CrearLuzPuntual("Luz_Borde_2", new Vector3(8, 2, 0), new Color(0, 0, 0.8f), 1f, 8f));

        // Zona teleport
        CrearZonaTeleport("TercerPiso", new Vector3(0, 0.01f, -8), new Color(0.5f, 0, 1, 0.3f));

        // Audio
        var audioParent = new GameObject("--- AUDIO ---");
        CrearMusicaAmbiente("MusicaAmbiente", audioParent.transform);

        // Controller
        var controllerObj = new GameObject("CapituloRevelacionController");
        controllerObj.AddComponent<CapituloRevelacion>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[VRScenarioBuilder] Escena Capitulo 3 construida.");
    }

    // ===========================================
    //  ESCENA: SIMULACION ISTR
    // ===========================================

    static void ConstruirEscenaISTR()
    {
        var scene = CrearEscenaNueva("SimulacionISTR");
        CrearIluminacionBase();

        var xrOrigin = CrearXROrigin();
        var managers = CrearManagers();
        CrearCanvasFade(xrOrigin.transform);
        CrearSubtitleDisplay(xrOrigin.transform);
        CrearHUD(xrOrigin.transform);

        // === PISO 1: Cuarto de Maquinas ===
        var piso1Parent = new GameObject("--- PISO 1 ---");

        // Suelo Piso 1
        var sueloPiso1 = GameObject.CreatePrimitive(PrimitiveType.Plane);
        sueloPiso1.name = "Suelo_Piso1";
        sueloPiso1.transform.SetParent(piso1Parent.transform);
        sueloPiso1.transform.localScale = new Vector3(3, 1, 3);

        // Paredes piso 1
        var p1 = new GameObject("Paredes_Piso1");
        p1.transform.SetParent(piso1Parent.transform);
        CrearPared("P1_N", new Vector3(0, 1.5f, 15), new Vector3(30, 3, 0.2f)).transform.SetParent(p1.transform);
        CrearPared("P1_S", new Vector3(0, 1.5f, -15), new Vector3(30, 3, 0.2f)).transform.SetParent(p1.transform);
        CrearPared("P1_E", new Vector3(15, 1.5f, 0), new Vector3(0.2f, 3, 30)).transform.SetParent(p1.transform);
        CrearPared("P1_W", new Vector3(-15, 1.5f, 0), new Vector3(0.2f, 3, 30)).transform.SetParent(p1.transform);

        // Racks de servidores
        for (int i = 0; i < 5; i++)
        {
            var rack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rack.name = "Rack_Servidor_" + (i + 1);
            rack.transform.SetParent(piso1Parent.transform);
            rack.transform.position = new Vector3(-10 + i * 4, 1, 5);
            rack.transform.localScale = new Vector3(0.8f, 2f, 0.6f);
            var rackRenderer = rack.GetComponent<Renderer>();
            if (rackRenderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.15f, 0.15f, 0.2f);
                rackRenderer.material = mat;
            }
        }

        // Rack averiado (interactuable)
        var rackAveriado = CrearObjetoInteractuable("RackAveriado", new Vector3(2, 1, 5), PrimitiveType.Cube, Color.red);
        rackAveriado.transform.localScale = new Vector3(0.8f, 2f, 0.6f);
        rackAveriado.transform.SetParent(piso1Parent.transform);

        // Terminal de error
        var terminalError = CrearObjetoInteractuable("TerminalError", new Vector3(2, 1.5f, 4.5f), PrimitiveType.Cube, new Color(0, 0.3f, 0));
        terminalError.transform.localScale = new Vector3(0.4f, 0.3f, 0.05f);
        terminalError.transform.SetParent(piso1Parent.transform);

        // Palanca de reinicio
        var palancaReinicio = CrearObjetoInteractuable("PalancaReinicio", new Vector3(2, 0.8f, 5.3f), PrimitiveType.Cylinder, Color.yellow);
        palancaReinicio.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
        palancaReinicio.transform.SetParent(piso1Parent.transform);

        // Puerta a gradas
        var puertaGradas = CrearObjetoInteractuable("PuertaGradas", new Vector3(14.9f, 1.2f, 0), PrimitiveType.Cube, new Color(0.5f, 0.3f, 0.1f));
        puertaGradas.transform.localScale = new Vector3(0.1f, 2.4f, 1.2f);
        puertaGradas.transform.SetParent(piso1Parent.transform);
        puertaGradas.SetActive(false);

        // Luces piso 1
        CrearLuzPuntual("Luz_P1_1", new Vector3(-5, 2.8f, 5), new Color(0.3f, 0.5f, 0.3f), 1.5f, 12f);
        CrearLuzPuntual("Luz_P1_2", new Vector3(5, 2.8f, 5), new Color(0.3f, 0.5f, 0.3f), 1.5f, 12f);
        CrearLuzPuntual("Luz_P1_Roja", new Vector3(2, 2.5f, 5), Color.red, 0.8f, 5f);

        // === GRADAS ===
        var gradasParent = new GameObject("--- GRADAS ---");
        gradasParent.transform.position = new Vector3(20, 0, 0);

        for (int vuelta = 0; vuelta < 3; vuelta++)
        {
            for (int step = 0; step < 10; step++)
            {
                var escalon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                escalon.name = $"Escalon_{vuelta}_{step}";
                escalon.transform.SetParent(gradasParent.transform);
                float y = vuelta * 3f + step * 0.3f;
                float z = step * 0.4f;
                escalon.transform.localPosition = new Vector3(0, y, z);
                escalon.transform.localScale = new Vector3(2f, 0.15f, 0.4f);
            }
        }

        CrearLuzPuntual("Luz_Gradas_1", new Vector3(20, 2, 2), new Color(0.2f, 0.2f, 0.3f), 0.8f, 8f);
        CrearLuzPuntual("Luz_Gradas_2", new Vector3(20, 5, 4), new Color(0.2f, 0.2f, 0.3f), 0.8f, 8f);

        // === PISO 3: Laboratorio ===
        var piso3Parent = new GameObject("--- PISO 3 ---");
        piso3Parent.transform.position = new Vector3(0, 10, 0);

        var sueloPiso3 = GameObject.CreatePrimitive(PrimitiveType.Plane);
        sueloPiso3.name = "Suelo_Piso3";
        sueloPiso3.transform.SetParent(piso3Parent.transform);
        sueloPiso3.transform.localScale = new Vector3(2.5f, 1, 2.5f);
        sueloPiso3.transform.localPosition = Vector3.zero;

        // Paredes piso 3
        var p3 = new GameObject("Paredes_Piso3");
        p3.transform.SetParent(piso3Parent.transform);
        CrearPared("P3_N", new Vector3(0, 11.5f, 12), new Vector3(25, 3, 0.2f)).transform.SetParent(p3.transform);
        CrearPared("P3_S", new Vector3(0, 11.5f, -12), new Vector3(25, 3, 0.2f)).transform.SetParent(p3.transform);
        CrearPared("P3_E", new Vector3(12, 11.5f, 0), new Vector3(0.2f, 3, 25)).transform.SetParent(p3.transform);
        CrearPared("P3_W", new Vector3(-12, 11.5f, 0), new Vector3(0.2f, 3, 25)).transform.SetParent(p3.transform);

        // Computadora anomala
        var pcAnomala = CrearObjetoInteractuable("ComputadoraAnomala", new Vector3(5, 10.8f, 8), PrimitiveType.Cube, new Color(0.1f, 0.5f, 0.1f));
        pcAnomala.transform.localScale = new Vector3(0.5f, 0.4f, 0.05f);
        pcAnomala.transform.SetParent(piso3Parent.transform);

        // Interruptor transmision
        var interruptor = CrearObjetoInteractuable("InterruptorTransmision", new Vector3(-5, 11, 8), PrimitiveType.Cube, Color.red);
        interruptor.transform.localScale = new Vector3(0.2f, 0.3f, 0.1f);
        interruptor.transform.SetParent(piso3Parent.transform);

        // Monitor anomalo (luz)
        var luzMonitor = CrearLuzPuntual("LuzMonitorAnomalo", new Vector3(5, 11, 8), Color.green, 2f, 5f);
        luzMonitor.enabled = false;

        // Anomalia final (oculta)
        var anomalia = new GameObject("ObjetoAnomaliaFinal");
        anomalia.transform.position = new Vector3(0, 11, 5);
        var anomaliaVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        anomaliaVisual.transform.SetParent(anomalia.transform);
        anomaliaVisual.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        var anomRenderer = anomaliaVisual.GetComponent<Renderer>();
        if (anomRenderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.8f, 0, 0);
            mat.SetFloat("_Smoothness", 0.95f);
            anomRenderer.material = mat;
        }
        anomalia.SetActive(false);

        CrearLuzPuntual("Luz_P3_1", new Vector3(0, 12.5f, 0), new Color(0.1f, 0.1f, 0.2f), 0.5f, 15f);

        // Zonas de teleport
        CrearZonaTeleport("ISTR_Piso1", new Vector3(0, 0.01f, -10), new Color(0, 1, 0, 0.3f));
        CrearZonaTeleport("ISTR_Gradas", new Vector3(20, 0.01f, 0), new Color(1, 1, 0, 0.3f));
        CrearZonaTeleport("ISTR_Piso3", new Vector3(0, 10.01f, -8), new Color(1, 0, 0, 0.3f));
        CrearZonaTeleport("ISTR_Lab", new Vector3(5, 10.01f, 5), new Color(0.5f, 0, 1, 0.3f));

        // Audio
        var audioParent = new GameObject("--- AUDIO ---");
        CrearMusicaAmbiente("MusicaAmbiente", audioParent.transform);
        CrearMusicaAmbiente("SFXChannel", audioParent.transform);

        // Gestor Historia ISTR
        var gestorObj = new GameObject("GestorHistoriaISTR");
        gestorObj.AddComponent<GestorHistoriaISTR>();

        // Controlador ISTR VR
        var istrController = new GameObject("SimulacionISTRController");
        istrController.AddComponent<SimulacionISTRController>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[VRScenarioBuilder] Escena ISTR construida.");
    }

    // ===========================================
    //  BUILD SETTINGS
    // ===========================================

    static void AgregarEscenasAlBuild()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] nuevasEscenas = {
            SCENES_PATH + "MenuPrincipalVR.unity",
            SCENES_PATH + "HistoriaCapitulo1_Despertar.unity",
            SCENES_PATH + "HistoriaCapitulo2_Exploracion.unity",
            SCENES_PATH + "HistoriaCapitulo3_Revelacion.unity",
            SCENES_PATH + "SimulacionISTR.unity"
        };

        foreach (var path in nuevasEscenas)
        {
            bool exists = false;
            foreach (var s in scenes)
            {
                if (s.path == path) { exists = true; s.enabled = true; break; }
            }
            if (!exists)
                scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[VRScenarioBuilder] Escenas agregadas al Build Settings.");
    }
}
#endif
