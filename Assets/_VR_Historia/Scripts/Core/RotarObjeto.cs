using UnityEngine;

public class RotarObjeto : MonoBehaviour
{
    [Tooltip("Velocidad de rotación en grados por segundo")]
    public float velocidadRotacion = 45f;

    void Update()
    {
        transform.Rotate(0f, velocidadRotacion * Time.deltaTime, 0f, Space.Self);
    }
}
