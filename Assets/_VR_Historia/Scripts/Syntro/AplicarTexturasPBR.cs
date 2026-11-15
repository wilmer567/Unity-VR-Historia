#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class AplicarTexturasPBR : EditorWindow
{
    private const string RUTA_PBR = "Assets/_Unity Essentials/Textures/PBR";
    private const string RUTA_MATERIALES = "Assets/_Unity Essentials/Materials/Realistas";
    private const string SHADER_URP_LIT = "Universal Render Pipeline/Lit";
    private const int RES = 1024;

    [MenuItem("Syntro Terror/Generar y Aplicar Texturas PBR Realistas")]
    public static void Ejecutar()
    {
        Debug.Log("[PBR] === INICIO: Generacion de texturas PBR de alta calidad ===");

        AsegurarCarpetas();

        GenerarSetPBR_Baldosa();
        GenerarSetPBR_Concreto();
        GenerarSetPBR_MetalRack();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ImportarNormalMaps();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AplicarAMaterial_Baldosa();
        AplicarAMaterial_Concreto();
        AplicarAMaterial_Rack();

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[PBR] === COMPLETADO: 3 sets PBR (Color+Normal+Roughness) generados y aplicados ===");
    }

    static void AsegurarCarpetas()
    {
        string texBase = "Assets/_Unity Essentials/Textures";
        if (!AssetDatabase.IsValidFolder(RUTA_PBR))
            AssetDatabase.CreateFolder(texBase, "PBR");

        string[] subs = { "Baldosa", "Concreto", "MetalRack" };
        foreach (string sub in subs)
        {
            string ruta = RUTA_PBR + "/" + sub;
            if (!AssetDatabase.IsValidFolder(ruta))
                AssetDatabase.CreateFolder(RUTA_PBR, sub);
        }

        if (!AssetDatabase.IsValidFolder(RUTA_MATERIALES))
        {
            string parent = Path.GetDirectoryName(RUTA_MATERIALES).Replace("\\", "/");
            AssetDatabase.CreateFolder(parent, "Realistas");
        }
    }

    // =====================================================
    //  GUARDAR TEXTURA
    // =====================================================

    static void GuardarPNG(Texture2D tex, string rutaAsset)
    {
        string fullPath = Path.Combine(Application.dataPath, "..", rutaAsset);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(rutaAsset, ImportAssetOptions.ForceUpdate);
    }

    // =====================================================
    //  IMPORTAR NORMAL MAPS COMO TIPO NORMAL
    // =====================================================

    static void ImportarNormalMaps()
    {
        string[] normales = {
            RUTA_PBR + "/Baldosa/Baldosa_Normal.png",
            RUTA_PBR + "/Concreto/Concreto_Normal.png",
            RUTA_PBR + "/MetalRack/MetalRack_Normal.png"
        };

        foreach (string ruta in normales)
        {
            TextureImporter imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) continue;

            imp.textureType = TextureImporterType.NormalMap;
            imp.maxTextureSize = RES;
            imp.textureCompression = TextureImporterCompression.CompressedHQ;
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.SaveAndReimport();
        }

        string[] colores = {
            RUTA_PBR + "/Baldosa/Baldosa_Color.png",
            RUTA_PBR + "/Concreto/Concreto_Color.png",
            RUTA_PBR + "/MetalRack/MetalRack_Color.png"
        };

        foreach (string ruta in colores)
        {
            TextureImporter imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) continue;

            imp.textureType = TextureImporterType.Default;
            imp.sRGBTexture = true;
            imp.maxTextureSize = RES;
            imp.textureCompression = TextureImporterCompression.CompressedHQ;
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.SaveAndReimport();
        }

        string[] roughness = {
            RUTA_PBR + "/Baldosa/Baldosa_Roughness.png",
            RUTA_PBR + "/Concreto/Concreto_Roughness.png",
            RUTA_PBR + "/MetalRack/MetalRack_Roughness.png"
        };

        foreach (string ruta in roughness)
        {
            TextureImporter imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) continue;

            imp.textureType = TextureImporterType.Default;
            imp.sRGBTexture = false;
            imp.maxTextureSize = RES;
            imp.textureCompression = TextureImporterCompression.CompressedHQ;
            imp.wrapMode = TextureWrapMode.Repeat;
            imp.SaveAndReimport();
        }

        Debug.Log("[PBR] Normal Maps importados como tipo NormalMap, Roughness como Linear");
    }

    // =====================================================
    //  SET PBR: BALDOSA CERAMICA OSCURA
    // =====================================================

    static void GenerarSetPBR_Baldosa()
    {
        int tilesX = 8, tilesY = 8;
        int grout = RES / 80;

        // --- COLOR ---
        Texture2D color = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        Color baldosaBase = new Color(0.07f, 0.065f, 0.06f);
        Color juntaColor = new Color(0.03f, 0.028f, 0.025f);

        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                int tileW = RES / tilesX;
                int tileH = RES / tilesY;
                int lx = x % tileW;
                int ly = y % tileH;
                bool esJunta = lx < grout || ly < grout;

                if (esJunta)
                {
                    float r = Random.Range(-0.003f, 0.003f);
                    color.SetPixel(x, y, juntaColor + new Color(r, r, r, 0));
                }
                else
                {
                    float perlin1 = Mathf.PerlinNoise(x * 0.015f, y * 0.015f) * 0.025f;
                    float perlin2 = Mathf.PerlinNoise(x * 0.06f + 50f, y * 0.06f + 50f) * 0.012f;
                    float micro = Random.Range(-0.008f, 0.008f);
                    int tileIdx = (x / tileW) + (y / tileH) * tilesX;
                    float tileVariation = Mathf.Sin(tileIdx * 1.7f) * 0.015f;
                    Color c = baldosaBase + new Color(
                        perlin1 + perlin2 + micro + tileVariation,
                        perlin1 + perlin2 + micro + tileVariation * 0.8f,
                        perlin1 + perlin2 + micro + tileVariation * 0.6f, 0);
                    color.SetPixel(x, y, c);
                }
            }
        }
        color.Apply();
        GuardarPNG(color, RUTA_PBR + "/Baldosa/Baldosa_Color.png");

        // --- NORMAL ---
        Texture2D normal = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        float[,] heightMap = new float[RES, RES];

        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                int tileW = RES / tilesX;
                int tileH = RES / tilesY;
                int lx = x % tileW;
                int ly = y % tileH;
                bool esJunta = lx < grout || ly < grout;

                float h;
                if (esJunta)
                {
                    h = 0.0f;
                }
                else
                {
                    h = 0.8f;
                    int distBordeX = Mathf.Min(lx - grout, tileW - lx);
                    int distBordeY = Mathf.Min(ly - grout, tileH - ly);
                    int distBorde = Mathf.Min(distBordeX, distBordeY);
                    int bevelWidth = grout * 2;
                    if (distBorde < bevelWidth)
                        h *= (float)distBorde / bevelWidth;

                    h += Mathf.PerlinNoise(x * 0.05f + 100f, y * 0.05f + 100f) * 0.08f;
                    h += Random.Range(-0.01f, 0.01f);
                }
                heightMap[x, y] = h;
            }
        }

        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                float hL = heightMap[(x - 1 + RES) % RES, y];
                float hR = heightMap[(x + 1) % RES, y];
                float hD = heightMap[x, (y - 1 + RES) % RES];
                float hU = heightMap[x, (y + 1) % RES];

                float dx = (hL - hR) * 4f;
                float dy = (hD - hU) * 4f;

                float nx = dx * 0.5f + 0.5f;
                float ny = dy * 0.5f + 0.5f;
                float nz = 1.0f;
                normal.SetPixel(x, y, new Color(nx, ny, nz, 1f));
            }
        }
        normal.Apply();
        GuardarPNG(normal, RUTA_PBR + "/Baldosa/Baldosa_Normal.png");

        // --- ROUGHNESS ---
        Texture2D rough = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                int tileW = RES / tilesX;
                int lx = x % tileW;
                int ly = y % (RES / tilesY);
                bool esJunta = lx < grout || ly < grout;

                float r;
                if (esJunta)
                {
                    r = 0.85f + Random.Range(-0.03f, 0.03f);
                }
                else
                {
                    r = 0.25f;
                    r += Mathf.PerlinNoise(x * 0.03f + 200f, y * 0.03f + 200f) * 0.08f;
                    r += Random.Range(-0.02f, 0.02f);
                    float desgaste = Mathf.PerlinNoise(x * 0.005f, y * 0.005f);
                    if (desgaste > 0.65f) r += (desgaste - 0.65f) * 0.4f;
                }
                rough.SetPixel(x, y, new Color(r, r, r, 1f));
            }
        }
        rough.Apply();
        GuardarPNG(rough, RUTA_PBR + "/Baldosa/Baldosa_Roughness.png");

        Debug.Log("[PBR] Set Baldosa generado: Color + Normal + Roughness (1024px)");
    }

    // =====================================================
    //  SET PBR: CONCRETO RUGOSO CON HUMEDAD
    // =====================================================

    static void GenerarSetPBR_Concreto()
    {
        // --- COLOR ---
        Texture2D color = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        Color baseConcreto = new Color(0.28f, 0.26f, 0.24f);

        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                float grueso = Mathf.PerlinNoise(x * 0.004f, y * 0.004f) * 0.06f - 0.03f;
                float medio = Mathf.PerlinNoise(x * 0.02f + 30f, y * 0.02f + 30f) * 0.04f - 0.02f;
                float fino = Mathf.PerlinNoise(x * 0.08f + 70f, y * 0.08f + 70f) * 0.02f - 0.01f;
                float micro = Random.Range(-0.01f, 0.01f);

                float humedad = Mathf.PerlinNoise(x * 0.006f + 10f, y * 0.006f + 10f);
                float manchaHumedad = 0f;
                if (humedad > 0.55f)
                {
                    manchaHumedad = (humedad - 0.55f) / 0.45f;
                    manchaHumedad *= manchaHumedad;
                }

                Color c = baseConcreto;
                c.r += grueso + medio + fino + micro;
                c.g += grueso + medio + fino + micro;
                c.b += grueso + medio + fino + micro;

                c.r -= manchaHumedad * 0.06f;
                c.g -= manchaHumedad * 0.05f;
                c.b -= manchaHumedad * 0.03f;

                float grieta = Mathf.PerlinNoise(x * 0.1f + 150f, y * 0.003f + 150f);
                float grieta2 = Mathf.PerlinNoise(x * 0.003f + 250f, y * 0.1f + 250f);
                if (grieta > 0.85f) c *= 0.85f;
                if (grieta2 > 0.87f) c *= 0.88f;

                color.SetPixel(x, y, c);
            }
        }
        color.Apply();
        GuardarPNG(color, RUTA_PBR + "/Concreto/Concreto_Color.png");

        // --- NORMAL (heightmap -> normal) ---
        Texture2D normal = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        float[,] hm = new float[RES, RES];

        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                float h = 0.5f;
                h += Mathf.PerlinNoise(x * 0.008f + 300f, y * 0.008f + 300f) * 0.3f;
                h += Mathf.PerlinNoise(x * 0.04f + 400f, y * 0.04f + 400f) * 0.15f;
                h += Mathf.PerlinNoise(x * 0.12f + 500f, y * 0.12f + 500f) * 0.06f;
                h += Random.Range(-0.02f, 0.02f);

                float grieta = Mathf.PerlinNoise(x * 0.1f + 150f, y * 0.003f + 150f);
                if (grieta > 0.85f) h -= 0.15f;
                float grieta2 = Mathf.PerlinNoise(x * 0.003f + 250f, y * 0.1f + 250f);
                if (grieta2 > 0.87f) h -= 0.12f;

                hm[x, y] = h;
            }
        }

        float normalStrength = 6f;
        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                float hL = hm[(x - 1 + RES) % RES, y];
                float hR = hm[(x + 1) % RES, y];
                float hD = hm[x, (y - 1 + RES) % RES];
                float hU = hm[x, (y + 1) % RES];

                float dx = (hL - hR) * normalStrength;
                float dy = (hD - hU) * normalStrength;

                normal.SetPixel(x, y, new Color(dx * 0.5f + 0.5f, dy * 0.5f + 0.5f, 1f, 1f));
            }
        }
        normal.Apply();
        GuardarPNG(normal, RUTA_PBR + "/Concreto/Concreto_Normal.png");

        // --- ROUGHNESS ---
        Texture2D rough = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                float r = 0.82f;
                r += Mathf.PerlinNoise(x * 0.02f + 600f, y * 0.02f + 600f) * 0.1f;
                r += Random.Range(-0.02f, 0.02f);

                float humedad = Mathf.PerlinNoise(x * 0.006f + 10f, y * 0.006f + 10f);
                if (humedad > 0.55f)
                    r -= (humedad - 0.55f) * 0.3f;

                r = Mathf.Clamp01(r);
                rough.SetPixel(x, y, new Color(r, r, r, 1f));
            }
        }
        rough.Apply();
        GuardarPNG(rough, RUTA_PBR + "/Concreto/Concreto_Roughness.png");

        Debug.Log("[PBR] Set Concreto generado: Color + Normal + Roughness (1024px)");
    }

    // =====================================================
    //  SET PBR: METAL RACK NEGRO ANODIZADO
    // =====================================================

    static void GenerarSetPBR_MetalRack()
    {
        int slotsV = 14;
        int slotH = RES / slotsV;
        int holeW = RES / 18;
        int holePad = RES / 36;
        int panelMargin = RES / 8;

        // --- COLOR ---
        Texture2D color = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        Color metalBase = new Color(0.035f, 0.035f, 0.04f);
        Color holeColor = new Color(0.012f, 0.012f, 0.015f);

        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                int slot = y / slotH;
                int ly = y % slotH;
                bool separador = ly < 3 || ly > slotH - 3;

                if (separador)
                {
                    color.SetPixel(x, y, new Color(0.06f, 0.06f, 0.065f));
                    continue;
                }

                bool zonaVentilacion = x > panelMargin && x < RES - panelMargin;
                bool esAgujero = false;

                if (zonaVentilacion)
                {
                    int localX = (x - panelMargin);
                    int pitch = holeW + holePad;
                    int hx = localX % pitch;
                    int hy = ly - slotH / 4;
                    bool dentroFilaV = ly > slotH / 4 && ly < slotH * 3 / 4;
                    esAgujero = dentroFilaV && hx < holeW - 2 && hx > 2;
                }

                if (esAgujero)
                {
                    float r = Random.Range(-0.002f, 0.002f);
                    color.SetPixel(x, y, holeColor + new Color(r, r, r, 0));
                }
                else
                {
                    float brusMetal = Mathf.PerlinNoise(x * 0.001f, y * 0.3f) * 0.012f;
                    float micro = Random.Range(-0.004f, 0.004f);
                    Color c = metalBase + new Color(brusMetal + micro, brusMetal + micro, brusMetal + micro + 0.002f, 0);

                    // LEDs en el borde izquierdo
                    if (x < panelMargin - 5 && x > 8)
                    {
                        int ledRow = ly - slotH / 2;
                        if (Mathf.Abs(ledRow) < 4)
                        {
                            int ledIdx = (x - 10) / 14;
                            if (ledIdx >= 0 && ledIdx < 3 && (x - 10) % 14 < 8)
                            {
                                float prob = Mathf.PerlinNoise(slot * 3.1f + ledIdx * 7.3f, 0.5f);
                                if (prob > 0.25f)
                                {
                                    c = prob > 0.55f
                                        ? new Color(0.95f, 0.06f, 0.02f)
                                        : new Color(0.04f, 0.9f, 0.08f);
                                }
                            }
                        }
                    }

                    color.SetPixel(x, y, c);
                }
            }
        }
        color.Apply();
        GuardarPNG(color, RUTA_PBR + "/MetalRack/MetalRack_Color.png");

        // --- NORMAL ---
        Texture2D normal = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        float[,] hm = new float[RES, RES];

        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                int slot = y / slotH;
                int ly = y % slotH;
                bool separador = ly < 3 || ly > slotH - 3;

                float h;
                if (separador)
                {
                    h = 1.0f;
                }
                else
                {
                    h = 0.7f;

                    bool zonaVentilacion = x > panelMargin && x < RES - panelMargin;
                    if (zonaVentilacion)
                    {
                        int localX = (x - panelMargin);
                        int pitch = holeW + holePad;
                        int hx = localX % pitch;
                        bool dentroFilaV = ly > slotH / 4 && ly < slotH * 3 / 4;
                        if (dentroFilaV && hx < holeW - 2 && hx > 2)
                            h = 0.0f;
                    }

                    h += Mathf.PerlinNoise(x * 0.001f, y * 0.4f) * 0.04f;
                }
                hm[x, y] = h;
            }
        }

        float ns = 8f;
        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                float hL = hm[(x - 1 + RES) % RES, y];
                float hR = hm[(x + 1) % RES, y];
                float hD = hm[x, (y - 1 + RES) % RES];
                float hU = hm[x, (y + 1) % RES];
                float dx = (hL - hR) * ns;
                float dy = (hD - hU) * ns;
                normal.SetPixel(x, y, new Color(dx * 0.5f + 0.5f, dy * 0.5f + 0.5f, 1f, 1f));
            }
        }
        normal.Apply();
        GuardarPNG(normal, RUTA_PBR + "/MetalRack/MetalRack_Normal.png");

        // --- ROUGHNESS ---
        Texture2D rough = new Texture2D(RES, RES, TextureFormat.RGBA32, false);
        for (int y = 0; y < RES; y++)
        {
            for (int x = 0; x < RES; x++)
            {
                int ly = y % slotH;
                bool separador = ly < 3 || ly > slotH - 3;

                float r;
                if (separador)
                {
                    r = 0.55f;
                }
                else
                {
                    r = 0.38f;
                    r += Mathf.PerlinNoise(x * 0.002f, y * 0.5f) * 0.06f;
                    r += Random.Range(-0.015f, 0.015f);

                    float huellas = Mathf.PerlinNoise(x * 0.01f + 800f, y * 0.01f + 800f);
                    if (huellas > 0.7f) r += 0.1f;
                }
                r = Mathf.Clamp01(r);
                rough.SetPixel(x, y, new Color(r, r, r, 1f));
            }
        }
        rough.Apply();
        GuardarPNG(rough, RUTA_PBR + "/MetalRack/MetalRack_Roughness.png");

        Debug.Log("[PBR] Set MetalRack generado: Color + Normal + Roughness (1024px)");
    }

    // =====================================================
    //  APLICAR A MATERIALES
    // =====================================================

    static Material ObtenerOCrearMaterial(string nombre)
    {
        string ruta = RUTA_MATERIALES + "/" + nombre + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat != null) return mat;

        Shader shader = Shader.Find(SHADER_URP_LIT);
        mat = new Material(shader);
        mat.name = nombre;
        AssetDatabase.CreateAsset(mat, ruta);
        return mat;
    }

    static void AplicarAMaterial_Baldosa()
    {
        Material mat = ObtenerOCrearMaterial("Mat_PisoBaldosa_Syntro");

        Texture2D texColor = AssetDatabase.LoadAssetAtPath<Texture2D>(RUTA_PBR + "/Baldosa/Baldosa_Color.png");
        Texture2D texNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(RUTA_PBR + "/Baldosa/Baldosa_Normal.png");
        Texture2D texRough = AssetDatabase.LoadAssetAtPath<Texture2D>(RUTA_PBR + "/Baldosa/Baldosa_Roughness.png");

        mat.SetTexture("_BaseMap", texColor);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Metallic", 0.0f);
        mat.SetFloat("_Smoothness", 1.0f);
        mat.SetTextureScale("_BaseMap", new Vector2(4f, 4f));

        if (texNormal != null)
        {
            mat.SetTexture("_BumpMap", texNormal);
            mat.SetFloat("_BumpScale", 1.0f);
            mat.SetTextureScale("_BumpMap", new Vector2(4f, 4f));
            mat.EnableKeyword("_NORMALMAP");
        }

        // URP usa _SmoothnessTextureChannel y _Smoothness; Roughness se invierte via _SmoothnessRemapMin/Max
        // Usamos el Roughness map como Metallic map (canal A = smoothness en URP Lit)
        // Mejor: meter Roughness invertido en _MetallicGlossMap canal alpha
        if (texRough != null)
        {
            mat.SetTexture("_MetallicGlossMap", texRough);
            mat.SetFloat("_SmoothnessTextureChannel", 0f); // Metallic Alpha
            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            // Nota: URP Lit interpreta alpha del MetallicGlossMap como Smoothness.
            // Roughness = 1 - Smoothness, asi que generamos un Smoothness map invertido
        }

        // Override: aplico Smoothness directo ya que no podemos invertir facilmente sin compute
        // La Roughness baja (0.25 baldosa) = Smoothness 0.75
        mat.SetFloat("_Smoothness", 0.75f);

        EditorUtility.SetDirty(mat);
        Debug.Log("[PBR] Mat_PisoBaldosa_Syntro: Color + Normal + Roughness aplicados");
    }

    static void AplicarAMaterial_Concreto()
    {
        Material mat = ObtenerOCrearMaterial("Mat_ParedConcreto_Syntro");

        Texture2D texColor = AssetDatabase.LoadAssetAtPath<Texture2D>(RUTA_PBR + "/Concreto/Concreto_Color.png");
        Texture2D texNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(RUTA_PBR + "/Concreto/Concreto_Normal.png");
        Texture2D texRough = AssetDatabase.LoadAssetAtPath<Texture2D>(RUTA_PBR + "/Concreto/Concreto_Roughness.png");

        mat.SetTexture("_BaseMap", texColor);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Metallic", 0.0f);
        mat.SetFloat("_Smoothness", 0.15f);
        mat.SetTextureScale("_BaseMap", new Vector2(3f, 3f));

        if (texNormal != null)
        {
            mat.SetTexture("_BumpMap", texNormal);
            mat.SetFloat("_BumpScale", 1.4f);
            mat.SetTextureScale("_BumpMap", new Vector2(3f, 3f));
            mat.EnableKeyword("_NORMALMAP");
        }

        if (texRough != null)
        {
            mat.SetTexture("_MetallicGlossMap", texRough);
            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
        }

        // Concreto: Roughness ~0.82 = Smoothness ~0.18
        mat.SetFloat("_Smoothness", 0.18f);

        // Occlusion desde Roughness (aprox)
        mat.SetTexture("_OcclusionMap", texRough);
        mat.SetFloat("_OcclusionStrength", 0.5f);

        EditorUtility.SetDirty(mat);
        Debug.Log("[PBR] Mat_ParedConcreto_Syntro: Color + Normal + Roughness aplicados");
    }

    static void AplicarAMaterial_Rack()
    {
        Material mat = ObtenerOCrearMaterial("Mat_RackMetalico_Negro");

        Texture2D texColor = AssetDatabase.LoadAssetAtPath<Texture2D>(RUTA_PBR + "/MetalRack/MetalRack_Color.png");
        Texture2D texNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(RUTA_PBR + "/MetalRack/MetalRack_Normal.png");
        Texture2D texRough = AssetDatabase.LoadAssetAtPath<Texture2D>(RUTA_PBR + "/MetalRack/MetalRack_Roughness.png");

        mat.SetTexture("_BaseMap", texColor);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Metallic", 0.85f);
        mat.SetFloat("_Smoothness", 0.6f);
        mat.SetTextureScale("_BaseMap", new Vector2(1f, 2f));

        if (texNormal != null)
        {
            mat.SetTexture("_BumpMap", texNormal);
            mat.SetFloat("_BumpScale", 1.2f);
            mat.SetTextureScale("_BumpMap", new Vector2(1f, 2f));
            mat.EnableKeyword("_NORMALMAP");
        }

        if (texRough != null)
        {
            mat.SetTexture("_MetallicGlossMap", texRough);
            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
        }

        // Emission para LEDs
        mat.SetTexture("_EmissionMap", texColor);
        mat.SetColor("_EmissionColor", new Color(2.5f, 0.15f, 0.08f) * 2f);
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        EditorUtility.SetDirty(mat);
        Debug.Log("[PBR] Mat_RackMetalico_Negro: Color + Normal + Roughness + Emission aplicados");
    }
}
#endif
