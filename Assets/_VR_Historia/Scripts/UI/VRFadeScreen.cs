using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VRFadeScreen : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private float defaultDuration = 1f;
    [SerializeField] private AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Flash de Terror")]
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.4f);

    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.raycastTarget = false;
        }
    }

    public Coroutine FadeOut(float duration = -1f)
    {
        if (duration < 0f) duration = defaultDuration;
        return StartCoroutine(Fade(0f, 1f, duration, fadeColor));
    }

    public Coroutine FadeIn(float duration = -1f)
    {
        if (duration < 0f) duration = defaultDuration;
        return StartCoroutine(Fade(1f, 0f, duration, fadeColor));
    }

    public Coroutine FadeOutIn(float holdTime = 0.5f, float fadeDuration = -1f)
    {
        if (fadeDuration < 0f) fadeDuration = defaultDuration;
        return StartCoroutine(FadeOutInCoroutine(holdTime, fadeDuration));
    }

    public Coroutine FlashTerror(float intensidad = 1f, float duracion = 0.3f)
    {
        return StartCoroutine(FlashCoroutine(flashColor * intensidad, duracion));
    }

    public Coroutine FlashColor(Color color, float duracion = 0.3f)
    {
        return StartCoroutine(FlashCoroutine(color, duracion));
    }

    IEnumerator FadeOutInCoroutine(float holdTime, float fadeDuration)
    {
        yield return Fade(0f, 1f, fadeDuration, fadeColor);
        yield return new WaitForSeconds(holdTime);
        yield return Fade(1f, 0f, fadeDuration, fadeColor);
    }

    IEnumerator FlashCoroutine(Color color, float duracion)
    {
        if (fadeImage == null) yield break;

        float halfDur = duracion * 0.5f;
        yield return Fade(0f, color.a, halfDur * 0.3f, color);
        yield return Fade(color.a, 0f, halfDur * 1.7f, color);
    }

    IEnumerator Fade(float from, float to, float duration, Color color)
    {
        if (fadeImage == null) yield break;

        fadeImage.raycastTarget = true;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            float curved = easingCurve.Evaluate(normalized);
            float alpha = Mathf.Lerp(from, to, curved);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, to);
        fadeImage.raycastTarget = to > 0.5f;
    }
}
