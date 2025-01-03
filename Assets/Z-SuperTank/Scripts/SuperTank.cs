using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SuperTank : Agent
{
    [SerializeField] private Transform tankHead;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float raycastDistance = 1f; // Distancia del rayo para detectar suelo
    [SerializeField] private Transform leftTrack; // Punto de origen del raycast izquierdo
    [SerializeField] private Transform rightTrack; // Punto de origen del raycast derecho

    private float speedMultiplier = 1f;
    private float fireRate = 0.5f;
    private float nextFireTime = 0f;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Si no se asignaron los puntos de las orugas, crearlos automáticamente
        if (leftTrack == null)
        {
            GameObject left = new GameObject("LeftTrack");
            leftTrack = left.transform;
            leftTrack.parent = transform;
            leftTrack.localPosition = new Vector3(-0.5f, 0f, 0f); // Ajusta según el tamaño de tu tanque
        }
        if (rightTrack == null)
        {
            GameObject right = new GameObject("RightTrack");
            rightTrack = right.transform;
            rightTrack.parent = transform;
            rightTrack.localPosition = new Vector3(0.5f, 0f, 0f); // Ajusta según el tamaño de tu tanque
        }
    }

    private bool IsGrounded()
    {
        // Lanzar rayos desde ambas orugas
        bool leftGrounded = Physics.Raycast(leftTrack.position, Vector3.down, raycastDistance);
        bool rightGrounded = Physics.Raycast(rightTrack.position, Vector3.down, raycastDistance);

        // Debug visual de los rayos
        Debug.DrawRay(leftTrack.position, Vector3.down * raycastDistance, leftGrounded ? Color.green : Color.red);
        Debug.DrawRay(rightTrack.position, Vector3.down * raycastDistance, rightGrounded ? Color.green : Color.red);

        // El tanque está en el suelo si al menos una oruga lo toca
        return leftGrounded || rightGrounded;
    }

    private void ApplyMovement(float moveZ, float rotate, float headRotate)
    {
        float moveSpeed = 3f * speedMultiplier;
        float rotationSpeed = 100f * speedMultiplier;
        float headRotationSpeed = 50f * speedMultiplier;

        // Solo permitir movimiento si está en contacto con el suelo
        if (IsGrounded())
        {
            // Movimiento hacia adelante/atrás
            transform.localPosition += transform.forward * moveZ * Time.deltaTime * moveSpeed;
            
            // Rotación del tanque (permitir girar incluso si solo una oruga toca el suelo)
            transform.Rotate(Vector3.up, rotate * Time.deltaTime * rotationSpeed);
        }

        // La cabeza siempre puede girar independientemente de si está en el suelo
        tankHead.Rotate(Vector3.up, headRotate * Time.deltaTime * headRotationSpeed);
    }

    private void Update()
    {
        float moveZ = Input.GetAxis("Vertical");
        float rotate = Input.GetAxis("Horizontal");
        
        float headRotate = 0f;
        if (Input.GetKey(KeyCode.Q)) headRotate = -1f;
        if (Input.GetKey(KeyCode.E)) headRotate = 1f;

        ApplyMovement(moveZ, rotate, headRotate);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {        
        float rotate = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float headRotate = actions.ContinuousActions[2];
        bool shouldFire = actions.DiscreteActions.Length > 0 && actions.DiscreteActions[0] == 1;

        ApplyMovement(moveZ, rotate, headRotate);

        if (shouldFire)
        {
            Fire();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        // Asegurarnos de que tenemos suficiente espacio para las acciones
        if (continuousActions.Length >= 3)
        {
            continuousActions[0] = Input.GetAxisRaw("Horizontal");
            continuousActions[1] = Input.GetAxisRaw("Vertical");
            
            if (Input.GetKey(KeyCode.Q))
                continuousActions[2] = -1f;
            else if (Input.GetKey(KeyCode.E))
                continuousActions[2] = 1f;
            else
                continuousActions[2] = 0f;
        }

        // Asegurarnos de que tenemos espacio para la acción discreta
        if (discreteActions.Length > 0)
        {
            discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        }
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
}
