using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class TrafficLightAgent : Agent
{
    public TrafficLight trafficLight;
    public TrafficSensor trafficSensor; // Script đo số lượng/xe

    public override void OnEpisodeBegin()
    {
        // Reset đèn về trạng thái ban đầu nếu cần
        trafficLight.SetLightState(Color.red);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Quan sát ví dụ: số lượng xe đang chờ
        //sensor.AddObservation(trafficSensor.waitingCarCount);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        switch (action)
        {
            case 0:
                trafficLight.SetLightState(Color.green);
                break;
            case 1:
                trafficLight.SetLightState(Color.yellow);
                break;
            case 2:
                trafficLight.SetLightState(Color.red);
                break;
        }

        // Thưởng hoặc phạt
        //float reward = -trafficSensor.waitingCarCount * 0.01f;
        AddReward(reward);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 2; // đỏ mặc định
    }
}
