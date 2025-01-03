using UnityEngine;

public class TankProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f; // El proyectil se destruirá después de este tiempo

    private void Start()
    {
        // Destruir el proyectil después de lifeTime segundos
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Mover el proyectil hacia adelante
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si golpeó a un cañón
        if (other.CompareTag("Canon"))
        {
            Debug.Log($"¡Proyectil impactó en el cañón: {other.gameObject.name}!");
            Destroy(other.gameObject); // Destruir el cañón
            Destroy(gameObject); // Destruir el proyectil
        }
    }
} 