using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightController3 : MonoBehaviour
{
    public TrafficLight[] northSouthLights;
    public TrafficLight[] eastWestLights;

    public float yellowDuration = 2f;
    private LightState currentMainState = LightState.Red;

    public void ResetLights()
    {
        SetLightState(0); // mặc định NS xanh
    }

    public void SetLightState(int action)
    {
        // 0 = NS xanh, EW đỏ | 1 = EW xanh, NS đỏ
        if (action == 0 && currentMainState != LightState.Green)
        {
            StartCoroutine(SwitchLights(northSouthLights, eastWestLights));
            currentMainState = LightState.Green;
        }
        else if (action == 1 && currentMainState != LightState.Red)
        {
            StartCoroutine(SwitchLights(eastWestLights, northSouthLights));
            currentMainState = LightState.Red;
        }
    }

    private IEnumerator SwitchLights(TrafficLight[] toGreen, TrafficLight[] toRed)
    {
        // tất cả đèn đang xanh → vàng → đỏ
        foreach (var light in toRed)
            light.SetLightState(Color.red, LightState.Yellow);

        yield return new WaitForSeconds(yellowDuration);

        foreach (var light in toRed)
            light.SetLightState(Color.red, LightState.Red);

        foreach (var light in toGreen)
            light.SetLightState(Color.green, LightState.Green);
    }

    public float GetTotalWaitingTime()
    {
        float total = 0f;
        foreach (var light in northSouthLights)
            if (light.currentState != LightState.Green) total += 1;

        foreach (var light in eastWestLights)
            if (light.currentState != LightState.Green) total += 1;

        return total;
    }
}
