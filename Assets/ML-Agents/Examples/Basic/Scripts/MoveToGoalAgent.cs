using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using NUnit.Framework.Internal;
using Unity.MLAgents.Actuators;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform; // For unity
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    public override void OnEpisodeBegin()
    {
        // transform.localPosition = Vector3.zero;
        // transform.localPosition = new Vector3(0, 1, 0);

        transform.localPosition = new Vector3(Random.Range(-7f, +7f), 1, Random.Range(2, 7));
        targetTransform.localPosition = new Vector3(Random.Range(-7f, +7f), 1, Random.Range(0, -7));

    
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); // Our localPosition
        sensor.AddObservation(targetTransform.localPosition); // Unity target localPosition
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
    private void OnTriggerEnter(Collider other){
        if (other.TryGetComponent<Goal>(out Goal goal)){
            SetReward(1f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        if (other.TryGetComponent<Wall>(out Wall wall)){
            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
