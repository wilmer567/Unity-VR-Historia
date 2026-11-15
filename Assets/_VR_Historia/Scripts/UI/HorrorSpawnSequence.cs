using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HorrorSpawnSequence : MonoBehaviour
{
    [Header("Orientacion Inicial del Jugador")]
    [Tooltip("Arrastra aqui un objeto vacio ubicado en la entrada del pasillo oscuro para que el jugador mire hacia alla al empezar")]
    public Transform lookTargetAtSpawn;

    [Header("Efecto de Encendido de Visor")]
    public Image blackScreenFade;
    public TextMeshProUGUI bootMessageText;
    public AudioSource breathingOrStaticAudio;

    void Start()
    {
        if (lookTargetAtSpawn != null)
        {
            Vector3 direction = (lookTargetAtSpawn.position - transform.position).normalized;
            direction.y = 0f;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        StartCoroutine(BootSequenceRoutine());
    }

    private IEnumerator BootSequenceRoutine()
    {
        if (blackScreenFade != null)
            blackScreenFade.color = Color.black;

        if (breathingOrStaticAudio != null)
            breathingOrStaticAudio.Play();

        if (bootMessageText != null)
        {
            bootMessageText.text = "SISTEMA DE VISION DE EMERGENCIA // ENERGIA AL 14%";
            yield return new WaitForSeconds(1.2f);
            bootMessageText.text = "ADVERTENCIA: RACK PRINCIPAL SIN RESPUESTA";
            yield return new WaitForSeconds(1.0f);
            bootMessageText.text = "";
        }

        if (blackScreenFade != null)
        {
            float elapsed = 0f;
            float duration = 2.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                blackScreenFade.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }
            blackScreenFade.gameObject.SetActive(false);
        }
    }
}
