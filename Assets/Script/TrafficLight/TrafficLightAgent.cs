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
        Debug.Log("===== Episode Start =====");
        timer = 0f;

        controller.ResetIntersection();
        carSpawner.ClearAllCars();
        carSpawner.ResetAndSpawnRandomCars(controller, 8);

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

    int proposedDirection = actions.DiscreteActions[0];
    int direction = proposedDirection;
    int baseGreenTime = actions.DiscreteActions[1] + 3;

    int thisCar = controller.GetCarCountAtDirection(direction);
    int otherDirection = 1 - direction;
    int otherCar = controller.GetCarCountAtDirection(otherDirection);

    int carDiff = Mathf.Abs(thisCar - otherCar);

    // ===== Logic mở 2 lần liên tiếp nếu chênh lệch nhiều =====
    if (direction == lastDirection)
    {
        if (carDiff <= 4)
        {
            // Nếu xe 2 hướng không chênh lệch nhiều => phải luân phiên
            AddReward(-1f);
            Debug.LogWarning($"[REWARD] Hướng xe gần bằng nhau mà vẫn mở lại hướng {direction} => -1");

            direction = otherDirection;
            thisCar = controller.GetCarCountAtDirection(direction);
            otherCar = controller.GetCarCountAtDirection(1 - direction);
            sameDirectionCount = 1;
        }
        else if (sameDirectionCount >= 2&& carDiff < 6)
        {
            // Mở quá 2 lần liên tiếp, kể cả khi chênh nhiều => vẫn phạt
            AddReward(-2f);
            Debug.LogWarning($"[REWARD] Cố mở hướng {direction} >2 lần liên tiếp => -2");

            direction = otherDirection;
            thisCar = controller.GetCarCountAtDirection(direction);
            otherCar = controller.GetCarCountAtDirection(1 - direction);
            sameDirectionCount = 1;
        }
        else
        {
            Debug.Log($"[CHO PHÉP] Mở lại hướng {direction} vì đang tắc hơn (chênh lệch = {carDiff})");
        }
    }

    // === Tính thời gian đèn xanh ===
    int greenTime = Mathf.Clamp(baseGreenTime + thisCar / 2, 3, 15);

    if (carDiff >= 6)
    {
        // Tăng thêm đèn xanh nếu đang tắc nhiều hơn
        greenTime = Mathf.Clamp(greenTime + 2, 3, 15);
        Debug.Log($"[TỐI ƯU] Hướng {direction} đang tắc hơn nhiều => tăng thời gian đèn");
    }

    if (thisCar >= 8 && otherCar <= 2)
    {
        greenTime = Mathf.Clamp(greenTime + 2, 3, 15);
        Debug.LogWarning("[TỐI ƯU] Hướng đang tắc, hướng còn lại gần như trống => rút ngắn thời gian đèn");
    }

    if (thisCar <= 1 && otherCar >= 10)
    {
        Debug.LogWarning("Hướng này không có xe, hướng kia tắc => đổi đèn sớm");
        greenTime = 1;
    }

    Debug.Log($"[ACTION] Chọn hướng: {(direction == 0 ? "Đông-Tây" : "Nam-Bắc")} | Thời gian đèn xanh: {greenTime}s");
    Debug.Log($"[CARS] Xe tại hướng {direction}: {thisCar} | Hướng còn lại: {otherCar}");

    StartCoroutine(RunPhase(direction, greenTime));

    if (direction == lastDirection)
        sameDirectionCount++;
    else
        sameDirectionCount = 1;

    lastDirection = direction;

    // === Tính reward ===
    float reward = 0f;
    float currentWaitingTime = controller.GetTotalWaitingTime();
    int currentCarCount = controller.GetTotalCarCount();

    float deltaWaitingTime = lastTotalWaitingTime - currentWaitingTime;
    int deltaCars = lastTotalCarCount - currentCarCount;

    if (deltaCars > 0)
    {
        float r = deltaCars * 1f;
        reward += r;
        Debug.Log($"[REWARD] Giảm xe: {deltaCars} => +{r}");
    }

    if (deltaWaitingTime > 0)
    {
        float r = deltaWaitingTime * 1f;
        reward += r;
        Debug.Log($"[REWARD] Giảm thời gian chờ: {deltaWaitingTime:F2} => +{r:F2}");
    }

    if (thisCar == 0)
    {
        reward -= 1f;
        Debug.LogWarning("[REWARD] Không có xe ở hướng được mở => -1");
    }

    if (direction != controller.GetMostCongestedDirection() && otherCar - thisCar > 5)
    {
        reward -= 2f;
        Debug.LogWarning("[REWARD] Bỏ qua hướng đang tắc nặng => -2");
    }

    Debug.Log($"[TOTAL REWARD] => {reward:F2}\n");

    SetReward(reward);

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
            Debug.Log("===== Episode End =====\n");
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discrete = actionsOut.DiscreteActions;

        int dt = controller.GetCarCountAtDirection(0); // Đông-Tây
        int nb = controller.GetCarCountAtDirection(1); // Nam-Bắc

        discrete[0] = (dt >= nb) ? 0 : 1;
    }
}
