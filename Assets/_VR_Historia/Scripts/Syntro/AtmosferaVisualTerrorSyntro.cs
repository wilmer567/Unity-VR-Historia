#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmosferaVisualTerrorSyntro
{
    [MenuItem("Syntro Terror/Configurar Atmosfera Visual Terror")]
    public static void Ejecutar()
    {
        Debug.Log("[AtmosferaTerror] Configurando atmosfera visual de terror...");

        EditorUtility.DisplayProgressBar("Atmosfera Terror", "Post-Processing...", 0.1f);
        ConfigurarPostProcessing();

        EditorUtility.DisplayProgressBar("Atmosfera Terror", "Niebla y ambiente...", 0.3f);
        ConfigurarNiebla();

        EditorUtility.DisplayProgressBar("Atmosfera Terror", "Particulas atmosfericas...", 0.5f);
        GameObject contenedor = CrearContenedorParticulas();

        EditorUtility.DisplayProgressBar("Atmosfera Terror", "Polvo flotante...", 0.6f);
        CrearPolvoFlotante(contenedor.transform);

        EditorUtility.DisplayProgressBar("Atmosfera Terror", "Humo servidor...", 0.7f);
        ParticleSystem humo = CrearHumoServidor(contenedor.transform);

        EditorUtility.DisplayProgressBar("Atmosfera Terror", "Aliento frio...", 0.8f);
        ParticleSystem aliento = CrearAlientoFrio(contenedor.transform);

        EditorUtility.DisplayProgressBar("Atmosfera Terror", "Iluminacion dinamica...", 0.85f);
        CrearIluminacionDinamica(contenedor.transform);

        EditorUtility.DisplayProgressBar("Atmosfera Terror", "Asignando referencias...", 0.9f);
        AsignarParticulas(humo, aliento);

        EditorUtility.ClearProgressBar();
        Debug.Log("[AtmosferaTerror] Atmosfera visual configurada: post-processing, niebla, 3 sistemas de particulas, iluminacion dinamica.");
    }

    // ===========================================================
    //  POST-PROCESSING
    // ===========================================================

    static void ConfigurarPostProcessing()
    {
        GameObject ppObj = GameObject.Find("PostProcessing_Terror_Syntro");
        if (ppObj == null)
        {
            ppObj = new GameObject("PostProcessing_Terror_Syntro");
            Undo.RegisterCreatedObjectUndo(ppObj, "Crear PostProcessing Terror");
        }

        Volume vol = ppObj.GetComponent<Volume>();
        if (vol == null) vol = ppObj.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 1f;

        string perfilPath = "Assets/Settings/TerrorPostProcessProfile.asset";
        VolumeProfile perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(perfilPath);
        if (perfil == null)
        {
            perfil = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(perfil, perfilPath);
        }

        perfil.components.Clear();

        var vignette = perfil.Add<Vignette>();
        vignette.intensity.Override(0.25f);
        vignette.smoothness.Override(0.35f);
        vignette.color.Override(new Color(0.05f, 0f, 0f));

        var color = perfil.Add<ColorAdjustments>();
        color.postExposure.Override(0.4f);
        color.contrast.Override(15f);
        color.saturation.Override(-10f);

        var grain = perfil.Add<FilmGrain>();
        grain.intensity.Override(0.35f);
        grain.response.Override(0.6f);
        grain.type.Override(FilmGrainLookup.Thin1);

        var bloom = perfil.Add<Bloom>();
        bloom.threshold.Override(0.9f);
        bloom.intensity.Override(0.5f);

        var chroma = perfil.Add<ChromaticAberration>();
        chroma.intensity.Override(0.15f);

        vol.profile = perfil;

        EditorUtility.SetDirty(perfil);
        EditorUtility.SetDirty(ppObj);
        AssetDatabase.SaveAssetIfDirty(perfil);

        Debug.Log("[AtmosferaTerror] Post-processing: vignette, color grading, film grain, bloom, chromatic aberration.");
    }

    // ===========================================================
    //  NIEBLA Y AMBIENTE
    // ===========================================================

    static void ConfigurarNiebla()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.01f;
        RenderSettings.fogColor = new Color(0.15f, 0.15f, 0.2f);
        RenderSettings.ambientLight = new Color(0.25f, 0.25f, 0.3f);
        RenderSettings.ambientMode = AmbientMode.Flat;

        EditorSceneManager_MarkDirty();
        Debug.Log("[AtmosferaTerror] Niebla suave y luz ambiente iluminada configuradas.");
    }

    static void EditorSceneManager_MarkDirty()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    // ===========================================================
    //  CONTENEDOR DE PARTICULAS
    // ===========================================================

    static GameObject CrearContenedorParticulas()
    {
        GameObject contenedor = GameObject.Find("Atmosfera_Particulas_Syntro");
        if (contenedor == null)
        {
            contenedor = new GameObject("Atmosfera_Particulas_Syntro");
            Undo.RegisterCreatedObjectUndo(contenedor, "Crear Contenedor Particulas");
        }
        return contenedor;
    }

    // ===========================================================
    //  MATERIALES DE PARTICULA
    // ===========================================================

    static Material CrearMatParticula(string nombre, Color color)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (sh == null)
        {
            Debug.LogWarning("[AtmosferaTerror] Shader Particles/Unlit no encontrado, usando fallback.");
            sh = Shader.Find("Particles/Standard Unlit");
        }
        Material mat = new Material(sh);
        mat.name = nombre;
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_Blend", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        return mat;
    }

    // ===========================================================
    //  POLVO FLOTANTE
    // ===========================================================

    static void CrearPolvoFlotante(Transform padre)
    {
        GameObject obj = BuscarOCrear("Particulas_Polvo_Syntro", padre);
        obj.transform.localPosition = new Vector3(0f, 2f, 0f);

        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps == null) ps = obj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 8f;
        main.startSpeed = 0.05f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.6f, 0.5f, 0.4f, 0.3f);
        main.loop = true;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 25f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(10f, 5f, 30f);

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.6f, 0.5f, 0.4f), 0f),
                new GradientColorKey(new Color(0.6f, 0.5f, 0.4f), 0.5f),
                new GradientColorKey(new Color(0.6f, 0.5f, 0.4f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = grad;

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.material = CrearMatParticula("Mat_Polvo_Terror", new Color(0.6f, 0.5f, 0.4f, 0.3f));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        EditorUtility.SetDirty(obj);
    }

    // ===========================================================
    //  HUMO SERVIDOR
    // ===========================================================

    static ParticleSystem CrearHumoServidor(Transform padre)
    {
        GameObject obj = BuscarOCrear("Particulas_Humo_Syntro", padre);
        obj.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps == null) ps = obj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 4f;
        main.startSpeed = 0.3f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.maxParticles = 50;
        main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.15f);
        main.loop = true;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 8f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.3f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = 0.2f;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 2f));

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.material = CrearMatParticula("Mat_Humo_Terror", new Color(0.3f, 0.3f, 0.3f, 0.15f));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        EditorUtility.SetDirty(obj);
        return ps;
    }

    // ===========================================================
    //  ALIENTO FRIO
    // ===========================================================

    static ParticleSystem CrearAlientoFrio(Transform padre)
    {
        GameObject obj = BuscarOCrear("Particulas_Aliento_Syntro", padre);
        obj.transform.localPosition = new Vector3(0f, 1.5f, 0.3f);

        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps == null) ps = obj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 2f;
        main.startSpeed = 0.5f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        main.maxParticles = 30;
        main.startColor = new Color(0.7f, 0.8f, 0.9f, 0.2f);
        main.loop = true;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 15f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.05f;

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.material = CrearMatParticula("Mat_Aliento_Terror", new Color(0.7f, 0.8f, 0.9f, 0.2f));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        EditorUtility.SetDirty(obj);
        return ps;
    }

    // ===========================================================
    //  ILUMINACION DINAMICA
    // ===========================================================

    static void CrearIluminacionDinamica(Transform padre)
    {
        GameObject obj = BuscarOCrear("IluminacionDinamica_Syntro", padre);
        if (obj.GetComponent<IluminacionDinamicaSyntro>() == null)
            obj.AddComponent<IluminacionDinamicaSyntro>();
        EditorUtility.SetDirty(obj);
    }

    // ===========================================================
    //  ASIGNAR PARTICULAS A CONTROLADOR SENTIDOS
    // ===========================================================

    static void AsignarParticulas(ParticleSystem humo, ParticleSystem aliento)
    {
        var sentidos = Object.FindAnyObjectByType<ControladorSentidosVR>();
        if (sentidos == null)
        {
            Debug.Log("[AtmosferaTerror] ControladorSentidosVR no encontrado, las particulas se asignaran al ejecutar JUEGO COMPLETO.");
            return;
        }

        var so = new SerializedObject(sentidos);

        var propHumo = so.FindProperty("humoServidor");
        if (propHumo != null && propHumo.objectReferenceValue == null)
            propHumo.objectReferenceValue = humo;

        var propAliento = so.FindProperty("particulasAliento");
        if (propAliento != null && propAliento.objectReferenceValue == null)
            propAliento.objectReferenceValue = aliento;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(sentidos);

        Debug.Log("[AtmosferaTerror] Particulas asignadas a ControladorSentidosVR (humo + aliento).");
    }

    // ===========================================================
    //  UTILIDADES
    // ===========================================================

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
}
#endif
