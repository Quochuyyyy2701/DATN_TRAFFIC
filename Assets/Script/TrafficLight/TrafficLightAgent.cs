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
        trafficLight.SetLightState(Color.red, LightState.Red);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Quan sát ví dụ: số lượng xe đang chờ
        sensor.AddObservation(trafficSensor.waitingCarCount);
    }

    private float timeSinceLastChange = 0f;
    public float changeCooldown = 5f; // giây

    public override void OnActionReceived(ActionBuffers actions)
    {
        timeSinceLastChange += Time.deltaTime;
        if (timeSinceLastChange < changeCooldown)
            return; // chưa đủ thời gian để đổi đèn

        timeSinceLastChange = 0f;

        int action = actions.DiscreteActions[0];
        switch (action)
        {
            case 0:
                trafficLight.SetLightState(Color.green, LightState.Green);
                break;
            case 1:
                trafficLight.SetLightState(Color.yellow, LightState.Yellow);
                break;
            case 2:
                trafficLight.SetLightState(Color.red, LightState.Red);
                break;
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 2; // đỏ mặc định
    }
}
