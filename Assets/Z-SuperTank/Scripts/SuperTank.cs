using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SuperTank: Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform tankHead;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float maxRollAngle = 45f; // Ángulo máximo de inclinación lateral permitido
    [SerializeField] private bool showDebugAngles = false; // Para depuración
    [SerializeField] private int maxHealth = 100;
    public int currentHealth;
    private float speedMultiplier = 1f;
    private float fireRate = 0.5f;
    private float nextFireTime = 0f;
    private Rigidbody rb;


    public float performanceScore = 0f;

    public override void OnEpisodeBegin()
    {
        currentHealth = maxHealth;
        performanceScore = 0f;

        // Posición aleatoria del tanque
        transform.localPosition = new Vector3(Random.Range(-6f, 6f), 0.2f, Random.Range(-6f, 6f));
        //transform.localPosition = new Vector3(-2.15f,0.22f,-1.08f);
        // Posición aleatoria del objetivo
        //targetTransform.localPosition = new Vector3(Random.Range(-6f, 6f), 0.2f, Random.Range(-6f, 6f));
        //targetTransform.localPosition = new Vector3(0.6f,0.22f,-1.23f);
        // Rotación aleatoria del tanque (solo en el eje Y)
        float randomYRotation = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0f, randomYRotation, 0f);
        // Restablecer la rotación de la torreta
        tankHead.localRotation = Quaternion.identity;
        // Inicializar el Rigidbody
        rb = GetComponent<Rigidbody>();
    }


    
    public override void OnActionReceived(ActionBuffers actions)
    {        
        float rotate = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float headRotate = actions.ContinuousActions[2];
        int shootAction = actions.DiscreteActions[0];

        ApplyMovement(moveZ, rotate, headRotate);
        
        if (shootAction == 1)
        {
            Fire();
        }
        
        // if (moveZ > 0)
        // {
        //     AddReward(1f);
        // }

        // Penalización por cada paso para incentivar eficiencia
        AddReward(-0.1f);
        performanceScore += -0.1f;

        // Recompensa positiva por apuntar al objetivo
        float aimingReward = CalculateRewardForAiming();
        AddReward(aimingReward);
        performanceScore += aimingReward;

        if (IsFlipped())
        {
            Debug.Log("¡El tanque se volcó!");
            SetReward(-1000f); // Penalización por volcarse
            EndEpisode();
        }
    }

    private bool IsFlipped()
    {
        // Obtener el ángulo de inclinación en el eje X (pitch) y Z (roll)
        float pitchAngle = transform.rotation.eulerAngles.x;
        float rollAngle = transform.rotation.eulerAngles.z;

        // Normalizar los ángulos para que estén en el rango [-180, 180]
        if (pitchAngle > 180f) pitchAngle -= 360f;
        if (rollAngle > 180f) rollAngle -= 360f;

        // Considerar el tanque volcado si está inclinado más de 90 grados en cualquier dirección
        return Mathf.Abs(pitchAngle) > 90f || Mathf.Abs(rollAngle) > 90f;
    }

    private float CalculateRewardForAiming()
    {
        // Vector de dirección desde el cañón hacia el objetivo
        Vector3 directionToTarget = (targetTransform.position - tankHead.position).normalized;
        // Vector que apunta hacia adelante desde la cabeza del tanque
        Vector3 headForward = tankHead.forward;

        // Calcular el ángulo entre la cabeza del tanque y el objetivo
        float angle = Vector3.Angle(headForward, directionToTarget);

        // Recompensa máxima cuando el ángulo es 0 grados (perfectamente alineado)
        // Ajustar la recompensa proporcionalmente. Menos de 10 grados obtiene una buena recompensa.
        if (angle < 10f)
        {
            return (10f - angle); // Recompensa inversamente proporcional al ángulo
        }

        return -0.1f; //si no apunta al objetivo recompensa negativa
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
            
        if (Input.GetKey(KeyCode.Q))
            continuousActions[2] = -1f;
        else if (Input.GetKey(KeyCode.E))
            continuousActions[2] = 1f;
        else
            continuousActions[2] = 0f;

        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private bool CanMove()
    {
        // Obtener el ángulo de inclinación lateral (roll)
        float rollAngle = transform.rotation.eulerAngles.z;
        if (rollAngle > 180) rollAngle -= 360; // Normalizar a -180 a 180

        if (showDebugAngles)
        {
            //Debug.Log($"Roll angle: {rollAngle}");
        }

        // Si la inclinación lateral es demasiado alta, no permitir movimiento
        return Mathf.Abs(rollAngle) < maxRollAngle;
    }

    private void ApplyMovement(float moveZ, float rotate, float headRotate)
    {
        float moveSpeed = 3f * speedMultiplier;
        float rotationSpeed = 100f * speedMultiplier;
        float headRotationSpeed = 50f * speedMultiplier;

        // Solo permitir movimiento si el tanque no está demasiado inclinado
        if (CanMove())
        {
            transform.localPosition += transform.forward * moveZ * Time.deltaTime * moveSpeed;
            transform.Rotate(Vector3.up, rotate * Time.deltaTime * rotationSpeed);
        }

        // La cabeza siempre puede girar
        tankHead.Rotate(Vector3.up, headRotate * Time.deltaTime * headRotationSpeed);
    }

    private void Fire()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        Vector3 spawnPosition = firePoint != null ? firePoint.position : tankHead.position;

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, tankHead.rotation);
        TankProjectile tankProjectile = projectile.GetComponent<TankProjectile>();
        if (tankProjectile != null)
        {
            tankProjectile.Initialize(this);
        }
        Debug.Log("¡Tanque disparando!");

        // AddReward(-10f);
        // performanceScore += -10f;
        // Aplicar retroceso basado en la dirección de la torreta
        if (rb != null)
        {
            // Retroceso en la dirección opuesta al apuntado de la torreta
            Vector3 recoilDirection = -tankHead.forward;
            rb.AddForce(recoilDirection * 2.5f, ForceMode.Impulse);

            // Aplicar torque para inclinar hacia atrás dependiendo de la dirección de la torreta
            Vector3 recoilTorque = Vector3.Cross(tankHead.forward, Vector3.up);
            rb.AddTorque(recoilTorque * 100f, ForceMode.Impulse);
        }
    }


    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        Debug.Log($"SuperTank: Multiplicador de velocidad establecido a {multiplier}");
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
        //Debug.Log(Reward);
        SetReward(-1000f); // Penalización por morir
        performanceScore += -1000f;
        EndEpisode(); // Reiniciar el episodio (para ML-Agents)
    }

    private void OnTriggerEnter(Collider other){
        
        if (other.TryGetComponent<Limit>(out Limit limit)){
            Debug.Log($"Se choca con: {other.gameObject.name}!");
            SetReward(-1000f);
            performanceScore += -1000f;
            EndEpisode();
        }
        // if (other.CompareTag("Canon")){
        //     Debug.Log($"Se choca con: {other.gameObject.name}!");
        //     SetReward(-5f);
        //     performanceScore += -5f;
        // }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); // Our localPosition
        sensor.AddObservation(targetTransform.localPosition); // Unity target localPosition
    }

}
