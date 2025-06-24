using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections;

public class TrafficLightAgent : Agent
{
    public TrafficLightController controller;
    public CarSpawner carSpawner;
    public float episodeDuration = 60f; // ⬅️ Tăng thời gian để agent có nhiều hành động hơn
    private float timer;
    private bool isPhaseRunning = false;
    private float lastTotalWaitingTime = 0f;
    private int lastTotalCarCount = 0;

    private int lastDirection = -1;
    private int sameDirectionCount = 0;

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Start");
        timer = 0f;

        controller.ResetIntersection();
        carSpawner.ClearAllCars();
        carSpawner.ResetAndSpawnRandomCars(controller, 5);

        lastTotalWaitingTime = controller.GetTotalWaitingTime();
        lastTotalCarCount = controller.GetTotalCarCount();

        lastDirection = -1;
        sameDirectionCount = 0;

        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        int directionCount = controller.GetDirectionCount();
        float maxCars = 20f; // ⬅️ Giả sử mỗi hướng tối đa có 20 xe

        for (int i = 0; i < directionCount; i++)
        {
            float normalized = controller.GetCarCountAtDirection(i) / maxCars;
            sensor.AddObservation(normalized); // ✅ Normalize số xe
        }

        sensor.AddObservation(lastDirection);
        sensor.AddObservation(sameDirectionCount / 5f);
        sensor.AddObservation(controller.GetTotalWaitingTime() / 200f); // ⬅️ Normalize thời gian chờ nếu cần
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isPhaseRunning) return;

        int direction = actions.DiscreteActions[0]; // 0: DT, 1: NB
        int greenTimeStep = actions.DiscreteActions[1];
        int greenTime = 5 + greenTimeStep; // 5 → 15s

        Debug.Log($"Action received: Direction={direction}, GreenTime={greenTime}");

        if (direction == lastDirection)
            sameDirectionCount++;
        else
            sameDirectionCount = 0;

        lastDirection = direction;

        StartCoroutine(RunPhase(direction, greenTime));

        // --- TÍNH REWARD ---
        float reward = 0f;

        int totalCarsBefore = lastTotalCarCount;
        float waitingTimeBefore = lastTotalWaitingTime;

        float currentWaitingTime = controller.GetTotalWaitingTime();
        int currentCarCount = controller.GetTotalCarCount();

        float deltaWaitingTime = waitingTimeBefore - currentWaitingTime;
        int deltaCars = totalCarsBefore - currentCarCount;

        // 1. Giảm thời gian chờ
        reward += deltaWaitingTime * 0.5f;

        // 2. Giảm tổng số xe
        reward += deltaCars * 0.2f;

        // 3. Phạt nếu chọn hướng không có xe
        int carCount = controller.GetCarCountAtDirection(direction);
        if (carCount == 0)
        {
            reward -= 2f; // ✅ Phạt mạnh hơn
            Debug.LogWarning("Chọn hướng không có xe!");
        }

        // 4. Thưởng nếu chọn hướng có nhiều xe
        reward += carCount * 0.1f;

        // 5. Phạt nếu chọn liên tục cùng 1 hướng
        if (sameDirectionCount >= 2)
        {
            reward -= sameDirectionCount * 1.5f; // ✅ Phạt tăng dần
            Debug.LogWarning("Chọn cùng hướng liên tục!");
        }

        // 6. Phạt nếu không cải thiện
        if (deltaWaitingTime <= 0 && deltaCars <= 0)
        {
            reward -= 0.5f;
        }

        // 7. Phạt nếu không chọn hướng đang tắc nhất
        int mostCongestedDir = controller.GetMostCongestedDirection();
        if (direction != mostCongestedDir && controller.GetCarCountAtDirection(mostCongestedDir) > 3)
        {
            reward -= 3f; // ✅ Phạt nặng nếu bỏ qua hướng tắc
            Debug.LogWarning("Bỏ qua hướng tắc nhất!");
        }
        else if (direction == mostCongestedDir && carCount > 3)
        {
            reward += 2f; // ✅ Thưởng nếu chọn đúng hướng tắc
        }

        // 8. Phạt nếu thời gian đèn xanh quá dài so với số xe
        if (greenTime > carCount * 2 + 2)
        {
            reward -= 0.5f;
        }

        SetReward(reward);
        lastTotalWaitingTime = currentWaitingTime;
        lastTotalCarCount = currentCarCount;
    }

    private IEnumerator RunPhase(int direction, int greenTime)
    {
        isPhaseRunning = true;
        yield return StartCoroutine(controller.SetGreenPhase(direction, greenTime));
        isPhaseRunning = false;
        RequestDecision();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (Academy.Instance.IsCommunicatorOn)
        {
            if (timer >= episodeDuration)
            {
                EndEpisode();
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discrete = actionsOut.DiscreteActions;

        int dt = controller.GetCarCountAtDirection(0);
        int nb = controller.GetCarCountAtDirection(1);

        discrete[0] = (dt >= nb) ? 0 : 1;
        discrete[1] = Mathf.Clamp((Mathf.Max(dt, nb) / 2), 0, 10);
    }
}
