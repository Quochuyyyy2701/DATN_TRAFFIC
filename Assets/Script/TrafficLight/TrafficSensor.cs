using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSensor : MonoBehaviour
{
    public int waitingCarCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car")) waitingCarCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car")) waitingCarCount--;
    }
}

