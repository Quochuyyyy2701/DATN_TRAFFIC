using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    public TrafficLight[] trafficLights; // gán 4 đèn
    public TrafficSensor[] sensors;      // gán 4 sensor

    public void SetAllLights(LightState state)
    {
        foreach (var light in trafficLights)
        {
            switch (state)
            {
                case LightState.Green: light.SetLightState(Color.green, LightState.Green); break;
                case LightState.Yellow: light.SetLightState(Color.yellow, LightState.Yellow); break;
                case LightState.Red: light.SetLightState(Color.red, LightState.Red); break;
            }
        }
    }

    public int TotalWaitingCars()
    {
        int total = 0;
        foreach (var s in sensors)
            total += s.waitingCarCount;
        return total;
    }
}

