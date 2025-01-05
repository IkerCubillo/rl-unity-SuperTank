using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TurretShoot : Agent
{
    [Header("Referencias")]
    public Transform turret;         // La base principal de la torreta
    public Transform canon;          // El cañón que rota verticalmente
    public Transform firePoint;      // El punto desde donde se disparan los proyectiles
    public Transform objetivo;       // Referencia al tanque (objetivo)

    [Header("Parámetros de Movimiento")]
    public float limiteElevacionMin = -10f;
    public float limiteElevacionMax = 25f;

    [Header("Parámetros de Disparo")]
    public GameObject proyectilPrefab;
    public float fuerzaProyectil = 20f;
    public float tiempoEntreDisparos = 1f;

    private float tiempoUltimoDisparo;

    [Header("Debug")]
    public bool tieneLineaDeVision;

    public override void OnEpisodeBegin()
    {
        // Reinicia la posición de la torreta (si fuera necesario)
        turret.localRotation = Quaternion.identity;
        canon.localRotation = Quaternion.identity;

        // Reposiciona al objetivo (tanque) de manera aleatoria
        objetivo.localPosition = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Posición relativa del objetivo respecto a la torreta
        Vector3 posicionRelativa = objetivo.position - transform.position;
        sensor.AddObservation(posicionRelativa);

        // Ángulo actual de la torreta (horizontal)
        sensor.AddObservation(turret.localRotation.eulerAngles.y / 360f);

        // Ángulo actual del cañón (vertical)
        sensor.AddObservation(canon.localRotation.eulerAngles.x / 360f);

        // Línea de visión al objetivo (bool)
        tieneLineaDeVision = TieneLineaDeVision();
        sensor.AddObservation(tieneLineaDeVision ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Acciones:
        // actions.ContinuousActions[0]: Rotación horizontal de la torreta
        // actions.ContinuousActions[1]: Elevación vertical del cañón
        // actions.DiscreteActions[0]: Disparar (0 = no disparar, 1 = disparar)

        float rotacionHorizontal = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rotacionVertical = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // Rotar la base de la torreta (horizontal)
        turret.Rotate(Vector3.up, rotacionHorizontal * 180f * Time.deltaTime);

        // Ajustar la elevación del cañón
        float nuevoAnguloElevacion = Mathf.Clamp(canon.localRotation.eulerAngles.x + (rotacionVertical * 90f * Time.deltaTime), limiteElevacionMin, limiteElevacionMax);
        canon.localRotation = Quaternion.Euler(nuevoAnguloElevacion, 0, 0);

        // Disparo
        if (actions.DiscreteActions[0] == 1 && Time.time - tiempoUltimoDisparo >= tiempoEntreDisparos && tieneLineaDeVision)
        {
            Disparar();
            tiempoUltimoDisparo = Time.time;

            // Recompensa por disparar con éxito
            AddReward(1f);
        }
        else if (actions.DiscreteActions[0] == 1)
        {
            // Penalización por disparar sin tener línea de visión
            AddReward(-0.1f);
        }
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
        RaycastHit hit;
        Vector3 direccion = objetivo.position - firePoint.position;
        if (Physics.Raycast(firePoint.position, direccion.normalized, out hit))
        {
            return hit.transform == objetivo;
        }
        return false;
    }

    private void Disparar()
    {
        if (proyectilPrefab != null)
        {
            GameObject proyectil = Instantiate(proyectilPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = proyectil.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(firePoint.forward * fuerzaProyectil, ForceMode.Impulse);
            }
        }
    }
}
