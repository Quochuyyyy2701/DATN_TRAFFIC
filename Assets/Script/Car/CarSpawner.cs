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
        Vector3 spawnPos = segment.wayPoint.GetStartPoint().position;
        Quaternion spawnRot = segment.wayPoint.GetStartPoint().rotation;

        // Giả sử kích thước collider gần đúng là 2x1x4 (Width x Height x Length)
        Vector3 carSize = new Vector3(2f, 1f, 4f); // Điều chỉnh theo size thật của xe

        if (!IsSpawnPositionClear(spawnPos, spawnRot, carSize))
        {
            Debug.Log("Chỗ spawn bị chiếm, bỏ qua.");
            return;
        }

        CarMove newCar = Instantiate(
            listCar[Random.Range(0, listCar.Length)],
            spawnPos,
            spawnRot
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

            for (int i = 0; i < maxSpawn; i++)
            {
                Spawner(direction.segment);
                yield return new WaitForSeconds(1f); // Đợi 1s trước khi spawn xe tiếp theo
            }
        }
    }
    private bool IsSpawnPositionClear(Vector3 position, Quaternion rotation, Vector3 size, float checkRadius = 0.5f)
    {
        // Tính center của BoxCast (dịch lên để tránh check trúng mặt đất)
        Vector3 center = position + Vector3.up * 0.5f;

        // Dùng box để kiểm tra va chạm
        Collider[] colliders = Physics.OverlapBox(center, size / 2f, rotation, LayerMask.GetMask("Car"));

        return colliders.Length == 0; // Không có xe nào gần → vị trí trống
    }

}
