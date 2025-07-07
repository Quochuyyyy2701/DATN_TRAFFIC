using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections;

public class TrafficLightAgent : Agent
{
    public TrafficLightController controller;
    public CarSpawner carSpawner;
    public float episodeDuration = 60f;

    private float timer;
    private bool isPhaseRunning = false;
    private float lastTotalWaitingTime = 0f;
    private int lastTotalCarCount = 0;

    private int lastDirection = -1;
    private int sameDirectionCount = 0;

    public override void OnEpisodeBegin()
    {
       // Debug.Log("===== Episode Start =====");
        timer = 0f;

        controller.ResetIntersection();
        carSpawner.ClearAllCars();
        //carSpawner.ResetAndSpawnRandomCars(controller, 8);

        lastTotalWaitingTime = controller.GetTotalWaitingTime();
        lastTotalCarCount = controller.GetTotalCarCount();

        lastDirection = -1;
        sameDirectionCount = 0;

        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        int directionCount = controller.GetDirectionCount();
        float maxCars = 20f;

        for (int i = 0; i < directionCount; i++)
        {
            float normalized = controller.GetCarCountAtDirection(i) / maxCars;
            sensor.AddObservation(normalized);
        }

        sensor.AddObservation((lastDirection + 1f) / 2f); // Normalize về [0, 1]

        sensor.AddObservation(sameDirectionCount / 5f);
        sensor.AddObservation(controller.GetTotalWaitingTime() / 200f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isPhaseRunning) return;

        int direction = actions.DiscreteActions[0];
        int baseGreenTime = actions.DiscreteActions[1] + 3;

        int thisCar = controller.GetCarCountAtDirection(direction);
        int otherDirection = 1 - direction;
        int otherCar = controller.GetCarCountAtDirection(otherDirection);

        int greenTime = Mathf.Clamp(baseGreenTime + thisCar / 2, 3, 15);

        //Debug.Log($"[ACTION] Chọn hướng: {(direction == 0 ? "Đông-Tây" : "Nam-Bắc")} | Thời gian đèn xanh: {greenTime}s");
        //Debug.Log($"[CARS] Xe tại hướng {direction}: {thisCar} | Hướng còn lại: {otherCar}");

        StartCoroutine(RunPhase(direction, greenTime));

        lastDirection = direction;

        // === Tính reward ===
        float reward = 0f;
        float currentWaitingTime = controller.GetTotalWaitingTime();
        int currentCarCount = controller.GetTotalCarCount();

        float deltaWaitingTime = lastTotalWaitingTime - currentWaitingTime;
        int deltaCars = lastTotalCarCount - currentCarCount;

        if (deltaCars > 0)
        {
            reward += deltaCars * 1f;
           // Debug.Log($"[REWARD] Giảm xe: {deltaCars} => +{deltaCars}");
        }

        if (deltaWaitingTime > 0)
        {
            reward += deltaWaitingTime * 1f;
           // Debug.Log($"[REWARD] Giảm thời gian chờ: {deltaWaitingTime:F2} => +{deltaWaitingTime:F2}");
        }

        // Phạt nhẹ nếu mở hướng không có xe
        if (thisCar == 0)
        {
            reward -= 1f;
           // Debug.LogWarning("[REWARD] Không có xe ở hướng được mở => -1");
        }

        SetReward(reward);

        //Debug.Log($"[TOTAL REWARD] => {reward:F2}\n");

        lastTotalWaitingTime = currentWaitingTime;
        lastTotalCarCount = currentCarCount;
    }


    private IEnumerator RunPhase(int direction, int greenTime)
    {
        isPhaseRunning = true;
        yield return StartCoroutine(controller.SetGreenPhase(direction, greenTime));
        isPhaseRunning = false;
        RequestDecision(); // Gọi lại hành động tiếp theo
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (Academy.Instance.IsCommunicatorOn && timer >= episodeDuration)
        {
           // Debug.Log("===== Episode End =====\n");
            EndEpisode();
        }
    }

 
}
