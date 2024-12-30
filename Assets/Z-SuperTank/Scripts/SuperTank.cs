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
        float rotate = actions.ContinuousActions[0]; // Rotación
        float moveZ = actions.ContinuousActions[1]; // Movimiento hacia adelante/atrás
        float headRotate = actions.ContinuousActions[2];

        float moveSpeed = 3f;
        float rotationSpeed = 100f;
        float headRotationSpeed = 50f;

        // Movimiento hacia adelante o atrás
        transform.localPosition += transform.forward * moveZ * Time.deltaTime * moveSpeed;

        // Rotación
        transform.Rotate(Vector3.up, rotate * Time.deltaTime * rotationSpeed);

        // Girar la cabeza del tanque
        tankHead.Rotate(Vector3.up, headRotate * Time.deltaTime * headRotationSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        // Movimiento de la cabeza (Q y E para girar)
        if (Input.GetKey(KeyCode.Q))
            continuousActions[2] = -1f; // Girar la cabeza a la izquierda
        else if (Input.GetKey(KeyCode.E))
            continuousActions[2] = 1f; // Girar la cabeza a la derecha
        else
            continuousActions[2] = 0f; // Sin rotación
    }
}
