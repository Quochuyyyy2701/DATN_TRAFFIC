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
        List<DirectionGroup> groupToGreen = (directionIndex == 0) ? directionsDT : directionsNB;
        List<DirectionGroup> groupToRed = (directionIndex == 0) ? directionsNB : directionsDT;

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
        }

        foreach (var group in directionsNB)
        {
            group.light.SetLightState(Color.red, LightState.Red);
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


}
