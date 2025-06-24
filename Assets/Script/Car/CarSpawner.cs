using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public CarMove[] listCar;
    public Segment[] listPos;
    public int spawnPerDirection = 3;
    public List<CarMove> listSpawn;

    private void Awake()
    {
        if (listSpawn == null)
            listSpawn = new List<CarMove>();
    }

    public void ClearAllCars()
    {
        foreach (var car in listSpawn)
        {
            Destroy(car.gameObject);
        }
        listSpawn.Clear(); // Xóa tất cả phần tử trong list để count = 0
    }

    public void SpawnRandomCars()
    {
        for (int i = 0; i < spawnPerDirection; i++)
        {
            foreach (Segment pos in listPos)
            {
                Spawner(pos);
            }
        }
    }

    public void SpawnRandomPos()
    {
        Spawner(listPos[Random.Range(0, listPos.Length)]);
    }

    public void Spawner(Segment segment)
    {
        CarMove newCar = Instantiate(
            listCar[Random.Range(0, listCar.Length)],
            segment.wayPoint.GetStartPoint().position,
            segment.wayPoint.GetStartPoint().rotation
        );
        newCar.CurrentSegment = segment;
        listSpawn.Add(newCar);
    }

    public void ResetAndSpawnRandomCars(TrafficLightController controller, int maxSpawnPerDirection = 5)
    {
        ClearAllCars();

        if (controller == null) return;

        // Spawn xe cho cả nhóm Đông-Tây và Nam-Bắc
        StartCoroutine(SpawnForGroupCoroutine(controller.directionsDT, maxSpawnPerDirection));
        StartCoroutine(SpawnForGroupCoroutine(controller.directionsNB, maxSpawnPerDirection));
    }

    private IEnumerator SpawnForGroupCoroutine(List<TrafficLightController.DirectionGroup> group, int maxSpawn)
    {
        foreach (var direction in group)
        {
            int spawnCount = Random.Range(1, maxSpawn + 1); // VD: từ 1 đến maxSpawn

            for (int i = 0; i < spawnCount; i++)
            {
                Spawner(direction.segment);
                yield return new WaitForSeconds(1f); // Đợi 1s trước khi spawn xe tiếp theo
            }
        }
    }
}
