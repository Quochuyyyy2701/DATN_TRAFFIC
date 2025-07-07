using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

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
    public event Action<LightState> OnLightChanged;
    public float cooldown;
    public float greenDuration = 5f;
    public float yellowDuration = 2f;
    public float redDuration = 5f;
    public LightState currentState;
    public TextMeshProUGUI textCooldownTime;
    private void Start()
    {
        //StartCoroutine(CycleTrafficLights());
    }

    private IEnumerator CycleTrafficLights()
    {
        while (true)
        {
            // GREEN ON
            SetLightState(Color.green, LightState.Green, 0);
            yield return new WaitForSeconds(greenDuration);

            // YELLOW ON
            SetLightState(Color.yellow, LightState.Yellow, 0);
            yield return new WaitForSeconds(yellowDuration);

            // RED ON
            SetLightState(Color.red, LightState.Red, 0);
            yield return new WaitForSeconds(redDuration);
        }
    }

    public void Update()
    {
       
        textCooldownTime.text = cooldown.ToString();

    }
    void DecreaseCooldown()
    {
        if (cooldown > 0)
        {
            cooldown--;
        }

        if (cooldown <= 0)
        {
            cooldown = 0;
            CancelInvoke("DecreaseCooldown"); // Dừng gọi khi cooldown về 0
        }
    }

    private void FixedUpdate()
    {
        Traffic();
    }
    public void SetLightState(Color color, LightState currentStage,float time)
    {
        CancelInvoke("DecreaseCooldown");
        cooldown = time;
        InvokeRepeating("DecreaseCooldown", 1f, 1f);
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
