using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;

public class EscaleraParadojaEscher : MonoBehaviour
{
    [Header("Referencias Escena")]
    public Transform contenedorGradas;
    public Transform jugador;
    public Collider triggerDescanso;

    [Header("Distorsión Sutil Durante Subida")]
    [Tooltip("Inclinación máxima en X (forward/back tilt) en grados")]
    public float maxInclinacionX = 2f;
    [Tooltip("Inclinación máxima en Z (lateral tilt) en grados")]
    public float maxInclinacionZ = 1.2f;
    public float velocidadInclinacion = 0.4f;
    [Tooltip("Velocidad del vaivén sinusoidal adicional")]
    public float frecuenciaVaiven = 0.7f;

    [Header("Inversión Perceptual en Descanso")]
    [Tooltip("Grados de rotación brusca al llegar al descanso (progresivo por ciclo)")]
    public float rotacionInversionBase = 12f;
    public float rotacionInversionMaxima = 28f;
    public float duracionInversion = 0.7f;

    [Header("Háptica VR")]
    public float amplitudHapticaBase = 0.5f;
    public float duracionHaptica = 0.6f;

    [Header("Audio Distorsión")]
    public AudioClip sonidoDistorsionGrave;
    [Range(0f, 1f)]
    public float volumenDistorsion = 0.65f;

    [Header("Ciclos")]
    public int ciclosParaLiberar = 3;

    int cicloActual;
    bool jugadorEnGradas;
    bool inversionEnCurso;
    Quaternion rotacionOriginal;
    AudioSource audioSource;

    float inclinacionActualX;
    float inclinacionActualZ;
    float inclinacionObjetivoX;
    float inclinacionObjetivoZ;
    float tiempoSubida;
    float faseVaiven;

    Quaternion invRotInicio;
    Quaternion invRotPico;
    Quaternion invRotFinal;
    float invProgreso;

    void Start()
    {
        if (contenedorGradas != null)
            rotacionOriginal = contenedorGradas.localRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 25f;

        if (jugador == null)
        {
            Camera cam = Camera.main;
            if (cam != null) jugador = cam.transform;
        }

        faseVaiven = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (contenedorGradas == null || jugador == null) return;

        if (cicloActual >= ciclosParaLiberar)
        {
            RestaurarSuave();
            return;
        }

        if (inversionEnCurso)
        {
            ActualizarInversion();
            return;
        }

        if (jugadorEnGradas)
            ActualizarDistorsionSubida();
    }

    public void JugadorEntroGradas()
    {
        if (cicloActual >= ciclosParaLiberar) return;

        jugadorEnGradas = true;
        tiempoSubida = 0f;

        float factorCiclo = 1f + cicloActual * 0.6f;
        inclinacionObjetivoX = Random.Range(0.4f, 1f) * maxInclinacionX * factorCiclo;
        inclinacionObjetivoZ = Random.Range(-1f, 1f) * maxInclinacionZ * factorCiclo;

        if (Random.value > 0.5f)
            inclinacionObjetivoX = -inclinacionObjetivoX;
    }

    public void JugadorLlegoDescanso()
    {
        if (cicloActual >= ciclosParaLiberar || inversionEnCurso) return;
        IniciarInversion();
    }

    public void JugadorSalioGradas()
    {
        jugadorEnGradas = false;
    }

    void ActualizarDistorsionSubida()
    {
        tiempoSubida += Time.deltaTime;

        float t = velocidadInclinacion * Time.deltaTime;
        inclinacionActualX = Mathf.Lerp(inclinacionActualX, inclinacionObjetivoX, t);
        inclinacionActualZ = Mathf.Lerp(inclinacionActualZ, inclinacionObjetivoZ, t);

        float wobbleX = Mathf.Sin(tiempoSubida * frecuenciaVaiven + faseVaiven) * 0.4f * (1 + cicloActual);
        float wobbleZ = Mathf.Cos(tiempoSubida * frecuenciaVaiven * 0.8f + faseVaiven) * 0.25f * (1 + cicloActual);

        Quaternion distorsion = Quaternion.Euler(
            inclinacionActualX + wobbleX,
            0f,
            inclinacionActualZ + wobbleZ);

        contenedorGradas.localRotation = rotacionOriginal * distorsion;
    }

    void IniciarInversion()
    {
        inversionEnCurso = true;
        invProgreso = 0f;

        invRotInicio = contenedorGradas.localRotation;

        float progresoCiclo = (float)(cicloActual + 1) / ciclosParaLiberar;
        float anguloInv = Mathf.Lerp(rotacionInversionBase, rotacionInversionMaxima, progresoCiclo);

        float dirX = Random.value > 0.5f ? 1f : -1f;
        float dirZ = Random.Range(-0.5f, 0.5f);
        invRotPico = rotacionOriginal * Quaternion.Euler(anguloInv * dirX, 0f, anguloInv * dirZ * 0.4f);

        float residuoX = Random.Range(-0.8f, 0.8f) * (cicloActual + 1);
        float residuoZ = Random.Range(-0.4f, 0.4f) * (cicloActual + 1);
        invRotFinal = rotacionOriginal * Quaternion.Euler(residuoX, 0f, residuoZ);

        EnviarHapticaAmbosControladores();

        if (sonidoDistorsionGrave != null)
            audioSource.PlayOneShot(sonidoDistorsionGrave, volumenDistorsion);
    }

    void ActualizarInversion()
    {
        invProgreso += Time.deltaTime / duracionInversion;

        if (invProgreso < 0.35f)
        {
            float t = invProgreso / 0.35f;
            t = t * t * (3f - 2f * t);
            contenedorGradas.localRotation = Quaternion.Slerp(invRotInicio, invRotPico, t);
        }
        else if (invProgreso < 0.55f)
        {
            contenedorGradas.localRotation = invRotPico;
        }
        else if (invProgreso < 1f)
        {
            float t = (invProgreso - 0.55f) / 0.45f;
            t = t * t * (3f - 2f * t);
            contenedorGradas.localRotation = Quaternion.Slerp(invRotPico, invRotFinal, t);
        }
        else
        {
            contenedorGradas.localRotation = invRotFinal;
            inversionEnCurso = false;
            cicloActual++;

            inclinacionActualX = 0f;
            inclinacionActualZ = 0f;

            if (cicloActual >= ciclosParaLiberar)
                StartCoroutine(RestaurarConRetardo());
        }
    }

    IEnumerator RestaurarConRetardo()
    {
        yield return new WaitForSeconds(0.5f);

        float duracion = 1.5f;
        float elapsed = 0f;
        Quaternion desde = contenedorGradas.localRotation;

        while (elapsed < duracion)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duracion;
            t = t * t * (3f - 2f * t);
            contenedorGradas.localRotation = Quaternion.Slerp(desde, rotacionOriginal, t);
            yield return null;
        }

        contenedorGradas.localRotation = rotacionOriginal;
    }

    void RestaurarSuave()
    {
        if (Quaternion.Angle(contenedorGradas.localRotation, rotacionOriginal) < 0.01f)
        {
            contenedorGradas.localRotation = rotacionOriginal;
            return;
        }

        contenedorGradas.localRotation = Quaternion.Slerp(
            contenedorGradas.localRotation, rotacionOriginal, Time.deltaTime * 2f);
    }

    void EnviarHapticaAmbosControladores()
    {
        EnviarHaptica(InputDeviceRole.LeftHanded);
        EnviarHaptica(InputDeviceRole.RightHanded);
    }

    void EnviarHaptica(InputDeviceRole role)
    {
        var dispositivos = new List<InputDevice>();
        InputDeviceCharacteristics characteristics = role == InputDeviceRole.LeftHanded
            ? (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left)
            : (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right);
        InputDevices.GetDevicesWithCharacteristics(characteristics, dispositivos);

        float amplitud = Mathf.Clamp01(amplitudHapticaBase * (1f + cicloActual * 0.35f));
        foreach (var d in dispositivos)
            d.SendHapticImpulse(0, amplitud, duracionHaptica);
    }

    public void Reiniciar()
    {
        StopAllCoroutines();
        cicloActual = 0;
        jugadorEnGradas = false;
        inversionEnCurso = false;
        inclinacionActualX = 0f;
        inclinacionActualZ = 0f;

        if (contenedorGradas != null)
            contenedorGradas.localRotation = rotacionOriginal;
    }
}
