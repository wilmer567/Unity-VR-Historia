using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingConfigurator : MonoBehaviour
{
    [Header("Volume a configurar")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private bool configurarAlIniciar = true;

    [Header("=== Tonemapping ===")]
    [SerializeField] private bool aces = true;

    [Header("=== Bloom ===")]
    [SerializeField] private float bloomIntensity = 1f;
    [SerializeField] private float bloomThreshold = 0.8f;
    [SerializeField] private float bloomScatter = 0.7f;

    [Header("=== Vignette Base ===")]
    [SerializeField] private float vignetteIntensity = 0.35f;
    [SerializeField] private float vignetteSmoothness = 0.3f;

    [Header("=== Color Adjustments ===")]
    [SerializeField] private float postExposure = 0.1f;
    [SerializeField] private float contrast = 15f;
    [SerializeField] private float saturation = -10f;
    [SerializeField] private Color colorFilter = new Color(0.9f, 0.92f, 1f);

    [Header("=== Film Grain ===")]
    [SerializeField] private float filmGrainIntensity = 0.15f;

    [Header("=== Chromatic Aberration ===")]
    [SerializeField] private float chromaticIntensity = 0.08f;

    [Header("=== Motion Blur (VR: desactivar) ===")]
    [SerializeField] private bool motionBlurActivo;

    void Start()
    {
        if (configurarAlIniciar)
            Configurar();
    }

    public void Configurar()
    {
        if (globalVolume == null)
        {
            globalVolume = FindAnyObjectByType<Volume>();
            if (globalVolume == null) return;
        }

        VolumeProfile profile = globalVolume.profile;

        if (profile.TryGet(out Tonemapping tonemapping))
        {
            tonemapping.active = true;
            tonemapping.mode.value = aces ? TonemappingMode.ACES : TonemappingMode.Neutral;
            tonemapping.mode.overrideState = true;
        }

        if (profile.TryGet(out Bloom bloom))
        {
            bloom.active = true;
            bloom.intensity.value = bloomIntensity;
            bloom.intensity.overrideState = true;
            bloom.threshold.value = bloomThreshold;
            bloom.threshold.overrideState = true;
            bloom.scatter.value = bloomScatter;
            bloom.scatter.overrideState = true;
        }

        if (profile.TryGet(out Vignette vignette))
        {
            vignette.active = true;
            vignette.intensity.value = vignetteIntensity;
            vignette.intensity.overrideState = true;
            vignette.smoothness.value = vignetteSmoothness;
            vignette.smoothness.overrideState = true;
        }

        if (profile.TryGet(out ColorAdjustments color))
        {
            color.active = true;
            color.postExposure.value = postExposure;
            color.postExposure.overrideState = true;
            color.contrast.value = contrast;
            color.contrast.overrideState = true;
            color.saturation.value = saturation;
            color.saturation.overrideState = true;
            color.colorFilter.value = colorFilter;
            color.colorFilter.overrideState = true;
        }

        if (profile.TryGet(out FilmGrain grain))
        {
            grain.active = true;
            grain.intensity.value = filmGrainIntensity;
            grain.intensity.overrideState = true;
        }

        if (profile.TryGet(out ChromaticAberration chromatic))
        {
            chromatic.active = true;
            chromatic.intensity.value = chromaticIntensity;
            chromatic.intensity.overrideState = true;
        }

        if (profile.TryGet(out MotionBlur motionBlur))
        {
            motionBlur.active = motionBlurActivo;
        }
    }
}
