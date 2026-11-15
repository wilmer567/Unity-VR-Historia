using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SyntroIntroController : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statusLogText;
    public CanvasGroup fadeOverlay;

    [Header("Configuracion de Escena")]
    public string nextSceneName = "Subsuelo_01";
    public AudioSource ambientDrone;
    public AudioSource buttonClickSfx;

    private string originalTitle = "SYNTRO";
    private readonly char[] glitchChars = { '#', '%', '&', '0', '1', 'X', '!' };

    void Start()
    {
        if (fadeOverlay != null) fadeOverlay.alpha = 0f;
        StartCoroutine(GlitchTitleRoutine());
        StartCoroutine(TypeTerminalLogs());
    }

    public void OnStartProtocolPressed()
    {
        StartCoroutine(TransitionToLevel());
    }

    private IEnumerator GlitchTitleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2.5f, 5.5f));

            for (int i = 0; i < 3; i++)
            {
                char[] corrupted = originalTitle.ToCharArray();
                int index = Random.Range(0, corrupted.Length);
                corrupted[index] = glitchChars[Random.Range(0, glitchChars.Length)];
                titleText.text = new string(corrupted);
                titleText.color = new Color(0.85f, 0.1f, 0.1f);
                yield return new WaitForSeconds(0.06f);
            }

            titleText.text = originalTitle;
            titleText.color = Color.white;
        }
    }

    private IEnumerator TypeTerminalLogs()
    {
        string[] logs = {
            "> CONEXION CON SUPERFICIE: PERDIDA",
            "> ADVERTENCIA: ANOMALIA DETECTADA EN RACK 04",
            "> PROTOCOLO DE CONTENCION COMPROMETIDO..."
        };

        if (statusLogText == null) yield break;
        statusLogText.text = "";

        foreach (string line in logs)
        {
            foreach (char c in line)
            {
                statusLogText.text += c;
                yield return new WaitForSeconds(0.03f);
            }
            statusLogText.text += "\n";
            yield return new WaitForSeconds(0.4f);
        }
    }

    private IEnumerator TransitionToLevel()
    {
        if (buttonClickSfx != null) buttonClickSfx.Play();

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (fadeOverlay != null) fadeOverlay.alpha = t;
            if (ambientDrone != null) ambientDrone.volume = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
