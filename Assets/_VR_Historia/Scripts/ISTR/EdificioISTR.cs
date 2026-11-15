#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class EdificioISTR : EditorWindow
{
    private const string RUTA_MAT = "Assets/_Unity Essentials/Materials/Realistas";

    static Material matParedExterior, matParedInterior, matPiso, matTecho;
    static Material matPuertaMadera, matPuertaMetal, matMarcoPuerta, matVentanilla;
    static Material matZocalo, matMoldura, matNumero, matPisoSuperior;
    static Material matVidrio, matBarandal, matLuzPasillo;

    [MenuItem("ISTR Terror/Construir Edificio ISTR Completo")]
    public static void Ejecutar()
    {
        Debug.Log("[Edificio] === INICIO: Construccion del edificio ISTR ===");

        AsegurarCarpetas();
        CrearMaterialesEdificio();

        GameObject edificio = GameObject.Find("Edificio_ISTR");
        if (edificio != null) Undo.DestroyObjectImmediate(edificio);

        edificio = new GameObject("Edificio_ISTR");
        edificio.transform.position = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(edificio, "Crear Edificio ISTR");

        ConstruirPiso1(edificio.transform);
        ConstruirPiso3(edificio.transform);
        ConstruirExteriorEdificio(edificio.transform);

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[Edificio] === COMPLETADO: Edificio ISTR con pasillos y puertas decorativas ===");
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
    //  MATERIALES
    // =====================================================

    static void CrearMaterialesEdificio()
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");

        matParedExterior = CrearMat("Mat_ParedExt_ISTR", lit,
            new Color(0.65f, 0.6f, 0.52f), 0f, 0.2f);
        matParedInterior = CrearMat("Mat_ParedInt_ISTR", lit,
            new Color(0.42f, 0.44f, 0.40f), 0f, 0.15f);
        matPiso = CrearMat("Mat_PisoPasillo_ISTR", lit,
            new Color(0.18f, 0.17f, 0.15f), 0f, 0.55f);
        matPisoSuperior = CrearMat("Mat_PisoSuperior_ISTR", lit,
            new Color(0.15f, 0.16f, 0.18f), 0f, 0.5f);
        matTecho = CrearMat("Mat_TechoPasillo_ISTR", lit,
            new Color(0.7f, 0.7f, 0.68f), 0f, 0.25f);
        matPuertaMadera = CrearMat("Mat_PuertaMadera_ISTR", lit,
            new Color(0.22f, 0.13f, 0.07f), 0f, 0.45f);
        matPuertaMetal = CrearMat("Mat_PuertaMetal_ISTR", lit,
            new Color(0.3f, 0.3f, 0.32f), 0.6f, 0.4f);
        matMarcoPuerta = CrearMat("Mat_MarcoPuerta_ISTR", lit,
            new Color(0.35f, 0.33f, 0.30f), 0.1f, 0.3f);
        matVentanilla = CrearMatEmisivo("Mat_Ventanilla_ISTR", lit,
            new Color(0.05f, 0.08f, 0.12f), new Color(0.03f, 0.05f, 0.08f));
        matZocalo = CrearMat("Mat_Zocalo_ISTR", lit,
            new Color(0.12f, 0.11f, 0.10f), 0f, 0.35f);
        matMoldura = CrearMat("Mat_Moldura_ISTR", lit,
            new Color(0.55f, 0.53f, 0.48f), 0f, 0.2f);
        matNumero = CrearMatEmisivo("Mat_NumeroSala_ISTR", lit,
            new Color(0.7f, 0.65f, 0.5f), new Color(0.15f, 0.12f, 0.05f));
        matVidrio = CrearMat("Mat_VidrioPuerta_ISTR", lit,
            new Color(0.1f, 0.12f, 0.15f), 0f, 0.9f);
        matBarandal = CrearMat("Mat_Barandal_ISTR", lit,
            new Color(0.25f, 0.25f, 0.27f), 0.8f, 0.5f);

        matLuzPasillo = CrearMatEmisivo("Mat_LuzPasillo_ISTR", lit,
            new Color(0.9f, 0.85f, 0.7f), new Color(0.8f, 0.7f, 0.45f) * 1.5f);

        Debug.Log("[Edificio] 15 materiales de edificio creados");
    }

    static Material CrearMat(string nombre, Shader shader, Color color, float met, float smooth)
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
        mat.SetFloat("_Metallic", met);
        mat.SetFloat("_Smoothness", smooth);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material CrearMatEmisivo(string nombre, Shader shader, Color color, Color emission)
    {
        Material mat = CrearMat(nombre, shader, color, 0f, 0.3f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emission);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // =====================================================
    //  HELPERS
    // =====================================================

    static GameObject Cubo(string nombre, Transform padre, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.isStatic = true;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        Undo.RegisterCreatedObjectUndo(go, nombre);
        return go;
    }

    static GameObject Contenedor(string nombre, Transform padre, Vector3 pos)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        Undo.RegisterCreatedObjectUndo(go, nombre);
        return go;
    }

    static void CrearPuertaCompleta(string nombre, Transform padre, Vector3 pos,
        float rotY, bool esMetal, bool conVentanilla, string etiqueta)
    {
        GameObject puerta = Contenedor(nombre, padre, pos);
        puerta.transform.localEulerAngles = new Vector3(0f, rotY, 0f);

        float anchoP = 0.9f;
        float altoP = 2.1f;

        // Marco
        Cubo("Marco_Sup", puerta.transform,
            new Vector3(0f, altoP + 0.04f, 0f), new Vector3(anchoP + 0.16f, 0.08f, 0.12f), matMarcoPuerta);
        Cubo("Marco_Izq", puerta.transform,
            new Vector3(-(anchoP / 2f + 0.04f), altoP / 2f, 0f), new Vector3(0.08f, altoP, 0.12f), matMarcoPuerta);
        Cubo("Marco_Der", puerta.transform,
            new Vector3(anchoP / 2f + 0.04f, altoP / 2f, 0f), new Vector3(0.08f, altoP, 0.12f), matMarcoPuerta);

        // Hoja de puerta
        Material matHoja = esMetal ? matPuertaMetal : matPuertaMadera;
        Cubo("Hoja", puerta.transform,
            new Vector3(0f, altoP / 2f, 0f), new Vector3(anchoP, altoP, 0.05f), matHoja);

        // Manija
        Cubo("Manija", puerta.transform,
            new Vector3(0.32f, 1.0f, -0.04f), new Vector3(0.1f, 0.03f, 0.04f),
            esMetal ? matMarcoPuerta : matPuertaMetal);

        // Ventanilla de vidrio
        if (conVentanilla)
        {
            Cubo("Ventanilla", puerta.transform,
                new Vector3(0f, 1.55f, -0.01f), new Vector3(0.3f, 0.3f, 0.01f), matVidrio);
        }

        // Placa con numero/nombre
        Cubo("Placa", puerta.transform,
            new Vector3(anchoP / 2f + 0.09f, 1.7f, 0f), new Vector3(0.15f, 0.08f, 0.01f), matNumero);
    }

    static void CrearLuzPasillo(Transform padre, Vector3 pos)
    {
        Cubo("PanelLuz", padre,
            new Vector3(pos.x, pos.y, pos.z), new Vector3(0.6f, 0.03f, 0.15f), matLuzPasillo);

        GameObject luzGO = new GameObject("Luz_Pasillo");
        luzGO.transform.SetParent(padre);
        luzGO.transform.localPosition = pos - new Vector3(0f, 0.15f, 0f);
        Light luz = luzGO.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = new Color(0.9f, 0.8f, 0.55f);
        luz.intensity = 0.6f;
        luz.range = 4f;
        luz.shadows = LightShadows.None;
        Undo.RegisterCreatedObjectUndo(luzGO, "Luz pasillo");
    }

    // =====================================================
    //  PISO 1: PASILLO CON PUERTAS DECORATIVAS
    // =====================================================

    static void ConstruirPiso1(Transform edificio)
    {
        // Piso1_CuartoMaquinas ocupa X[-5,5] Z[-4,4] Y[0,3.6]
        // Construimos pasillo en Z negativo (detras de la pared de entrada)
        // y alas laterales

        GameObject piso1 = Contenedor("Piso1_Edificio", edificio, Vector3.zero);

        // === PASILLO PRINCIPAL (eje X, detras de entrada) ===
        float pasilloZ = -4f;
        float anchoPasillo = 2.4f;
        float largoPasillo = 20f;
        float altoPared = 3.5f;

        GameObject pasillo1 = Contenedor("Pasillo_P1_Principal", piso1.transform,
            new Vector3(0f, 0f, pasilloZ - anchoPasillo / 2f));

        // Piso del pasillo
        Cubo("Piso_Pasillo_P1", pasillo1.transform,
            new Vector3(0f, -0.05f, 0f),
            new Vector3(largoPasillo, 0.1f, anchoPasillo), matPiso);

        // Techo del pasillo
        Cubo("Techo_Pasillo_P1", pasillo1.transform,
            new Vector3(0f, altoPared + 0.05f, 0f),
            new Vector3(largoPasillo, 0.1f, anchoPasillo), matTecho);

        // Pared sur del pasillo (la lejana)
        Cubo("Pared_Sur_P1", pasillo1.transform,
            new Vector3(0f, altoPared / 2f, -anchoPasillo / 2f),
            new Vector3(largoPasillo, altoPared, 0.15f), matParedInterior);

        // Pared norte es la Pared_Entrada existente, solo extendemos laterales
        // Pared este (cierre)
        Cubo("Pared_Este_P1", pasillo1.transform,
            new Vector3(largoPasillo / 2f, altoPared / 2f, 0f),
            new Vector3(0.15f, altoPared, anchoPasillo), matParedInterior);
        // Pared oeste (cierre)
        Cubo("Pared_Oeste_P1", pasillo1.transform,
            new Vector3(-largoPasillo / 2f, altoPared / 2f, 0f),
            new Vector3(0.15f, altoPared, anchoPasillo), matParedInterior);

        // Zocalo en ambas paredes del pasillo
        Cubo("Zocalo_Norte_P1", pasillo1.transform,
            new Vector3(0f, 0.06f, anchoPasillo / 2f - 0.02f),
            new Vector3(largoPasillo, 0.12f, 0.03f), matZocalo);
        Cubo("Zocalo_Sur_P1", pasillo1.transform,
            new Vector3(0f, 0.06f, -anchoPasillo / 2f + 0.02f),
            new Vector3(largoPasillo, 0.12f, 0.03f), matZocalo);

        // Moldura superior
        Cubo("Moldura_Norte_P1", pasillo1.transform,
            new Vector3(0f, altoPared - 0.05f, anchoPasillo / 2f - 0.02f),
            new Vector3(largoPasillo, 0.06f, 0.04f), matMoldura);
        Cubo("Moldura_Sur_P1", pasillo1.transform,
            new Vector3(0f, altoPared - 0.05f, -anchoPasillo / 2f + 0.02f),
            new Vector3(largoPasillo, 0.06f, 0.04f), matMoldura);

        // --- PUERTAS en pared norte (dan al area del cuarto maquinas, no se abren) ---
        CrearPuertasLadoNorte_P1(pasillo1.transform, anchoPasillo);

        // --- PUERTAS en pared sur (habitaciones fantasma) ---
        CrearPuertasLadoSur_P1(pasillo1.transform, anchoPasillo);

        // --- HABITACIONES FANTASMA detras pared sur (solo fachada) ---
        CrearFachadasHabitaciones_P1(piso1.transform, pasilloZ - anchoPasillo);

        // --- Iluminacion del pasillo ---
        for (int i = -4; i <= 4; i++)
        {
            CrearLuzPasillo(pasillo1.transform,
                new Vector3(i * 2.2f, altoPared - 0.02f, 0f));
        }

        // === ALA LATERAL DERECHA ===
        ConstruirAlaPiso1(piso1.transform, "Ala_Derecha_P1", 5.5f, true);

        // === ALA LATERAL IZQUIERDA ===
        ConstruirAlaPiso1(piso1.transform, "Ala_Izquierda_P1", -5.5f, false);

        Debug.Log("[Edificio] Piso 1 construido: pasillo principal + 2 alas + 16 puertas");
    }

    static void CrearPuertasLadoNorte_P1(Transform pasillo, float anchoPasillo)
    {
        // Puertas en la pared norte del pasillo (junto al cuarto de maquinas)
        string[] nombres = {
            "Almacen_Tecnico", "Sala_UPS_Backup", "Oficina_Jefe_TI",
            "Bodega_Equipos", "Cuarto_Telecomunicaciones"
        };
        float startX = -8f;
        float spacing = 3.2f;

        for (int i = 0; i < nombres.Length; i++)
        {
            float x = startX + i * spacing;
            // Evitar la zona donde esta la puerta real del cuarto de maquinas [-2, 2]
            if (x > -2.5f && x < 2.5f) continue;

            CrearPuertaCompleta(
                "Puerta_" + nombres[i], pasillo,
                new Vector3(x, 0f, anchoPasillo / 2f - 0.07f),
                0f, i % 2 == 0, true, nombres[i]);
        }
    }

    static void CrearPuertasLadoSur_P1(Transform pasillo, float anchoPasillo)
    {
        string[] nombres = {
            "Lab_Electronica", "Aula_101", "Aula_102", "Bano_Hombres",
            "Bano_Mujeres", "Aula_103", "Sala_Profesores", "Secretaria"
        };
        float startX = -8.5f;
        float spacing = 2.5f;

        for (int i = 0; i < nombres.Length; i++)
        {
            float x = startX + i * spacing;
            bool esMetal = nombres[i].Contains("Bano");
            bool conVentanilla = !nombres[i].Contains("Bano");

            CrearPuertaCompleta(
                "Puerta_" + nombres[i], pasillo,
                new Vector3(x, 0f, -anchoPasillo / 2f + 0.07f),
                180f, esMetal, conVentanilla, nombres[i]);
        }
    }

    static void CrearFachadasHabitaciones_P1(Transform piso1, float zBase)
    {
        // Crear paredes que sugieran habitaciones detras de las puertas
        GameObject fachadas = Contenedor("Fachadas_P1", piso1.transform, Vector3.zero);
        float profundidad = 4f;
        float altoPared = 3.5f;

        // Pared exterior trasera
        Cubo("ParedExterior_Sur", fachadas.transform,
            new Vector3(0f, altoPared / 2f, zBase - profundidad),
            new Vector3(22f, altoPared, 0.2f), matParedExterior);

        // Paredes divisorias entre habitaciones (visibles por ventanillas)
        for (int i = 0; i < 9; i++)
        {
            float x = -9f + i * 2.5f;
            Cubo($"Divisoria_P1_{i}", fachadas.transform,
                new Vector3(x, altoPared / 2f, zBase - profundidad / 2f),
                new Vector3(0.12f, altoPared, profundidad), matParedInterior);
        }

        // Piso de habitaciones
        Cubo("Piso_Hab_P1", fachadas.transform,
            new Vector3(0f, -0.05f, zBase - profundidad / 2f),
            new Vector3(22f, 0.1f, profundidad), matPiso);
    }

    static void ConstruirAlaPiso1(Transform piso1, string nombre, float xBase, bool derecha)
    {
        float dir = derecha ? 1f : -1f;
        float largoAla = 10f;
        float anchoPasillo = 2f;
        float altoPared = 3.5f;

        GameObject ala = Contenedor(nombre, piso1.transform, Vector3.zero);

        // Pasillo del ala (eje Z)
        Cubo("Piso_Ala", ala.transform,
            new Vector3(xBase + dir * anchoPasillo / 2f, -0.05f, -4f + largoAla / 2f),
            new Vector3(anchoPasillo, 0.1f, largoAla), matPiso);
        Cubo("Techo_Ala", ala.transform,
            new Vector3(xBase + dir * anchoPasillo / 2f, altoPared + 0.05f, -4f + largoAla / 2f),
            new Vector3(anchoPasillo, 0.1f, largoAla), matTecho);

        // Pared exterior del ala
        Cubo("ParedExt_Ala", ala.transform,
            new Vector3(xBase + dir * anchoPasillo, altoPared / 2f, -4f + largoAla / 2f),
            new Vector3(0.15f, altoPared, largoAla), matParedExterior);

        // Pared interior (contra edificio existente)
        Cubo("ParedInt_Ala", ala.transform,
            new Vector3(xBase, altoPared / 2f, -4f + largoAla / 2f),
            new Vector3(0.15f, altoPared, largoAla), matParedInterior);

        // Pared fondo del ala
        Cubo("ParedFondo_Ala", ala.transform,
            new Vector3(xBase + dir * anchoPasillo / 2f, altoPared / 2f, -4f + largoAla),
            new Vector3(anchoPasillo, altoPared, 0.15f), matParedInterior);

        // Zocalos
        Cubo("Zocalo_Ext_Ala", ala.transform,
            new Vector3(xBase + dir * (anchoPasillo - 0.02f), 0.06f, -4f + largoAla / 2f),
            new Vector3(0.03f, 0.12f, largoAla), matZocalo);

        // Puertas del ala (en la pared exterior)
        string[] nombresAla;
        if (derecha)
            nombresAla = new string[] { "Lab_Fisica", "Lab_Quimica", "Deposito_1", "Archivo" };
        else
            nombresAla = new string[] { "Lab_Computo_2", "Aula_201", "Conserjeria", "Mantenimiento" };

        for (int i = 0; i < nombresAla.Length; i++)
        {
            float z = -3f + i * 2.5f;
            CrearPuertaCompleta(
                "Puerta_" + nombresAla[i], ala.transform,
                new Vector3(xBase + dir * (anchoPasillo - 0.07f), 0f, z),
                derecha ? 90f : -90f,
                i == 2, i != 2, nombresAla[i]);
        }

        // Luces del ala
        for (int i = 0; i < 4; i++)
        {
            float z = -2.5f + i * 2.5f;
            CrearLuzPasillo(ala.transform,
                new Vector3(xBase + dir * anchoPasillo / 2f, altoPared - 0.02f, z));
        }

        Debug.Log($"[Edificio] {nombre} construida con 4 puertas");
    }

    // =====================================================
    //  PISO 3: PASILLO LABORATORIO CON PUERTAS
    // =====================================================

    static void ConstruirPiso3(Transform edificio)
    {
        // Piso3_Laboratorio: pos global (0,8,18), X[-4,4] Z[-1,9] local → global Z[17,27]
        float yBase = 8f;
        float zOffset = 18f;

        GameObject piso3 = Contenedor("Piso3_Edificio", edificio, new Vector3(0f, yBase, zOffset));

        float anchoPasillo = 2.2f;
        float largoPasillo = 18f;
        float altoPared = 3.5f;
        float pasilloZ = -1f - anchoPasillo / 2f;

        GameObject pasillo3 = Contenedor("Pasillo_P3", piso3.transform,
            new Vector3(0f, 0f, pasilloZ));

        // Piso
        Cubo("Piso_Pasillo_P3", pasillo3.transform,
            new Vector3(0f, -0.05f, 0f),
            new Vector3(largoPasillo, 0.1f, anchoPasillo), matPisoSuperior);

        // Techo
        Cubo("Techo_Pasillo_P3", pasillo3.transform,
            new Vector3(0f, altoPared + 0.05f, 0f),
            new Vector3(largoPasillo, 0.1f, anchoPasillo), matTecho);

        // Pared sur
        Cubo("Pared_Sur_P3", pasillo3.transform,
            new Vector3(0f, altoPared / 2f, -anchoPasillo / 2f),
            new Vector3(largoPasillo, altoPared, 0.15f), matParedInterior);

        // Cierres laterales
        Cubo("Cierre_Este_P3", pasillo3.transform,
            new Vector3(largoPasillo / 2f, altoPared / 2f, 0f),
            new Vector3(0.15f, altoPared, anchoPasillo), matParedInterior);
        Cubo("Cierre_Oeste_P3", pasillo3.transform,
            new Vector3(-largoPasillo / 2f, altoPared / 2f, 0f),
            new Vector3(0.15f, altoPared, anchoPasillo), matParedInterior);

        // Zocalos
        Cubo("Zocalo_N_P3", pasillo3.transform,
            new Vector3(0f, 0.06f, anchoPasillo / 2f - 0.02f),
            new Vector3(largoPasillo, 0.12f, 0.03f), matZocalo);
        Cubo("Zocalo_S_P3", pasillo3.transform,
            new Vector3(0f, 0.06f, -anchoPasillo / 2f + 0.02f),
            new Vector3(largoPasillo, 0.12f, 0.03f), matZocalo);

        // Molduras
        Cubo("Moldura_N_P3", pasillo3.transform,
            new Vector3(0f, altoPared - 0.05f, anchoPasillo / 2f - 0.02f),
            new Vector3(largoPasillo, 0.06f, 0.04f), matMoldura);
        Cubo("Moldura_S_P3", pasillo3.transform,
            new Vector3(0f, altoPared - 0.05f, -anchoPasillo / 2f + 0.02f),
            new Vector3(largoPasillo, 0.06f, 0.04f), matMoldura);

        // --- PUERTAS NORTE (hacia laboratorio, decorativas) ---
        string[] puertasNorte = {
            "Lab_Redes", "Lab_Sistemas", "Lab_Investigacion",
            "Deposito_Quimicos", "Sala_Reuniones"
        };
        for (int i = 0; i < puertasNorte.Length; i++)
        {
            float x = -7f + i * 3.2f;
            if (x > -2f && x < 2f) continue;
            CrearPuertaCompleta("Puerta_" + puertasNorte[i], pasillo3.transform,
                new Vector3(x, 0f, anchoPasillo / 2f - 0.07f),
                0f, false, true, puertasNorte[i]);
        }

        // --- PUERTAS SUR (habitaciones fantasma) ---
        string[] puertasSur = {
            "Aula_301", "Aula_302", "Aula_303", "Sala_Docentes_P3",
            "Bano_P3", "Lab_Simulacion", "Coordinacion"
        };
        for (int i = 0; i < puertasSur.Length; i++)
        {
            float x = -7.5f + i * 2.4f;
            bool esMetal = puertasSur[i].Contains("Bano");
            CrearPuertaCompleta("Puerta_" + puertasSur[i], pasillo3.transform,
                new Vector3(x, 0f, -anchoPasillo / 2f + 0.07f),
                180f, esMetal, !esMetal, puertasSur[i]);
        }

        // Fachadas detras pared sur (habitaciones sugeridas)
        CrearFachadasHabitaciones_P3(piso3.transform, pasilloZ - anchoPasillo / 2f, largoPasillo);

        // Luces del pasillo P3
        for (int i = -3; i <= 3; i++)
        {
            CrearLuzPasillo(pasillo3.transform,
                new Vector3(i * 2.5f, altoPared - 0.02f, 0f));
        }

        // Barandal decorativo al inicio del pasillo (junto a gradas)
        GameObject barandal = Contenedor("Barandal_P3", piso3.transform,
            new Vector3(0f, 0f, -1.5f));
        Cubo("Pasamanos", barandal.transform,
            new Vector3(0f, 1.05f, 0f), new Vector3(3f, 0.05f, 0.05f), matBarandal);
        for (int i = 0; i < 6; i++)
        {
            float x = -1.25f + i * 0.5f;
            Cubo($"Balaustre_{i}", barandal.transform,
                new Vector3(x, 0.52f, 0f), new Vector3(0.03f, 1.0f, 0.03f), matBarandal);
        }

        Debug.Log("[Edificio] Piso 3 construido: pasillo + 12 puertas + barandal");
    }

    static void CrearFachadasHabitaciones_P3(Transform piso3, float zBase, float largo)
    {
        GameObject fachadas = Contenedor("Fachadas_P3", piso3.transform, Vector3.zero);
        float profundidad = 4f;
        float altoPared = 3.5f;

        Cubo("ParedExterior_Sur_P3", fachadas.transform,
            new Vector3(0f, altoPared / 2f, zBase - profundidad),
            new Vector3(largo + 2f, altoPared, 0.2f), matParedExterior);

        for (int i = 0; i < 8; i++)
        {
            float x = -8f + i * 2.4f;
            Cubo($"Divisoria_P3_{i}", fachadas.transform,
                new Vector3(x, altoPared / 2f, zBase - profundidad / 2f),
                new Vector3(0.12f, altoPared, profundidad), matParedInterior);
        }

        Cubo("Piso_Hab_P3", fachadas.transform,
            new Vector3(0f, -0.05f, zBase - profundidad / 2f),
            new Vector3(largo + 2f, 0.1f, profundidad), matPisoSuperior);
    }

    // =====================================================
    //  EXTERIOR DEL EDIFICIO (fachada visible)
    // =====================================================

    static void ConstruirExteriorEdificio(Transform edificio)
    {
        GameObject exterior = Contenedor("Exterior_ISTR", edificio, Vector3.zero);

        float altoPiso = 3.7f;
        float largoEdificio = 24f;
        float anchoEdificio = 16f;

        // === PISO 1: Muros exteriores ===
        // Fachada frontal (sur, Z negativo)
        Cubo("Fachada_Frontal_P1", exterior.transform,
            new Vector3(0f, altoPiso / 2f, -10.5f),
            new Vector3(largoEdificio, altoPiso, 0.25f), matParedExterior);

        // Laterales exteriores
        Cubo("Fachada_Este_P1", exterior.transform,
            new Vector3(largoEdificio / 2f, altoPiso / 2f, -3f),
            new Vector3(0.25f, altoPiso, anchoEdificio), matParedExterior);
        Cubo("Fachada_Oeste_P1", exterior.transform,
            new Vector3(-largoEdificio / 2f, altoPiso / 2f, -3f),
            new Vector3(0.25f, altoPiso, anchoEdificio), matParedExterior);

        // === ENTREPISO (losa entre P1 y P3) ===
        Cubo("Losa_Entrepiso", exterior.transform,
            new Vector3(0f, altoPiso, -3f),
            new Vector3(largoEdificio, 0.3f, anchoEdificio), matParedExterior);

        // === PISO 3: Muros exteriores ===
        float y3 = 8f;
        Cubo("Fachada_Frontal_P3", exterior.transform,
            new Vector3(0f, y3 + altoPiso / 2f, 18f - anchoEdificio / 2f - 1f),
            new Vector3(largoEdificio, altoPiso, 0.25f), matParedExterior);

        Cubo("Fachada_Este_P3", exterior.transform,
            new Vector3(largoEdificio / 2f, y3 + altoPiso / 2f, 18f),
            new Vector3(0.25f, altoPiso, anchoEdificio), matParedExterior);
        Cubo("Fachada_Oeste_P3", exterior.transform,
            new Vector3(-largoEdificio / 2f, y3 + altoPiso / 2f, 18f),
            new Vector3(0.25f, altoPiso, anchoEdificio), matParedExterior);

        // Techo final
        Cubo("Techo_Edificio", exterior.transform,
            new Vector3(0f, y3 + altoPiso, 18f),
            new Vector3(largoEdificio + 0.5f, 0.2f, anchoEdificio + 0.5f), matParedExterior);

        // === DETALLES DE FACHADA ===
        // Cornisa/moldura horizontal entre pisos
        Cubo("Cornisa_P1", exterior.transform,
            new Vector3(0f, altoPiso + 0.2f, -10.55f),
            new Vector3(largoEdificio + 0.4f, 0.15f, 0.35f), matMoldura);

        // Ventanas decorativas en fachada frontal P1
        for (int v = 0; v < 8; v++)
        {
            float x = -9f + v * 2.8f;
            Cubo($"Ventana_P1_{v}", exterior.transform,
                new Vector3(x, altoPiso * 0.55f, -10.52f),
                new Vector3(1.2f, 1.0f, 0.03f), matVidrio);

            // Marco de ventana
            Cubo($"MarcoVent_P1_{v}_Sup", exterior.transform,
                new Vector3(x, altoPiso * 0.55f + 0.55f, -10.53f),
                new Vector3(1.3f, 0.06f, 0.04f), matMarcoPuerta);
            Cubo($"MarcoVent_P1_{v}_Inf", exterior.transform,
                new Vector3(x, altoPiso * 0.55f - 0.55f, -10.53f),
                new Vector3(1.3f, 0.06f, 0.04f), matMarcoPuerta);
        }

        Debug.Log("[Edificio] Exterior: fachadas, losa, cornisa, 8 ventanas");
    }
}
#endif
