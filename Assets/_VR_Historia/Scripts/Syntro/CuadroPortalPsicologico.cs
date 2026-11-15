using UnityEngine;

public class CuadroPortalPsicologico : MonoBehaviour
{
    [Header("Referencia")]
    public Transform camaraJugador;

    [Header("Seguimiento de Mirada")]
    [Range(0f, 1f)]
    public float intensidadSeguimiento = 0.3f;
    public float maxRotacion = 15f;
    public bool evitarMirada;

    [Header("Portal Hipnótico")]
    public float velocidadOscilacion = 1.2f;
    public float amplitudEscala = 0.08f;
    public float amplitudProfundidad = 0.15f;
    public float distanciaActivacion = 6f;

    [Header("Reposicionamiento")]
    public Transform[] posicionesPared;
    public float anguloFOVUmbral = 55f;
    public float intervaloMinReposicion = 5f;

    Renderer cuadroRenderer;
    Vector3 escalaOriginal;
    Quaternion rotacionBaseLocal;
    float faseOscilacion;
    float tiempoUltimoReposicion;
    int indicePosActual;
    bool eraVisible;

    void Start()
    {
        cuadroRenderer = GetComponent<Renderer>();
        escalaOriginal = transform.localScale;
        rotacionBaseLocal = transform.localRotation;
        faseOscilacion = Random.Range(0f, Mathf.PI * 2f);
        tiempoUltimoReposicion = Time.time;

        if (camaraJugador == null)
        {
            Camera cam = Camera.main;
            if (cam != null) camaraJugador = cam.transform;
        }
    }

    void Update()
    {
        if (camaraJugador == null) return;

        float distancia = Vector3.Distance(camaraJugador.position, transform.position);
        float proximidad = Mathf.Clamp01(1f - distancia / distanciaActivacion);

        AplicarSeguimientoMirada(proximidad);
        AplicarOscilacionPortal(proximidad);
        VerificarReposicion();
    }

    void AplicarSeguimientoMirada(float proximidad)
    {
        if (proximidad < 0.01f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation, rotacionBaseLocal, Time.deltaTime * 2f);
            return;
        }

        Vector3 dirHaciaCamara = (camaraJugador.position - transform.position).normalized;
        Vector3 dirLocal = transform.InverseTransformDirection(dirHaciaCamara);

        float rotX = Mathf.Clamp(dirLocal.y * maxRotacion, -maxRotacion, maxRotacion);
        float rotY = Mathf.Clamp(-dirLocal.x * maxRotacion, -maxRotacion, maxRotacion);

        if (evitarMirada)
        {
            rotX = -rotX;
            rotY = -rotY;
        }

        rotX *= proximidad;
        rotY *= proximidad;

        Quaternion rotObjetivo = rotacionBaseLocal * Quaternion.Euler(rotX, rotY, 0f);
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation, rotObjetivo, intensidadSeguimiento * Time.deltaTime * 3f);
    }

    void AplicarOscilacionPortal(float proximidad)
    {
        faseOscilacion += Time.deltaTime * velocidadOscilacion;

        float pulsoRapido = Mathf.Sin(faseOscilacion);
        float pulsoLento = Mathf.Sin(faseOscilacion * 0.618f);

        float intensidad = proximidad * proximidad;

        float escalaXY = 1f + pulsoRapido * amplitudEscala * intensidad;
        float escalaZ = 1f + pulsoLento * amplitudProfundidad * intensidad;

        transform.localScale = new Vector3(
            escalaOriginal.x * escalaXY,
            escalaOriginal.y * escalaXY,
            escalaOriginal.z * escalaZ);
    }

    void VerificarReposicion()
    {
        bool visibleAhora = EsVisibleParaJugador();

        if (eraVisible && !visibleAhora
            && posicionesPared != null && posicionesPared.Length > 1
            && Time.time - tiempoUltimoReposicion > intervaloMinReposicion)
        {
            Reposicionar();
            tiempoUltimoReposicion = Time.time;
        }

        eraVisible = visibleAhora;
    }

    bool EsVisibleParaJugador()
    {
        if (cuadroRenderer != null && !cuadroRenderer.isVisible)
            return false;

        Vector3 dirAlCuadro = (transform.position - camaraJugador.position).normalized;
        float angulo = Vector3.Angle(camaraJugador.forward, dirAlCuadro);
        return angulo < anguloFOVUmbral;
    }

    void Reposicionar()
    {
        int nuevo = indicePosActual;
        int intentos = 0;
        while (nuevo == indicePosActual && intentos < 20)
        {
            nuevo = Random.Range(0, posicionesPared.Length);
            intentos++;
        }

        indicePosActual = nuevo;
        Transform destino = posicionesPared[indicePosActual];

        transform.position = destino.position;
        rotacionBaseLocal = destino.localRotation;
        transform.localRotation = rotacionBaseLocal;
        escalaOriginal = destino.localScale;
        transform.localScale = escalaOriginal;
    }
}
