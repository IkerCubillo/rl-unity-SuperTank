using UnityEngine;

public class TankProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;

    private SuperTank tankAgent;
    private EnemyTurret enemyTurret;

    public void Initialize(SuperTank agent)
    {
        tankAgent = agent;
    }

    public void Initialize(EnemyTurret turret)
    {
        enemyTurret = turret;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Proyectil colisionó con: {other.gameObject.name} (Tag: {other.tag})");

        if (other.CompareTag("Canon"))
        {
            Debug.Log($"¡Proyectil impactó en el cañón: {other.gameObject.name}!");
            
            if (tankAgent != null)
            {
                tankAgent.AddReward(1000.0f);
                tankAgent.performanceScore += 1000.0f;
                //tankAgent.EndEpisode();
            }
            
            //Destroy(other.gameObject);
            Destroy(gameObject);

            if (enemyTurret != null)
            {
                tankAgent.EndEpisode();
            }
        }

        if (other.TryGetComponent<Limit>(out Limit limit)){
            Destroy(gameObject);
            tankAgent.AddReward(-10f);
            tankAgent.performanceScore += -10f;
        }

        else
        {
            // Opcional: destruir el proyectil al golpear cualquier cosa
            // Destroy(gameObject);
        }
    }
} 