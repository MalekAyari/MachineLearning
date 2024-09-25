using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class MoveToTarget : Agent
{
    public CharacterAnimationManager characterAnimationManager;
    public float moveX;
    public float moveZ;
    [SerializeField] private List<Transform> checkpoints;
    private Transform nextCheckpoint;
    private int currentCheck = 0;
    private float distanceToCheckpoint;
    private float yLevel;
    [SerializeField] private int ObstaclesPerFloor;
    [SerializeField] private List<Transform> obstacles = new List<Transform>();
    [SerializeField] private List<Transform> obstaclePositions1 = new List<Transform>();
    [SerializeField] private List<Transform> obstaclePositions2 = new List<Transform>();
    [SerializeField] private List<Transform> obstaclePositions3 = new List<Transform>();
    [SerializeField] private List<Transform> stairs = new List<Transform>();
    public void RandomizePosition(List<Transform> list, int floor)
    {
        List<int> indexes = new List<int>();
        for (int i = 0; i < ObstaclesPerFloor; i++)
        {
            int index;
            do
            {
                index = Random.Range(0, list.Count);
            } while (indexes.Contains(index));

            indexes.Add(index);
            obstacles[i + ObstaclesPerFloor * (floor - 1)].position = list[index].position;
        }
    }

    public override void OnEpisodeBegin()
    {
        characterAnimationManager.OnResetEpisode();
        StartCoroutine(Wait(0.5f));

        yLevel = transform.localPosition.y;

        foreach (Transform obj in checkpoints)
        {
            obj.gameObject.SetActive(true);
        }

        currentCheck = 0;
        nextCheckpoint = checkpoints[currentCheck];

        //RandomizePosition(obstaclePositions1, 1);
        //RandomizePosition(obstaclePositions2, 2);
        //RandomizePosition(obstaclePositions3, 3);
    }

    IEnumerator Wait(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(distanceToCheckpoint);

        foreach (Transform checkpoint in checkpoints)
        {
            sensor.AddObservation(checkpoint.localPosition);
        }
        
        foreach (Transform stair in stairs)
        {
            sensor.AddObservation(stair.localPosition);
        }

        foreach (Transform obstacle in obstacles)
        {
            sensor.AddObservation(obstacle.transform.localPosition);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        moveX = actions.ContinuousActions[0];
        moveZ = actions.ContinuousActions[1];

        AddReward(-0.00001f);

        // Check if the agent has gotten closer to the checkpoint
        float currentDistance = Vector3.Distance(transform.localPosition, nextCheckpoint.localPosition);
        if (distanceToCheckpoint > currentDistance)
        {
            AddReward(0.00002f);
        } else
        {
            AddReward(-0.00001f);
        }
        distanceToCheckpoint = currentDistance;

        // Reward for moving upward
        if (yLevel < transform.localPosition.y)
        {
            AddReward(0.5f);
        }
        yLevel = transform.localPosition.y;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Reward"))
        {
            other.gameObject.SetActive(false);
            AddReward(20); 
            currentCheck++;

            if (currentCheck < checkpoints.Count)
            {
                nextCheckpoint = checkpoints[currentCheck];
                Debug.Log($"Checkpoint {currentCheck} reached!");
            }
            else
            {
                Debug.Log("All checkpoints reached!");
                Time.timeScale = 0f;
            }
        }
        else if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("obstacle"))
        {
            AddReward(-5);
            characterAnimationManager.characterController.enabled = false;
            EndEpisode();
        }
    }
}
