using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSensor : MonoBehaviour
{
    public TrafficLightController controller;
    public int waitingCarCount = 0;
    private Dictionary<GameObject, float> carArrivalTimes = new Dictionary<GameObject, float>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            waitingCarCount++;

            if (!carArrivalTimes.ContainsKey(other.gameObject))
                carArrivalTimes.Add(other.gameObject, Time.time);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            waitingCarCount--;

            if (carArrivalTimes.ContainsKey(other.gameObject))
            {
                float waitTime = Time.time - carArrivalTimes[other.gameObject];
                controller.RegisterCarPassed(waitTime); // ✅ Gọi đúng controller tương ứng
                carArrivalTimes.Remove(other.gameObject);
            }
        }
    }

    public void ResetSensor()
    {
        waitingCarCount = 0;
    }
}

