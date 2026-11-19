#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ConectarPisosEdificio : EditorWindow
{
    static readonly string RUTA_MAT = "Assets/_Unity Essentials/Materials/Realistas";

    static Material matPiso, matPared, matTecho, matEscalon, matBarandal;
    static Material matPisoSotano, matParedSotano, matLedVerde, matPuerta;

    [MenuItem("Syntro Terror/Conectar Pisos — Sotano + Piso 2 + Gradas")]
    public static void Ejecutar()
    {
        Debug.Log("[Pisos] === INICIO: Conectando pisos del edificio ===");

        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        CrearMateriales(lit);

        GameObject edificio = GameObject.Find("Edificio_Syntro");
        if (edificio == null)
        {
            Debug.LogError("[Pisos] No se encontro Edificio_Syntro. Ejecuta primero 'Construir Edificio Syntro Completo'");
            return;
        }

        ConstruirSotano(edificio.transform);
        ConstruirGradasSotanoAPiso1(edificio.transform);
        ConstruirPiso2(edificio.transform);
        ConstruirGradasPiso1APiso2(edificio.transform);
        ConstruirGradasPiso2APiso3(edificio.transform);
        CrearPuntosNavegacion(edificio.transform);

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[Pisos] === COMPLETADO: Sotano + Piso 2 + 3 escaleras conectadas ===");
    }

    static void CrearMateriales(Shader lit)
    {
        matPiso = CargarOCrear("Mat_PisoPasillo_Syntro", lit,
            new Color(0.25f, 0.22f, 0.2f), 0f, 0.35f);
        matPared = CargarOCrear("Mat_ParedInterior_Syntro", lit,
            new Color(0.82f, 0.78f, 0.72f), 0f, 0.2f);
        matTecho = CargarOCrear("Mat_Techo_Syntro", lit,
            new Color(0.9f, 0.88f, 0.85f), 0f, 0.15f);
        matEscalon = CargarOCrear("Mat_Escalon_Syntro", lit,
            new Color(0.3f, 0.28f, 0.25f), 0.1f, 0.4f);
        matBarandal = CargarOCrear("Mat_Barandal_Syntro", lit,
            new Color(0.15f, 0.15f, 0.16f), 0.8f, 0.5f);
        matPisoSotano = CargarOCrear("Mat_PisoSotano_Syntro", lit,
            new Color(0.12f, 0.12f, 0.13f), 0f, 0.25f);
        matParedSotano = CargarOCrear("Mat_ParedSotano_Syntro", lit,
            new Color(0.15f, 0.14f, 0.13f), 0f, 0.15f);
        matPuerta = CargarOCrear("Mat_PuertaMetal_Syntro", lit,
            new Color(0.25f, 0.25f, 0.27f), 0.6f, 0.4f);

        matLedVerde = CargarOCrear("Mat_LED_Verde_DC", lit,
            new Color(0.01f, 0.01f, 0.01f), 0f, 0.4f);
        if (matLedVerde != null)
        {
            matLedVerde.EnableKeyword("_EMISSION");
            matLedVerde.SetColor("_EmissionColor", new Color(0f, 2f, 0.2f) * 3f);
        }
    }

    // ==========================================================
    //  SOTANO (Y = -4, debajo de la sala de servidores)
    // ==========================================================

    static void ConstruirSotano(Transform edificio)
    {
        if (edificio.Find("Sotano_Edificio") != null) return;

        float yBase = -4f;
        float alto = 3.5f;
        float ancho = 14f;
        float largo = 18f;

        GameObject sotano = Contenedor("Sotano_Edificio", edificio, new Vector3(0f, yBase, 0f));

        // Piso del sotano
        Cubo("Piso_Sotano", sotano.transform,
            new Vector3(0f, -0.05f, 0f), new Vector3(largo, 0.1f, ancho), matPisoSotano);

        // Techo del sotano (piso del Piso 1)
        Cubo("Techo_Sotano", sotano.transform,
            new Vector3(0f, alto + 0.05f, 0f), new Vector3(largo, 0.15f, ancho), matPisoSotano);

        // Paredes perimetrales
        Cubo("Pared_Norte_Sot", sotano.transform,
            new Vector3(0f, alto / 2f, ancho / 2f), new Vector3(largo, alto, 0.2f), matParedSotano);
        Cubo("Pared_Sur_Sot", sotano.transform,
            new Vector3(0f, alto / 2f, -ancho / 2f), new Vector3(largo, alto, 0.2f), matParedSotano);
        Cubo("Pared_Este_Sot", sotano.transform,
            new Vector3(largo / 2f, alto / 2f, 0f), new Vector3(0.2f, alto, ancho), matParedSotano);
        Cubo("Pared_Oeste_Sot", sotano.transform,
            new Vector3(-largo / 2f, alto / 2f, 0f), new Vector3(0.2f, alto, ancho), matParedSotano);

        // Pasillo central del sotano
        Cubo("Divisoria_Sot_L", sotano.transform,
            new Vector3(-3f, alto / 2f, -2f), new Vector3(0.15f, alto, 10f), matParedSotano);
        Cubo("Divisoria_Sot_R", sotano.transform,
            new Vector3(3f, alto / 2f, -2f), new Vector3(0.15f, alto, 10f), matParedSotano);

        // Sala de generadores (fondo del sotano)
        Cubo("Pared_Generadores", sotano.transform,
            new Vector3(0f, alto / 2f, -4f), new Vector3(6f, alto, 0.15f), matParedSotano);

        // Puerta al cuarto de generadores
        Cubo("Puerta_Generadores", sotano.transform,
            new Vector3(0f, 1.1f, -4f), new Vector3(1.2f, 2.2f, 0.08f), matPuerta);

        // Tuberias en el techo del sotano
        for (int i = 0; i < 5; i++)
        {
            float x = -6f + i * 3f;
            CrearCilindro("Tuberia_Sot_" + i, sotano.transform,
                new Vector3(x, alto - 0.2f, 0f), new Vector3(0.12f, ancho / 2f, 0.12f),
                Quaternion.Euler(0f, 0f, 90f), matBarandal);
        }

        // Luces tenues del sotano
        for (int i = 0; i < 3; i++)
        {
            float x = -4f + i * 4f;
            GameObject luzObj = new GameObject($"Luz_Sotano_{i}");
            luzObj.transform.SetParent(sotano.transform);
            luzObj.transform.localPosition = new Vector3(x, alto - 0.3f, 0f);
            Light luz = luzObj.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.range = 6f;
            luz.intensity = 0.5f;
            luz.color = new Color(0.8f, 0.6f, 0.3f);
            Undo.RegisterCreatedObjectUndo(luzObj, "Luz sotano");
        }

        Debug.Log("[Pisos] Sotano construido (18x14m) con pasillo central y sala de generadores");
    }

    // ==========================================================
    //  PISO 2 (Y = 4, entre Piso 1 y Piso 3)
    // ==========================================================

    static void ConstruirPiso2(Transform edificio)
    {
        if (edificio.Find("Piso2_Edificio") != null) return;

        float yBase = 4f;
        float alto = 3.5f;
        float anchoPasillo = 2.4f;
        float largoPasillo = 18f;

        GameObject piso2 = Contenedor("Piso2_Edificio", edificio, new Vector3(0f, yBase, 9f));

        // Pasillo principal eje X
        GameObject pasillo2 = Contenedor("Pasillo_P2", piso2.transform, Vector3.zero);

        Cubo("Piso_P2", pasillo2.transform,
            new Vector3(0f, -0.05f, 0f), new Vector3(largoPasillo, 0.1f, anchoPasillo), matPiso);
        Cubo("Techo_P2", pasillo2.transform,
            new Vector3(0f, alto + 0.05f, 0f), new Vector3(largoPasillo, 0.1f, anchoPasillo), matTecho);

        // Pared norte
        Cubo("Pared_Norte_P2", pasillo2.transform,
            new Vector3(0f, alto / 2f, anchoPasillo / 2f),
            new Vector3(largoPasillo, alto, 0.15f), matPared);
        // Pared sur
        Cubo("Pared_Sur_P2", pasillo2.transform,
            new Vector3(0f, alto / 2f, -anchoPasillo / 2f),
            new Vector3(largoPasillo, alto, 0.15f), matPared);

        // Cierres laterales
        Cubo("Cierre_Este_P2", pasillo2.transform,
            new Vector3(largoPasillo / 2f, alto / 2f, 0f),
            new Vector3(0.15f, alto, anchoPasillo), matPared);
        Cubo("Cierre_Oeste_P2", pasillo2.transform,
            new Vector3(-largoPasillo / 2f, alto / 2f, 0f),
            new Vector3(0.15f, alto, anchoPasillo), matPared);

        // Puertas de oficinas (lado norte)
        string[] oficinas = { "Oficina_201", "Oficina_202", "Archivo", "Sala_Reunion", "Oficina_203", "Bano_P2" };
        for (int i = 0; i < oficinas.Length; i++)
        {
            float x = -6f + i * 2.5f;
            Cubo("Puerta_" + oficinas[i], pasillo2.transform,
                new Vector3(x, 1.1f, anchoPasillo / 2f - 0.06f),
                new Vector3(0.9f, 2.2f, 0.06f), matPuerta);

            // Marco de puerta
            Cubo("Marco_L_" + oficinas[i], pasillo2.transform,
                new Vector3(x - 0.5f, 1.1f, anchoPasillo / 2f - 0.04f),
                new Vector3(0.06f, 2.3f, 0.08f), matBarandal);
            Cubo("Marco_R_" + oficinas[i], pasillo2.transform,
                new Vector3(x + 0.5f, 1.1f, anchoPasillo / 2f - 0.04f),
                new Vector3(0.06f, 2.3f, 0.08f), matBarandal);
        }

        // Luces del pasillo P2
        for (int i = -3; i <= 3; i++)
        {
            GameObject luzObj = new GameObject($"Luz_P2_{i}");
            luzObj.transform.SetParent(pasillo2.transform);
            luzObj.transform.localPosition = new Vector3(i * 2.5f, alto - 0.1f, 0f);
            Light luz = luzObj.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.range = 4f;
            luz.intensity = 0.7f;
            luz.color = new Color(0.85f, 0.8f, 0.7f);
            Undo.RegisterCreatedObjectUndo(luzObj, "Luz P2");
        }

        // Habitaciones detras de pared sur (sugeridas)
        float profHab = 4f;
        Cubo("ParedExterior_Sur_P2", piso2.transform,
            new Vector3(0f, alto / 2f, -anchoPasillo / 2f - profHab),
            new Vector3(largoPasillo + 1f, alto, 0.2f), matPared);
        Cubo("Piso_Hab_P2", piso2.transform,
            new Vector3(0f, -0.05f, -anchoPasillo / 2f - profHab / 2f),
            new Vector3(largoPasillo + 1f, 0.1f, profHab), matPiso);

        Debug.Log("[Pisos] Piso 2 construido: pasillo + 6 oficinas con puertas");
    }

    // ==========================================================
    //  GRADAS: SOTANO → PISO 1
    // ==========================================================

    static void ConstruirGradasSotanoAPiso1(Transform edificio)
    {
        if (edificio.Find("Gradas_Sotano_P1") != null) return;

        // Escalera en el extremo este del edificio
        float xBase = 7f;
        float yInicio = -4f;
        float yFin = 0f;
        float dY = yFin - yInicio; // 4m de altura
        int escalones = 20;

        GameObject gradas = Contenedor("Gradas_Sotano_P1", edificio, new Vector3(xBase, yInicio, -2f));
        ConstruirEscalera(gradas.transform, escalones, dY, 0f, "SotP1");

        // Paredes de la caja de escaleras
        float altoTotal = dY + 0.5f;
        Cubo("Pared_L_SotP1", gradas.transform,
            new Vector3(-1.3f, altoTotal / 2f, escalones * 0.18f / 2f),
            new Vector3(0.15f, altoTotal + 1f, escalones * 0.18f + 1f), matParedSotano);
        Cubo("Pared_R_SotP1", gradas.transform,
            new Vector3(1.3f, altoTotal / 2f, escalones * 0.18f / 2f),
            new Vector3(0.15f, altoTotal + 1f, escalones * 0.18f + 1f), matParedSotano);

        // Luz en la escalera
        GameObject luzEsc = new GameObject("Luz_Gradas_SotP1");
        luzEsc.transform.SetParent(gradas.transform);
        luzEsc.transform.localPosition = new Vector3(0f, dY - 0.3f, escalones * 0.09f);
        Light luz = luzEsc.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 5f;
        luz.intensity = 0.4f;
        luz.color = new Color(0.7f, 0.5f, 0.3f);
        Undo.RegisterCreatedObjectUndo(luzEsc, "Luz gradas sot-p1");

        Debug.Log("[Pisos] Gradas Sotano → Piso 1 construidas (20 escalones)");
    }

    // ==========================================================
    //  GRADAS: PISO 1 → PISO 2
    // ==========================================================

    static void ConstruirGradasPiso1APiso2(Transform edificio)
    {
        if (edificio.Find("Gradas_P1_P2") != null) return;

        float xBase = 7f;
        float yInicio = 0f;
        float yFin = 4f;
        float dY = yFin - yInicio;
        int escalones = 20;

        GameObject gradas = Contenedor("Gradas_P1_P2", edificio, new Vector3(xBase, yInicio, 2f));
        ConstruirEscalera(gradas.transform, escalones, dY, 0f, "P1P2");

        float altoTotal = dY + 0.5f;
        Cubo("Pared_L_P1P2", gradas.transform,
            new Vector3(-1.3f, altoTotal / 2f, escalones * 0.18f / 2f),
            new Vector3(0.15f, altoTotal + 1f, escalones * 0.18f + 1f), matPared);
        Cubo("Pared_R_P1P2", gradas.transform,
            new Vector3(1.3f, altoTotal / 2f, escalones * 0.18f / 2f),
            new Vector3(0.15f, altoTotal + 1f, escalones * 0.18f + 1f), matPared);

        // Descanso intermedio
        Cubo("Descanso_P1P2", gradas.transform,
            new Vector3(0f, dY / 2f, escalones * 0.09f),
            new Vector3(2.4f, 0.1f, 1.5f), matEscalon);

        GameObject luzEsc = new GameObject("Luz_Gradas_P1P2");
        luzEsc.transform.SetParent(gradas.transform);
        luzEsc.transform.localPosition = new Vector3(0f, dY - 0.3f, escalones * 0.09f);
        Light luz = luzEsc.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 5f;
        luz.intensity = 0.6f;
        luz.color = new Color(0.8f, 0.75f, 0.65f);
        Undo.RegisterCreatedObjectUndo(luzEsc, "Luz gradas p1-p2");

        Debug.Log("[Pisos] Gradas Piso 1 → Piso 2 construidas (20 escalones + descanso)");
    }

    // ==========================================================
    //  GRADAS: PISO 2 → PISO 3 (con trampa de terror)
    // ==========================================================

    static void ConstruirGradasPiso2APiso3(Transform edificio)
    {
        if (edificio.Find("Gradas_P2_P3") != null) return;

        float xBase = 7f;
        float yInicio = 4f;
        float yFin = 8f;
        float dY = yFin - yInicio;
        int escalones = 20;

        GameObject gradas = Contenedor("Gradas_P2_P3", edificio, new Vector3(xBase, yInicio, 12f));
        ConstruirEscalera(gradas.transform, escalones, dY, 0f, "P2P3");

        float altoTotal = dY + 0.5f;
        Cubo("Pared_L_P2P3", gradas.transform,
            new Vector3(-1.3f, altoTotal / 2f, escalones * 0.18f / 2f),
            new Vector3(0.15f, altoTotal + 1f, escalones * 0.18f + 1f), matPared);
        Cubo("Pared_R_P2P3", gradas.transform,
            new Vector3(1.3f, altoTotal / 2f, escalones * 0.18f / 2f),
            new Vector3(0.15f, altoTotal + 1f, escalones * 0.18f + 1f), matPared);

        // Descanso intermedio (aqui actua TrampaGradasSyntro)
        GameObject descanso = new GameObject("Descanso_P2P3_Trigger");
        descanso.transform.SetParent(gradas.transform);
        descanso.transform.localPosition = new Vector3(0f, dY / 2f, escalones * 0.09f);
        Cubo("Piso_Descanso_P2P3", descanso.transform,
            Vector3.zero, new Vector3(2.4f, 0.1f, 1.8f), matEscalon);

        // Trigger para TrampaGradasSyntro
        BoxCollider trigger = descanso.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(2.4f, 2f, 1.8f);
        trigger.center = new Vector3(0f, 1f, 0f);
        Undo.RegisterCreatedObjectUndo(descanso, "Descanso trigger P2P3");

        // Cartel de piso
        GameObject cartel = Cubo("Cartel_Piso", gradas.transform,
            new Vector3(1.2f, dY / 2f + 1.5f, escalones * 0.09f),
            new Vector3(0.6f, 0.4f, 0.03f), matPuerta);

        // Punto de llegada al Piso 3
        GameObject llegada = new GameObject("PuntoLlegada_Piso3");
        llegada.transform.SetParent(gradas.transform);
        llegada.transform.localPosition = new Vector3(0f, dY + 0.2f, escalones * 0.18f + 0.5f);
        Undo.RegisterCreatedObjectUndo(llegada, "Punto llegada P3");

        // Punto retorno inferior (para bucle de terror)
        GameObject retorno = new GameObject("PuntoRetorno_Inferior");
        retorno.transform.SetParent(gradas.transform);
        retorno.transform.localPosition = new Vector3(0f, 0.2f, -0.5f);
        Undo.RegisterCreatedObjectUndo(retorno, "Punto retorno inferior");

        GameObject luzEsc = new GameObject("Luz_Gradas_P2P3");
        luzEsc.transform.SetParent(gradas.transform);
        luzEsc.transform.localPosition = new Vector3(0f, dY - 0.3f, escalones * 0.09f);
        Light luz = luzEsc.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.range = 5f;
        luz.intensity = 0.5f;
        luz.color = new Color(0.7f, 0.65f, 0.55f);
        Undo.RegisterCreatedObjectUndo(luzEsc, "Luz gradas p2-p3");

        Debug.Log("[Pisos] Gradas Piso 2 → Piso 3 construidas (20 escalones + descanso con trigger)");
    }

    // ==========================================================
    //  PUNTOS DE NAVEGACION
    // ==========================================================

    static void CrearPuntosNavegacion(Transform edificio)
    {
        if (edificio.Find("Puntos_Navegacion") != null) return;

        GameObject nav = Contenedor("Puntos_Navegacion", edificio, Vector3.zero);

        CrearPuntoNav("Spawn_Sotano", nav.transform, new Vector3(0f, -3.5f, 0f));
        CrearPuntoNav("Spawn_Piso1", nav.transform, new Vector3(0f, 0.5f, -3f));
        CrearPuntoNav("Spawn_Piso2", nav.transform, new Vector3(0f, 4.5f, 9f));
        CrearPuntoNav("Spawn_Piso3", nav.transform, new Vector3(0f, 8.5f, 18f));

        CrearPuntoNav("Entrada_Gradas_SotP1", nav.transform, new Vector3(7f, -3.5f, -2f));
        CrearPuntoNav("Salida_Gradas_SotP1", nav.transform, new Vector3(7f, 0.5f, -2f));
        CrearPuntoNav("Entrada_Gradas_P1P2", nav.transform, new Vector3(7f, 0.5f, 2f));
        CrearPuntoNav("Salida_Gradas_P1P2", nav.transform, new Vector3(7f, 4.5f, 5.6f));
        CrearPuntoNav("Entrada_Gradas_P2P3", nav.transform, new Vector3(7f, 4.5f, 12f));
        CrearPuntoNav("Salida_Gradas_P2P3", nav.transform, new Vector3(7f, 8.5f, 15.6f));

        Debug.Log("[Pisos] 10 puntos de navegacion creados (spawns + entradas/salidas de gradas)");
    }

    static void CrearPuntoNav(string nombre, Transform padre, Vector3 pos)
    {
        GameObject punto = new GameObject(nombre);
        punto.transform.SetParent(padre);
        punto.transform.localPosition = pos;
        Undo.RegisterCreatedObjectUndo(punto, nombre);
    }

    // ==========================================================
    //  CONSTRUCTOR DE ESCALERA GENERICA
    // ==========================================================

    static void ConstruirEscalera(Transform padre, int escalones, float alturaTotal,
        float rotY, string prefijo)
    {
        float alturaPorEscalon = alturaTotal / escalones;
        float profPorEscalon = 0.18f;
        float anchoEscalon = 2.2f;

        // Barandal izquierdo
        GameObject barandalL = Contenedor("Barandal_L_" + prefijo, padre, Vector3.zero);
        // Barandal derecho
        GameObject barandalR = Contenedor("Barandal_R_" + prefijo, padre, Vector3.zero);

        for (int i = 0; i < escalones; i++)
        {
            float y = i * alturaPorEscalon;
            float z = i * profPorEscalon;

            // Escalon
            GameObject esc = Cubo($"Escalon_{prefijo}_{i}", padre,
                new Vector3(0f, y + alturaPorEscalon / 2f, z),
                new Vector3(anchoEscalon, alturaPorEscalon, profPorEscalon), matEscalon);
            esc.isStatic = true;

            // Balaustres cada 3 escalones
            if (i % 3 == 0)
            {
                Cubo($"Balaustre_L_{i}", barandalL.transform,
                    new Vector3(-anchoEscalon / 2f - 0.04f, y + 0.5f, z),
                    new Vector3(0.04f, 1f, 0.04f), matBarandal);
                Cubo($"Balaustre_R_{i}", barandalR.transform,
                    new Vector3(anchoEscalon / 2f + 0.04f, y + 0.5f, z),
                    new Vector3(0.04f, 1f, 0.04f), matBarandal);
            }
        }

        // Pasamanos
        float totalZ = escalones * profPorEscalon;
        float angulo = Mathf.Atan2(alturaTotal, totalZ) * Mathf.Rad2Deg;

        GameObject pasamanosL = Cubo("Pasamanos_L_" + prefijo, barandalL.transform,
            new Vector3(-anchoEscalon / 2f - 0.04f, alturaTotal / 2f + 0.5f, totalZ / 2f),
            new Vector3(0.06f, 0.04f, Mathf.Sqrt(totalZ * totalZ + alturaTotal * alturaTotal)),
            matBarandal);
        pasamanosL.transform.localRotation = Quaternion.Euler(angulo, 0f, 0f);

        GameObject pasamanosR = Cubo("Pasamanos_R_" + prefijo, barandalR.transform,
            new Vector3(anchoEscalon / 2f + 0.04f, alturaTotal / 2f + 0.5f, totalZ / 2f),
            new Vector3(0.06f, 0.04f, Mathf.Sqrt(totalZ * totalZ + alturaTotal * alturaTotal)),
            matBarandal);
        pasamanosR.transform.localRotation = Quaternion.Euler(angulo, 0f, 0f);

        // Collider para caminar por la escalera
        GameObject rampCollider = new GameObject("RampCollider_" + prefijo);
        rampCollider.transform.SetParent(padre);
        rampCollider.transform.localPosition = new Vector3(0f, alturaTotal / 2f, totalZ / 2f);
        rampCollider.transform.localRotation = Quaternion.Euler(angulo, 0f, 0f);
        BoxCollider col = rampCollider.AddComponent<BoxCollider>();
        col.size = new Vector3(anchoEscalon, 0.05f,
            Mathf.Sqrt(totalZ * totalZ + alturaTotal * alturaTotal));
        Undo.RegisterCreatedObjectUndo(rampCollider, "Ramp collider");
    }

    // ==========================================================
    //  HELPERS
    // ==========================================================

    static GameObject Contenedor(string nombre, Transform padre, Vector3 pos)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        Undo.RegisterCreatedObjectUndo(go, nombre);
        return go;
    }

    static GameObject Cubo(string nombre, Transform padre, Vector3 pos, Vector3 escala, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Undo.RegisterCreatedObjectUndo(go, nombre);
        return go;
    }

    static void CrearCilindro(string nombre, Transform padre, Vector3 pos, Vector3 escala,
        Quaternion rot, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.transform.localRotation = rot;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        Undo.RegisterCreatedObjectUndo(go, nombre);
    }

    static Material CargarOCrear(string nombre, Shader shader, Color color, float metallic, float smoothness)
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
}
#endif
