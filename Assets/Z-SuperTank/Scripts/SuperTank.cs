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
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        float moveSpeed = 3f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        continousActions[0] = Input.GetAxisRaw("Horizontal");
        continousActions[1] = Input.GetAxisRaw("Vertical");
    }
}
