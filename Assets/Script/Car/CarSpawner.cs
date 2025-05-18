using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public static CarSpawner Instance;
    public CarMove[] listCar;
    public Segment[] listPos;
    public void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public void SpawnRandomPos()
    {
        Spawner(listPos[Random.Range(0, listPos.Length)]);
    }    

    public void Spawner(Segment segment)
    {
        CarMove newCar = Instantiate(listCar[Random.Range(0, listCar.Length)],transform.position ,Quaternion.identity);
        newCar.CurrentSegment = segment;
    }    

}
