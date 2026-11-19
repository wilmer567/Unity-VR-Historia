using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class SimulacionCompletaSyntro : MonoBehaviour
{
    [Header("=== Movimiento Jugador ===")]
    public float velocidadCaminar = 3.2f;
    public float velocidadCorrer = 5.5f;
    public float sensibilidadMouse = 1.8f;
    public float gravedad = -15f;
    public float alturaOjos = 1.65f;

    [Header("=== Referencias HUD ===")]
    public CanvasGroup panelFade;
    public CanvasGroup panelMensaje;
    public TextMeshProUGUI textoMensaje;
    public TextMeshProUGUI textoObjetivos;
    public TextMeshProUGUI textoInteraccion;

    [Header("=== Audio Intro ===")]
    public AudioClip latidoIntroClip;

    [Header("=== Puntos de Referencia ===")]
    public Transform spawnInicial;
    public Transform puntoServidorFinal;

    // ==========================================
    //  ESTADO INTERNO
    // ==========================================

    private CharacterController controlador;
    private Transform jugadorTransform;
    private Transform camaraTransform;
    private float rotX;
    private float rotY;
    private float velVertical;

    private bool simulacionActiva;
    private bool movimientoBloqueado;
    private Coroutine corutinaMensaje;

    private bool rackLocalizado;
    private bool errorLeido;
    private bool reinicioHecho;
    private bool labEncontrado;
    private bool pcInvestigada;
    private bool gradasDisparoListo = true;
    private float alturaDisparoGradas = 3.8f;

    private enum FaseFinal { Ninguna, Transicion, EnServidor, Fin }
    private FaseFinal faseFinal = FaseFinal.Ninguna;

    // ==========================================
    //  ZONAS DE INTERACCION
    // ==========================================

    private class Zona
    {
        public string nombre;
        public Vector3 posicion;
        public float radio;
        public bool requiereInput;
        public FaseSyntro faseRequerida;
        public string prompt;
        public System.Action accion;
        public System.Func<bool> condicion;
        public bool disparado;
    }

    private List<Zona> zonas = new List<Zona>();
    private Zona zonaActiva;

    // ==========================================
    //  INICIO
    // ==========================================

    void Start()
    {
        CrearJugador();
        RegistrarZonas();
        ConectarEventos();

        if (panelFade != null) panelFade.alpha = 1f;
        simulacionActiva = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(IntroduccionFade());
    }

    IEnumerator IntroduccionFade()
    {
        movimientoBloqueado = true;

        if (panelFade != null) panelFade.alpha = 1f;

        // Silencio tenso — 2 segundos en negro total
        yield return new WaitForSeconds(2f);

        // Latido cardíaco lejano
        AudioSource latido = gameObject.AddComponent<AudioSource>();
        latido.spatialBlend = 0f;
        latido.volume = 0f;
        latido.loop = true;
        if (latidoIntroClip != null) { latido.clip = latidoIntroClip; latido.pitch = 0.6f; latido.Play(); }

        // Fade del latido creciente durante la oscuridad
        float t = 0f;
        while (t < 3f)
        {
            t += Time.deltaTime;
            float p = t / 3f;
            latido.volume = Mathf.Lerp(0f, 0.35f, p);
            latido.pitch = Mathf.Lerp(0.6f, 0.85f, p);
            yield return null;
        }

        // Respiración agitada (texto)
        MostrarMensajeHUD("...");
        yield return new WaitForSeconds(1.5f);

        // Primer parpadeo — abrir los ojos brevemente
        yield return FadePantalla(1f, 0.7f, 0.3f);
        yield return new WaitForSeconds(0.15f);
        yield return FadePantalla(0.7f, 1f, 0.2f);
        yield return new WaitForSeconds(0.8f);

        // Segundo parpadeo — más largo
        yield return FadePantalla(1f, 0.4f, 0.4f);
        yield return new WaitForSeconds(0.3f);
        yield return FadePantalla(0.4f, 0.9f, 0.3f);
        yield return new WaitForSeconds(0.5f);

        // Tercer parpadeo — los ojos se abren
        yield return FadePantalla(0.9f, 0.15f, 0.6f);
        yield return new WaitForSeconds(0.2f);
        yield return FadePantalla(0.15f, 0.5f, 0.2f);
        yield return new WaitForSeconds(0.3f);

        // Ojos completamente abiertos
        yield return FadePantalla(0.5f, 0f, 1.5f);

        // Latido se calma
        t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            latido.volume = Mathf.Lerp(0.35f, 0.08f, t / 2f);
            latido.pitch = Mathf.Lerp(0.85f, 0.7f, t / 2f);
            yield return null;
        }

        MostrarMensajeHUD("¿Dónde... estoy?");
        yield return new WaitForSeconds(3f);

        // Activar el terror psicológico sutilmente
        if (TerrorPsychologyManager.Instance != null)
            TerrorPsychologyManager.Instance.PulsoTerror(0.08f, 2f);

        movimientoBloqueado = false;

        // Eliminar el AudioSource temporal
        StartCoroutine(FadeOutYDestruir(latido, 5f));
    }

    IEnumerator FadeOutYDestruir(AudioSource source, float duracion)
    {
        float volInicial = source.volume;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(volInicial, 0f, t / duracion);
            yield return null;
        }
        source.Stop();
        Destroy(source);
    }

    // ==========================================
    //  CREACION DEL JUGADOR
    // ==========================================

    void CrearJugador()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        camaraTransform = cam.transform;

        GameObject jugadorObj = new GameObject("Jugador_Syntro");
        jugadorObj.tag = "Player";
        jugadorObj.layer = 0;

        Vector3 spawn = spawnInicial != null
            ? spawnInicial.position
            : new Vector3(0f, 0.1f, -2f);
        jugadorObj.transform.position = spawn;

        camaraTransform.SetParent(jugadorObj.transform);
        camaraTransform.localPosition = new Vector3(0f, alturaOjos, 0f);
        camaraTransform.localRotation = Quaternion.identity;

        controlador = jugadorObj.AddComponent<CharacterController>();
        controlador.height = 1.8f;
        controlador.radius = 0.28f;
        controlador.center = new Vector3(0f, 0.9f, 0f);
        controlador.slopeLimit = 55f;
        controlador.stepOffset = 0.35f;
        controlador.skinWidth = 0.04f;

        jugadorTransform = jugadorObj.transform;
        rotY = jugadorObj.transform.eulerAngles.y;
    }

    // ==========================================
    //  UPDATE PRINCIPAL
    // ==========================================

    void Update()
    {
        if (!simulacionActiva) return;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            // Cheat / Debug: F2 para ir al Segundo Piso / Gradas
            if (kb.f2Key.wasPressedThisFrame)
            {
                Vector3 destinoPiso2 = new Vector3(8f, 4.2f, 2f);
                TeletransportarJugador(destinoPiso2);
                Debug.Log("[Syntro] Teletransporte de depuración al Segundo Piso / Gradas activado.");
            }

            // Cheat / Debug: F3 para saltar el bucle de gradas y subir al Piso 3
            if (kb.f3Key.wasPressedThisFrame)
            {
                if (TrampaGradasSyntro.Instance != null)
                {
                    TrampaGradasSyntro.Instance.ForzarCompletarBucle();
                    Debug.Log("[Syntro] Bucle de gradas forzado a completarse mediante F3.");
                }
                else
                {
                    Vector3 destinoPiso3 = new Vector3(0f, 8.3f, 7.5f);
                    TeletransportarJugador(destinoPiso3);
                    Debug.Log("[Syntro] Teletransporte directo al Piso 3 mediante F3.");
                }
            }
        }

        ActualizarMovimiento();
        ActualizarZonas();
        ActualizarTriggerGradas();
        ActualizarHUD();
    }

    // ==========================================
    //  MOVIMIENTO PRIMERA PERSONA
    // ==========================================

    void ActualizarMovimiento()
    {
        if (controlador == null || movimientoBloqueado) return;

        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb == null && mouse == null) return;

        if (mouse != null)
        {
            Vector2 delta = mouse.delta.ReadValue();
            rotY += delta.x * sensibilidadMouse * 0.08f;
            rotX -= delta.y * sensibilidadMouse * 0.08f;
            rotX = Mathf.Clamp(rotX, -85f, 85f);

            jugadorTransform.rotation = Quaternion.Euler(0f, rotY, 0f);
            camaraTransform.localRotation = Quaternion.Euler(rotX, 0f, 0f);
        }

        Vector3 movimiento = Vector3.zero;
        if (kb != null)
        {
            Vector2 input = Vector2.zero;
            if (kb.wKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed) input.y -= 1f;
            if (kb.aKey.isPressed) input.x -= 1f;
            if (kb.dKey.isPressed) input.x += 1f;
            input = Vector2.ClampMagnitude(input, 1f);

            bool correr = kb.leftShiftKey.isPressed;
            float vel = correr ? velocidadCorrer : velocidadCaminar;

            movimiento = jugadorTransform.right * input.x + jugadorTransform.forward * input.y;
            movimiento.y = 0f;
            movimiento = movimiento.normalized * vel;
        }

        if (controlador.isGrounded)
            velVertical = -2f;
        else
            velVertical += gravedad * Time.deltaTime;

        movimiento.y = velVertical;
        controlador.Move(movimiento * Time.deltaTime);

        if (jugadorTransform.position.y < -5f)
            TeletransportarJugador(spawnInicial != null ? spawnInicial.position : Vector3.up);
    }

    void TeletransportarJugador(Vector3 destino)
    {
        controlador.enabled = false;
        jugadorTransform.position = destino;
        controlador.enabled = true;
        velVertical = 0f;
    }

    // ==========================================
    //  REGISTRO DE ZONAS DE INTERACCION
    // ==========================================

    void RegistrarZonas()
    {
        Transform rack1 = BuscarObjeto("Rack_Servidor_1");
        Transform terminal = BuscarObjeto("Terminal_Mantenimiento");
        Transform puerta = BuscarObjeto("Puerta_Pasillo");

        Vector3 posRack = rack1 != null ? rack1.position : new Vector3(3f, 1f, 2f);
        Vector3 posTerminal = terminal != null ? terminal.position : new Vector3(-3f, 1f, -2f);
        Vector3 posPuerta = puerta != null ? puerta.position : new Vector3(0f, 1.5f, 4f);

        // --- PISO 1 ---

        zonas.Add(new Zona
        {
            nombre = "Rack Servidor",
            posicion = posRack,
            radio = 2.5f,
            requiereInput = true,
            faseRequerida = FaseSyntro.Piso1_Exploracion,
            prompt = "[E] Examinar rack del servidor",
            accion = () =>
            {
                rackLocalizado = true;
                GestorHistoriaSyntro.Instance?.Evento_RackLocalizado();
            }
        });

        zonas.Add(new Zona
        {
            nombre = "Terminal Mantenimiento",
            posicion = posTerminal,
            radio = 2f,
            requiereInput = true,
            faseRequerida = FaseSyntro.Piso1_Exploracion,
            prompt = "[E] Leer terminal de mantenimiento",
            condicion = () => rackLocalizado,
            accion = () =>
            {
                errorLeido = true;
                GestorHistoriaSyntro.Instance?.Evento_ErrorLeido();
            }
        });

        zonas.Add(new Zona
        {
            nombre = "Reiniciar Rack",
            posicion = posRack + Vector3.up * 0.5f,
            radio = 2.2f,
            requiereInput = true,
            faseRequerida = FaseSyntro.Piso1_ErrorServidor,
            prompt = "[E] Reiniciar rack manualmente",
            accion = () =>
            {
                reinicioHecho = true;
                GestorHistoriaSyntro.Instance?.Evento_ReinicioManual();
            }
        });

        zonas.Add(new Zona
        {
            nombre = "Puerta Gradas",
            posicion = posPuerta,
            radio = 1.8f,
            requiereInput = false,
            faseRequerida = FaseSyntro.Piso1_SalidaGradas,
            accion = () =>
            {
                GestorHistoriaSyntro.Instance?.Evento_JugadorEntraGradas();
            }
        });

        // --- PISO 3 ---

        Transform piso3 = BuscarObjeto("Piso3_Laboratorio");
        Vector3 basePiso3 = piso3 != null ? piso3.position : new Vector3(0f, 8f, 18f);

        zonas.Add(new Zona
        {
            nombre = "Entrada Lab",
            posicion = basePiso3 + new Vector3(0f, 0.5f, 1.5f),
            radio = 3.5f,
            requiereInput = false,
            faseRequerida = FaseSyntro.Piso3_Exploracion,
            accion = () =>
            {
                labEncontrado = true;
                GestorHistoriaSyntro.Instance?.Evento_LaboratorioEncontrado();
            }
        });

        zonas.Add(new Zona
        {
            nombre = "Computadora Anomala",
            posicion = basePiso3 + new Vector3(0f, 1f, 5f),
            radio = 2.5f,
            requiereInput = true,
            faseRequerida = FaseSyntro.Piso3_Exploracion,
            prompt = "[E] Investigar computadora",
            condicion = () => labEncontrado,
            accion = () =>
            {
                pcInvestigada = true;
                GestorHistoriaSyntro.Instance?.Evento_ComputadoraInvestigada();
            }
        });

        zonas.Add(new Zona
        {
            nombre = "Transmision Anomala",
            posicion = basePiso3 + new Vector3(0f, 1f, 5.5f),
            radio = 3f,
            requiereInput = true,
            faseRequerida = FaseSyntro.Piso3_AnomaliaFinal,
            prompt = "[E] Cortar transmision anomala",
            accion = () =>
            {
                GestorHistoriaSyntro.Instance?.Evento_TransmisionCortada();
            }
        });
    }

    // ==========================================
    //  DETECCION DE ZONAS
    // ==========================================

    void ActualizarZonas()
    {
        if (jugadorTransform == null) return;
        var gestor = GestorHistoriaSyntro.Instance;
        if (gestor == null) return;

        zonaActiva = null;

        foreach (var zona in zonas)
        {
            if (zona.disparado) continue;
            if (gestor.FaseActual != zona.faseRequerida) continue;
            if (zona.condicion != null && !zona.condicion()) continue;

            float dist = Vector3.Distance(jugadorTransform.position, zona.posicion);
            if (dist > zona.radio) continue;

            if (zona.requiereInput)
            {
                zonaActiva = zona;

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    zona.accion?.Invoke();
                    zona.disparado = true;
                    zonaActiva = null;
                }
            }
            else
            {
                zona.accion?.Invoke();
                zona.disparado = true;
            }
        }
    }

    // ==========================================
    //  TRIGGER GRADAS (POR ALTURA)
    // ==========================================

    void ActualizarTriggerGradas()
    {
        if (jugadorTransform == null) return;
        var gestor = GestorHistoriaSyntro.Instance;
        if (gestor == null) return;
        if (gestor.FaseActual != FaseSyntro.Gradas_BuclePsicologico) return;
        if (TrampaGradasSyntro.Instance == null) return;

        float y = jugadorTransform.position.y;

        if (y < 1f) gradasDisparoListo = true;

        if (y > alturaDisparoGradas && gradasDisparoListo)
        {
            gradasDisparoListo = false;
            TrampaGradasSyntro.Instance.JugadorLlegoAlDescanso();

            var escher = FindAnyObjectByType<EscaleraParadojaEscher>();
            if (escher != null) escher.JugadorLlegoDescanso();
        }
    }

    // ==========================================
    //  HUD
    // ==========================================

    void ActualizarHUD()
    {
        if (textoInteraccion != null)
        {
            textoInteraccion.text = zonaActiva != null ? zonaActiva.prompt : "";
        }

        if (textoObjetivos != null)
        {
            var gestor = GestorHistoriaSyntro.Instance;
            if (gestor != null)
                textoObjetivos.text = ObtenerTextoObjetivos(gestor.FaseActual);
        }
    }

    string ObtenerTextoObjetivos(FaseSyntro fase)
    {
        switch (fase)
        {
            case FaseSyntro.Piso1_Inicio:
                return "TURNO NOCTURNO\n\nEsperando instrucciones...";
            case FaseSyntro.Piso1_Exploracion:
                return "PISO 1 - CUARTO DE MAQUINAS\n\n"
                    + (rackLocalizado ? "<color=#55FF55>[+]</color>" : "[ ]") + " Localizar rack averiado\n"
                    + (errorLeido ? "<color=#55FF55>[+]</color>" : "[ ]") + " Leer error en terminal\n"
                    + "[ ] Reiniciar rack";
            case FaseSyntro.Piso1_ErrorServidor:
                return "PISO 1 - ERROR CRITICO\n\n"
                    + "<color=#55FF55>[+]</color> Rack localizado\n"
                    + "<color=#55FF55>[+]</color> Error identificado\n"
                    + (reinicioHecho ? "<color=#55FF55>[+]</color>" : "[ ]") + " Reiniciar rack manualmente";
            case FaseSyntro.Piso1_ReinicioManual:
            case FaseSyntro.Piso1_SalidaGradas:
                return "PISO 1 - SALIDA\n\n"
                    + "Dirígete a las gradas.\nSube al tercer piso.";
            case FaseSyntro.Gradas_Subida:
            case FaseSyntro.Gradas_BuclePsicologico:
                return "GRADAS\n\nSube al tercer piso.";
            case FaseSyntro.Gradas_Completado:
                return "GRADAS\n\n<color=#55FF55>Piso 3 alcanzado.</color>";
            case FaseSyntro.Piso3_Entrada:
            case FaseSyntro.Piso3_Exploracion:
                return "PISO 3 - LABORATORIO\n\n"
                    + (labEncontrado ? "<color=#55FF55>[+]</color>" : "[ ]") + " Encontrar laboratorio\n"
                    + (pcInvestigada ? "<color=#55FF55>[+]</color>" : "[ ]") + " Investigar computadora\n"
                    + "[ ] Detener transmision";
            case FaseSyntro.Piso3_AnomaliaFinal:
                return "PISO 3 - ANOMALIA\n\n"
                    + "<color=#FF5555>Corta la transmision.</color>";
            case FaseSyntro.Piso3_Climax:
                return "";
            case FaseSyntro.Completado:
                return "";
            default:
                return "";
        }
    }

    void MostrarMensajeHUD(string texto)
    {
        if (textoMensaje == null || panelMensaje == null) return;

        if (corutinaMensaje != null) StopCoroutine(corutinaMensaje);
        corutinaMensaje = StartCoroutine(AnimarMensaje(texto));
    }

    IEnumerator AnimarMensaje(string texto)
    {
        textoMensaje.text = texto;

        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            panelMensaje.alpha = Mathf.Lerp(0f, 1f, t / 0.4f);
            yield return null;
        }
        panelMensaje.alpha = 1f;

        float espera = Mathf.Clamp(texto.Length * 0.06f, 3f, 8f);
        yield return new WaitForSeconds(espera);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            panelMensaje.alpha = Mathf.Lerp(1f, 0f, t / 1f);
            yield return null;
        }
        panelMensaje.alpha = 0f;
    }

    // ==========================================
    //  CONEXION CON GESTOR DE HISTORIA
    // ==========================================

    void ConectarEventos()
    {
        var gestor = GestorHistoriaSyntro.Instance;
        if (gestor == null)
        {
            gestor = FindAnyObjectByType<GestorHistoriaSyntro>();
        }
        if (gestor == null)
        {
            Debug.LogWarning("[SimulacionSyntro] GestorHistoriaSyntro no encontrado.");
            return;
        }

        gestor.OnMensaje += MostrarMensajeHUD;
        gestor.OnFaseCambiada += OnFaseCambiada;
        gestor.OnHistoriaCompleta += OnHistoriaCompleta;

        InyectarReferencias(gestor);
    }

    void InyectarReferencias(GestorHistoriaSyntro gestor)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var tipo = gestor.GetType();

        if (CampoEsNull(gestor, "puertaHaciaGradas"))
        {
            Transform puerta = BuscarObjeto("Puerta_Pasillo");
            if (puerta != null)
                tipo.GetField("puertaHaciaGradas", flags)?.SetValue(gestor, puerta.gameObject);
        }

        if (CampoEsNull(gestor, "musicaSource"))
        {
            AudioSource musica = ObtenerOCrearAudio(gestor.gameObject, "MusicaSource_Syntro");
            tipo.GetField("musicaSource", flags)?.SetValue(gestor, musica);
        }

        if (CampoEsNull(gestor, "sfxSource"))
        {
            AudioSource sfx = ObtenerOCrearAudio(gestor.gameObject, "SFXSource_Syntro");
            tipo.GetField("sfxSource", flags)?.SetValue(gestor, sfx);
        }

        var trampa = TrampaGradasSyntro.Instance ?? FindAnyObjectByType<TrampaGradasSyntro>();
        if (trampa != null)
        {
            var tipoTrampa = trampa.GetType();

            if (panelFade != null && CampoEsNull(trampa, "panelFadeNegro"))
                tipoTrampa.GetField("panelFadeNegro", flags)?.SetValue(trampa, panelFade);

            if (CampoEsNull(trampa, "puntoRetornoInferior"))
            {
                Transform retorno = BuscarObjeto("PuntoRetorno_Gradas");
                if (retorno != null)
                    tipoTrampa.GetField("puntoRetornoInferior", flags)?.SetValue(trampa, retorno);
            }

            if (CampoEsNull(trampa, "puntoLlegadaPiso3"))
            {
                Transform llegada = BuscarObjeto("PuntoLlegada_Piso3");
                if (llegada != null)
                    tipoTrampa.GetField("puntoLlegadaPiso3", flags)?.SetValue(trampa, llegada);
            }
        }

        var escher = FindAnyObjectByType<EscaleraParadojaEscher>();
        if (escher != null)
        {
            var tipoEscher = escher.GetType();
            Transform bloque = BuscarObjeto("Bloque_Gradas_Terror");
            if (bloque != null && CampoEsNull(escher, "contenedorGradas"))
                tipoEscher.GetField("contenedorGradas", flags)?.SetValue(escher, bloque);
        }
    }

    bool CampoEsNull(object obj, string campo)
    {
        var field = obj.GetType().GetField(campo,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);
        if (field == null) return true;
        return field.GetValue(obj) == null;
    }

    AudioSource ObtenerOCrearAudio(GameObject go, string nombre)
    {
        Transform hijo = go.transform.Find(nombre);
        if (hijo != null)
        {
            AudioSource src = hijo.GetComponent<AudioSource>();
            if (src != null) return src;
        }
        GameObject audioObj = new GameObject(nombre);
        audioObj.transform.SetParent(go.transform);
        audioObj.transform.localPosition = Vector3.zero;
        return audioObj.AddComponent<AudioSource>();
    }

    // ==========================================
    //  HANDLERS DE EVENTOS
    // ==========================================

    void OnFaseCambiada(FaseSyntro fase)
    {
        if (fase == FaseSyntro.Gradas_Subida)
        {
            var escher = FindAnyObjectByType<EscaleraParadojaEscher>();
            if (escher != null) escher.JugadorEntroGradas();
        }
    }

    void OnHistoriaCompleta()
    {
        if (faseFinal != FaseFinal.Ninguna) return;
        StartCoroutine(SecuenciaFinalServidor());
    }

    // ==========================================
    //  SECUENCIA FINAL: CUARTO DE SERVIDORES
    // ==========================================

    IEnumerator SecuenciaFinalServidor()
    {
        faseFinal = FaseFinal.Transicion;
        movimientoBloqueado = true;

        yield return new WaitForSeconds(3f);
        yield return FadePantalla(0f, 1f, 2.5f);

        Vector3 destino = puntoServidorFinal != null
            ? puntoServidorFinal.position
            : new Vector3(0f, 0.15f, 0f);
        TeletransportarJugador(destino);

        rotY = 0f;
        rotX = 0f;
        jugadorTransform.rotation = Quaternion.identity;
        camaraTransform.localRotation = Quaternion.identity;

        yield return new WaitForSeconds(1.5f);

        faseFinal = FaseFinal.EnServidor;
        movimientoBloqueado = false;

        yield return FadePantalla(1f, 0f, 4f);

        MostrarMensajeHUD("Vuelves al cuarto de servidores... algo ha cambiado.");

        yield return new WaitForSeconds(12f);

        if (ControladorSentidosVR.Instance != null)
            ControladorSentidosVR.Instance.ForzarAlerta(0.6f);

        MostrarMensajeHUD("No estas solo aqui.");

        yield return new WaitForSeconds(6f);

        if (ControladorSentidosVR.Instance != null)
            ControladorSentidosVR.Instance.DisparoDeEmergencia();

        yield return new WaitForSeconds(4f);

        movimientoBloqueado = true;
        yield return FadePantalla(0f, 1f, 3f);

        faseFinal = FaseFinal.Fin;

        MostrarPantallaFin();
    }

    void MostrarPantallaFin()
    {
        simulacionActiva = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (textoMensaje != null && panelMensaje != null)
        {
            textoMensaje.text = "FIN\n\nSyntro — Capitulo 1: El Despertar";
            textoMensaje.fontSize = 36;
            textoMensaje.fontStyle = FontStyles.Bold;
            textoMensaje.alignment = TextAlignmentOptions.Center;
            panelMensaje.alpha = 1f;

            var rect = panelMensaje.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.15f, 0.3f);
                rect.anchorMax = new Vector2(0.85f, 0.7f);
            }
        }

        if (textoObjetivos != null) textoObjetivos.text = "";
        if (textoInteraccion != null) textoInteraccion.text = "";
    }

    // ==========================================
    //  UTILIDADES
    // ==========================================

    IEnumerator FadePantalla(float desde, float hasta, float duracion)
    {
        if (panelFade == null) yield break;

        panelFade.gameObject.SetActive(true);
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            panelFade.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        panelFade.alpha = hasta;

        if (hasta <= 0.01f)
            panelFade.blocksRaycasts = false;
        else
            panelFade.blocksRaycasts = true;
    }

    Transform BuscarObjeto(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        return go != null ? go.transform : null;
    }

    void OnDestroy()
    {
        var gestor = GestorHistoriaSyntro.Instance;
        if (gestor != null)
        {
            gestor.OnMensaje -= MostrarMensajeHUD;
            gestor.OnFaseCambiada -= OnFaseCambiada;
            gestor.OnHistoriaCompleta -= OnHistoriaCompleta;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
