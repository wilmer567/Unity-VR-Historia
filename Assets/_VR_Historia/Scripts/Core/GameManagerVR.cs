using System;
using System.Collections.Generic;
using UnityEngine;

public enum Capitulo
{
    Despertar = 1,
    Exploracion = 2,
    Revelacion = 3
}

[Serializable]
public class ProgressData
{
    public int capituloActual = 1;
    public List<string> flagsActivos = new List<string>();
    public string ultimaZona = "";
    public string fechaGuardado = "";
}

[Serializable]
public class DialogoEspacial
{
    public string id;
    public AudioClip clip;
    public string texto;
    [Range(1f, 30f)]
    public float alcance = 10f;
}

[Serializable]
public class EventoCapitulo
{
    public Capitulo capitulo;
    public string nombre;
    public AudioClip audioAmbiente;
    public DialogoEspacial[] dialogos;
    public string[] flagsRequeridos;
}

public class GameManagerVR : MonoBehaviour
{
    public static GameManagerVR Instance { get; private set; }

    [Header("Capitulos")]
    [SerializeField] private EventoCapitulo[] eventos;

    [Header("Audio")]
    [SerializeField] private AudioSource musicaAmbiente;
    [SerializeField] private AudioSource canalDialogo;

    [Header("Estado")]
    [SerializeField] private Capitulo capituloActual = Capitulo.Despertar;

    private readonly HashSet<string> flags = new HashSet<string>();
    private const string SaveKey = "VR_Historia_Progress";

    public Capitulo CapituloActual => capituloActual;

    public event Action<Capitulo> OnCapituloCambiado;
    public event Action<DialogoEspacial> OnDialogoIniciado;
    public event Action OnDialogoTerminado;
    public event Action<string> OnFlagActivado;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CargarProgreso();
    }

    void Start()
    {
        AplicarEstadoCapitulo(capituloActual);
    }

    public void AvanzarCapitulo()
    {
        int siguiente = (int)capituloActual + 1;
        if (siguiente > (int)Capitulo.Revelacion)
        {
            Debug.Log("[GameManagerVR] Ya se encuentra en el ultimo capitulo.");
            return;
        }
        EstablecerCapitulo((Capitulo)siguiente);
    }

    public void EstablecerCapitulo(Capitulo nuevo)
    {
        if (nuevo == capituloActual) return;

        capituloActual = nuevo;
        AplicarEstadoCapitulo(nuevo);
        GuardarProgreso();
        OnCapituloCambiado?.Invoke(nuevo);
        Debug.Log($"[GameManagerVR] Capitulo cambiado a: {nuevo}");
    }

    public void ActivarFlag(string flag)
    {
        if (flags.Add(flag))
        {
            OnFlagActivado?.Invoke(flag);
            GuardarProgreso();
        }
    }

    public bool TieneFlag(string flag) => flags.Contains(flag);

    public bool CumpleRequisitos(string[] flagsRequeridos)
    {
        if (flagsRequeridos == null || flagsRequeridos.Length == 0) return true;
        foreach (string f in flagsRequeridos)
        {
            if (!flags.Contains(f)) return false;
        }
        return true;
    }

    public void ReproducirDialogo(DialogoEspacial dialogo, Vector3 posicionOrigen)
    {
        if (dialogo == null || dialogo.clip == null) return;
        if (canalDialogo == null)
        {
            Debug.LogWarning("[GameManagerVR] No hay AudioSource asignado para dialogos.");
            return;
        }

        canalDialogo.transform.position = posicionOrigen;
        canalDialogo.spatialBlend = 1f;
        canalDialogo.maxDistance = dialogo.alcance;
        canalDialogo.clip = dialogo.clip;
        canalDialogo.Play();
        OnDialogoIniciado?.Invoke(dialogo);
    }

    public void DetenerDialogo()
    {
        if (canalDialogo != null && canalDialogo.isPlaying)
        {
            canalDialogo.Stop();
            OnDialogoTerminado?.Invoke();
        }
    }

    public void GuardarProgreso()
    {
        ProgressData data = new ProgressData
        {
            capituloActual = (int)capituloActual,
            flagsActivos = new List<string>(flags),
            fechaGuardado = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void CargarProgreso()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        string json = PlayerPrefs.GetString(SaveKey);
        ProgressData data = JsonUtility.FromJson<ProgressData>(json);
        if (data == null) return;

        capituloActual = (Capitulo)data.capituloActual;
        flags.Clear();
        foreach (string f in data.flagsActivos)
            flags.Add(f);
    }

    public void BorrarProgreso()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        flags.Clear();
        capituloActual = Capitulo.Despertar;
        AplicarEstadoCapitulo(capituloActual);
    }

    private void AplicarEstadoCapitulo(Capitulo cap)
    {
        EventoCapitulo evento = BuscarEvento(cap);
        if (evento == null) return;

        if (musicaAmbiente != null && evento.audioAmbiente != null)
        {
            musicaAmbiente.clip = evento.audioAmbiente;
            musicaAmbiente.loop = true;
            musicaAmbiente.Play();
        }
    }

    private EventoCapitulo BuscarEvento(Capitulo cap)
    {
        if (eventos == null) return null;
        foreach (EventoCapitulo e in eventos)
        {
            if (e.capitulo == cap) return e;
        }
        return null;
    }
}