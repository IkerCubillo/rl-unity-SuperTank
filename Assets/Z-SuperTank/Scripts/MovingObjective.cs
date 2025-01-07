using UnityEngine;

public class MovingObjective : MonoBehaviour
{
    [SerializeField] public int maxHealth = 100;
    [SerializeField] public int currentHealth;
    public float speed = 3f; // Velocidad de movimiento del tanque
    public float boundary = 6f; // Límite del mapa
    private Vector3 direction; // Dirección actual del tanque

    private void Start()
    {
        currentHealth = maxHealth;
        // Dirección inicial hacia adelante
        direction = Vector3.forward;
    }
    public void OnEpisodeBegin()
    {
        currentHealth = maxHealth;
    }
    private void Update()
    {
        // // Mover el tanque en la dirección actual
        // transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // // Comprobar si el tanque ha alcanzado los límites del mapa
        // if (Mathf.Abs(transform.localPosition.x) >= boundary || Mathf.Abs(transform.localPosition.z) >= boundary)
        // {
        //     RotateRight(); // Rotar hacia la derecha
        // }
    }

    private void RotateRight()
    {
        // Rotar el tanque 90 grados a la derecha
        transform.Rotate(0, 90, 0);

        // Actualizar la dirección basándose en la rotación actual
        direction = transform.forward;

        // Ajustar la posición dentro de los límites
        Vector3 localPosition = transform.localPosition;
        localPosition.x = Mathf.Clamp(localPosition.x, -boundary, boundary);
        localPosition.z = Mathf.Clamp(localPosition.z, -boundary, boundary);
        transform.localPosition = localPosition;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"¡Tanque dañado! Vida restante: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("¡Tanque destruido!");
    }
}
