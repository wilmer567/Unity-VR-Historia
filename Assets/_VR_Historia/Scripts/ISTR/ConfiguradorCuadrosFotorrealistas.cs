#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class ConfiguradorCuadrosFotorrealistas : EditorWindow
{
    private const string RUTA_FOTOS = "Assets/_Unity Essentials/Textures/FotosReales";
    private const string RUTA_MATERIALES = "Assets/_Unity Essentials/Materials/Realistas";
    private const string SHADER_PORTAL = "ISTR/PortalFotoMezcla";
    private const string SHADER_URP_LIT = "Universal Render Pipeline/Lit";
    private const int TEX_SIZE = 1024;

    struct ConfigFoto
    {
        public string nombre;
        public string descripcion;
        public Color colorDominante;
        public Color colorSecundario;
        public TipoFoto tipo;
    }

    enum TipoFoto
    {
        Pasillo,
        Fachada,
        Laboratorio,
        Escaleras,
        ServidorRack,
        PatioNocturno
    }

    static readonly ConfigFoto[] FOTOS = new ConfigFoto[]
    {
        new ConfigFoto {
            nombre = "Foto_Pasillo_ISTR",
            descripcion = "Pasillo largo institucional con luces fluorescentes",
            colorDominante = new Color(0.18f, 0.20f, 0.22f),
            colorSecundario = new Color(0.35f, 0.38f, 0.30f),
            tipo = TipoFoto.Pasillo
        },
        new ConfigFoto {
            nombre = "Foto_Fachada_ISTR",
            descripcion = "Fachada del instituto al atardecer",
            colorDominante = new Color(0.30f, 0.22f, 0.15f),
            colorSecundario = new Color(0.55f, 0.35f, 0.20f),
            tipo = TipoFoto.Fachada
        },
        new ConfigFoto {
            nombre = "Foto_Laboratorio_ISTR",
            descripcion = "Laboratorio de computacion con monitores encendidos",
            colorDominante = new Color(0.08f, 0.10f, 0.18f),
            colorSecundario = new Color(0.15f, 0.40f, 0.55f),
            tipo = TipoFoto.Laboratorio
        },
        new ConfigFoto {
            nombre = "Foto_Escaleras_ISTR",
            descripcion = "Escaleras interiores oscuras con luz tenue",
            colorDominante = new Color(0.10f, 0.09f, 0.08f),
            colorSecundario = new Color(0.25f, 0.20f, 0.15f),
            tipo = TipoFoto.Escaleras
        },
        new ConfigFoto {
            nombre = "Foto_Servidores_ISTR",
            descripcion = "Sala de servidores con LEDs parpadeantes",
            colorDominante = new Color(0.04f, 0.04f, 0.06f),
            colorSecundario = new Color(0.8f, 0.1f, 0.05f),
            tipo = TipoFoto.ServidorRack
        },
        new ConfigFoto {
            nombre = "Foto_Patio_ISTR",
            descripcion = "Patio del instituto de noche con farola solitaria",
            colorDominante = new Color(0.03f, 0.04f, 0.07f),
            colorSecundario = new Color(0.6f, 0.5f, 0.25f),
            tipo = TipoFoto.PatioNocturno
        }
    };

    [MenuItem("ISTR Terror/Configurar Cuadros Fotorrealistas con Portal")]
    public static void Ejecutar()
    {
        Debug.Log("[CuadrosFoto] === INICIO: Cuadros fotorrealistas con mezcla portal ===");

        AsegurarCarpetas();
        GenerarFotosReales();
        CrearMaterialesCuadros();
        AsignarACuadrosEnEscena();
        ConfigurarPortalesEnCuadros();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[CuadrosFoto] === COMPLETADO: 6 cuadros configurados con foto + portal ===");
    }

    static void AsegurarCarpetas()
    {
        if (!AssetDatabase.IsValidFolder(RUTA_FOTOS))
        {
            string parent = Path.GetDirectoryName(RUTA_FOTOS).Replace("\\", "/");
            AssetDatabase.CreateFolder(parent, "FotosReales");
        }
        if (!AssetDatabase.IsValidFolder(RUTA_MATERIALES))
        {
            string parent = Path.GetDirectoryName(RUTA_MATERIALES).Replace("\\", "/");
            AssetDatabase.CreateFolder(parent, "Realistas");
        }
    }

    // =====================================================
    //  GENERACION DE FOTOS PROCEDURALES REALISTAS
    // =====================================================

    static void GenerarFotosReales()
    {
        for (int i = 0; i < FOTOS.Length; i++)
        {
            string ruta = RUTA_FOTOS + "/" + FOTOS[i].nombre + ".png";
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(ruta) != null) continue;

            Texture2D tex = GenerarFotoProcedural(FOTOS[i]);
            string fullPath = Path.Combine(Application.dataPath, "..", ruta);
            File.WriteAllBytes(fullPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.maxTextureSize = TEX_SIZE;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }

            Debug.Log($"[CuadrosFoto] Foto generada: {FOTOS[i].nombre} — {FOTOS[i].descripcion}");
        }
    }

    static Texture2D GenerarFotoProcedural(ConfigFoto config)
    {
        Texture2D tex = new Texture2D(TEX_SIZE, TEX_SIZE, TextureFormat.RGBA32, true);

        switch (config.tipo)
        {
            case TipoFoto.Pasillo:     PintarPasillo(tex, config); break;
            case TipoFoto.Fachada:     PintarFachada(tex, config); break;
            case TipoFoto.Laboratorio: PintarLaboratorio(tex, config); break;
            case TipoFoto.Escaleras:   PintarEscaleras(tex, config); break;
            case TipoFoto.ServidorRack:PintarServidores(tex, config); break;
            case TipoFoto.PatioNocturno:PintarPatioNocturno(tex, config); break;
        }

        AplicarVignetteTextura(tex, 0.4f);
        AplicarRuidoFotografico(tex, 0.015f);
        tex.Apply(true);
        return tex;
    }

    // --- Pasillo institucional ---
    static void PintarPasillo(Texture2D tex, ConfigFoto cfg)
    {
        int s = tex.width;
        for (int y = 0; y < s; y++)
        {
            float ny = (float)y / s;
            for (int x = 0; x < s; x++)
            {
                float nx = (float)x / s;
                Color c;

                if (ny < 0.35f)
                {
                    // Piso con baldosas
                    float tile = Mathf.Abs(Mathf.Sin(nx * 20f)) * Mathf.Abs(Mathf.Sin(ny * 30f));
                    c = Color.Lerp(cfg.colorDominante * 0.6f, cfg.colorDominante * 0.9f, tile);
                    float perspectiva = 1f - ny / 0.35f;
                    c *= (0.5f + perspectiva * 0.5f);
                }
                else if (ny < 0.85f)
                {
                    // Paredes
                    float manchas = Mathf.PerlinNoise(nx * 5f, ny * 8f);
                    c = Color.Lerp(cfg.colorSecundario * 0.7f, cfg.colorSecundario, manchas);
                    float centroX = Mathf.Abs(nx - 0.5f);
                    c *= (0.7f + (1f - centroX) * 0.3f);
                }
                else
                {
                    // Techo con luces fluorescentes
                    c = cfg.colorDominante * 0.4f;
                    bool esLuz = Mathf.Abs(nx - 0.5f) < 0.04f && ((int)(ny * 40f) % 6 < 3);
                    if (esLuz) c = new Color(0.9f, 0.85f, 0.7f);
                }

                tex.SetPixel(x, y, c);
            }
        }
    }

    // --- Fachada al atardecer ---
    static void PintarFachada(Texture2D tex, ConfigFoto cfg)
    {
        int s = tex.width;
        for (int y = 0; y < s; y++)
        {
            float ny = (float)y / s;
            for (int x = 0; x < s; x++)
            {
                float nx = (float)x / s;
                Color c;

                if (ny > 0.65f)
                {
                    // Cielo atardecer
                    float grad = (ny - 0.65f) / 0.35f;
                    Color cieloAbajo = new Color(0.7f, 0.35f, 0.15f);
                    Color cieloArriba = new Color(0.15f, 0.1f, 0.25f);
                    c = Color.Lerp(cieloAbajo, cieloArriba, grad);
                    float nubes = Mathf.PerlinNoise(nx * 4f + 0.5f, ny * 2f);
                    if (nubes > 0.55f) c += new Color(0.1f, 0.05f, 0.02f) * (nubes - 0.55f) * 3f;
                }
                else if (ny > 0.15f)
                {
                    // Edificio
                    c = cfg.colorDominante;
                    int pisos = (int)((ny - 0.15f) / 0.5f * 4f);
                    bool esVentana = ((int)(nx * 16f) % 3 == 1) && (pisos % 2 == 0 || pisos % 2 == 1);
                    if (esVentana && Mathf.Abs(Mathf.Sin(nx * 50f)) > 0.3f)
                    {
                        float brillo = Random.Range(0.02f, 0.15f);
                        c = new Color(0.4f + brillo, 0.35f + brillo, 0.15f);
                    }
                    c += new Color(1, 1, 1) * Mathf.PerlinNoise(nx * 30f, ny * 30f) * 0.03f;
                }
                else
                {
                    // Acera/suelo
                    c = cfg.colorDominante * 0.5f;
                    c += new Color(1, 1, 1) * Mathf.PerlinNoise(nx * 20f, ny * 20f) * 0.04f;
                }

                tex.SetPixel(x, y, c);
            }
        }
    }

    // --- Laboratorio con monitores ---
    static void PintarLaboratorio(Texture2D tex, ConfigFoto cfg)
    {
        int s = tex.width;
        for (int y = 0; y < s; y++)
        {
            float ny = (float)y / s;
            for (int x = 0; x < s; x++)
            {
                float nx = (float)x / s;
                Color c = cfg.colorDominante;

                if (ny < 0.3f)
                {
                    c = cfg.colorDominante * 0.5f;
                    float tile = Mathf.Abs(Mathf.Sin(nx * 25f) * Mathf.Sin(ny * 25f));
                    c = Color.Lerp(c, c * 1.2f, tile);
                }
                else if (ny < 0.7f)
                {
                    float mesaY = ny - 0.3f;
                    if (mesaY < 0.08f)
                    {
                        c = new Color(0.15f, 0.12f, 0.08f);
                    }
                    else
                    {
                        int monitor = (int)(nx * 6f);
                        float localX = (nx * 6f) - monitor;
                        bool dentroMonitor = localX > 0.15f && localX < 0.85f && mesaY > 0.12f && mesaY < 0.35f;
                        if (dentroMonitor)
                        {
                            float scanline = Mathf.Sin(ny * 200f) * 0.05f;
                            Color pantalla = Color.Lerp(cfg.colorSecundario, new Color(0.1f, 0.3f, 0.5f),
                                Mathf.PerlinNoise(nx * 3f, ny * 5f));
                            c = pantalla + new Color(scanline, scanline, scanline);
                        }
                        else
                        {
                            c = cfg.colorDominante * 0.7f;
                        }
                    }
                }
                else
                {
                    c = cfg.colorDominante * 0.8f;
                    float manchas = Mathf.PerlinNoise(nx * 10f, ny * 10f);
                    c = Color.Lerp(c, c * 0.6f, manchas);
                }

                tex.SetPixel(x, y, c);
            }
        }
    }

    // --- Escaleras oscuras ---
    static void PintarEscaleras(Texture2D tex, ConfigFoto cfg)
    {
        int s = tex.width;
        for (int y = 0; y < s; y++)
        {
            float ny = (float)y / s;
            for (int x = 0; x < s; x++)
            {
                float nx = (float)x / s;

                int escalon = (int)(ny * 12f);
                float localY = (ny * 12f) - escalon;
                bool esHuella = localY > 0.6f;

                Color c;
                if (esHuella)
                {
                    c = cfg.colorDominante * (0.7f + escalon * 0.02f);
                    float desgaste = Mathf.PerlinNoise(nx * 15f, ny * 8f);
                    c = Color.Lerp(c, c * 0.6f, desgaste * 0.3f);
                }
                else
                {
                    c = cfg.colorSecundario * (0.5f + localY * 0.5f);
                }

                float centroX = 1f - Mathf.Abs(nx - 0.5f) * 1.5f;
                float luzTenue = Mathf.Max(0.3f, centroX * (0.4f + ny * 0.6f));
                c *= luzTenue;

                if (ny > 0.7f && Mathf.Abs(nx - 0.5f) < 0.05f)
                    c += new Color(0.15f, 0.12f, 0.05f) * (1f - (ny - 0.7f) / 0.3f);

                tex.SetPixel(x, y, c);
            }
        }
    }

    // --- Sala de servidores ---
    static void PintarServidores(Texture2D tex, ConfigFoto cfg)
    {
        int s = tex.width;
        for (int y = 0; y < s; y++)
        {
            float ny = (float)y / s;
            for (int x = 0; x < s; x++)
            {
                float nx = (float)x / s;
                Color c = cfg.colorDominante;

                int rack = (int)(nx * 5f);
                float localX = (nx * 5f) - rack;
                bool dentroRack = localX > 0.08f && localX < 0.92f;

                if (dentroRack && ny > 0.1f && ny < 0.95f)
                {
                    int slot = (int)((ny - 0.1f) * 14f);
                    float localSlotY = ((ny - 0.1f) * 14f) - slot;

                    if (localSlotY < 0.1f || localSlotY > 0.9f)
                    {
                        c = new Color(0.06f, 0.06f, 0.07f);
                    }
                    else
                    {
                        c = cfg.colorDominante * 1.2f;
                        // LEDs
                        if (localX < 0.2f && localSlotY > 0.4f && localSlotY < 0.6f)
                        {
                            float prob = Mathf.PerlinNoise(rack * 3.7f, slot * 2.3f);
                            if (prob > 0.3f)
                            {
                                c = prob > 0.6f ? new Color(0.9f, 0.08f, 0.03f) : new Color(0.05f, 0.85f, 0.1f);
                                c *= 1.5f;
                            }
                        }
                    }
                }
                else
                {
                    c = cfg.colorDominante * 0.5f;
                }

                tex.SetPixel(x, y, c);
            }
        }
    }

    // --- Patio nocturno con farola ---
    static void PintarPatioNocturno(Texture2D tex, ConfigFoto cfg)
    {
        int s = tex.width;
        Vector2 farola = new Vector2(0.35f, 0.55f);

        for (int y = 0; y < s; y++)
        {
            float ny = (float)y / s;
            for (int x = 0; x < s; x++)
            {
                float nx = (float)x / s;
                Color c;

                if (ny > 0.55f)
                {
                    // Cielo nocturno
                    float grad = (ny - 0.55f) / 0.45f;
                    c = Color.Lerp(new Color(0.02f, 0.03f, 0.06f), new Color(0.005f, 0.008f, 0.025f), grad);
                    // Estrellas
                    float estrella = Mathf.PerlinNoise(nx * 80f, ny * 80f);
                    if (estrella > 0.92f)
                        c += new Color(0.5f, 0.5f, 0.4f) * (estrella - 0.92f) * 10f;
                }
                else
                {
                    // Suelo patio
                    c = cfg.colorDominante * 0.8f;
                    float detalle = Mathf.PerlinNoise(nx * 25f, ny * 25f);
                    c = Color.Lerp(c, c * 0.7f, detalle * 0.4f);
                }

                // Luz de farola
                float distFarola = Vector2.Distance(new Vector2(nx, ny), farola);
                float luzFarola = Mathf.Max(0f, 1f - distFarola * 3f);
                luzFarola = luzFarola * luzFarola;
                c += new Color(cfg.colorSecundario.r, cfg.colorSecundario.g, cfg.colorSecundario.b) * luzFarola * 0.6f;

                // Poste de farola
                if (Mathf.Abs(nx - farola.x) < 0.008f && ny < farola.y && ny > 0.1f)
                    c = new Color(0.05f, 0.05f, 0.05f);
                // Foco
                if (distFarola < 0.02f && ny > farola.y - 0.02f)
                    c = cfg.colorSecundario * 2f;

                tex.SetPixel(x, y, c);
            }
        }
    }

    // --- Efectos de post-foto ---
    static void AplicarVignetteTextura(Texture2D tex, float fuerza)
    {
        int s = tex.width;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float nx = (float)x / s * 2f - 1f;
                float ny = (float)y / s * 2f - 1f;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);
                float vig = 1f - Mathf.Clamp01((dist - 0.5f) * fuerza * 2f);
                Color c = tex.GetPixel(x, y);
                c.r *= vig; c.g *= vig; c.b *= vig;
                tex.SetPixel(x, y, c);
            }
        }
    }

    static void AplicarRuidoFotografico(Texture2D tex, float intensidad)
    {
        int s = tex.width;
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                Color c = tex.GetPixel(x, y);
                float ruido = Random.Range(-intensidad, intensidad);
                c.r += ruido; c.g += ruido; c.b += ruido;
                tex.SetPixel(x, y, c);
            }
        }
    }

    // =====================================================
    //  MATERIALES URP CON SHADER PORTAL
    // =====================================================

    static void CrearMaterialesCuadros()
    {
        Shader shaderPortal = Shader.Find(SHADER_PORTAL);
        Shader shaderFallback = Shader.Find(SHADER_URP_LIT);
        bool usarShaderCustom = shaderPortal != null;

        if (!usarShaderCustom)
            Debug.LogWarning("[CuadrosFoto] Shader ISTR/PortalFotoMezcla no compilado aun. Usando URP/Lit como fallback. Re-ejecutar despues de compilar.");

        for (int i = 0; i < FOTOS.Length; i++)
        {
            string matNombre = "Mat_Cuadro_" + (i + 1) + "_ISTR";
            string matRuta = RUTA_MATERIALES + "/" + matNombre + ".mat";

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matRuta);
            if (mat == null)
            {
                mat = new Material(usarShaderCustom ? shaderPortal : shaderFallback);
                mat.name = matNombre;
                AssetDatabase.CreateAsset(mat, matRuta);
            }
            else
            {
                mat.shader = usarShaderCustom ? shaderPortal : shaderFallback;
            }

            Texture2D foto = AssetDatabase.LoadAssetAtPath<Texture2D>(
                RUTA_FOTOS + "/" + FOTOS[i].nombre + ".png");

            if (usarShaderCustom)
            {
                mat.SetTexture("_FotoReal", foto);
                mat.SetFloat("_Mezcla", 0f);
                mat.SetFloat("_BrilloEmision", 0.5f);
                mat.SetColor("_TinteFoto", Color.white);
                mat.SetFloat("_VignetteIntensidad", 0.3f);
                mat.SetFloat("_Smoothness", 0.85f);
                mat.SetFloat("_Metallic", 0f);
                mat.SetFloat("_BumpScale", 0.3f);
            }
            else
            {
                mat.SetTexture("_BaseMap", foto);
                mat.SetColor("_BaseColor", Color.white);
                mat.SetFloat("_Smoothness", 0.85f);
                mat.SetFloat("_Metallic", 0f);
                mat.EnableKeyword("_EMISSION");
                mat.SetTexture("_EmissionMap", foto);
                mat.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f));
            }

            EditorUtility.SetDirty(mat);
        }

        Debug.Log("[CuadrosFoto] 6 materiales de cuadro creados");
    }

    // =====================================================
    //  ASIGNACION A CUADROS EN ESCENA
    // =====================================================

    static void AsignarACuadrosEnEscena()
    {
        int asignados = 0;
        for (int i = 1; i <= 6; i++)
        {
            string nombreCuadro = "Cuadro_ISTR_" + i;
            GameObject cuadro = GameObject.Find(nombreCuadro);
            if (cuadro == null)
            {
                Debug.LogWarning("[CuadrosFoto] No encontrado en escena: " + nombreCuadro);
                continue;
            }

            // Buscar el Interior_N (lienzo)
            Transform lienzo = cuadro.transform.Find("Interior_" + i);
            if (lienzo == null)
            {
                // Fallback: buscar cualquier hijo con "Interior"
                foreach (Transform child in cuadro.transform)
                {
                    if (child.name.StartsWith("Interior"))
                    {
                        lienzo = child;
                        break;
                    }
                }
            }

            if (lienzo == null)
            {
                Debug.LogWarning("[CuadrosFoto] No se encontro lienzo Interior en " + nombreCuadro);
                continue;
            }

            Renderer rend = lienzo.GetComponent<Renderer>();
            if (rend == null)
            {
                rend = lienzo.GetComponentInChildren<Renderer>();
            }

            if (rend == null)
            {
                Debug.LogWarning("[CuadrosFoto] Sin Renderer en lienzo de " + nombreCuadro);
                continue;
            }

            string matRuta = RUTA_MATERIALES + "/Mat_Cuadro_" + i + "_ISTR.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matRuta);
            if (mat != null)
            {
                rend.sharedMaterial = mat;
                asignados++;
            }
        }

        Debug.Log($"[CuadrosFoto] Materiales asignados a {asignados} lienzos");
    }

    // =====================================================
    //  CONFIGURAR PORTALES: MEZCLA FOTO + CAMARA
    // =====================================================

    static void ConfigurarPortalesEnCuadros()
    {
        int configurados = 0;
        for (int i = 1; i <= 6; i++)
        {
            string nombreCuadro = "Cuadro_ISTR_" + i;
            GameObject cuadro = GameObject.Find(nombreCuadro);
            if (cuadro == null) continue;

            PortalCamaraCuadro portal = cuadro.GetComponent<PortalCamaraCuadro>();
            if (portal == null)
            {
                portal = cuadro.AddComponent<PortalCamaraCuadro>();
                Undo.RegisterCreatedObjectUndo(cuadro, "Agregar portal a " + nombreCuadro);
            }

            // Buscar el renderer del lienzo
            Transform lienzo = cuadro.transform.Find("Interior_" + i);
            if (lienzo == null)
            {
                foreach (Transform child in cuadro.transform)
                {
                    if (child.name.StartsWith("Interior"))
                    {
                        lienzo = child;
                        break;
                    }
                }
            }

            if (lienzo != null)
            {
                Renderer rend = lienzo.GetComponent<Renderer>();
                if (rend == null) rend = lienzo.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    portal.lienzoCuadro = rend;
                }
            }

            // Asignar modo segun posicion del cuadro
            switch (i)
            {
                case 1: case 2:
                    portal.modo = PortalCamaraCuadro.ModoPortal.VigilarEspalda;
                    portal.distanciaActivacion = 6f;
                    break;
                case 3: case 4:
                    portal.modo = PortalCamaraCuadro.ModoPortal.ParadojaSubirBajar;
                    portal.distanciaActivacion = 7f;
                    break;
                case 5: case 6:
                    portal.modo = PortalCamaraCuadro.ModoPortal.VentanaOtroPiso;
                    portal.distanciaActivacion = 8f;
                    break;
            }

            portal.resolucion = 512;
            portal.fpsObjetivo = 24;
            portal.distanciaRenderMaxima = 12f;

            EditorUtility.SetDirty(cuadro);
            configurados++;
        }

        Debug.Log($"[CuadrosFoto] {configurados} portales configurados en cuadros");
    }
}
#endif
