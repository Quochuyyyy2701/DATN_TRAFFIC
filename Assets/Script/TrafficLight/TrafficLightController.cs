using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    [System.Serializable]
    public class DirectionGroup
    {
        public string name;
        public TrafficLight light;     // Một đèn duy nhất
        public TrafficSensor sensor;   // Một sensor duy nhất
        public Segment segment;
    }

    public List<DirectionGroup> directionsDT; // Nhóm Đông-Tây
    public List<DirectionGroup> directionsNB; // Nhóm Nam-Bắc
    public float yellowDuration = 3f;
    public bool isUsingModel;
    public int greenTime;
    [HideInInspector] public float currentGreenTimeLeft = 0f;
    public float greenCountdown_DT = 0f; // thời gian còn lại đến lượt đèn xanh của hướng Đông-Tây
    public float greenCountdown_NB = 0f; // thời gian còn lại đến lượt đèn xanh của hướng Nam-Bắc

    public void Start()
    {
        StartSequentialMode();
            
    }
    /// <summary>
    /// Trả về tổng số xe đang chờ tại tất cả các hướng
    /// </summary>
    public float GetTotalWaitingTime()
    {
        float total = 0;

        foreach (var group in directionsDT)
            total += group.sensor.waitingCarCount;

        foreach (var group in directionsNB)
            total += group.sensor.waitingCarCount;

        return total;
    }

    /// <summary>
    /// Set pha đèn xanh cho nhóm DT hoặc NB. directionIndex = 0 (DT), 1 (NB)
    /// </summary>
    public IEnumerator SetGreenPhase(int directionIndex, int greenTime)
    {
        if(isUsingModel == false) yield break;
        List<DirectionGroup> groupToGreen = (directionIndex == 0) ? directionsDT : directionsNB;
        List<DirectionGroup> groupToRed = (directionIndex == 0) ? directionsNB : directionsDT;
        this.greenTime = greenTime;
        currentGreenTimeLeft = greenTime;
        // Nhóm còn lại chuyển sang đỏ
        foreach (var group in groupToRed)
        {
            group.light.SetLightState(Color.red, LightState.Red);
        }

        // Nhóm được chọn chuyển sang xanh
        foreach (var group in groupToGreen)
        {
            group.light.SetLightState(Color.green, LightState.Green);
        }
        while (currentGreenTimeLeft > 0f)
        {
            yield return new WaitForSeconds(1f);
            currentGreenTimeLeft -= 1f;
        }

        yield return new WaitForSeconds(greenTime);

        // Pha vàng sau đèn xanh (nếu muốn)
        foreach (var group in groupToGreen)
        {
            group.light.SetLightState(Color.yellow, LightState.Yellow);
        }

        yield return new WaitForSeconds(yellowDuration);

        foreach (var group in groupToGreen)
        {
            group.light.SetLightState(Color.red, LightState.Red);
        }
    }
    public IEnumerator SetGreenPhase2(int directionIndex, int greenTime)
    {
        
        List<DirectionGroup> groupToGreen = (directionIndex == 0) ? directionsDT : directionsNB;
        List<DirectionGroup> groupToRed = (directionIndex == 0) ? directionsNB : directionsDT;
        this.greenTime = greenTime;
        currentGreenTimeLeft = greenTime;
        // Nhóm còn lại chuyển sang đỏ
        foreach (var group in groupToRed)
        {
            group.light.SetLightState(Color.red, LightState.Red);
        }

        // Nhóm được chọn chuyển sang xanh
        foreach (var group in groupToGreen)
        {
            group.light.SetLightState(Color.green, LightState.Green);
        }
        while (currentGreenTimeLeft > 0f)
        {
            yield return new WaitForSeconds(1f);
            currentGreenTimeLeft -= 1f;
        }


        yield return new WaitForSeconds(greenTime);

        // Pha vàng sau đèn xanh (nếu muốn)
        foreach (var group in groupToGreen)
        {
            group.light.SetLightState(Color.yellow, LightState.Yellow);
        }

        yield return new WaitForSeconds(yellowDuration);

        foreach (var group in groupToGreen)
        {
            group.light.SetLightState(Color.red, LightState.Red);
        }
    }

    /// <summary>
    /// Số lượng nhóm điều khiển (DT và NB)
    /// </summary>
    public int GetDirectionCount() => 2;

    /// <summary>
    /// Lấy tổng số xe trong nhóm 0 (DT) hoặc 1 (NB)
    /// </summary>
    public int GetCarCountAtDirection(int index)
    {
        int total = 0;
        var list = (index == 0) ? directionsDT : directionsNB;

        foreach (var group in list)
        {
            total += group.sensor.waitingCarCount;
        }

        return total;
    }

    /// <summary>
    /// Reset tất cả đèn về đỏ
    /// </summary>
    public void ResetIntersection()
    {
        foreach (var group in directionsDT)
        {
            group.light.SetLightState(Color.red, LightState.Red);
		group.sensor.ResetSensor();
        }

        foreach (var group in directionsNB)
        {
            group.light.SetLightState(Color.red, LightState.Red);
		group.sensor.ResetSensor();
        }
    }
    public int GetMostCongestedDirection()
    {
        int maxCars = -1;
        int bestDir = -1;

        for (int i = 0; i < GetDirectionCount(); i++)
        {
            int count = GetCarCountAtDirection(i);
            if (count > maxCars)
            {
                maxCars = count;
                bestDir = i;
            }
        }

        return bestDir;
    }

    public int GetTotalCarCount()
    {
        int total = 0;
        for (int i = 0; i < GetDirectionCount(); i++)
        {
            total += GetCarCountAtDirection(i);
        }
        return total;
    }
    private int totalPassedCars = 0;
    private float totalWaitingTime = 0f;
    private int totalWaitedCars = 0;
    private int minutePassedCars = 0;
    private float minuteWaitingTime = 0f;
    private int minuteWaitedCars = 0;

    /// <summary>
    /// Gọi hàm này khi một xe qua ngã tư, truyền vào thời gian chờ của xe đó
    /// </summary>
    public void RegisterCarPassed(float waitingTime)
    {
        totalPassedCars++;
        totalWaitingTime += waitingTime;
        totalWaitedCars++;

        // Thống kê theo phút
        minutePassedCars++;
        minuteWaitingTime += waitingTime;
        minuteWaitedCars++;
    }



    /// <summary>
    /// Trả về tổng số xe đã qua ngã tư này
    /// </summary>
    public int GetPassedCarCount()
    {
        return totalPassedCars;
    }

    /// <summary>
    /// Trả về thời gian chờ trung bình của các xe đã qua
    /// </summary>
    public float GetAverageWaitingTime()
    {
        if (totalWaitedCars == 0) return 0f;
        return totalWaitingTime / totalWaitedCars;
    }
    public float GetAverageCarsWaiting()
    {
        int totalCars = GetTotalCarCount();
        int totalDirs = GetDirectionCount();
        return totalDirs > 0 ? (float)totalCars / totalDirs : 0f;
    }
    /// Trả về số xe đã qua trong 1 phút gần nhất
    public int GetMinutePassedCarCount()
    {
        return minutePassedCars;
    }

    public int GetGreenTime()
    {
        return greenTime;
    }
    /// Trả về tổng thời gian chờ trong 1 phút gần nhất
    public float GetMinuteTotalWaitingTime()
    {
        return minuteWaitingTime;
    }

    /// Trả về thời gian chờ trung bình trong 1 phút gần nhất
    public float GetMinuteAverageWaitingTime()
    {
        return minuteWaitedCars == 0 ? 0f : minuteWaitingTime / minuteWaitedCars;
    }
    public void ResetMinuteStats()
    {
        minutePassedCars = 0;
        minuteWaitingTime = 0f;
        minuteWaitedCars = 0;
    }
    private Coroutine sequentialRoutine;

    public void StartSequentialMode()
    {
        if (sequentialRoutine != null)
            StopCoroutine(sequentialRoutine);
        sequentialRoutine = StartCoroutine(RunSequentialLights());
    }

    public void StopSequentialMode()
    {
        if (sequentialRoutine != null)
        {
            StopCoroutine(sequentialRoutine);
            sequentialRoutine = null;
        }
    }

    /// <summary>
    /// Chạy tuần tự từng hướng với thời gian cố định (ví dụ 10s mỗi hướng)
    /// </summary>
    private IEnumerator RunSequentialLights()
    {
        int direction = 0; // 0: Đông-Tây, 1: Nam-Bắc
        while (isUsingModel == false) // chỉ chạy khi không dùng AI
        {
            yield return StartCoroutine(SetGreenPhase2(direction, 10)); // 10 giây xanh mỗi pha
            direction = 1 - direction; // chuyển hướng
        }
    }
    
}
