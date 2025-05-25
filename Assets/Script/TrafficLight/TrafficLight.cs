using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum LightState
{
    Green,
    Yellow,
    Red
}



public class TrafficLight : CustomMonoBehaviour
{
    [SerializeField] protected Light trafficLight;
    [SerializeField] protected Collider trafficCollider;
    public event Action<LightState>  OnLightChanged;
    public float greenDuration = 5f;
    public float yellowDuration = 2f;
    public float redDuration = 5f;
    public LightState currentState;
    private void Start()
    {
        //StartCoroutine(CycleTrafficLights());
    }

    private IEnumerator CycleTrafficLights()
    {
        while (true)
        {
            // GREEN ON
            SetLightState(Color.green, LightState.Green);
            yield return new WaitForSeconds(greenDuration);

            // YELLOW ON
            SetLightState(Color.yellow, LightState.Yellow);
            yield return new WaitForSeconds(yellowDuration);

            // RED ON
            SetLightState(Color.red , LightState.Red);
            yield return new WaitForSeconds(redDuration);
        }
    }

    private void FixedUpdate()
    {
        Traffic();
    }
    public void SetLightState(Color color, LightState currentStage)
    {
        trafficLight.color = color;
        currentState = currentStage;
    }

    public override void LoadComponent()
    {
        LoadTrafficLight();
        LoadTrafficCollider();
    }
    public void LoadTrafficLight()
    {
        if (trafficLight != null) return;
        trafficLight = transform.GetComponentInChildren<Light>();
    }     
    public void LoadTrafficCollider()
    {
        if (trafficCollider != null) return;
        trafficCollider = transform.GetComponent<Collider>();
    }    

    public void Traffic()
    {
        if(trafficLight.color == Color.red)
        {
            trafficCollider.enabled = true;
            OnLightChanged?.Invoke(currentState);
           
        } else if(trafficLight.color == Color.green)
        {
            trafficCollider.enabled = false;
            OnLightChanged?.Invoke(currentState);
           
        } else if(trafficLight.color == Color.yellow)
        {
            trafficCollider.enabled = true;
            OnLightChanged?.Invoke(currentState);
        }    
    }    
    
}
