using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SuperTank: Agent
{
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

    public override void OnEpisodeBegin()
    {
        currentHealth = maxHealth;
        
        // transform.localPosition = new Vector3(-0.5f, 0.2f, -4);
        transform.rotation = Quaternion.identity;
        tankHead.localRotation = Quaternion.identity;

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
            Debug.Log($"Roll angle: {rollAngle}");
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
            //tankProjectile.Initialize(this);
        }
        Debug.Log("¡Tanque disparando!");

        // Aplicar retroceso
        if (rb != null)
        {
            // Aplicar una fuerza hacia atrás
            rb.AddForce(-transform.forward * 5f, ForceMode.Impulse);
            // Inclinar el tanque hacia atrás
            rb.AddTorque(Vector3.right * 250f, ForceMode.Impulse);
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
        //AddReward(-1f); // Penalización por morir
        //EndEpisode(); // Reiniciar el episodio (para ML-Agents)
    }

}
