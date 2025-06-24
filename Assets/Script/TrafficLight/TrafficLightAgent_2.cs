using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class TrafficLightAgent_2 : Agent
{
    public TrafficSensor[] sensors_NorthSouth;
    public TrafficSensor[] sensors_EastWest;
    public TrafficLightController2 lightController; // script điều khiển đèn giao lộ

    public override void OnEpisodeBegin()
    {
        lightController.ResetLights(); // reset về trạng thái đèn ban đầu
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observation: số lượng xe mỗi hướng
        float count_NS = 0f;
        float count_EW = 0f;

        foreach (var s in sensors_NorthSouth)
            count_NS += s.waitingCarCount;

        foreach (var s in sensors_EastWest)
            count_EW += s.waitingCarCount;

        sensor.AddObservation(count_NS);
        sensor.AddObservation(count_EW);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0]; // 0: NS xanh, 1: EW xanh

        lightController.SetLightState(action);

        // Reward: khuyến khích giảm tắc đường
        float totalWaitTime = lightController.GetTotalWaitingTime();
        float reward = -totalWaitTime;
        SetReward(reward);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0; // manual control nếu cần
    }
}
