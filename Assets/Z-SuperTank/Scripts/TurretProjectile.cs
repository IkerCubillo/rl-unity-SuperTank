using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    private float speed;
    private float lifeTime = 3f;
    private int damage = 34; // 3 golpes para destruir el tanque

    public void Initialize(float projectileSpeed)
    {
        speed = projectileSpeed;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Proyectil de torreta impactó en el tanque!");
            
            SuperTank tank = other.GetComponent<SuperTank>();
            if (tank != null)
            {
                tank.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
        if (other.TryGetComponent<Limit>(out Limit limit)){
            Destroy(gameObject);
        }
    }
} 