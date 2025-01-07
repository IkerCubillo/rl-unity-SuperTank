using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    private float speed;
    private float lifeTime = 2f;
    private int damage = 34; // 3 golpes para destruir el tanque
    private TurretShoot ownerAgent;

    public void Initialize(float projectileSpeed, TurretShoot agent)
    {
        speed = projectileSpeed;
        ownerAgent = agent;
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

            if (ownerAgent != null)
            {
                ownerAgent.AddReward(100f); // Recompensa adicional por impactar al tanque
                ownerAgent.rewardAccumulada += 100f;
                Debug.Log("Puntos +100 impacto");

                if (tank.currentHealth <= 0)
                {
                    ownerAgent.AddReward(1000f); // Recompensa adicional por impactar al tanque
                    ownerAgent.rewardAccumulada += 1000f;
                    Debug.Log("Puntos +1000 impacto");
                    ownerAgent.EndEpisode();
                }
            }
            
            Destroy(gameObject);
        }
                if (other.CompareTag("MovingObjective"))
        {
            Debug.Log("¡Proyectil de torreta impactó en el tanque!");
            
            MovingObjective objective = other.GetComponent<MovingObjective>();
            if (objective != null)
            {
                objective.TakeDamage(damage);
            }

            if (ownerAgent != null)
            {
                ownerAgent.AddReward(100f); // Recompensa adicional por impactar al tanque
                ownerAgent.rewardAccumulada += 100f;
                Debug.Log("Puntos +100 impacto");

                if (objective.currentHealth <= 0)
                {
                    ownerAgent.AddReward(1000f); // Recompensa adicional por impactar al tanque
                    ownerAgent.rewardAccumulada += 1000f;
                    Debug.Log("Puntos +1000 impacto");
                    objective.currentHealth = objective.maxHealth;
                    ownerAgent.EndEpisode();
                }
            }
            
            Destroy(gameObject);
        }

    }
} 