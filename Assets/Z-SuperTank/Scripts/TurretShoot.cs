using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TurretShoot : Agent
{   
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;


    [Header("Referencias")]
    public Transform turretCanon;  // El cañón que rota verticalmente
    public Transform firePoint; // Punto desde donde se disparan los proyectiles
    public Transform objetivo;     // Referencia al tanque

    [Header("Parámetros de Movimiento")]
    public float limiteElevacionMin = -25f;
    public float limiteElevacionMax = 25f;

    [Header("Parámetros de Disparo")]
    public GameObject projectilePrefab;
    public float fuerzaProyectil = 20f;
    public float fireRate = 0.5f;
    public float projectileSpeed = 0f;
    
    
    [Header("Debug")]
    public bool tieneLineaDeVision;
    public float rewardAccumulada = 0f;

    private float nextFireTime = 1f;
    private float tiempoUltimoDisparo;

    
    public override void OnEpisodeBegin()
    {
        floorMeshRenderer.material = normalMaterial;
        // turretCanon.localRotation = Quaternion.identity;
        turretCanon.localRotation = Quaternion.Euler(3f, 0f, 0f);

        float randomX = Random.Range(0, 2) == 0 ? Random.Range(-6f, -1f) : Random.Range(1f, 6f);
        float randomZ = Random.Range(0, 2) == 0 ? Random.Range(-6f, -1f) : Random.Range(1f, 6f);

        objetivo.localPosition = new Vector3(randomX, 0.21f, randomZ);
        //objetivo.localPosition = new Vector3(Random.Range(-5, 5), 0.21f, Random.Range(-5, 5));

        rewardAccumulada = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Posición relativa del objetivo respecto a la torreta
        Vector3 posicionRelativa = objetivo.position - transform.position;
        sensor.AddObservation(posicionRelativa);

        // 2. Ángulo horizontal de la torreta (normalizado entre -0.5 y 0.5)
        float anguloHorizontal = Mathf.DeltaAngle(0, turretCanon.localRotation.eulerAngles.y) / 360f;
        sensor.AddObservation(anguloHorizontal);

        // 3. Ángulo vertical del cañón (normalizado entre -0.5 y 0.5, dado el rango -10° a 25°)
        float anguloVertical = Mathf.Clamp(turretCanon.localRotation.eulerAngles.x, -10f, 25f) / 360f;
        sensor.AddObservation(anguloVertical);

        // 4. Línea de visión al objetivo (bool convertido a float)
        tieneLineaDeVision = TieneLineaDeVision();
        sensor.AddObservation(tieneLineaDeVision ? 1f : 0f);

        // 5. Distancia al objetivo
        float distancia = Vector3.Distance(objetivo.position, transform.position);
        sensor.AddObservation(distancia);

        // 6. Velocidad relativa del objetivo
        if (objetivo.TryGetComponent<Rigidbody>(out Rigidbody rbObjetivo))
        {
            Vector3 velocidadObjetivo = rbObjetivo.velocity;
            sensor.AddObservation(velocidadObjetivo);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        // 7. Estado de disparo reciente (cooldown normalizado)
        float tiempoRestanteParaDisparo = Mathf.Max(0, nextFireTime - Time.time);
        sensor.AddObservation(tiempoRestanteParaDisparo);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        // Acciones:
        // actions.ContinuousActions[0]: Rotación horizontal de la cabeza de la torreta
        // actions.ContinuousActions[1]: Elevación vertical del cañón
        // actions.DiscreteActions[0]: Disparar (0 = no disparar, 1 = disparar)

        float rotation = actions.ContinuousActions[0];
        float elevation = actions.ContinuousActions[1];

        int shootAction = actions.DiscreteActions[0];

        float rotationSpeed = 100f;
        float elevationSpeed = 25f;

         // 1. Rotación horizontal (Yaw)
        // Rotar el cañón horizontalmente alrededor del eje Y
        turretCanon.Rotate(Vector3.up, rotation * rotationSpeed * Time.deltaTime);

        // 2. Elevación vertical (Pitch)
        // Obtener el ángulo actual en el eje X
        Vector3 currentRotation = turretCanon.localEulerAngles;

        // Convertir el ángulo para manejar correctamente los límites
        float currentPitch = currentRotation.x;
        if (currentPitch > 180f) currentPitch -= 360f; // Ajuste para ángulos negativos

        // Calcular el nuevo ángulo basado en la entrada del usuario/agente
        float newPitch = Mathf.Clamp(currentPitch - (elevation * elevationSpeed * Time.deltaTime), 
                                    limiteElevacionMin, limiteElevacionMax);

        // Aplicar el nuevo ángulo solo al eje X, manteniendo Y y Z intactos
        turretCanon.localEulerAngles = new Vector3(newPitch, currentRotation.y, currentRotation.z);


        // Disparo
        if (shootAction == 1 && tieneLineaDeVision)
        {
            Fire();
            tiempoUltimoDisparo = Time.time;

            // Recompensa por disparar con éxito
            AddReward(2f);
            rewardAccumulada += 2f;
            Debug.Log("Puntos +10, disparo vision");
        }
        else if (shootAction == 1)
        {   
            Fire();
            tiempoUltimoDisparo = Time.time;
            // Penalización por disparar sin tener línea de visión
            AddReward(-1f);
            rewardAccumulada -= 1f;
            Debug.Log("Puntos -5, disparo no vision");
        }

         // 4. Recompensa por buena alineación
        if (EstaEnFrente())
        {
            AddReward(0.1f); // Pequeña recompensa por buena alineación
            rewardAccumulada += 0.1f;
            Debug.Log("Puntos +0.1,  delante");
        }

        // 5. Penalización por rotación excesiva
        if (!EstaEnFrente())
        {
            AddReward(-0.1f); // Penalización por rotaciones innecesarias
            rewardAccumulada -= 0.1f;
            Debug.Log("Puntos -0.1, no delante");
        }

        AddReward(-0.01f);
        rewardAccumulada -= 0.01f;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        // Controles manuales para pruebas:
        continuousActions[0] = Input.GetAxis("Horizontal"); // Rotación horizontal con teclas
        continuousActions[1] = Input.GetAxis("Vertical");   // Elevación del cañón con teclas
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // Disparar con la barra espaciadora
    }

    private bool TieneLineaDeVision()
    {
        // Primero comprobar si el objetivo está en frente del cañón
        if (!EstaEnFrente())
        {
            return false;
        }

        // Realizar un Raycast para comprobar obstrucciones
        RaycastHit hit;
        Vector3 direccion = objetivo.position - firePoint.position;
        if (Physics.Raycast(firePoint.position, direccion.normalized, out hit))
        {
            return hit.transform == objetivo;
        }
        return false;
    }

    private bool EstaEnFrente()
    {
        // Dirección desde el cañón hacia el objetivo
        Vector3 direccionHaciaObjetivo = (objetivo.position - firePoint.position).normalized;

        // Dirección hacia adelante del cañón
        Vector3 direccionFrontalCanon = turretCanon.forward;

        // Producto escalar para calcular el ángulo horizontal (plano XZ)
        float dotProductHorizontal = Vector3.Dot(new Vector3(direccionHaciaObjetivo.x, 0, direccionHaciaObjetivo.z).normalized,
                                                new Vector3(direccionFrontalCanon.x, 0, direccionFrontalCanon.z).normalized);

        // Producto escalar para calcular el ángulo vertical
        float dotProductVertical = Vector3.Dot(direccionHaciaObjetivo, direccionFrontalCanon);

        // Ángulo horizontal (en grados)
        float anguloHorizontal = Mathf.Acos(dotProductHorizontal) * Mathf.Rad2Deg;

        // Ángulo vertical (en grados)
        float anguloVertical = Mathf.Acos(dotProductVertical) * Mathf.Rad2Deg;

        // Comprobar si el ángulo está dentro del rango permitido (45° horizontal y 10° vertical)
        return anguloHorizontal <= 45f && anguloVertical <= 15f;
    }


    private void Fire()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        Vector3 spawnPosition = firePoint != null ? firePoint.position : turretCanon.position;
        
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        TurretProjectile projectileScript = projectile.AddComponent<TurretProjectile>();
        projectileScript.Initialize(projectileSpeed, this);
        Debug.Log($"Torreta {gameObject.name} disparando!");
    }
}
