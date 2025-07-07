using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public  static Spawner _instance;
    public CarMove[] listCar;

    public List<CarMove> listCarSpawn = new List<CarMove>();
    public void Awake()
    {
        _instance = this;
    }
    public void SpawnerCar(Segment segment)
    {
        CarMove newCar = Instantiate(
            listCar[Random.Range(0, listCar.Length)],
            segment.wayPoint.GetStartPoint().position,
            segment.wayPoint.GetStartPoint().rotation
        );
        newCar.CurrentSegment = segment;
        listCarSpawn.Add(newCar);
    }

    public void ClearAllCar()
    {
        foreach (var car in listCarSpawn)
        {
            Destroy(car.gameObject);
        }
        listCarSpawn.Clear(); // Xóa tất cả phần tử trong list để count = 0
    }    
}
