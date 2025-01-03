using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SuperTank : Agent
{
    // [SerializeField] private Transform targetTransform; // For unity
    // [SerializeField] private Material winMaterial;
    // [SerializeField] private Material loseMaterial;
    // [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] private Transform tankHead;
    [SerializeField] private GameObject projectilePrefab; // Arrastra aquí el prefab del proyectil
    [SerializeField] private Transform firePoint; // Punto desde donde sale el disparo
    private float speedMultiplier = 1f;
    private float fireRate = 0.5f; // Tiempo entre disparos
    private float nextFireTime = 0f;

    public override void OnEpisodeBegin()
    {
        // transform.localPosition = Vector3.zero;
        transform.localPosition = new Vector3(2, 0.5f, -2);

        // transform.localPosition = new Vector3(Random.Range(-7f, +7f), 1, Random.Range(2, 7));
        // targetTransform.localPosition = new Vector3(Random.Range(-7f, +7f), 1, Random.Range(0, -7));

    
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); // Our localPosition
        // sensor.AddObservation(targetTransform.localPosition); // Unity target localPosition
    }

    public override void OnActionReceived(ActionBuffers actions)
    {        
        float rotate = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float headRotate = actions.ContinuousActions[2];

        float baseSpeed = 3f;
        float baseRotationSpeed = 100f;
        float baseHeadRotationSpeed = 50f;

        // Aplicamos el multiplicador a todas las velocidades
        float moveSpeed = baseSpeed * speedMultiplier;
        float rotationSpeed = baseRotationSpeed * speedMultiplier;
        float headRotationSpeed = baseHeadRotationSpeed * speedMultiplier;

        transform.localPosition += transform.forward * moveZ * Time.deltaTime * moveSpeed;
        transform.Rotate(Vector3.up, rotate * Time.deltaTime * rotationSpeed);
        tankHead.Rotate(Vector3.up, headRotate * Time.deltaTime * headRotationSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        
        // Movimiento de la cabeza (Q y E para girar)
        if (Input.GetKey(KeyCode.Q))
            continuousActions[2] = -1f;
        else if (Input.GetKey(KeyCode.E))
            continuousActions[2] = 1f;
        else
            continuousActions[2] = 0f;

        // Disparar con la barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    private void Fire()
    {
        if (Time.time < nextFireTime) return; // Verificar si podemos disparar

        nextFireTime = Time.time + fireRate;

        // Si no hay un punto de disparo específico, usar la posición de la cabeza
        Vector3 spawnPosition = firePoint != null ? firePoint.position : tankHead.position;
        
        // Crear el proyectil
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, tankHead.rotation);
        Debug.Log("¡Tanque disparando!");
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        Debug.Log($"SuperTank: Multiplicador de velocidad establecido a {multiplier}");
    }
}
