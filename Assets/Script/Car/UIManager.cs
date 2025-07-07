using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Const
{
    public static bool isUsingModel1 = true;
    public static bool isUsingModel2 = true;
    public static bool isUsingModel3 = true;
    public static bool isSpawnerCar = false;
}

public class UIManager : MonoBehaviour
{
    public Button buttonSpawn;
    public Button hideInfo;
    public Button resetCar;
    public Button resetTest;
    public Image BG;
    public TextMeshProUGUI textTF1;
    public TextMeshProUGUI textTF2;
    public TextMeshProUGUI textTF3;

    public Toggle toggle1;
    public Toggle toggle2;
    public Toggle toggle3;

    public TrafficLightController lightController1;
    public TrafficLightController lightController2;
    public TrafficLightController lightController3;

    private bool isAvtive;
    private Coroutine trafficInfoCoroutine;

    void Start()
    {
        toggle1.onValueChanged.AddListener((isOn) => UpdateModelUsage(1, isOn));
        toggle2.onValueChanged.AddListener((isOn) => UpdateModelUsage(2, isOn));
        toggle3.onValueChanged.AddListener((isOn) => UpdateModelUsage(3, isOn));
        buttonSpawn.onClick.AddListener(() => { ButtonSpawn(); });
        hideInfo.onClick.AddListener(() => { HideInfo(); });
        resetCar.onClick.AddListener(() => { ResetCar(); });
        resetTest.onClick.AddListener(() => { ResetTest(); });

        buttonSpawn.GetComponentInChildren<TextMeshProUGUI>().text = "SpawnerCar: " + (Const.isSpawnerCar ? "On" : "Off");

        trafficInfoCoroutine = StartCoroutine(UpdateTrafficInfoEveryMinute());
    }

    public void ButtonSpawn()
    {
        Const.isSpawnerCar = !Const.isSpawnerCar;
        buttonSpawn.GetComponentInChildren<TextMeshProUGUI>().text = "SpawnerCar: " + (Const.isSpawnerCar ? "On" : "Off");
    }

    private void UpdateModelUsage(int index, bool isUsingModel)
    {
        switch (index)
        {
            case 1:
                Const.isUsingModel1 = isUsingModel;
                lightController1.ForceModelCheck();
                break;
            case 2:
                Const.isUsingModel2 = isUsingModel;
                lightController2.ForceModelCheck();
                break;
            case 3:
                Const.isUsingModel3 = isUsingModel;
                lightController3.ForceModelCheck();
                break;
        }
    }

    public void HideInfo()
    {
        isAvtive = !isAvtive;
        BG.gameObject.SetActive(isAvtive);
    }

    public void GetInfoTrafficController()
    {
        string info1 = GetIntersectionInfo(lightController1, "Giao lộ 1", 1);
        string info2 = GetIntersectionInfo(lightController2, "Giao lộ 2", 2);
        string info3 = GetIntersectionInfo(lightController3, "Giao lộ 3", 3);

        textTF1.text = info1;
        textTF2.text = info2;
        textTF3.text = info3;
    }

    public void ResetMinuteStats()
    {
        lightController1?.ResetMinuteStats();
        lightController2?.ResetMinuteStats();
        lightController3?.ResetMinuteStats();
    }

    public void ResetCar()
    {
        Spawner._instance.ClearAllCar();
        // ✅ Reset xe
        lightController1.ResetAllCar();
        lightController2.ResetAllCar();
        lightController3.ResetAllCar();

        // ✅ Reset thống kê chờ
        ResetMinuteStats();
        
        // ✅ Reset lại đồng hồ đếm 60s
        if (trafficInfoCoroutine != null)
        {
            StopCoroutine(trafficInfoCoroutine);
        }
        trafficInfoCoroutine = StartCoroutine(UpdateTrafficInfoEveryMinute());
    }

    private IEnumerator UpdateTrafficInfoEveryMinute()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f);

            GetInfoTrafficController();
            ResetMinuteStats();
        }
    }
    public void ResetTest()
    {
        lightController1.ResetAllCar();
        lightController2.ResetAllCar();
        lightController3.ResetAllCar();
        ResetMinuteStats();
        if (trafficInfoCoroutine != null)
        {
            StopCoroutine(trafficInfoCoroutine);
        }
        trafficInfoCoroutine = StartCoroutine(UpdateTrafficInfoEveryMinute());
        lightController1.spawner.ResetAndSpawnRandomCars(lightController1, 8);
        lightController2.spawner.ResetAndSpawnRandomCars(lightController2, 8);
        lightController3.spawner.ResetAndSpawnRandomCars(lightController3, 8);
    }
    private string GetIntersectionInfo(TrafficLightController controller, string name, int index)
    {
        int passedLastMinute = controller.GetMinutePassedCarCount();
        float avgWaitTimeMinute = controller.GetMinuteAverageWaitingTime();
        float totalWaitTimeMinute = controller.GetMinuteTotalWaitingTime();

        bool isAI = index switch
        {
            1 => Const.isUsingModel1,
            2 => Const.isUsingModel2,
            3 => Const.isUsingModel3,
            _ => true
        };

        return $"{name} ({(isAI ? "AI điều khiển" : "Thủ công")})\n" +
               $"=== 1 phút gần nhất ===\n" +
               $"- Xe qua: {passedLastMinute}\n" +
               $"- TG chờ TB: {avgWaitTimeMinute:F2}s\n" +
               $"- Tổng TG chờ: {totalWaitTimeMinute:F2}s\n";
    }
}
