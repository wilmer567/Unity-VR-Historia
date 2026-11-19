#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;

public class ConfiguradorGraficosRealistas : EditorWindow
{
    private const string RUTA_MATERIALES = "Assets/_Unity Essentials/Materials/Realistas";
    private const string SHADER_URP_LIT = "Universal Render Pipeline/Lit";

    [MenuItem("Syntro Terror/Configurar Graficos Fotorrealistas")]
    public static void Ejecutar()
    {
        Debug.Log("[ConfigGraficos] === INICIO: Configuracion Fotorrealista URP ===");

        AsegurarCarpetas();
        CrearMaterialesPBR();
        AsignarMaterialesAEscena();
        ConfigurarReflectionProbes();
        ConfigurarNiebla();
        ConfigurarPostProcessing();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[ConfigGraficos] === COMPLETADO: Acabado fotorrealista aplicado ===");
    }

    // =====================================================
    //  CARPETAS
    // =====================================================

    static void AsegurarCarpetas()
    {
        if (!AssetDatabase.IsValidFolder(RUTA_MATERIALES))
        {
            string parent = Path.GetDirectoryName(RUTA_MATERIALES).Replace("\\", "/");
            AssetDatabase.CreateFolder(parent, "Realistas");
        }
        string rutaTexturas = RUTA_MATERIALES + "/Texturas";
        if (!AssetDatabase.IsValidFolder(rutaTexturas))
            AssetDatabase.CreateFolder(RUTA_MATERIALES, "Texturas");
    }

    // =====================================================
    //  MATERIALES PBR
    // =====================================================

    static void CrearMaterialesPBR()
    {
        CrearMat_PisoBaldosa();
        CrearMat_ParedConcreto();
        CrearMat_RackMetalico();
        CrearMat_MarcoMaderaVidrio();
        Debug.Log("[ConfigGraficos] 4 materiales PBR creados en " + RUTA_MATERIALES);
    }

    static Material ObtenerOCrearMaterial(string nombre)
    {
        string ruta = RUTA_MATERIALES + "/" + nombre + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat != null) return mat;

        Shader shader = Shader.Find(SHADER_URP_LIT);
        if (shader == null)
        {
            Debug.LogError("[ConfigGraficos] No se encontro el shader URP/Lit");
            return null;
        }
        mat = new Material(shader);
        mat.name = nombre;
        AssetDatabase.CreateAsset(mat, ruta);
        return mat;
    }

    // --- Piso Baldosa: ceramico oscuro semi-reflectante con cuadricula ---
    static void CrearMat_PisoBaldosa()
    {
        Material mat = ObtenerOCrearMaterial("Mat_PisoBaldosa_Syntro");
        if (mat == null) return;

        Texture2D texAlbedo = GenerarTexturaBaldosa(512, new Color(0.08f, 0.07f, 0.065f), new Color(0.04f, 0.035f, 0.03f));
        GuardarTextura(texAlbedo, "Tex_Baldosa_Albedo");
        mat.SetTexture("_BaseMap", texAlbedo);
        mat.SetColor("_BaseColor", new Color(0.12f, 0.11f, 0.1f));
        mat.SetFloat("_Smoothness", 0.75f);
        mat.SetFloat("_Metallic", 0.0f);
        mat.SetTextureScale("_BaseMap", new Vector2(6f, 6f));

        Texture2D texNormal = GenerarNormalMapBaldosa(512);
        GuardarTextura(texNormal, "Tex_Baldosa_Normal");
        mat.SetTexture("_BumpMap", texNormal);
        mat.SetFloat("_BumpScale", 0.8f);
        mat.SetTextureScale("_BumpMap", new Vector2(6f, 6f));
        mat.EnableKeyword("_NORMALMAP");

        EditorUtility.SetDirty(mat);
    }

    // --- Pared Concreto: rugosa mate con manchas humedad ---
    static void CrearMat_ParedConcreto()
    {
        Material mat = ObtenerOCrearMaterial("Mat_ParedConcreto_Syntro");
        if (mat == null) return;

        Texture2D texAlbedo = GenerarTexturaConcreto(512, new Color(0.25f, 0.24f, 0.22f));
        GuardarTextura(texAlbedo, "Tex_Concreto_Albedo");
        mat.SetTexture("_BaseMap", texAlbedo);
        mat.SetColor("_BaseColor", new Color(0.28f, 0.26f, 0.24f));
        mat.SetFloat("_Smoothness", 0.15f);
        mat.SetFloat("_Metallic", 0.0f);
        mat.SetTextureScale("_BaseMap", new Vector2(3f, 3f));

        Texture2D texNormal = GenerarNormalMapConcreto(512);
        GuardarTextura(texNormal, "Tex_Concreto_Normal");
        mat.SetTexture("_BumpMap", texNormal);
        mat.SetFloat("_BumpScale", 1.2f);
        mat.SetTextureScale("_BumpMap", new Vector2(3f, 3f));
        mat.EnableKeyword("_NORMALMAP");

        Texture2D texOcclusion = GenerarOcclusionConcreto(512);
        GuardarTextura(texOcclusion, "Tex_Concreto_Occlusion");
        mat.SetTexture("_OcclusionMap", texOcclusion);
        mat.SetFloat("_OcclusionStrength", 0.7f);

        EditorUtility.SetDirty(mat);
    }

    // --- Rack Metalico: metal anodizado negro con LEDs emisivos ---
    static void CrearMat_RackMetalico()
    {
        Material mat = ObtenerOCrearMaterial("Mat_RackMetalico_Negro");
        if (mat == null) return;

        Texture2D texAlbedo = GenerarTexturaRejillaRack(512);
        GuardarTextura(texAlbedo, "Tex_Rack_Albedo");
        mat.SetTexture("_BaseMap", texAlbedo);
        mat.SetColor("_BaseColor", new Color(0.03f, 0.03f, 0.035f));
        mat.SetFloat("_Smoothness", 0.6f);
        mat.SetFloat("_Metallic", 0.85f);
        mat.SetTextureScale("_BaseMap", new Vector2(2f, 4f));

        Texture2D texEmission = GenerarTexturaLEDsRack(512);
        GuardarTextura(texEmission, "Tex_Rack_Emission");
        mat.SetTexture("_EmissionMap", texEmission);
        mat.SetColor("_EmissionColor", new Color(3f, 0.2f, 0.1f) * 2.5f);
        mat.SetTextureScale("_EmissionMap", new Vector2(2f, 4f));
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        Texture2D texNormal = GenerarNormalMapRejilla(512);
        GuardarTextura(texNormal, "Tex_Rack_Normal");
        mat.SetTexture("_BumpMap", texNormal);
        mat.SetFloat("_BumpScale", 1.0f);
        mat.SetTextureScale("_BumpMap", new Vector2(2f, 4f));
        mat.EnableKeyword("_NORMALMAP");

        EditorUtility.SetDirty(mat);
    }

    // --- Marco Madera con Vidrio: madera barnizada + cristal reflectante ---
    static void CrearMat_MarcoMaderaVidrio()
    {
        Material mat = ObtenerOCrearMaterial("Mat_MarcoMaderaVidrio");
        if (mat == null) return;

        Texture2D texAlbedo = GenerarTexturaMadera(512, new Color(0.12f, 0.06f, 0.03f));
        GuardarTextura(texAlbedo, "Tex_Madera_Albedo");
        mat.SetTexture("_BaseMap", texAlbedo);
        mat.SetColor("_BaseColor", new Color(0.15f, 0.08f, 0.04f));
        mat.SetFloat("_Smoothness", 0.7f);
        mat.SetFloat("_Metallic", 0.0f);
        mat.SetTextureScale("_BaseMap", new Vector2(1f, 2f));

        Texture2D texNormal = GenerarNormalMapMadera(512);
        GuardarTextura(texNormal, "Tex_Madera_Normal");
        mat.SetTexture("_BumpMap", texNormal);
        mat.SetFloat("_BumpScale", 0.6f);
        mat.SetTextureScale("_BumpMap", new Vector2(1f, 2f));
        mat.EnableKeyword("_NORMALMAP");

        mat.SetFloat("_Surface", 0); // Opaque
        mat.SetFloat("_Blend", 0);

        EditorUtility.SetDirty(mat);
    }

    // =====================================================
    //  GENERADORES DE TEXTURAS PROCEDURALES
    // =====================================================

    static Texture2D GenerarTexturaBaldosa(int size, Color colorBaldosa, Color colorJunta)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        int tileSize = size / 8;
        int groutWidth = Mathf.Max(2, size / 128);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool esJunta = (x % tileSize < groutWidth) || (y % tileSize < groutWidth);
                float ruido = Random.Range(-0.02f, 0.02f);
                Color c = esJunta ? colorJunta : colorBaldosa;
                c += new Color(ruido, ruido, ruido, 0f);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    static Texture2D GenerarNormalMapBaldosa(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        Color normalFlat = new Color(0.5f, 0.5f, 1f, 1f);
        Color normalEdge = new Color(0.3f, 0.3f, 1f, 1f);
        int tileSize = size / 8;
        int groutWidth = Mathf.Max(3, size / 100);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int modX = x % tileSize;
                int modY = y % tileSize;
                bool bordeX = modX < groutWidth || modX > tileSize - groutWidth;
                bool bordeY = modY < groutWidth || modY > tileSize - groutWidth;
                Color c = (bordeX || bordeY) ? normalEdge : normalFlat;
                float ruido = Random.Range(-0.03f, 0.03f);
                c.r += ruido;
                c.g += ruido;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerarTexturaConcreto(int size, Color baseColor)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float ruidoFino = Random.Range(-0.04f, 0.04f);
                float manchaHumedad = Mathf.PerlinNoise(x * 0.008f, y * 0.008f);
                float oscurecimiento = manchaHumedad > 0.6f ? -0.06f * (manchaHumedad - 0.6f) / 0.4f : 0f;
                Color c = baseColor + new Color(ruidoFino + oscurecimiento, ruidoFino + oscurecimiento, ruidoFino + oscurecimiento * 1.2f, 0f);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerarNormalMapConcreto(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = 0.5f + Random.Range(-0.08f, 0.08f);
                float ny = 0.5f + Random.Range(-0.08f, 0.08f);
                tex.SetPixel(x, y, new Color(nx, ny, 1f, 1f));
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerarOcclusionConcreto(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float ao = 0.85f + Mathf.PerlinNoise(x * 0.01f, y * 0.01f) * 0.15f;
                tex.SetPixel(x, y, new Color(ao, ao, ao, 1f));
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerarTexturaRejillaRack(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        Color metalBase = new Color(0.04f, 0.04f, 0.045f);
        Color holeColor = new Color(0.015f, 0.015f, 0.02f);
        int slotH = size / 16;
        int holeW = size / 20;
        int holeMargin = size / 40;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int modY = y % slotH;
                bool esRanura = modY > slotH / 4 && modY < slotH * 3 / 4;
                bool esAgujero = esRanura && (x % (holeW + holeMargin)) < holeW && x > size / 6 && x < size * 5 / 6;
                float ruido = Random.Range(-0.005f, 0.005f);
                Color c = esAgujero ? holeColor : metalBase;
                c += new Color(ruido, ruido, ruido, 0f);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerarTexturaLEDsRack(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        Color negro = Color.black;
        int slotH = size / 16;
        int ledSize = Mathf.Max(3, size / 128);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, negro);

        for (int slot = 0; slot < 16; slot++)
        {
            int baseY = slot * slotH + slotH / 2;
            int ledX = size / 12;
            for (int led = 0; led < 3; led++)
            {
                Color ledColor = (Random.value > 0.4f)
                    ? new Color(1f, 0.05f, 0.02f)
                    : new Color(0.05f, 1f, 0.1f);

                int cx = ledX + led * (ledSize * 3);
                for (int dy = -ledSize; dy <= ledSize; dy++)
                    for (int dx = -ledSize; dx <= ledSize; dx++)
                    {
                        int px = cx + dx;
                        int py = baseY + dy;
                        if (px >= 0 && px < size && py >= 0 && py < size)
                            if (dx * dx + dy * dy <= ledSize * ledSize)
                                tex.SetPixel(px, py, ledColor);
                    }
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerarNormalMapRejilla(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        int slotH = size / 16;
        Color normalFlat = new Color(0.5f, 0.5f, 1f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int modY = y % slotH;
                bool borde = modY < 2 || modY > slotH - 2;
                float nx = borde ? 0.35f : 0.5f + Random.Range(-0.02f, 0.02f);
                float ny = borde ? (modY < 2 ? 0.35f : 0.65f) : 0.5f + Random.Range(-0.02f, 0.02f);
                tex.SetPixel(x, y, new Color(nx, ny, 1f, 1f));
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerarTexturaMadera(int size, Color baseColor)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float veta = Mathf.Sin(y * 0.05f + Mathf.PerlinNoise(x * 0.02f, y * 0.005f) * 3f) * 0.03f;
                float ruido = Random.Range(-0.01f, 0.01f);
                Color c = baseColor + new Color(veta + ruido, veta * 0.6f + ruido, veta * 0.3f + ruido, 0f);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerarNormalMapMadera(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float veta = Mathf.Cos(y * 0.05f + Mathf.PerlinNoise(x * 0.02f, y * 0.005f) * 3f);
                float nx = 0.5f + veta * 0.05f;
                float ny = 0.5f + Random.Range(-0.02f, 0.02f);
                tex.SetPixel(x, y, new Color(nx, ny, 1f, 1f));
            }
        }
        tex.Apply(true);
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static void GuardarTextura(Texture2D tex, string nombre)
    {
        string ruta = RUTA_MATERIALES + "/Texturas/" + nombre + ".png";
        string fullPath = Path.Combine(Application.dataPath, "..", ruta);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (importer != null)
        {
            if (nombre.Contains("Normal"))
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = !nombre.Contains("Occlusion");
            }
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
    }

    // =====================================================
    //  ASIGNACION DE MATERIALES A ESCENA
    // =====================================================

    static void AsignarMaterialesAEscena()
    {
        Material matPiso = AssetDatabase.LoadAssetAtPath<Material>(RUTA_MATERIALES + "/Mat_PisoBaldosa_Syntro.mat");
        Material matPared = AssetDatabase.LoadAssetAtPath<Material>(RUTA_MATERIALES + "/Mat_ParedConcreto_Syntro.mat");
        Material matRack = AssetDatabase.LoadAssetAtPath<Material>(RUTA_MATERIALES + "/Mat_RackMetalico_Negro.mat");
        Material matMarco = AssetDatabase.LoadAssetAtPath<Material>(RUTA_MATERIALES + "/Mat_MarcoMaderaVidrio.mat");

        string[] zonas = { "Piso1_CuartoMaquinas", "Bloque_Gradas_Terror", "Piso3_Laboratorio" };
        int totalAsignados = 0;

        foreach (string zona in zonas)
        {
            GameObject obj = GameObject.Find(zona);
            if (obj == null)
            {
                Debug.LogWarning("[ConfigGraficos] No se encontro: " + zona);
                continue;
            }

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rend in renderers)
            {
                string nombreObj = rend.gameObject.name.ToLower();

                if (nombreObj.Contains("piso") || nombreObj.Contains("suelo") || nombreObj.Contains("floor")
                    || nombreObj.Contains("grada") || nombreObj.Contains("escalon") || nombreObj.Contains("stair"))
                {
                    rend.sharedMaterial = matPiso;
                    totalAsignados++;
                }
                else if (nombreObj.Contains("pared") || nombreObj.Contains("wall") || nombreObj.Contains("muro")
                         || nombreObj.Contains("techo") || nombreObj.Contains("ceiling"))
                {
                    rend.sharedMaterial = matPared;
                    totalAsignados++;
                }
                else if (nombreObj.Contains("rack") || nombreObj.Contains("servidor") || nombreObj.Contains("server")
                         || nombreObj.Contains("equipo") || nombreObj.Contains("maquina") || nombreObj.Contains("gabinete"))
                {
                    rend.sharedMaterial = matRack;
                    totalAsignados++;
                }
                else if (nombreObj.Contains("cuadro") || nombreObj.Contains("marco") || nombreObj.Contains("frame")
                         || nombreObj.Contains("ventana") || nombreObj.Contains("vitrina") || nombreObj.Contains("puerta"))
                {
                    rend.sharedMaterial = matMarco;
                    totalAsignados++;
                }
            }
        }

        Debug.Log($"[ConfigGraficos] Materiales asignados a {totalAsignados} renderers en escena");
    }

    // =====================================================
    //  REFLECTION PROBES
    // =====================================================

    static void ConfigurarReflectionProbes()
    {
        CrearReflectionProbe("ReflectionProbe_CuartoMaquinas", "Piso1_CuartoMaquinas",
            new Vector3(10f, 4f, 10f), ReflectionProbeMode.Baked);
        CrearReflectionProbe("ReflectionProbe_Gradas", "Bloque_Gradas_Terror",
            new Vector3(5f, 12f, 5f), ReflectionProbeMode.Realtime);
        CrearReflectionProbe("ReflectionProbe_Laboratorio", "Piso3_Laboratorio",
            new Vector3(10f, 4f, 10f), ReflectionProbeMode.Baked);

        Debug.Log("[ConfigGraficos] 3 Reflection Probes configurados");
    }

    static void CrearReflectionProbe(string nombre, string padreNombre, Vector3 tamano, ReflectionProbeMode modo)
    {
        GameObject existente = GameObject.Find(nombre);
        if (existente != null)
        {
            Undo.DestroyObjectImmediate(existente);
        }

        GameObject probeGO = new GameObject(nombre);
        Undo.RegisterCreatedObjectUndo(probeGO, "Crear " + nombre);

        GameObject padre = GameObject.Find(padreNombre);
        if (padre != null)
        {
            probeGO.transform.SetParent(padre.transform);
            Bounds bounds = CalcularBoundsHijos(padre);
            probeGO.transform.position = bounds.center;
            tamano = bounds.size + Vector3.one * 2f;
        }

        ReflectionProbe probe = probeGO.AddComponent<ReflectionProbe>();
        probe.mode = modo;
        probe.size = tamano;
        probe.resolution = 256;
        probe.hdr = true;
        probe.shadowDistance = 50f;
        probe.nearClipPlane = 0.1f;
        probe.farClipPlane = 100f;
        probe.intensity = 1.0f;

        if (modo == ReflectionProbeMode.Realtime)
        {
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
        }
    }

    static Bounds CalcularBoundsHijos(GameObject padre)
    {
        Renderer[] renderers = padre.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return new Bounds(padre.transform.position, Vector3.one * 10f);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    // =====================================================
    //  NIEBLA EXPONENCIAL
    // =====================================================

    static void ConfigurarNiebla()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.15f, 0.15f, 0.2f);
        RenderSettings.fogDensity = 0.015f;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.25f, 0.25f, 0.3f);

        Debug.Log("[ConfigGraficos] Niebla suave e iluminación ambiente activadas");
    }

    // =====================================================
    //  POST-PROCESSING (URP VOLUME)
    // =====================================================

    static void ConfigurarPostProcessing()
    {
        ActivarPostProcessingEnCamara();

        string nombreVolume = "GlobalVolume_Fotorrealista";
        GameObject volGO = GameObject.Find(nombreVolume);
        if (volGO != null)
            Undo.DestroyObjectImmediate(volGO);

        volGO = new GameObject(nombreVolume);
        Undo.RegisterCreatedObjectUndo(volGO, "Crear Global Volume");
        volGO.layer = 0;

        Volume volume = volGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        string rutaProfile = RUTA_MATERIALES + "/VolumeProfile_Fotorrealista.asset";
        AssetDatabase.CreateAsset(profile, rutaProfile);
        volume.profile = profile;

        // --- Bloom ---
        Bloom bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(0.8f);
        bloom.intensity.Override(1.2f);
        bloom.scatter.Override(0.65f);
        bloom.highQualityFiltering.Override(true);

        // --- Vignette ---
        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.25f);
        vignette.smoothness.Override(0.35f);
        vignette.color.Override(new Color(0f, 0f, 0.02f));

        // --- Tonemapping ACES ---
        Tonemapping tonemapping = profile.Add<Tonemapping>(true);
        tonemapping.mode.Override(TonemappingMode.ACES);

        // --- Color Adjustments: alto contraste frio y mayor iluminación ---
        ColorAdjustments colorAdj = profile.Add<ColorAdjustments>(true);
        colorAdj.postExposure.Override(0.8f);
        colorAdj.contrast.Override(20f);
        colorAdj.saturation.Override(-5f);
        colorAdj.colorFilter.Override(new Color(0.9f, 0.95f, 1.0f));

        // --- Film Grain sutil ---
        FilmGrain filmGrain = profile.Add<FilmGrain>(true);
        filmGrain.type.Override(FilmGrainLookup.Medium3);
        filmGrain.intensity.Override(0.2f);
        filmGrain.response.Override(0.6f);

        EditorUtility.SetDirty(profile);
        Debug.Log("[ConfigGraficos] Global Volume con post-processing creado: Bloom, Vignette, ACES, Color Adj, Film Grain");
    }

    static void ActivarPostProcessingEnCamara()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Object.FindAnyObjectByType<Camera>();
        }

        if (cam == null)
        {
            Debug.LogWarning("[ConfigGraficos] No se encontro Main Camera");
            return;
        }

        UniversalAdditionalCameraData camData = cam.GetComponent<UniversalAdditionalCameraData>();
        if (camData == null)
            camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

        camData.renderPostProcessing = true;
        camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        camData.antialiasingQuality = AntialiasingQuality.High;
        camData.dithering = true;

        EditorUtility.SetDirty(cam.gameObject);
        Debug.Log("[ConfigGraficos] Post-processing activado en camara: " + cam.name);
    }
}
#endif
