#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class EdificioSyntro : EditorWindow
{
    private const string RUTA_MAT = "Assets/_Unity Essentials/Materials/Realistas";

    static Material matParedExterior, matParedInterior, matPiso, matTecho;
    static Material matPuertaMadera, matPuertaMetal, matMarcoPuerta, matVentanilla;
    static Material matZocalo, matMoldura, matNumero, matPisoSuperior;
    static Material matVidrio, matBarandal, matLuzPasillo;
    static Material matPisoSotano, matParedSotano, matEscalon;

    [MenuItem("Syntro Terror/Construir Edificio Syntro Completo")]
    public static void Ejecutar()
    {
        Debug.Log("[Edificio] === INICIO: Construccion del edificio Syntro ===");

        AsegurarCarpetas();
        CrearMaterialesEdificio();

        GameObject edificio = GameObject.Find("Edificio_Syntro");
        if (edificio != null) Undo.DestroyObjectImmediate(edificio);

        edificio = new GameObject("Edificio_Syntro");
        edificio.transform.position = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(edificio, "Crear Edificio Syntro");

        ConstruirSotano(edificio.transform);
        ConstruirPiso1(edificio.transform);
        ConstruirPiso2(edificio.transform);
        ConstruirPiso3(edificio.transform);
        ConstruirGradas(edificio.transform);
        ConstruirExteriorEdificio(edificio.transform);

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[Edificio] === COMPLETADO: Edificio con Sotano + P1 + P2 + P3 + Gradas ===");
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

        matParedExterior = CrearMat("Mat_ParedExt_Syntro", lit,
            new Color(0.65f, 0.6f, 0.52f), 0f, 0.2f);
        matParedInterior = CrearMat("Mat_ParedInt_Syntro", lit,
            new Color(0.42f, 0.44f, 0.40f), 0f, 0.15f);
        matPiso = CrearMat("Mat_PisoPasillo_Syntro", lit,
            new Color(0.18f, 0.17f, 0.15f), 0f, 0.55f);
        matPisoSuperior = CrearMat("Mat_PisoSuperior_Syntro", lit,
            new Color(0.15f, 0.16f, 0.18f), 0f, 0.5f);
        matTecho = CrearMat("Mat_TechoPasillo_Syntro", lit,
            new Color(0.7f, 0.7f, 0.68f), 0f, 0.25f);
        matPuertaMadera = CrearMat("Mat_PuertaMadera_Syntro", lit,
            new Color(0.22f, 0.13f, 0.07f), 0f, 0.45f);
        matPuertaMetal = CrearMat("Mat_PuertaMetal_Syntro", lit,
            new Color(0.3f, 0.3f, 0.32f), 0.6f, 0.4f);
        matMarcoPuerta = CrearMat("Mat_MarcoPuerta_Syntro", lit,
            new Color(0.35f, 0.33f, 0.30f), 0.1f, 0.3f);
        matVentanilla = CrearMatEmisivo("Mat_Ventanilla_Syntro", lit,
            new Color(0.05f, 0.08f, 0.12f), new Color(0.03f, 0.05f, 0.08f));
        matZocalo = CrearMat("Mat_Zocalo_Syntro", lit,
            new Color(0.12f, 0.11f, 0.10f), 0f, 0.35f);
        matMoldura = CrearMat("Mat_Moldura_Syntro", lit,
            new Color(0.55f, 0.53f, 0.48f), 0f, 0.2f);
        matNumero = CrearMatEmisivo("Mat_NumeroSala_Syntro", lit,
            new Color(0.7f, 0.65f, 0.5f), new Color(0.15f, 0.12f, 0.05f));
        matVidrio = CrearMat("Mat_VidrioPuerta_Syntro", lit,
            new Color(0.1f, 0.12f, 0.15f), 0f, 0.9f);
        matBarandal = CrearMat("Mat_Barandal_Syntro", lit,
            new Color(0.25f, 0.25f, 0.27f), 0.8f, 0.5f);

        matLuzPasillo = CrearMatEmisivo("Mat_LuzPasillo_Syntro", lit,
            new Color(0.9f, 0.85f, 0.7f), new Color(0.8f, 0.7f, 0.45f) * 1.5f);

        matPisoSotano = CrearMat("Mat_PisoSotano_Syntro", lit,
            new Color(0.1f, 0.1f, 0.11f), 0f, 0.2f);
        matParedSotano = CrearMat("Mat_ParedSotano_Syntro", lit,
            new Color(0.14f, 0.13f, 0.12f), 0f, 0.12f);
        matEscalon = CrearMat("Mat_Escalon_Syntro", lit,
            new Color(0.25f, 0.23f, 0.2f), 0.15f, 0.4f);

        Debug.Log("[Edificio] 18 materiales de edificio creados");
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
        if (col != null) col.isTrigger = false;
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
        pasillo1.AddComponent<CuadrosFlotantesPasillo>();

        // Piso del pasillo
        Cubo("Piso_Pasillo_P1", pasillo1.transform,
            new Vector3(0f, -0.05f, 0f),
            new Vector3(largoPasillo, 0.1f, anchoPasillo), matPiso);

        // Techo del pasillo (dividido para dejar hueco en la caja de escaleras X=[6.5, 9.5])
        float tp1IzqAncho = 6.5f + largoPasillo / 2f;
        float tp1IzqX = (-largoPasillo / 2f + 6.5f) / 2f;
        float tp1DerAncho = largoPasillo / 2f - 9.5f;
        float tp1DerX = (9.5f + largoPasillo / 2f) / 2f;
        Cubo("Techo_Pasillo_P1_Izq", pasillo1.transform,
            new Vector3(tp1IzqX, altoPared + 0.05f, 0f), new Vector3(tp1IzqAncho, 0.1f, anchoPasillo), matTecho);
        if (tp1DerAncho > 0.1f)
            Cubo("Techo_Pasillo_P1_Der", pasillo1.transform,
                new Vector3(tp1DerX, altoPared + 0.05f, 0f), new Vector3(tp1DerAncho, 0.1f, anchoPasillo), matTecho);

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
        float centroAlaX = xBase + dir * anchoPasillo / 2f;
        float bordeDerAla = centroAlaX + anchoPasillo / 2f;

        Cubo("Piso_Ala", ala.transform,
            new Vector3(centroAlaX, -0.05f, -4f + largoAla / 2f),
            new Vector3(anchoPasillo, 0.1f, largoAla), matPiso);

        // Recortar techo y piso del ala si se solapan con la caja de escaleras (X > 6.5)
        float limiteEscalera = 6.5f;
        if (bordeDerAla > limiteEscalera && derecha)
        {
            float anchoRecortado = limiteEscalera - (centroAlaX - anchoPasillo / 2f);
            float centroRecortado = (centroAlaX - anchoPasillo / 2f + limiteEscalera) / 2f;
            Cubo("Techo_Ala", ala.transform,
                new Vector3(centroRecortado, altoPared + 0.05f, -4f + largoAla / 2f),
                new Vector3(anchoRecortado, 0.1f, largoAla), matTecho);
        }
        else
        {
            Cubo("Techo_Ala", ala.transform,
                new Vector3(centroAlaX, altoPared + 0.05f, -4f + largoAla / 2f),
                new Vector3(anchoPasillo, 0.1f, largoAla), matTecho);
        }

        // Pared exterior del ala (recortada si invade la caja de escaleras)
        float paredExtX = xBase + dir * anchoPasillo;
        if (paredExtX > limiteEscalera && derecha)
            paredExtX = limiteEscalera;
        Cubo("ParedExt_Ala", ala.transform,
            new Vector3(paredExtX, altoPared / 2f, -4f + largoAla / 2f),
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
    //  SOTANO (Y=-4, debajo del Piso 1)
    // =====================================================

    static void ConstruirSotano(Transform edificio)
    {
        GameObject sotano = Contenedor("Sotano_Edificio", edificio, new Vector3(0f, -4f, 0f));
        float alto = 3.5f;

        // Piso
        Cubo("Piso_Sotano", sotano.transform,
            new Vector3(0f, -0.05f, 0f), new Vector3(18f, 0.1f, 14f), matPisoSotano);
        // Techo
        Cubo("Techo_Sotano", sotano.transform,
            new Vector3(0f, alto, 0f), new Vector3(18f, 0.15f, 14f), matPisoSotano);

        // Paredes
        Cubo("Pared_N_Sot", sotano.transform,
            new Vector3(0f, alto / 2f, 7f), new Vector3(18f, alto, 0.2f), matParedSotano);
        Cubo("Pared_S_Sot", sotano.transform,
            new Vector3(0f, alto / 2f, -7f), new Vector3(18f, alto, 0.2f), matParedSotano);
        Cubo("Pared_E_Sot", sotano.transform,
            new Vector3(9f, alto / 2f, 0f), new Vector3(0.2f, alto, 14f), matParedSotano);
        Cubo("Pared_O_Sot", sotano.transform,
            new Vector3(-9f, alto / 2f, 0f), new Vector3(0.2f, alto, 14f), matParedSotano);

        // Pasillo central
        Cubo("Div_L_Sot", sotano.transform,
            new Vector3(-3f, alto / 2f, -2f), new Vector3(0.15f, alto, 8f), matParedSotano);
        Cubo("Div_R_Sot", sotano.transform,
            new Vector3(3f, alto / 2f, -2f), new Vector3(0.15f, alto, 8f), matParedSotano);

        // Puerta generadores
        Cubo("Puerta_Gen", sotano.transform,
            new Vector3(0f, 1.1f, -4f), new Vector3(1.2f, 2.2f, 0.08f), matPuertaMetal);

        // Tuberias techo
        for (int i = 0; i < 5; i++)
        {
            float x = -6f + i * 3f;
            GameObject tub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tub.name = $"Tuberia_Sot_{i}";
            tub.transform.SetParent(sotano.transform);
            tub.transform.localPosition = new Vector3(x, alto - 0.15f, 0f);
            tub.transform.localScale = new Vector3(0.1f, 7f, 0.1f);
            tub.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            tub.GetComponent<Renderer>().sharedMaterial = matBarandal;
            Object.DestroyImmediate(tub.GetComponent<Collider>());
            Undo.RegisterCreatedObjectUndo(tub, "Tuberia");
        }

        // Luces tenues
        for (int i = 0; i < 3; i++)
        {
            GameObject lz = new GameObject($"Luz_Sotano_{i}");
            lz.transform.SetParent(sotano.transform);
            lz.transform.localPosition = new Vector3(-4f + i * 4f, alto - 0.3f, 0f);
            Light l = lz.AddComponent<Light>();
            l.type = LightType.Point; l.range = 6f; l.intensity = 0.4f;
            l.color = new Color(0.7f, 0.5f, 0.25f);
            Undo.RegisterCreatedObjectUndo(lz, "Luz sotano");
        }

        // Apertura en techo para gradas (hueco)
        // Se deja un vacio en X=7..9 Z=-3..-1 del techo del sotano
        // (el techo ya fue creado solido, ahora cortamos creando paredes alrededor del hueco)
        // En realidad el hueco se logra no poniendo techo ahi — recreamos el techo en secciones
        Object.DestroyImmediate(sotano.transform.Find("Techo_Sotano")?.gameObject);
        Cubo("Techo_Sot_L", sotano.transform,
            new Vector3(-1f, alto, 0f), new Vector3(16f, 0.15f, 14f), matPisoSotano);
        Cubo("Techo_Sot_R", sotano.transform,
            new Vector3(8f, alto, 3f), new Vector3(2f, 0.15f, 8f), matPisoSotano);

        Debug.Log("[Edificio] Sotano construido: 18x14m con pasillo central y tuberias");
    }

    // =====================================================
    //  PISO 2 (Y=4, entre Piso 1 y Piso 3)
    // =====================================================

    static void ConstruirPiso2(Transform edificio)
    {
        float yBase = 4f;
        float alto = 3.5f;
        float anchoPas = 3.0f;
        float largoPas = 28f;

        GameObject piso2 = Contenedor("Piso2_Edificio_Ampliado", edificio, new Vector3(0f, yBase, 6f));
        GameObject pasillo = Contenedor("Pasillo_Principal_P2", piso2.transform, Vector3.zero);

        // Piso y techo principal (techo dividido para dejar hueco en la caja de escaleras)
        Cubo("Piso_P2", pasillo.transform,
            new Vector3(0f, -0.05f, 0f), new Vector3(largoPas, 0.1f, anchoPas), matPiso);

        // Techo P2: la caja de gradas está en global X=8, pero piso2 tiene offset (0,4,6)
        // → en coordenadas locales del pasillo la escalera está en X=8, Z entre -2 y 2
        // Dividimos el techo en dos tramos dejando hueco en X=[6.5, 9.5] local
        float techoIzqAncho = 6.5f + largoPas / 2f;
        float techoIzqX = (-largoPas / 2f + 6.5f) / 2f;
        float techoDerAncho = largoPas / 2f - 9.5f;
        float techoDerX = (9.5f + largoPas / 2f) / 2f;
        Cubo("Techo_P2_Izq", pasillo.transform,
            new Vector3(techoIzqX, alto + 0.05f, 0f), new Vector3(techoIzqAncho, 0.1f, anchoPas), matTecho);
        Cubo("Techo_P2_Der", pasillo.transform,
            new Vector3(techoDerX, alto + 0.05f, 0f), new Vector3(techoDerAncho, 0.1f, anchoPas), matTecho);

        // Cierres laterales
        Cubo("Cierre_O_P2", pasillo.transform,
            new Vector3(-largoPas / 2f, alto / 2f, 0f), new Vector3(0.15f, alto, anchoPas), matParedInterior);
        Cubo("Cierre_E_P2", pasillo.transform,
            new Vector3(largoPas / 2f, alto / 2f, 0f), new Vector3(0.15f, alto, anchoPas), matParedInterior);

        // Generar múltiples cuartos ingresables a ambos lados (Norte y Sur)
        string[] cuartosNorte = { "Oficina_201", "Oficina_202", "Archivo_Central", "Sala_Juntas", "Oficina_203", "Laboratorio_B", "Servicios_P2" };
        string[] cuartosSur = { "Almacen_Norte", "Oficina_204", "Sala_Monitoreo", "Biblioteca_Oculta", "Oficina_205", "Bano_Personal", "Zona_Descanso" };

        float profundidadCuarto = 5f;

        // Cuartos lado Norte (Z positivo)
        // Saltar cuartos que se solapan con la zona de escaleras (X=5..11)
        for (int i = 0; i < cuartosNorte.Length; i++)
        {
            float xPos = -10f + (i * 3.4f);
            if (xPos > 5.0f && xPos < 11.0f) continue;
            GameObject cuarto = Contenedor($"Cuarto_Norte_{cuartosNorte[i]}", piso2.transform, new Vector3(xPos, 0f, anchoPas / 2f + profundidadCuarto / 2f));

            // Suelo y techo del cuarto
            Cubo("Piso", cuarto.transform, new Vector3(0f, -0.05f, 0f), new Vector3(3.2f, 0.1f, profundidadCuarto), matPiso);
            Cubo("Techo", cuarto.transform, new Vector3(0f, alto + 0.05f, 0f), new Vector3(3.2f, 0.1f, profundidadCuarto), matTecho);

            // Paredes laterales del cuarto
            Cubo("Pared_Izq", cuarto.transform, new Vector3(-1.6f, alto / 2f, 0f), new Vector3(0.15f, alto, profundidadCuarto), matParedInterior);
            Cubo("Pared_Der", cuarto.transform, new Vector3(1.6f, alto / 2f, 0f), new Vector3(0.15f, alto, profundidadCuarto), matParedInterior);
            // Pared al fondo
            Cubo("Pared_Fondo", cuarto.transform, new Vector3(0f, alto / 2f, profundidadCuarto / 2f), new Vector3(3.2f, alto, 0.15f), matParedInterior);

            // Pared frontal con puerta abierta (hueco de entrada)
            Cubo("Pared_Frontal_Izq", cuarto.transform, new Vector3(-1.15f, alto / 2f, -profundidadCuarto / 2f), new Vector3(0.9f, alto, 0.15f), matParedInterior);
            Cubo("Pared_Frontal_Der", cuarto.transform, new Vector3(1.15f, alto / 2f, -profundidadCuarto / 2f), new Vector3(0.9f, alto, 0.15f), matParedInterior);
            Cubo("Dintel_Puerta", cuarto.transform, new Vector3(0f, 2.3f, -profundidadCuarto / 2f), new Vector3(1.4f, alto - 2.3f, 0.15f), matParedInterior);

            // Luz interior en cada cuarto
            GameObject luzCuarto = new GameObject($"Luz_{cuartosNorte[i]}");
            luzCuarto.transform.SetParent(cuarto.transform);
            luzCuarto.transform.localPosition = new Vector3(0f, alto - 0.5f, 0f);
            Light l = luzCuarto.AddComponent<Light>();
            l.type = LightType.Point; l.range = 4.5f; l.intensity = 0.8f;
            l.color = new Color(0.85f, 0.8f, 0.7f);
            Undo.RegisterCreatedObjectUndo(luzCuarto, "Luz cuarto norte");
        }

        // Cuartos lado Sur (Z negativo)
        // Saltar cuartos que se solapan con el corredor de conexión a escaleras (X=6.5..9.5)
        for (int i = 0; i < cuartosSur.Length; i++)
        {
            float xPos = -10f + (i * 3.4f);
            if (xPos > 5.0f && xPos < 11.0f) continue;
            GameObject cuarto = Contenedor($"Cuarto_Sur_{cuartosSur[i]}", piso2.transform, new Vector3(xPos, 0f, -anchoPas / 2f - profundidadCuarto / 2f));

            // Suelo y techo del cuarto
            Cubo("Piso", cuarto.transform, new Vector3(0f, -0.05f, 0f), new Vector3(3.2f, 0.1f, profundidadCuarto), matPiso);
            Cubo("Techo", cuarto.transform, new Vector3(0f, alto + 0.05f, 0f), new Vector3(3.2f, 0.1f, profundidadCuarto), matTecho);

            // Paredes laterales del cuarto
            Cubo("Pared_Izq", cuarto.transform, new Vector3(-1.6f, alto / 2f, 0f), new Vector3(0.15f, alto, profundidadCuarto), matParedInterior);
            Cubo("Pared_Der", cuarto.transform, new Vector3(1.6f, alto / 2f, 0f), new Vector3(0.15f, alto, profundidadCuarto), matParedInterior);
            // Pared al fondo
            Cubo("Pared_Fondo", cuarto.transform, new Vector3(0f, alto / 2f, -profundidadCuarto / 2f), new Vector3(3.2f, alto, 0.15f), matParedInterior);

            // Pared frontal con puerta abierta
            Cubo("Pared_Frontal_Izq", cuarto.transform, new Vector3(-1.15f, alto / 2f, profundidadCuarto / 2f), new Vector3(0.9f, alto, 0.15f), matParedInterior);
            Cubo("Pared_Frontal_Der", cuarto.transform, new Vector3(1.15f, alto / 2f, profundidadCuarto / 2f), new Vector3(0.9f, alto, 0.15f), matParedInterior);
            Cubo("Dintel_Puerta", cuarto.transform, new Vector3(0f, 2.3f, profundidadCuarto / 2f), new Vector3(1.4f, alto - 2.3f, 0.15f), matParedInterior);

            // Luz interior en cada cuarto
            GameObject luzCuarto = new GameObject($"Luz_{cuartosSur[i]}");
            luzCuarto.transform.SetParent(cuarto.transform);
            luzCuarto.transform.localPosition = new Vector3(0f, alto - 0.5f, 0f);
            Light l = luzCuarto.AddComponent<Light>();
            l.type = LightType.Point; l.range = 4.5f; l.intensity = 0.8f;
            l.color = new Color(0.85f, 0.8f, 0.7f);
            Undo.RegisterCreatedObjectUndo(luzCuarto, "Luz cuarto sur");
        }

        // Luces en el pasillo principal ampliado
        for (int i = -4; i <= 4; i++)
        {
            CrearLuzPasillo(pasillo.transform, new Vector3(i * 3f, alto - 0.02f, 0f));
        }

        // === CORREDOR DE CONEXIÓN: escalera → pasillo P2 ===
        // La escalera P1→P2 deja al jugador en global (8, 4, -4).
        // El pasillo P2 está en global Z=6. Conectamos con un corredor en eje Z.
        ConectarEscaleraConPiso2(piso2.transform, alto);

        Debug.Log("[Edificio] Piso 2 ampliado construido: pasillo principal de 28m + 14 cuartos + corredor de conexion a escaleras.");
    }

    static void ConectarEscaleraConPiso2(Transform piso2, float alto)
    {
        // Coordenadas locales respecto a piso2 (que está en global (0, 4, 6)):
        // Escalera llega a global (8, 4, -4) → local (8, 0, -10)
        // Pasillo P2 está en local Z=0 (borde sur a Z = -anchoPas/2 = -1.5)
        // Corredor abierto en ambos extremos: solo piso, techo, paredes laterales y luces

        GameObject corredor = Contenedor("Corredor_Conexion_Escalera_P2", piso2, Vector3.zero);

        float corridorStartZ = -10.5f;
        float corridorEndZ = -1.5f;
        float corridorLength = corridorEndZ - corridorStartZ;
        float corridorCenterZ = (corridorStartZ + corridorEndZ) / 2f;
        float corridorX = 8f;
        float corridorWidth = 2.8f;

        // Piso del corredor
        Cubo("Piso_Corredor_P2", corredor.transform,
            new Vector3(corridorX, -0.05f, corridorCenterZ),
            new Vector3(corridorWidth, 0.1f, corridorLength), matPiso);

        // Techo del corredor
        Cubo("Techo_Corredor_P2", corredor.transform,
            new Vector3(corridorX, alto + 0.05f, corridorCenterZ),
            new Vector3(corridorWidth, 0.1f, corridorLength), matTecho);

        // Pared izquierda (oeste) — solo guía visual, sin cerrar los extremos
        Cubo("Pared_O_Corredor_P2", corredor.transform,
            new Vector3(corridorX - corridorWidth / 2f, alto / 2f, corridorCenterZ),
            new Vector3(0.15f, alto, corridorLength), matParedInterior);

        // Pared derecha (este)
        Cubo("Pared_E_Corredor_P2", corredor.transform,
            new Vector3(corridorX + corridorWidth / 2f, alto / 2f, corridorCenterZ),
            new Vector3(0.15f, alto, corridorLength), matParedInterior);

        // SIN paredes en extremo sur (abierto hacia escalera) ni norte (abierto hacia pasillo)

        // Zócalos
        Cubo("Zocalo_O_Corredor", corredor.transform,
            new Vector3(corridorX - corridorWidth / 2f + 0.02f, 0.06f, corridorCenterZ),
            new Vector3(0.03f, 0.12f, corridorLength), matZocalo);
        Cubo("Zocalo_E_Corredor", corredor.transform,
            new Vector3(corridorX + corridorWidth / 2f - 0.02f, 0.06f, corridorCenterZ),
            new Vector3(0.03f, 0.12f, corridorLength), matZocalo);

        // Luces del corredor de conexión
        for (int i = 0; i < 4; i++)
        {
            float z = corridorStartZ + 1.5f + i * (corridorLength / 4f);
            CrearLuzPasillo(corredor.transform, new Vector3(corridorX, alto - 0.02f, z));
        }
    }

    // =====================================================
    //  GRADAS QUE CONECTAN TODOS LOS PISOS
    // =====================================================

    static void ConstruirGradas(Transform edificio)
    {
        // Caja de escaleras en el extremo este (X=7..9)
        GameObject cajaGradas = Contenedor("CajaGradas_Edificio", edificio, new Vector3(8f, 0f, 0f));

        // --- GRADAS SOTANO (Y=-4) → PISO 1 (Y=0) ---
        CrearTramoEscalera(cajaGradas.transform, "Gradas_Sot_P1",
            new Vector3(0f, -4f, -2f), 4f, 20, false);

        // --- GRADAS PISO 1 (Y=0) → PISO 2 (Y=4) ---
        CrearTramoEscalera(cajaGradas.transform, "Gradas_P1_P2",
            new Vector3(0f, 0f, 0f), 4f, 20, true);

        // --- GRADAS PISO 2 (Y=4) → PISO 3 (Y=8) ---
        CrearTramoEscalera(cajaGradas.transform, "Gradas_P2_P3",
            new Vector3(0f, 4f, 4f), 4f, 20, false);

        // Paredes de la caja de escaleras (toda la altura del edificio)
        float altoTotal = 12.5f; // de -4 a 8.5
        float yCenter = 2f; // centro vertical
        Cubo("Pared_E_CajaGrad", cajaGradas.transform,
            new Vector3(1.3f, yCenter, 2f), new Vector3(0.15f, altoTotal, 12f), matParedInterior);
        Cubo("Pared_O_CajaGrad", cajaGradas.transform,
            new Vector3(-1.3f, yCenter, 2f), new Vector3(0.15f, altoTotal, 12f), matParedInterior);

        // Luces en la caja de escaleras (una por piso)
        float[] lucesY = { -2f, 2f, 6f };
        for (int i = 0; i < lucesY.Length; i++)
        {
            GameObject lz = new GameObject($"Luz_CajaGrad_{i}");
            lz.transform.SetParent(cajaGradas.transform);
            lz.transform.localPosition = new Vector3(0f, lucesY[i], i * 2f);
            Light l = lz.AddComponent<Light>();
            l.type = LightType.Point; l.range = 5f; l.intensity = 0.5f;
            l.color = new Color(0.75f, 0.65f, 0.5f);
            Undo.RegisterCreatedObjectUndo(lz, "Luz caja gradas");
        }

        // Carteles de piso
        CrearCartelPiso(cajaGradas.transform, "SOTANO", new Vector3(1.2f, -2.5f, -1.5f));
        CrearCartelPiso(cajaGradas.transform, "PISO 1", new Vector3(1.2f, 1.5f, -0.5f));
        CrearCartelPiso(cajaGradas.transform, "PISO 2", new Vector3(1.2f, 5.5f, 2.5f));
        CrearCartelPiso(cajaGradas.transform, "PISO 3", new Vector3(1.2f, 9.5f, 5.5f));

        // Puntos de navegacion
        CrearPuntoNav("Spawn_Sotano", cajaGradas.transform, new Vector3(0f, -3.5f, -2.5f));
        CrearPuntoNav("PuntoLlegada_Piso3", cajaGradas.transform, new Vector3(0f, 8.3f, 7.5f));
        CrearPuntoNav("PuntoRetorno_Inferior", cajaGradas.transform, new Vector3(0f, 4.3f, 3.5f));

        // Descanso con trigger para TrampaGradasSyntro (entre P2 y P3)
        GameObject descansoTrigger = new GameObject("Descanso_P2P3_Trigger");
        descansoTrigger.transform.SetParent(cajaGradas.transform);
        descansoTrigger.transform.localPosition = new Vector3(0f, 6f, 5.8f);
        BoxCollider bc = descansoTrigger.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(2.2f, 2f, 1.5f);
        bc.center = new Vector3(0f, 1f, 0f);
        Cubo("Piso_Descanso", descansoTrigger.transform,
            Vector3.zero, new Vector3(2.2f, 0.1f, 1.5f), matEscalon);
        Undo.RegisterCreatedObjectUndo(descansoTrigger, "Descanso trigger");

        Debug.Log("[Edificio] 3 tramos de gradas construidos (Sotano→P1→P2→P3) con carteles y puntos de navegacion");
    }

    static void CrearTramoEscalera(Transform padre, string nombre, Vector3 posBase,
        float alturaTotal, int numEscalones, bool invertirZ)
    {
        GameObject tramo = Contenedor(nombre, padre, posBase);
        float dirZ = invertirZ ? -1f : 1f;
        float alturaPorEsc = alturaTotal / numEscalones;
        float profPorEsc = 0.2f;
        float anchoEsc = 2.2f;

        float totalZ = numEscalones * profPorEsc;
        float largo = Mathf.Sqrt(totalZ * totalZ + alturaTotal * alturaTotal);
        float angulo = Mathf.Atan2(alturaTotal, totalZ) * Mathf.Rad2Deg * dirZ;

        for (int i = 0; i < numEscalones; i++)
        {
            float y = i * alturaPorEsc;
            float z = i * profPorEsc * dirZ;

            // Escalon visible — conserva collider (superficie horizontal donde pisa el jugador)
            GameObject esc = Cubo($"Esc_{i}", tramo.transform,
                new Vector3(0f, y + alturaPorEsc / 2f, z),
                new Vector3(anchoEsc, alturaPorEsc, profPorEsc), matEscalon);
            esc.isStatic = true;

            // Contrahuella — sin collider (la cara vertical bloqueaba el movimiento)
            GameObject ch = Cubo($"CH_{i}", tramo.transform,
                new Vector3(0f, y, z - profPorEsc / 2f * dirZ),
                new Vector3(anchoEsc, alturaPorEsc, 0.02f), matParedInterior);
            Object.DestroyImmediate(ch.GetComponent<Collider>());

            // Balaustres cada 4 escalones
            if (i % 4 == 0)
            {
                Cubo($"Bal_L_{i}", tramo.transform,
                    new Vector3(-anchoEsc / 2f - 0.04f, y + 0.55f, z),
                    new Vector3(0.04f, 1.1f, 0.04f), matBarandal);
                Cubo($"Bal_R_{i}", tramo.transform,
                    new Vector3(anchoEsc / 2f + 0.04f, y + 0.55f, z),
                    new Vector3(0.04f, 1.1f, 0.04f), matBarandal);
            }
        }

        // --- RAMPA INVISIBLE PARA COLISION SUAVE (Fix para CharacterController) ---
        GameObject rampa = Cubo("Colision_Rampa_Suave", tramo.transform,
            new Vector3(0f, alturaTotal / 2f, (totalZ / 2f - profPorEsc / 2f) * dirZ),
            new Vector3(anchoEsc + 0.1f, 0.1f, largo + 0.2f), null);
        
        // La rotación debe ser la misma que el pasamanos
        rampa.transform.localRotation = Quaternion.Euler(angulo, 0f, 0f);
        
        // Quitamos el renderer para que sea invisible, pero mantenemos el collider
        Object.DestroyImmediate(rampa.GetComponent<Renderer>());
        // Aseguramos que no sea trigger
        rampa.GetComponent<Collider>().isTrigger = false;

        // Pasamanos (barra superior inclinada)
        GameObject pmL = Cubo("Pasamanos_L", tramo.transform,
            new Vector3(-anchoEsc / 2f - 0.04f, alturaTotal / 2f + 0.55f, totalZ * dirZ / 2f),
            new Vector3(0.06f, 0.04f, largo), matBarandal);
        pmL.transform.localRotation = Quaternion.Euler(angulo, 0f, 0f);

        GameObject pmR = Cubo("Pasamanos_R", tramo.transform,
            new Vector3(anchoEsc / 2f + 0.04f, alturaTotal / 2f + 0.55f, totalZ * dirZ / 2f),
            new Vector3(0.06f, 0.04f, largo), matBarandal);
        pmR.transform.localRotation = Quaternion.Euler(angulo, 0f, 0f);

        // Descanso al final del tramo
        Cubo("Descanso_" + nombre, tramo.transform,
            new Vector3(0f, alturaTotal, totalZ * dirZ),
            new Vector3(anchoEsc + 0.3f, 0.1f, 1.5f), matEscalon);
    }

    static void CrearCartelPiso(Transform padre, string texto, Vector3 pos)
    {
        GameObject cartel = Cubo("Cartel_" + texto.Replace(" ", ""), padre,
            pos, new Vector3(0.5f, 0.35f, 0.03f), matNumero);
        // El texto se agregaria via TextMeshPro en runtime
    }

    static void CrearPuntoNav(string nombre, Transform padre, Vector3 pos)
    {
        GameObject p = new GameObject(nombre);
        p.transform.SetParent(padre);
        p.transform.localPosition = pos;
        Undo.RegisterCreatedObjectUndo(p, nombre);
    }

    // =====================================================
    //  EXTERIOR DEL EDIFICIO (fachada visible)
    // =====================================================

    static void ConstruirExteriorEdificio(Transform edificio)
    {
        GameObject exterior = Contenedor("Exterior_Syntro", edificio, Vector3.zero);

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

        // === ENTREPISOS (con hueco para la caja de escaleras en X=6.5..9.5) ===
        float huecoXMin = 6.5f;
        float huecoXMax = 9.5f;
        float halfLargo = largoEdificio / 2f;

        float anchoIzq = huecoXMin + halfLargo;
        float centroIzqX = (-halfLargo + huecoXMin) / 2f;
        float anchoDer = halfLargo - huecoXMax;
        float centroDerX = (huecoXMax + halfLargo) / 2f;

        float[] losasY = { 0f, altoPiso, 8f };
        string[] losasNombre = { "SotP1", "P1P2", "P2P3" };
        for (int li = 0; li < 3; li++)
        {
            Cubo($"Losa_{losasNombre[li]}_Izq", exterior.transform,
                new Vector3(centroIzqX, losasY[li], -3f),
                new Vector3(anchoIzq, 0.2f, anchoEdificio), matParedExterior);
            Cubo($"Losa_{losasNombre[li]}_Der", exterior.transform,
                new Vector3(centroDerX, losasY[li], -3f),
                new Vector3(anchoDer, 0.2f, anchoEdificio), matParedExterior);
        }

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
