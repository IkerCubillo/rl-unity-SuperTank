using UnityEngine;

public class Proyectil : MonoBehaviour
{
    public float tiempoVida = 5f;

    private void Start()
    {
        // Destruir el proyectil después de un tiempo
        Destroy(gameObject, tiempoVida);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Opcional: Lógica al impactar contra algo
        // Por ejemplo, puedes destruir el proyectil al chocar:
        Destroy(gameObject);

        // También puedes detectar si golpea el tanque u otros objetos
        if (collision.gameObject.CompareTag("Tanque"))
        {
            Debug.Log("Impacto en el tanque");
            // Aquí podrías aplicar daño o recompensas
        }
    }
}
