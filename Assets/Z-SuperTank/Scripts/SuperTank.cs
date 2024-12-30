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

        float moveSpeed = 3f;
        float rotationSpeed = 100f;

        // Movimiento hacia adelante o atrás
        transform.localPosition += transform.forward * moveZ * Time.deltaTime * moveSpeed;

        // Rotación
        transform.Rotate(Vector3.up, rotate * Time.deltaTime * rotationSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        continousActions[0] = Input.GetAxisRaw("Horizontal");
        continousActions[1] = Input.GetAxisRaw("Vertical");
    }
}
