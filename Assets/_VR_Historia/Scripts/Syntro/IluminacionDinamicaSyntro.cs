using UnityEngine;

public class IluminacionDinamicaSyntro : MonoBehaviour
{
    [Header("=== Flicker Ambiental ===")]
    [SerializeField] private float intensidadFlickerBase = 0.03f;
    [SerializeField] private float intensidadFlickerTerror = 0.3f;
    [SerializeField] private float velocidadFlicker = 8f;

    private Light[] luces;
    private float[] intensidadesBase;
    private float nivelTerror;

    void Start()
    {
        luces = FindObjectsByType<Light>(FindObjectsSortMode.None);
        if (luces == null || luces.Length == 0) return;

        intensidadesBase = new float[luces.Length];
        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] != null)
                intensidadesBase[i] = luces[i].intensity;
        }

        if (GestorHistoriaSyntro.Instance != null)
            GestorHistoriaSyntro.Instance.OnFaseCambiada += AlCambiarFase;
    }

    void Update()
    {
        if (luces == null) return;

        float flicker = Mathf.Lerp(intensidadFlickerBase, intensidadFlickerTerror, nivelTerror);

        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i] == null) continue;

            float noise = Mathf.PerlinNoise(Time.time * velocidadFlicker + i * 100f, 0f);
            float mod = 1f + (noise - 0.5f) * flicker;
            luces[i].intensity = intensidadesBase[i] * mod;
        }
    }

    public void SetNivelTerror(float nivel)
    {
        nivelTerror = Mathf.Clamp01(nivel);
    }

    void AlCambiarFase(FaseSyntro fase)
    {
        switch (fase)
        {
            case FaseSyntro.Piso1_Inicio:
                nivelTerror = 0.1f;
                break;
            case FaseSyntro.Piso1_Exploracion:
                nivelTerror = 0.15f;
                break;
            case FaseSyntro.Piso1_ErrorServidor:
                nivelTerror = 0.25f;
                break;
            case FaseSyntro.Piso1_ReinicioManual:
                nivelTerror = 0.3f;
                break;
            case FaseSyntro.Piso1_SalidaGradas:
                nivelTerror = 0.35f;
                break;
            case FaseSyntro.Gradas_Subida:
                nivelTerror = 0.4f;
                break;
            case FaseSyntro.Gradas_BuclePsicologico:
                nivelTerror = 0.6f;
                break;
            case FaseSyntro.Gradas_Completado:
                nivelTerror = 0.7f;
                break;
            case FaseSyntro.Piso3_Entrada:
                nivelTerror = 0.6f;
                break;
            case FaseSyntro.Piso3_Exploracion:
                nivelTerror = 0.75f;
                break;
            case FaseSyntro.Piso3_AnomaliaFinal:
                nivelTerror = 0.85f;
                break;
            case FaseSyntro.Piso3_Climax:
                nivelTerror = 1f;
                break;
            case FaseSyntro.Completado:
                nivelTerror = 0f;
                break;
        }
    }

    void OnDestroy()
    {
        if (GestorHistoriaSyntro.Instance != null)
            GestorHistoriaSyntro.Instance.OnFaseCambiada -= AlCambiarFase;

        if (luces != null && intensidadesBase != null)
        {
            for (int i = 0; i < luces.Length; i++)
            {
                if (luces[i] != null)
                    luces[i].intensity = intensidadesBase[i];
            }
        }
    }
}
