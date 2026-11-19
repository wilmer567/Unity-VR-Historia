using UnityEngine;
using System.Collections;

public class CuadrosFlotantesPasillo : MonoBehaviour
{
    public static CuadrosFlotantesPasillo Instance { get; private set; }

    [Header("Configuración de Cuadros Flotantes")]
    [SerializeField] private int cantidadCuadros = 6;
    [SerializeField] private float velocidadMovimiento = 1.5f;
    [SerializeField] private float amplitudFlotacion = 0.8f;
    [SerializeField] private float rangoZMin = -6f;
    [SerializeField] private float rangoZMax = 6f;

    private GameObject[] cuadrosFlotantes;
    private Vector3[] posicionesOriginales;
    private float[] velocidadesFase;
    private bool activo = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Suscribirse al gestor de historia si existe
        if (GestorHistoriaSyntro.Instance != null)
        {
            GestorHistoriaSyntro.Instance.OnFaseCambiada += OnFaseCambiada;
        }
    }

    void OnDestroy()
    {
        if (GestorHistoriaSyntro.Instance != null)
        {
            GestorHistoriaSyntro.Instance.OnFaseCambiada -= OnFaseCambiada;
        }
        if (Instance == this) Instance = null;
    }

    void OnFaseCambiada(FaseSyntro nuevaFase)
    {
        if (nuevaFase == FaseSyntro.Piso1_SalidaGradas || nuevaFase == FaseSyntro.Gradas_Subida)
        {
            ActivarCuadrosFlotantes();
        }
    }

    public void ActivarCuadrosFlotantes()
    {
        if (activo) return;
        activo = true;

        cuadrosFlotantes = new GameObject[cantidadCuadros];
        posicionesOriginales = new Vector3[cantidadCuadros];
        velocidadesFase = new float[cantidadCuadros];

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        Material marcoMat = new Material(litShader);
        marcoMat.color = new Color(0.12f, 0.1f, 0.08f);
        marcoMat.SetFloat("_Metallic", 0.5f);
        marcoMat.SetFloat("_Smoothness", 0.4f);

        Transform parentTransform = transform;

        for (int i = 0; i < cantidadCuadros; i++)
        {
            GameObject cuadro = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cuadro.name = $"CuadroFlotante_{i + 1}";
            cuadro.transform.SetParent(parentTransform);

            float posX = (i % 2 == 0) ? -1.9f : 1.9f;
            float posY = 1.6f + ((i % 3) * 0.5f);
            float posZ = Mathf.Lerp(rangoZMin, rangoZMax, (float)i / cantidadCuadros);

            posicionesOriginales[i] = new Vector3(posX, posY, posZ);
            cuadro.transform.localPosition = posicionesOriginales[i];
            cuadro.transform.localScale = new Vector3(0.12f, 1.3f, 1.0f);

            cuadro.GetComponent<Renderer>().sharedMaterial = marcoMat;

            // Desactivar colliders para que no bloqueen al jugador
            Collider col = cuadro.GetComponent<Collider>();
            if (col != null) Destroy(col);

            velocidadesFase[i] = Random.Range(0.8f, 2.2f);
            cuadrosFlotantes[i] = cuadro;
        }

        Debug.Log("[Syntro] Cuadros flotantes activados y moviéndose por todo el pasillo.");
    }

    void Update()
    {
        if (!activo || cuadrosFlotantes == null) return;

        float time = Time.time;
        for (int i = 0; i < cuadrosFlotantes.Length; i++)
        {
            if (cuadrosFlotantes[i] == null) continue;

            Vector3 posOrig = posicionesOriginales[i];
            float vel = velocidadesFase[i];

            // Movimiento oscilatorio dinámico a lo largo del pasillo (eje Z), altura (eje Y) y lateral (eje X)
            float offsetZ = Mathf.Sin(time * vel * 0.4f + i) * 3.5f;
            float offsetY = Mathf.Cos(time * vel * 0.7f + i) * 0.6f;
            float offsetX = Mathf.Sin(time * vel * 0.25f) * 0.4f;

            Vector3 nuevaPos = posOrig + new Vector3(offsetX, offsetY, offsetZ);
            cuadrosFlotantes[i].transform.localPosition = nuevaPos;

            // Rotación inquietante y errática
            cuadrosFlotantes[i].transform.localRotation = Quaternion.Euler(
                Mathf.Sin(time * vel * 0.8f) * 8f,
                Mathf.Cos(time * vel * 0.6f + i) * 15f,
                Mathf.Sin(time * vel * 0.5f) * 5f
            );
        }
    }
}
